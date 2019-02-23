using System;
using System.Collections.Generic;
using System.Text;
using AzMigrate.Model;
using Microsoft.Azure.Documents.Client;

namespace AzMigrate.CosmosDB
{
    public class CosmosPartitionedCollection<TValue> : Repository<PartitionedItemKey, TValue>
        where TValue : SingleKeyEntity
    {
        private readonly string _partitionKeyPropertyName;

        public CosmosPartitionedCollection(string connectionString, string database, string collection, string partitionKeyPropertyName)
            : base(connectionString, database, collection)
        {
            if (string.IsNullOrEmpty(partitionKeyPropertyName))
            {
                throw new ArgumentNullException(nameof(partitionKeyPropertyName));
            }
            _partitionKeyPropertyName = partitionKeyPropertyName;
        }

        protected override PartitionedItemKey GetKeyFromKey(PartitionedItemKey key)
        {
            return key;
        }

        protected override string GetPartitionKey()
        {
            return _partitionKeyPropertyName;
        }
    }
}
