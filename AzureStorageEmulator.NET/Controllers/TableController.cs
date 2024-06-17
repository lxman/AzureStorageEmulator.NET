using AzureStorageEmulator.NET.Table.Models;
using AzureStorageEmulator.NET.Table.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureStorageEmulator.NET.Controllers
{
    [Route("devstoreaccount1")]
    [ApiController]
    [Host("*:10002")]
    public class TableController(ITableStorageService tableStorageService) : ControllerBase
    {
        [HttpGet]
        [Route("tables")]
        public IActionResult ListTables()
        {
            return tableStorageService.ListTables(HttpContext);
        }

        [HttpPost]
        [Route("tables")]
        public IActionResult CreateTable([FromBody] TableNameJson tableName)
        {
            return tableStorageService.CreateTable(tableName.TableName, HttpContext);
        }

        [HttpDelete("tables({tableName})")]
        public IActionResult DeleteTable(string tableName)
        {
            return tableStorageService.DeleteTable(tableName, HttpContext);
        }
    }
}