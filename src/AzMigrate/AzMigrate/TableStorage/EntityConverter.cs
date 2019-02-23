using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzMigrate.TableStorage
{
    class EntityConverter
    {
        public static TOutput ConvertTo<TOutput>(DynamicTableEntity entity)
        {
            var result = ConvertTo<TOutput>(entity.Properties);
            return result;
        }

        internal static TOutput ConvertToObject<TOutput>(DynamicTableEntity entity)
        {
            var result = ConvertTo<TOutput>(entity.Properties);
            return result;
        }

        public static TOutput ConvertTo<TOutput>(ITableEntity entity)
            where TOutput : class
        {
            if (entity is DynamicTableEntity dEntity)
            {
                var result = ConvertTo<TOutput>(dEntity.Properties);
                return result;
            }
            return null;
        }

        public static DynamicTableEntity ConvertTo<TInput>(TInput entity, string partitionKey, string rowKey)
            where TInput : class
        {
            var dynamicTableEntity = new DynamicTableEntity
            {
                RowKey = rowKey,
                PartitionKey = partitionKey,
                Properties = new Dictionary<string, EntityProperty>()
            };

            var jObject = JObject.FromObject(entity);

            foreach (var pair in jObject.Values<JProperty>().Select(WriteToEntityProperty).Where(pair => pair.HasValue))
            {
                dynamicTableEntity.Properties.Add(pair.Value);
            }

            return dynamicTableEntity;
        }

        private static TOutput ConvertTo<TOutput>(IDictionary<string, EntityProperty> properties)
        {
            var jobject = new JObject();
            foreach (var property in properties)
            {
                WriteToJObject(jobject, property);
            }
            return jobject.ToObject<TOutput>();
        }

        private static void WriteToJObject(JObject jObject, KeyValuePair<string, EntityProperty> property)
        {
            switch (property.Value.PropertyType)
            {
                case EdmType.Binary:
                    jObject.Add(property.Key, new JValue(property.Value.BinaryValue));
                    return;
                case EdmType.Boolean:
                    jObject.Add(property.Key, new JValue(property.Value.BooleanValue));
                    return;
                case EdmType.DateTime:
                    jObject.Add(property.Key, new JValue(property.Value.DateTime));
                    return;
                case EdmType.Double:
                    jObject.Add(property.Key, new JValue(property.Value.DoubleValue));
                    return;
                case EdmType.Guid:
                    jObject.Add(property.Key, new JValue(property.Value.GuidValue));
                    return;
                case EdmType.Int32:
                    jObject.Add(property.Key, new JValue(property.Value.Int32Value));
                    return;
                case EdmType.Int64:
                    jObject.Add(property.Key, new JValue(property.Value.Int64Value));
                    return;
                case EdmType.String:
                    jObject.Add(property.Key, new JValue(property.Value.StringValue));
                    return;
                default:
                    return;
            }
        }

        private static KeyValuePair<string, EntityProperty>? WriteToEntityProperty(JProperty property)
        {
            var value = property.Value;
            var name = property.Name;

            switch (value.Type)
            {
                case JTokenType.Bytes:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<byte[]>()));
                case JTokenType.Boolean:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<bool>()));
                case JTokenType.Date:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<DateTime>()));
                case JTokenType.Float:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<double>()));
                case JTokenType.Guid:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<Guid>()));
                case JTokenType.Integer:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<Int64>()));
                case JTokenType.String:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<string>()));
                default:
                    return null;
            }
        }
    }
}
