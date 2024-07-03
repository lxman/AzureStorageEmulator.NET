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
            PersistenceSettings settings = new()
            {
                Table = patchSettings.Table,
                Queue = patchSettings.Queue,
                Blob = patchSettings.Blob
            };
            switch (patchSettings.Action)
            {
                case Action.Backup:
                    await Persistence.Persist(settings, queueService, tableService, blobService);
                    break;
                case Action.Clear:
                    Persistence.Delete(settings, queueService, tableService, blobService);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(patchSettings.Action)} = {patchSettings.Action}");
            }
            return Ok(new Status());
        }
    }
}
