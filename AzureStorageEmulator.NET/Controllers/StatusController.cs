using AzureStorageEmulator.NET.Authorization;
using AzureStorageEmulator.NET.Blob.Services;
using AzureStorageEmulator.NET.Common;
using AzureStorageEmulator.NET.Queue.Services;
using AzureStorageEmulator.NET.Table.Services;
using Microsoft.AspNetCore.Mvc;
using Action = AzureStorageEmulator.NET.Common.Action;

namespace AzureStorageEmulator.NET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Host("*:10010")]
    public class StatusController(
        IQueueService queueService,
        ITableService tableService,
        IBlobService blobService) : ControllerBase
    {
        [HttpGet]
        [NoAuth]
        public IActionResult Get()
        {
            return Ok(new Status());
        }

        [HttpPatch("snapshot")]
        [NoAuth]
        public async Task<IActionResult> Patch([FromBody] PatchCommandSettings patchSettings)
        {
            switch (patchSettings.Action)
            {
                case Action.Backup:
                    PersistenceSettings persistenceSettings = new()
                    {
                        Table = patchSettings.Table,
                        Queue = patchSettings.Queue,
                        Blob = patchSettings.Blob
                    };
                    await Persistence.Persist(persistenceSettings, queueService, tableService, blobService);
                    break;

                case Action.Clear:
                    PersistenceSettings clearSettings = new()
                    {
                        ClearTable = patchSettings.Table,
                        ClearQueue = patchSettings.Queue,
                        ClearBlob = patchSettings.Blob
                    };
                    Persistence.Delete(clearSettings, queueService, tableService, blobService);
                    break;

                case Action.None:
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(patchSettings.Action)} = {patchSettings.Action}");
            }
            return Ok(new Status());
        }
    }
}