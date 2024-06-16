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
        public ActionResult<List<string>> ListTables()
        {
            return tableStorageService.ListTables();
        }

        [HttpPost]
        [Route("tables")]
        public ActionResult CreateTable([FromBody] TableNameJson tableName)
        {
            tableStorageService.CreateTable(tableName.TableName);
            return Ok(new TableNameJson { TableName = tableName.TableName });
        }

        [HttpDelete("tables({tableName})")]
        public ActionResult DeleteTable(string tableName)
        {
            if (tableStorageService.DeleteTable(tableName))
            {
                return Ok();
            }

            return NotFound();
        }
    }
}