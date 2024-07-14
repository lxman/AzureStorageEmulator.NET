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
        // TODO: Implement a mechanism to return 504 Gateway Timeout if processing takes too long

        /// <summary>
        /// ???
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetInfo()
        {
            return blobService.GetInfo(HttpContext);
        }

        /// <summary>
        /// Create a container
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="restype"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [QueryStringConstraint("restype", true)]
        [QueryStringConstraint("maxresults", false)]
        [QueryStringConstraint("include", false)]
        [QueryStringConstraint("delimiter", false)]
        [QueryStringConstraint("comp", false)]
        [HttpPut("{containerName}")]
        public IActionResult CreateContainer(string containerName, [FromQuery] string restype, [FromQuery] string timeout = "0")
        {
            return blobService.CreateContainer(containerName, HttpContext);
        }

        /// <summary>
        /// Returns information about a container
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="restype"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [QueryStringConstraint("restype", true)]
        [QueryStringConstraint("maxresults", false)]
        [QueryStringConstraint("include", false)]
        [QueryStringConstraint("delimiter", false)]
        [QueryStringConstraint("comp", false)]
        [HttpGet("{containerName}")]
        public Task<IActionResult> PingContainer(string containerName, [FromQuery] string restype, [FromQuery] string timeout = "0")
        {
            return blobService.PingContainer(containerName, HttpContext);
        }

        /// <summary>
        /// List the contents of a container
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="comp"></param>
        /// <param name="maxresults"></param>
        /// <param name="restype"></param>
        /// <param name="include"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        [QueryStringConstraint("restype", true)]
        [QueryStringConstraint("maxresults", true)]
        [QueryStringConstraint("include", true)]
        [QueryStringConstraint("delimiter", true)]
        [QueryStringConstraint("comp", true)]
        [HttpGet("{containerName}")]
        public Task<MemoryStream> ListContainerContents(
            string containerName,
            [FromQuery] string restype,
            [FromQuery] string include,
            [FromQuery] string delimiter,
            [FromQuery] string comp = "list",
            [FromQuery] string maxresults = "5000")
        {
            return blobService.ListContainerContents(containerName, HttpContext);
        }

        /// <summary>
        /// ???
        /// </summary>
        /// <param name="restype"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("$logs")]
        public IActionResult GetBlob([FromQuery] string? restype)
        {
            if (restype == "container") return NotFound();
            return Ok();
        }

        /// <summary>
        /// ???
        /// </summary>
        /// <param name="restype"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("$blobchangefeed")]
        public IActionResult GetBlobChangeFeed([FromQuery] string? restype)
        {
            if (restype == "container") return NotFound();
            return Ok();
        }

        /// <summary>
        /// Put a blob
        /// </summary>
        /// <param name="container"></param>
        /// <param name="fileSpec"></param>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{container}/{fileSpec}")]
        public IActionResult PutBlob(string container, string fileSpec, [FromBody] Stream fileContent)
        {
            return Ok();
        }

        /// <summary>
        /// Get information about a blob
        /// </summary>
        /// <param name="container"></param>
        /// <param name="fileSpec"></param>
        /// <returns></returns>
        [HttpHead]
        [Route("{container}/{filePath}")]
        public Task<IActionResult> GetBlobProperties(string container, string fileSpec)
        {
            return blobService.GetBlobProperties(container, fileSpec, HttpContext);
        }
    }
}