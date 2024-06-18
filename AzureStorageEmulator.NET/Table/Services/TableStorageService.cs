using System.Text.Json;
using AzureStorageEmulator.NET.Authorization;
using AzureStorageEmulator.NET.Authorization.Table;
using AzureStorageEmulator.NET.Common.HeaderManagement;
using AzureStorageEmulator.NET.Table.Models;
using Microsoft.AspNetCore.Mvc;
using TableStorage;

namespace AzureStorageEmulator.NET.Table.Services
{
    public class TableStorageService(
        ITableStorage storage,
        IHeaderManagement headerManagement,
        IAuthorizer<TableSharedKeyLiteAuthorizer> authorizer) : ITableStorageService
    {
        public IActionResult ListTables(HttpContext context)
        {
            if (!Authenticate(context.Request)) return new StatusCodeResult(403);
            List<string> tables = storage.ListTables();
            ListTablesResponse response = new() { Value = [] };
            tables.ForEach(table => response.Value.Add(new TableName { Name = table }));
            response.Metadata = $"{context.Request.Scheme}://{context.Request.Host}/{context.Request.Path.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries)[0]}/$metadata#Tables";
            headerManagement.SetResponseHeaders(context);
            return new OkObjectResult(JsonSerializer.Serialize(response));
        }

        public IActionResult CreateTable(string tableName, HttpContext context)
        {
            if (!Authenticate(context.Request)) return new StatusCodeResult(403);
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
            if (!Authenticate(context.Request)) return new StatusCodeResult(403);
            headerManagement.SetResponseHeaders(context);
            return storage.DeleteTable(tableName) ? new OkResult() : new NotFoundResult();
        }

        private bool Authenticate(HttpRequest request) => authorizer.Authorize(request);
    }
}