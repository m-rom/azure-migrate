using AzMigrate.Model;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AzMigrate.TableStorage
{
    public class HashedTable<TValue> : Repository<string, TValue>
        where TValue : SingleKeyEntity
    {
        private const int MAX_PARTITIONS = 64;
        private readonly SHA512Managed _shaManager;

        public HashedTable(string connectionString, string tableName)
            : base(connectionString, tableName)
        {
            _shaManager = new SHA512Managed();
        }

        public override IQueryable<TValue> CreateQuery()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<T> ExecuteQuery<T>(string query)
        {
            throw new NotImplementedException();
        }

        protected override PartitionedItemKey GetKeyFromEntity(TValue value)
        {
            if (string.IsNullOrEmpty(value.Id))
            {
                throw new ArgumentNullException(nameof(value.Id));
            }
            var ky = GetKeyFromKey(value.Id);
            return ky;
        }

        protected override PartitionedItemKey GetKeyFromKey(string id)
        {
            var data = Encoding.UTF8.GetBytes(id);
            byte[] hashResult = _shaManager.ComputeHash(data);
            var hashNumber = BitConverter.ToInt64(hashResult, 0);
            var partitionKey = Math.Abs(hashNumber % MAX_PARTITIONS).ToString("D10");

            return new PartitionedItemKey
            {
                PartitionKey = partitionKey,
                Id = id
            };
        }
    }
}
