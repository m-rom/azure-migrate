using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace AzMigrate.CosmosDB
{
    class CosmosConnectionStringBuilder
    {
        public CosmosConnectionStringBuilder(string connectionString)
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            if (builder.TryGetValue("AccountKey", out object key))
            {
                Key = $"{key}";
            }

            if (builder.TryGetValue("AccountEndpoint", out object uri))
            {
                Endpoint = new Uri($"{uri}");
            }
        }

        public Uri Endpoint { get; set; }

        public string Key { get; set; }
    }
}
