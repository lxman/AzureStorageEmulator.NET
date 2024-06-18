using System.Text.Json;
using AzureStorageEmulator.NET.Common.HeaderManagement;
using AzureStorageEmulator.NET.Table.Models;
using Microsoft.AspNetCore.Mvc;
using TableStorage;

namespace AzureStorageEmulator.NET.Table.Services
{
    public class TableService(
        ITableStorage storage,
        IHeaderManagement headerManagement) : ITableStorageService
    {
        public IActionResult ListTables(HttpContext context)
        {
            List<string> tables = storage.ListTables();
            ListTablesResponse response = new() { Value = [] };
            tables.ForEach(table => response.Value.Add(new TableName { Name = table }));
            response.Metadata = $"{context.Request.Scheme}://{context.Request.Host}/{context.Request.Path.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries)[0]}/$metadata#Tables";
            headerManagement.SetResponseHeaders(context);
            return new OkObjectResult(JsonSerializer.Serialize(response));
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
            headerManagement.SetResponseHeaders(context);
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(response),
                StatusCode = 201,
                ContentType = "application/json;odata=minimalmetadata"
            };
        }

        public IActionResult DeleteTable(string tableName, HttpContext context)
        {
            headerManagement.SetResponseHeaders(context);
            return storage.DeleteTable(tableName) ? new OkResult() : new NotFoundResult();
        }
    }
}