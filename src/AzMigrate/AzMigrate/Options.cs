using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzMigrate
{
    class Options
    {
        [Option('i', "ignoreErrors", Required = false, HelpText = "Ignore not migratable items.", Default = true)]
        public bool IgnoreErrors { get; set; }

        [Option('s', "source", Required = false, HelpText = "The source of your data.")]
        public string SourceConnectionString { get; set; }

        [Option("sourceTableName", Required = false, HelpText = "The source collection or table")]
        public string SourceTable { get; set; }

        [Option("sourceDbName", Required = false, HelpText = "The source database in case of CosmosDB")]
        public string SourceDatabase { get; set; }

        [Option('t', "target", Required = false, HelpText = "The target of your data.")]
        public string TargetConnectionString { get; set; }

        [Option("targetTableName", Required = false, HelpText = "The target collection or table")]
        public string TargetTable { get; set; }

        [Option("targetDbName", Required = false, HelpText = "The target database in case of CosmosDB")]
        public string TargetDatabase { get; set; }

        [Option("sourcePartitionKey", Required = false, HelpText = "The source table or collection PartitionKey property", Default = "")]
        public string SourcePartitionKey { get; set; }

        [Option("targetPartitionKey", Required = false, HelpText = "The target table or collection PartitionKey property", Default = "")]
        public string TargetPartitionKey { get; set; }

        public DataSourceType SourceType
        {
            get
            {
                return DataSourceFinder.Find(SourceConnectionString, SourcePartitionKey);
            }
        }

        public bool IsSourcePartitioned
        {
            get
            {
                return SourceType == DataSourceType.AzureStorageTablePartitioned || SourceType == DataSourceType.CosmosDBPartitioned;
            }
        }

        public DataSourceType TargetType
        {
            get
            {
                return DataSourceFinder.Find(TargetConnectionString, TargetPartitionKey);
            }
        }

        public bool IsTargetPartitioned
        {
            get
            {
                return TargetType == DataSourceType.AzureStorageTablePartitioned || TargetType == DataSourceType.CosmosDBPartitioned;
            }
        }
    }
}
