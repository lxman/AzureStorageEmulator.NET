using AzureStorageEmulator.NET.Blob;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AzureStorageEmulator.NET.Controllers
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
        /// <param name="restype"></param>
        /// <param name="include"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ListQueues([FromQuery] string comp, [FromQuery] string? restype = null, [FromQuery] string? include = null, [FromQuery] int? timeout = null)
        {
            Log.Information($"ListBlobs comp = {comp}, restype = {restype}, include = {include}, timeout = {timeout}");
            if (comp != "list" && comp != "properties") return new StatusCodeResult(400);
            switch (comp)
            {
                case "properties" when restype == "account":
                    return Ok();

                case "list" when include == "metadata" && timeout is not null:
                    return Ok();

                default:
                    {
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

        [HttpGet]
        [Route("$logs")]
        public IActionResult GetBlob([FromQuery] string? restype)
        {
            if (restype == "container") return NotFound();
            return Ok();
        }

        [HttpGet]
        [Route("$blobchangefeed")]
        public IActionResult GetBlobChangeFeed([FromQuery] string? restype)
        {
            if (restype == "container") return NotFound();
            return Ok();
        }
    }
}