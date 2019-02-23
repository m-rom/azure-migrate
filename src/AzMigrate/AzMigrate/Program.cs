using AzMigrate.Model;
using CommandLine;
using System;

namespace AzMigrate
{
    class Program
    {
        static void Main(string[] args)
        {
            // ###########################
            // Add special mappings here:
            AutoMapper.Mapper.Initialize(cfg =>
                cfg.CreateMap<SourceModel, TargetModel>()
            );
            // ###########################

            Parser.Default
                .ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    // ################################################
                    // Run without args?
                    // ################################################
                    // o.SourceConnectionString = "<ConnectionString>";
                    // o.SourceDatabase = "<DB>";                                   // [CosmosDB] 
                    // o.SourceTable = "<Table or Collection>";

                    // o.TargetConnectionString = "<ConnectionString>";
                    // o.TargetDatabase = "<DB>";                                   // [CosmosDB] 
                    // o.TargetTable = "<Table or Collection>";
                    // o.TargetPartitionKey = "<New partition key property name>";  // [CosmosDB] 
                    // ################################################

                    Console.WriteLine($"Start migration: {o.SourceType} -> {o.TargetType}");

                    MigrationManager.MigrateAsync<SourceModel, TargetModel>(o).GetAwaiter().GetResult();
                });
            Console.Read();
        }

        // ###########################
        // Your input model (Table or Collection model)
        class SourceModel : SingleKeyEntity
        {

        }

        // ###########################
        // Your output model (Table or Collection model)
        class TargetModel : SingleKeyEntity
        {

        }
    }
}
