using System.Text.Json;
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
        // TODO: Implement a mechanism to return 504 Gateway Timeout if processing takes too long

        #region TableOps

        /// <summary>
        /// List all tables in the emulator
        /// </summary>
        /// <returns>json tagged with odata metadata</returns>
        [HttpGet]
        [Route("tables")]
        public IActionResult ListTables()
        {
            return tableService.ListTables(HttpContext);
        }

        /// <summary>
        /// Create a table in the emulator
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>json tagged with odata metadata</returns>
        [HttpPost]
        [Route("tables")]
        public IActionResult CreateTable([FromBody] TableNameJson tableName)
        {
            return tableService.CreateTable(tableName.TableName, HttpContext);
        }

        /// <summary>
        /// Delete a table in the emulator
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>200</returns>
        [HttpDelete("tables({tableName})")]
        public IActionResult DeleteTable(string tableName)
        {
            return tableService.DeleteTable(tableName, HttpContext);
        }

        #endregion TableOps

        #region EntityOps

        /// <summary>
        /// Insert an entity into a table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="data"></param>
        /// <returns>json tagged with odata metadata</returns>
        [HttpPost("{tableName}")]
        public IActionResult Insert(string tableName, [FromBody] object data)
        {
            return tableService.Insert(tableName, (JsonElement)data, HttpContext);
        }

        /// <summary>
        /// Query a table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>json tagged with odata metadata</returns>
        [HttpGet("{tableName}()")]
        public Task<MemoryStream> QueryTable(string tableName)
        {
            return tableService.QueryTable(tableName, HttpContext);
        }

        #endregion EntityOps
    }
}