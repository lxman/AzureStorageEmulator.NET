using AzureStorageBlob.NET.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AzureStorageBlob.NET.Controllers
{
    [Route("devstoreaccount1")]
    [ApiController]
    [Host("*:10000")]
    public class BlobController(IBlobService blobService) : ControllerBase
    {
        /// <summary>
        /// List the queues in the storage account.
        /// </summary>
        /// <param name="comp"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ListQueues([FromQuery] string comp)
        {
            Log.Information($"ListBlobs comp = {comp}");
            if (comp != "list") return new StatusCodeResult(400);
            string result = blobService.GetBlobs();
            return new ContentResult
            {
                Content = result,
                ContentType = "application/xml",
                StatusCode = 200
            };
        }
    }
}