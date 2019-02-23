using AzMigrate.Model;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzMigrate.TableStorage
{
    public class PartitionedTable<TValue> : Repository<PartitionedItemKey, TValue>
        where TValue : SingleKeyEntity
    {
        private readonly string _customPartitionKeyColumn = "PartitionKey";

        public PartitionedTable(string connectionString, string table, string customPartitionKeyColumn = "PartitionKey")
            : base(connectionString, table)
        {
            _customPartitionKeyColumn = customPartitionKeyColumn;
        }

        public override IQueryable<TValue> CreateQuery()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<T> ExecuteQuery<T>(string query)
        {
            throw new NotImplementedException();
        }

        protected override PartitionedItemKey GetKeyFromKey(PartitionedItemKey key)
        {
            return key;
        }

        protected override string GetPartitionKey()
        {
            return _customPartitionKeyColumn;
        }

        protected override void ValidateKey(PartitionedItemKey key)
        {
            base.ValidateKey(key);
            if (string.IsNullOrEmpty(key.PartitionKey))
            {
                throw new ArgumentNullException(nameof(key.PartitionKey));
            }
            if (string.IsNullOrEmpty(key.Id))
            {
                throw new ArgumentNullException(nameof(key.Id));
            }
        }
    }
}
