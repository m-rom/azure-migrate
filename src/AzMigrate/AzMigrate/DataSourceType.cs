using System;
using System.Collections.Generic;
using System.Text;

namespace AzMigrate
{
    enum DataSourceType
    {
        AzureStorageTable,
        AzureStorageTablePartitioned,
        CosmosDB,
        CosmosDBPartitioned
    }
}
