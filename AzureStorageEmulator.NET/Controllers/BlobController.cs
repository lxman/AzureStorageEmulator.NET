using AzureStorageEmulator.NET.Blob.Attributes;
using AzureStorageEmulator.NET.Blob.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureStorageEmulator.NET.Controllers
{
    [Route("devstoreaccount1")]
    [ApiController]
    [Host("*:10000")]
    public class BlobController(IBlobService blobService) : ControllerBase
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

        [HttpGet("{containerName}")]
        public Task<IActionResult> PingContainer(string containerName)
        {
            return blobService.PingContainer(containerName, HttpContext);
        }

        [QueryParameterConstraint("comp")]
        [QueryParameterConstraint("maxresults")]
        [QueryParameterConstraint("restype")]
        [QueryParameterConstraint("include")]
        [QueryParameterConstraint("delimiter")]
        [HttpGet("{containerName}")]
        public Task<MemoryStream> ListContainerContents(string containerName)
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