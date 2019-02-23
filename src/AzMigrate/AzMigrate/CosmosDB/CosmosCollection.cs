using System;
using System.Collections.Generic;
using System.Text;
using AzMigrate.Model;
using Microsoft.Azure.Documents.Client;

namespace AzMigrate.CosmosDB
{
    public class CosmosCollection<TValue> : Repository<string, TValue>
        where TValue : SingleKeyEntity
    {
        public CosmosCollection(string connectionString, string database, string collection)
            : base(connectionString, database, collection)
        {

        }

        protected override PartitionedItemKey GetKeyFromKey(string key)
        {
            return new PartitionedItemKey { Id = key };
        }

        protected override string GetPartitionKey()
        {
            return null;
        }
    }
}
