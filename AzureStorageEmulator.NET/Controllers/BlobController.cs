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
        /// <returns></returns>
        [HttpPut("{containerName}")]
        public IActionResult CreateContainer(string containerName)
        {
            return blobService.CreateContainer(containerName, HttpContext);
        }

        /// <summary>
        /// Returns information about a container
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="comp"></param>
        /// <param name="restype"></param>
        /// <returns></returns>
        [QueryStringConstraint("restype", true)]
        [QueryStringConstraint("maxresults", false)]
        [QueryStringConstraint("include", false)]
        [QueryStringConstraint("delimiter", false)]
        [HttpGet("{containerName}")]
        public Task<IActionResult> PingContainer(string containerName)
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
        [QueryStringConstraint("comp", true)]
        [QueryStringConstraint("maxresults", true)]
        [QueryStringConstraint("restype", true)]
        [QueryStringConstraint("include", true)]
        [QueryStringConstraint("delimiter", true)]
        [HttpGet("{containerName}")]
        public Task<MemoryStream> ListContainerContents(string containerName)
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
    }
}