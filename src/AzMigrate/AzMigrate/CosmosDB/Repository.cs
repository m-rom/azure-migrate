using AzMigrate.Model;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzMigrate.CosmosDB
{
    public abstract class Repository<TKey, TValue> : IRepository<TKey, TValue>
        where TValue : SingleKeyEntity
    {
        protected readonly string _databaseId;
        protected readonly string _collectionId;

        protected Lazy<DocumentClient> _client;

        public Repository(string connectionString, string database, string collection)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            var csb = new CosmosConnectionStringBuilder(connectionString);
            if (csb == null)
            {
                throw new ArgumentNullException(nameof(csb));
            }
            if (csb.Endpoint == null)
            {
                throw new ArgumentNullException(nameof(csb.Endpoint));
            }
            if (string.IsNullOrEmpty(csb.Key))
            {
                throw new ArgumentNullException(nameof(csb.Key));
            }
            if (string.IsNullOrEmpty(database))
            {
                throw new ArgumentNullException(nameof(database));
            }
            if (string.IsNullOrEmpty(collection))
            {
                throw new ArgumentNullException(nameof(collection));
            }

            _databaseId = database;
            _collectionId = collection;

            _client = new Lazy<DocumentClient>(() =>
            {
                var client = new DocumentClient(csb.Endpoint, csb.Key);
                CosmosCollectionHelper.CreateCollection(client, _databaseId, _collectionId, GetPartitionKey());
                return client;
            });
        }

        public virtual Task DeleteAsync(TKey key)
        {
            var reqOptions = GetRequestOptions();
            var id = GetKeyFromKey(key).Id;
            return _client.Value.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _collectionId, id), reqOptions);
        }

        public virtual async Task<TValue> GetAsync(TKey key)
        {
            try
            {
                var reqOptions = GetRequestOptions();
                var id = GetKeyFromKey(key).Id;
                var result = await _client.Value.ReadDocumentAsync<TValue>(UriFactory.CreateDocumentUri(_databaseId, _collectionId, id), reqOptions);
                return result;
            }
            catch (DocumentClientException dex)
            {
                if (dex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public virtual async Task<TValue> UpsertAsync(TValue entity)
        {
            try
            {
                var reqOptions = GetRequestOptions(entity);
                var result = await _client.Value.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId), entity, reqOptions);
                return entity;
            }
            catch (DocumentClientException dex)
            {
                if (dex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public virtual async Task<IEnumerable<TValue>> GetAllAsync()
        {
            var query = CreateQuery().Where(i => i.Id != "");
            return query.ToList();
        }

        public virtual IQueryable<TValue> CreateQuery()
        {
            var feedOptions = GetFeedOptions();
            return _client.Value.CreateDocumentQuery<TValue>(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId), feedOptions);
        }

        public virtual IEnumerable<T> ExecuteQuery<T>(string sql)
        {
            var feedOptions = GetFeedOptions();
            var query = _client.Value.CreateDocumentQuery<T>(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId), new SqlQuerySpec(sql), feedOptions);
            return query.ToList();
        }

        protected virtual RequestOptions GetRequestOptions(TValue entity = null)
        {
            var key = GetKeyFromEntity(entity);
            if (string.IsNullOrEmpty(key.PartitionKey))
            {
                return null;
            }
            return new RequestOptions
            {
                PartitionKey = new PartitionKey(key.PartitionKey)
            };
        }

        protected virtual FeedOptions GetFeedOptions(TValue entity = null)
        {
            var key = GetKeyFromEntity(entity);
            if (string.IsNullOrEmpty(key.PartitionKey))
            {
                return null;
            }
            return new FeedOptions
            {
                PartitionKey = new PartitionKey(key.PartitionKey),
                EnableCrossPartitionQuery = false,
                MaxItemCount = -1
            };
        }

        protected abstract string GetPartitionKey();

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
            var id = GetPropertyValue(value, "Id");
            var partitionKeyValue = GetPropertyValue(value, GetPartitionKey());

            var ky = new PartitionedItemKey
            {
                PartitionKey = partitionKeyValue,
                Id = id
            };
            return ky;
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (_client != null)
                {
                    _client.Value.Dispose();
                    _client = null;
                }
            }
        }
    }
}
