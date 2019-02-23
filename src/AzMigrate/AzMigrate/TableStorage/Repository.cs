using AzMigrate.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzMigrate.TableStorage
{
    public abstract class Repository<TKey, TValue> : IRepository<TKey, TValue>
        where TValue : SingleKeyEntity
    {
        private readonly string _connectionString;
        private readonly string _tableName;

        protected CloudTable _table;

        public Repository(string connectionString, string tableName)
        {
            _connectionString = connectionString;
            _tableName = tableName;
        }

        public virtual async Task DeleteAsync(TKey key)
        {
            ValidateKey(key);

            var innerKey = GetKeyFromKey(key);

            var operation = TableOperation.Retrieve(innerKey.PartitionKey, innerKey.Id);
            var result = await GetTable().ExecuteAsync(operation).ConfigureAwait(false);

            var operation2 = TableOperation.Delete((ITableEntity)result.Result);
            await GetTable().ExecuteAsync(operation2).ConfigureAwait(false);
        }

        public virtual async Task<IEnumerable<TValue>> GetAllAsync()
        {
            TableContinuationToken token = null;
            var entities = new List<TValue>();
            do
            {
                var queryResult = await GetTable().ExecuteQuerySegmentedAsync(new TableQuery(), token);
                foreach (var result in queryResult.Results)
                {
                    var entity = EntityConverter.ConvertTo<TValue>(result);
                    entities.Add(entity);
                }
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entities;
        }

        public virtual async Task<TValue> GetAsync(TKey key)
        {
            ValidateKey(key);

            var innerKey = GetKeyFromKey(key);
            var operation = TableOperation.Retrieve(innerKey.PartitionKey, innerKey.Id);
            var result = await GetTable().ExecuteAsync(operation);
            var item = EntityConverter.ConvertTo<TValue>((ITableEntity)result.Result);
            return item;
        }

        public virtual async Task<TValue> UpsertAsync(TValue entity)
        {
            var ky = GetKeyFromEntity(entity);

            var tableEntity = EntityConverter.ConvertTo(entity, ky.PartitionKey, ky.Id);
            var operation = TableOperation.InsertOrMerge(tableEntity);
            var result = await GetTable().ExecuteAsync(operation);
            return result.Result as TValue;
        }

        public abstract IQueryable<TValue> CreateQuery();

        public abstract IEnumerable<T> ExecuteQuery<T>(string query);

        protected virtual void ValidateKey(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
        }
        protected abstract PartitionedItemKey GetKeyFromKey(TKey key);
        protected virtual PartitionedItemKey GetKeyFromEntity(TValue value)
        {
            if (string.IsNullOrEmpty(value.Id))
            {
                throw new ArgumentNullException(nameof(value.Id));
            }
            var ky = new PartitionedItemKey
            {
                Id = value.Id,
                PartitionKey = GetPropertyValue(value, GetPartitionKey())
            };
            return ky;
        }

        protected virtual string GetPartitionKey()
        {
            return "PartitionKey";
        }

        protected virtual CloudTable GetTable()
        {
            if (_table == null)
            {
                var account = CloudStorageAccount.Parse(_connectionString);
                var client = account.CreateCloudTableClient();
                _table = client.GetTableReference(_tableName);
                _table.CreateIfNotExistsAsync().GetAwaiter().GetResult();
                return _table;
            }
            return _table;
        }

        private string GetPropertyValue(TValue entity, string propertyName)
        {
            try
            {
                var value = entity.GetType().GetProperty(propertyName).GetValue(entity, null);
                if (value is string)
                {
                    return $"{value}";
                }
            }
            catch (Exception)
            {

            }
            return "";
        }
    }
}
