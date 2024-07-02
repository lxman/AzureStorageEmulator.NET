using System.Text;
using System.Text.Json;
using AzureStorageEmulator.NET.Common;
using AzureStorageEmulator.NET.JsonSerialization;
using AzureStorageEmulator.NET.Table.Models;
using DynamicODataToSQL;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SqlKata.Compilers;
using TableStorage;
using BsonDocument = LiteDB.BsonDocument;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AzureStorageEmulator.NET.Table.Services
{
    public interface ITableService : IStorageProvider
    {
        IActionResult QueryTables(HttpContext context);

        IActionResult CreateTable(string tableName, HttpContext context);

        IActionResult DeleteTable(string tableName, HttpContext context);

        IActionResult InsertEntity(string tableName, JsonElement document, HttpContext context);

        Task<MemoryStream> QueryEntities(string tableName, HttpContext context);
    }

    public class TableService(
        ITableStorage storage) : ITableService
    {
        private static JsonSerializerOptions options = new() { Converters = { new DictionaryStringObjectJsonConverter() } };

        public IActionResult QueryTables(HttpContext context)
        {
            List<string> tables = storage.QueryTables();
            ListTablesResponse response = new() { Value = [] };
            tables.ForEach(table => response.Value.Add(new TableName { Name = table }));
            response.Metadata = $"{context.Request.Scheme}://{context.Request.Host}/{context.Request.Path.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries)[0]}/$metadata#Tables";
            context.Response.Headers.ContentType = "application/json;odata=minimalmetadata";
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(response),
                StatusCode = 200,
                ContentType = "application/json;odata=minimalmetadata"
            };
        }

        public IActionResult CreateTable(string tableName, HttpContext context)
        {
            storage.CreateTable(tableName);
            CreateTableResponse response = new()
            {
                TableName = tableName,
                Metadata =
                    $"{context.Request.Scheme}://{context.Request.Host}/{context.Request.Path.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries)[0]}/$metadata#Tables/@Element"
            };
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(response),
                StatusCode = 201,
                ContentType = "application/json;odata=minimalmetadata"
            };
        }

        public IActionResult DeleteTable(string tableName, HttpContext context)
        {
            context.Response.Headers.ContentType = "application/json;odata=minimalmetadata";
            return storage.DeleteTable(tableName) ? new OkResult() : new NotFoundResult();
        }

        public IActionResult InsertEntity(string tableName, JsonElement document, HttpContext context)
        {
            BsonDocument bsonDocument = [];
            foreach (JsonProperty property in document.EnumerateObject())
            {
                if (property.Name is "PartitionKey" or "RowKey")
                {
                    bsonDocument[property.Name] = property.Value.ToString();
                    continue;
                }

                if (property.Name.EndsWith("@odata.type")) continue;
                string propName = property.Name;
                JsonElement propType = document.GetProperty($"{propName}@odata.type");
                bsonDocument[propName] = propType.ToString() switch
                {
                    "Edm.Int32" => new BsonValue(Convert.ToInt32(property.Value.ToString())),
                    "Edm.Int64" => new BsonValue(Convert.ToInt64(property.Value.ToString())),
                    "Edm.Double" => new BsonValue(Convert.ToDouble(property.Value.ToString())),
                    "Edm.Boolean" => new BsonValue(Convert.ToBoolean(property.Value.ToString())),
                    "Edm.DateTime" => new BsonValue(Convert.ToDateTime(property.Value.ToString())),
                    "Edm.String" => new BsonValue(property.Value.GetString()),
                    _ => new BsonValue(property.Value.GetString())
                };
            }
            bsonDocument["Timestamp"] = new BsonValue(DateTime.UtcNow);
            _ = storage.InsertEntity(tableName, bsonDocument);
            context.Response.Headers.ContentType = "application/json;odata=minimalmetadata";
            return new NoContentResult();
        }

        public async Task<MemoryStream> QueryEntities(string tableName, HttpContext context)
        {
            ListEntriesResponse response = new()
            {
                Metadata = $"{context.Request.Scheme}://{context.Request.Host}/{context.Request.Path.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries)[0]}/$metadata#Tables/@Element",
                Objects = []
            };
            List<BsonDocument> newList = [];
            if (context.Request.QueryString.HasValue && context.Request.Query.Count > 0)
            {
                Dictionary<string, string> queries = context.Request.Query.Count > 0 ? ParseQuery(context.Request.Query) : [];
                ODataToSqlConverter converter = new(new EdmModelBuilder(), new SqlServerCompiler());
                (string actualQuery, IDictionary<string, object> parameters) = converter.ConvertToSQL(tableName, queries);
                IAsyncEnumerable<BsonDocument> result = storage.QueryFromQueryString(tableName, actualQuery, parameters);
                await foreach (BsonDocument item in result)
                {
                    newList.Add(item);
                }
            }
            else
            {
                List<BsonDocument> results = storage.GetAll(tableName).ToList();
                results.ForEach(r =>
                {
                    if (r.ContainsKey("_id")) r.Remove("_id");
                });
                results.ForEach(r =>
                {
                    if (r.ContainsKey("Timestamp")) newList.Add(r);
                });
            }
            newList.ForEach(item =>
            {
                Dictionary<string, object> dictionary = [];
                response.Objects.Add(dictionary);
                item.Keys.ToList().ForEach(k =>
                {
                    switch (k)
                    {
                        case "PartitionKey" or "RowKey":
                            dictionary.Add(k, item[k].AsString);
                            return;
                        case "Timestamp":
                            {
                                string timeString = item[k].AsDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
                                dictionary.Add("odata.etag", $"W/\"datetime'{timeString}'\"");
                                dictionary.Add(k, timeString);
                                return;
                            }
                        default:
                            switch (item[k].Type)
                            {
                                case BsonType.Int32:
                                    dictionary.Add(k, item[k].AsInt32);
                                    break;
                                case BsonType.Int64:
                                    dictionary.Add(k, item[k].AsInt64);
                                    break;
                                case BsonType.Double:
                                    dictionary.Add(k, item[k].AsDouble);
                                    break;
                                case BsonType.Decimal:
                                    dictionary.Add(k, item[k].AsDecimal);
                                    break;
                                case BsonType.String:
                                    dictionary.Add(k, item[k].AsString);
                                    break;
                                case BsonType.Binary:
                                    dictionary.Add(k, item[k].AsBinary);
                                    break;
                                case BsonType.Guid:
                                    dictionary.Add(k, item[k].AsGuid);
                                    break;
                                case BsonType.Boolean:
                                    dictionary.Add(k, item[k].AsBoolean);
                                    break;
                                case BsonType.DateTime:
                                    dictionary.Add(k, item[k].AsDateTime);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException($"{k}: {item[k].Type}");
                            }
                            break;
                    }
                });
            });
            context.Response.Headers.ContentType = "application/json;odata=minimalmetadata";
            context.Response.Headers.Connection = "keep-alive";
            context.Response.Headers.KeepAlive = "timeout=5";
            MemoryStream ms = new(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response, options)));
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public async Task Persist(string location)
        {
            await storage.Persist(location);
        }

        public async Task Restore(string location)
        {
            await storage.Restore(location);
        }

        private static Dictionary<string, string> ParseQuery(IQueryCollection query)
        {
            Dictionary<string, string> filters = [];
            foreach (KeyValuePair<string, StringValues> keyValuePair in query)
            {
                filters.Add(keyValuePair.Key[1..], keyValuePair.Value.ToString().Replace("datetime", string.Empty));
            }

            return filters;
        }
    }
}