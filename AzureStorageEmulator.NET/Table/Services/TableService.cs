using System.Text;
using System.Text.Json;
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
    public interface ITableService
    {
        IActionResult ListTables(HttpContext context);

        IActionResult CreateTable(string tableName, HttpContext context);

        IActionResult DeleteTable(string tableName, HttpContext context);

        IActionResult Insert(string tableName, object document, HttpContext context);

        Task<MemoryStream> QueryTable(string tableName, HttpContext context);
    }

    public class TableService(
        ITableStorage storage) : ITableService
    {
        public IActionResult ListTables(HttpContext context)
        {
            List<string> tables = storage.ListTables();
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

        public IActionResult Insert(string tableName, object document, HttpContext context)
        {
            JsonElement data = (JsonElement)document;
            BsonDocument bsonDocument = [];
            foreach (JsonProperty property in data.EnumerateObject())
            {
                bsonDocument[property.Name] = property.Value.ToString();
            }
            bsonDocument["Timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:MM:ss.fffffffZ");
            _ = storage.Insert(tableName, bsonDocument);
            context.Response.Headers.ContentType = "application/json;odata=minimalmetadata";
            return new NoContentResult();
        }

        public async Task<MemoryStream> QueryTable(string tableName, HttpContext context)
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
                response.Objects.Add(BsonDocToDictionary(item));
            });
            context.Response.Headers.ContentType = "application/json;odata=minimalmetadata";
            context.Response.Headers.Connection = "keep-alive";
            context.Response.Headers.KeepAlive = "timeout=5";
            MemoryStream ms = new(Encoding.UTF8.GetBytes($"{JsonSerializer.Serialize(response)}"));
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        private static Dictionary<string, string> BsonDocToDictionary(BsonDocument doc)
        {
            Dictionary<string, string> dict = [];
            foreach (KeyValuePair<string, BsonValue> keyValuePair in doc)
            {
                dict.Add(keyValuePair.Key, keyValuePair.Value.AsString);
            }

            return dict;
        }

        private static Dictionary<string, string> ParseQuery(IQueryCollection query)
        {
            Dictionary<string, string> filters = [];
            foreach (KeyValuePair<string, StringValues> keyValuePair in query)
            {
                filters.Add(keyValuePair.Key[1..], keyValuePair.Value.ToString());
            }

            return filters;
        }
    }
}