using AzureStorageEmulator.NET.Blob.Attributes;
using AzureStorageEmulator.NET.Blob.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureStorageEmulator.NET.Controllers
{
    [Route("devstoreaccount1")]
    [ApiController]
    [Host("*:10000")]
    public class BlobController(IBlobService blobService, ILogger<BlobController> logger) : ControllerBase
    {
        [HttpGet]
        public IActionResult GetInfo()
        {
            return blobService.GetInfo(HttpContext);
        }

        [HttpPut("{containerName}")]
        public IActionResult CreateContainer(string containerName)
        {
            return blobService.CreateContainer(containerName, HttpContext);
        }

        [QueryStringConstraint("restype", true)]
        [QueryStringConstraint("maxresults", false)]
        [QueryStringConstraint("include", false)]
        [QueryStringConstraint("delimiter", false)]
        [HttpGet("{containerName}")]
        public Task<IActionResult> PingContainer(
            string containerName,
            [FromQuery] string comp,
            [FromQuery] string restype)
        {
            return blobService.PingContainer(containerName, HttpContext);
        }

        [QueryStringConstraint("comp", true)]
        [QueryStringConstraint("maxresults", true)]
        [QueryStringConstraint("restype", true)]
        [QueryStringConstraint("include", true)]
        [QueryStringConstraint("delimiter", true)]
        [HttpGet("{containerName}")]
        public Task<MemoryStream> ListContainerContents(
            string containerName,
            [FromQuery] string comp,
            [FromQuery] int maxresults,
            [FromQuery] string restype,
            [FromQuery] string include,
            [FromQuery] char delimiter)
        {
            return blobService.ListContainerContents(containerName, HttpContext);
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