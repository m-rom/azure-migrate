using System;
using System.Collections.Generic;
using System.Text;

namespace AzMigrate
{
    class DataSourceFinder
    {
        public static DataSourceType Find(string connectionString, string partitionKey = "")
        {
            if (connectionString.Contains("AccountEndpoint") && connectionString.Contains("AccountKey"))
            {
                if (!string.IsNullOrEmpty(partitionKey))
                {
                    return DataSourceType.CosmosDBPartitioned;
                }
                return DataSourceType.CosmosDB;
            }
            if ((connectionString.Contains("DefaultEndpointsProtocol") && connectionString.Contains("AccountName") || connectionString.Contains("UseDevelopmentStorage=true")))
            {
                if (!string.IsNullOrEmpty(partitionKey))
                {
                    return DataSourceType.AzureStorageTablePartitioned;
                }
                return DataSourceType.AzureStorageTable;
            }

            throw new NotSupportedException($"The provided data source type (connection string) is not supported yet");
        }
    }
}
