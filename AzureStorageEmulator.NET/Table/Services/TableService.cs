using System.Text;
using System.Text.Json;
using AzureStorageEmulator.NET.Table.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
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

        MemoryStream QueryTable(string tableName, HttpContext context);
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

        public MemoryStream QueryTable(string tableName, HttpContext context)
        {
            List<BsonDocument> results = storage.GetAll(tableName).ToList();
            ListEntriesResponse response = new()
            {
                Metadata = $"{context.Request.Scheme}://{context.Request.Host}/{context.Request.Path.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries)[0]}/$metadata#Tables/@Element",
                Objects = []
            };
            results.ForEach(r =>
            {
                if (r.ContainsKey("_id")) r.Remove("_id");
            });
            List<BsonDocument> newList = [];
            results.ForEach(r =>
            {
                if (r.ContainsKey("Timestamp")) newList.Add(r);
            });
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
    }
}