using AzureStorageEmulator.NET.Table.Models;
using AzureStorageEmulator.NET.Table.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureStorageEmulator.NET.Controllers
{
    [Route("devstoreaccount1")]
    [ApiController]
    [Host("*:10002")]
    public class TableController(ITableService tableService) : ControllerBase
    {
        [HttpGet]
        [Route("tables")]
        public IActionResult ListTables()
        {
            return tableService.ListTables(HttpContext);
        }

        [HttpPost]
        [Route("tables")]
        public IActionResult CreateTable([FromBody] TableNameJson tableName)
        {
            return tableService.CreateTable(tableName.TableName, HttpContext);
        }

        [HttpDelete("tables({tableName})")]
        public IActionResult DeleteTable(string tableName)
        {
            return tableService.DeleteTable(tableName, HttpContext);
        }

        [HttpPost("{tableName}")]
        public IActionResult GetTable(string tableName, [FromBody] object data)
        {
            return tableService.Insert(tableName, data, HttpContext);
        }

        [HttpGet("{tablename}()")]
        public Task<MemoryStream> QueryTable(string tableName)
        {
            return tableService.QueryTable(tableName, HttpContext);
        }
    }
}