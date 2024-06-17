using Microsoft.AspNetCore.Mvc;

namespace AzureStorageEmulator.NET.Table.Services
{
    public interface ITableStorageService
    {
        IActionResult ListTables(HttpContext context);
        IActionResult CreateTable(string tableName, HttpContext context);
        IActionResult DeleteTable(string tableName, HttpContext context);
    }
}