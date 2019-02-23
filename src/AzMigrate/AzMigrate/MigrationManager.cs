using AzMigrate.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzMigrate
{
    class MigrationManager
    {
        public static async Task MigrateAsync<TIn, TOut>(Options options)
            where TIn : SingleKeyEntity
            where TOut : SingleKeyEntity
        {
            IEnumerable<TIn> items = new List<TIn>();
            

            if (options.IsSourcePartitioned)
            {
                var source = GetPartitionedRepository<TIn>(options.SourceType, options.SourceConnectionString, options.SourceTable, options.SourceDatabase, options.SourcePartitionKey);
                items = await source.GetAllAsync();
            }
            else
            {
                var source = GetRepository<TIn>(options.SourceType, options.SourceConnectionString, options.SourceTable, options.SourceDatabase);
                items = await source.GetAllAsync();
            }

            if (options.IsTargetPartitioned)
            {
                var target = GetPartitionedRepository<TOut>(options.TargetType, options.TargetConnectionString, options.TargetTable, options.TargetDatabase, options.TargetPartitionKey);
                await MigrateAsync<TIn, TOut>(items, (item) => target.UpsertAsync(item), options.IgnoreErrors);
            }
            else
            {
                var target = GetRepository<TOut>(options.TargetType, options.TargetConnectionString, options.TargetTable, options.TargetDatabase);
                await MigrateAsync<TIn, TOut>(items, (item) => target.UpsertAsync(item), options.IgnoreErrors);
            }
        }

        private static async Task MigrateAsync<TIn, TOut>(IEnumerable<TIn> items, Func<TOut, Task> action, bool ignoreErrors = true)
        {
            var migratedItems = 0;
            foreach (var item in items)
            {
                try
                {
                    Console.WriteLine($"Migrating: {item}");

                    var targetItem = AutoMapper.Mapper.Map<TOut>(item);
                    await action(targetItem);
                    migratedItems++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Cannot migrate: {item}. {ex.Message}");
                    if (!ignoreErrors)
                    {
                        Console.WriteLine($"Migrated only: {migratedItems}/{items.Count()} items!");
                        throw;
                    }
                }
            }
            Console.WriteLine($"Migrated: {migratedItems} items");
        }


        private static IRepository<PartitionedItemKey, T> GetPartitionedRepository<T>(DataSourceType type, string connectionString, string table, string db, string partitionKeyProperty = "")
            where T : SingleKeyEntity
        {
            switch (type)
            {
                case DataSourceType.AzureStorageTablePartitioned:
                    return new TableStorage.PartitionedTable<T>(connectionString, table, partitionKeyProperty);

                case DataSourceType.CosmosDBPartitioned:
                    return new CosmosDB.CosmosPartitionedCollection<T>(connectionString, db, table, partitionKeyProperty);
            }
            throw new NotSupportedException($"Data {type} not supported yet!");
        }

        private static IRepository<string, T> GetRepository<T>(DataSourceType type, string connectionString, string table, string db)
            where T : SingleKeyEntity
        {
            switch (type)
            {
                case DataSourceType.AzureStorageTable:
                    return new TableStorage.HashedTable<T>(connectionString, table);

                case DataSourceType.CosmosDB:
                    return new CosmosDB.CosmosCollection<T>(connectionString, db, table);
            }
            throw new NotSupportedException($"Data {type} not supported yet!");
        }
    }
}
