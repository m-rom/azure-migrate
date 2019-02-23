using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace AzMigrate.CosmosDB
{
    internal class CosmosCollectionHelper
    {
        protected const int DEFAULT_RUS = 400;

        public static void CreateCollection(IDocumentClient client, string databaseId, string collectionId, string partitionKeyName = "")
        {
            var result = client.CreateDatabaseIfNotExistsAsync(new Database
            {
                Id = databaseId
            }).GetAwaiter().GetResult();

            if (result.StatusCode == System.Net.HttpStatusCode.Accepted
                || result.StatusCode == System.Net.HttpStatusCode.OK
                || result.StatusCode == System.Net.HttpStatusCode.Created)
            {
                var collection = new DocumentCollection
                {
                    Id = collectionId
                };

                if (!string.IsNullOrEmpty(partitionKeyName))
                {
                    if (!partitionKeyName.StartsWith("/"))
                    {
                        partitionKeyName = "/" + partitionKeyName;
                    }
                    collection.PartitionKey = new PartitionKeyDefinition
                    {
                        Paths = new Collection<string> { partitionKeyName }
                    };
                }
                var collectionCreationResult = client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseId), collection, new RequestOptions
                {
                    OfferThroughput = DEFAULT_RUS
                }).GetAwaiter().GetResult();

                if (collectionCreationResult.StatusCode == System.Net.HttpStatusCode.Accepted
                   || collectionCreationResult.StatusCode == System.Net.HttpStatusCode.OK
                   || collectionCreationResult.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    Task.Delay(500).GetAwaiter().GetResult();
                }
                else
                {
                    throw new NotSupportedException($"Collection '{collection}' cannot be created. ({collectionCreationResult.StatusCode})");
                }
            }
            else
            {
                throw new NotSupportedException($"Database '{databaseId}' cannot be created. Wrong Url or Key? ({result.StatusCode})");
            }
        }

    }
}
