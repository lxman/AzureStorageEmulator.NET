using AzureStorageEmulator.NET.Blob.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace AzureStorageEmulator.NET.Blob.Services
{
    public interface IBlobService
    {
        IActionResult GetInfo(HttpContext context);

        IActionResult CreateContainer(string containerName);

        public string GetBlobs();
    }

    public class BlobService(IBlobRoot root) : IBlobService
    {
        public IActionResult GetInfo(HttpContext context)
        {
            StringValues compValue = context.Request.Query["comp"];
            StringValues restypeValue = context.Request.Query["restype"];
            StringValues includeValue = context.Request.Query["include"];
            StringValues timeoutValue = context.Request.Query["timeout"];
            return compValue.Count switch
            {
                > 0 when compValue[0]?.ToLowerInvariant() == "properties" && restypeValue.Count > 0 &&
                         restypeValue[0]?.ToLowerInvariant() == "account" => new OkResult(),
                > 0 when compValue[0]?.ToLowerInvariant() == "list" && includeValue.Count > 0 &&
                         includeValue[0]?.ToLowerInvariant() == "metadata" => new OkResult(),
                > 0 when compValue[0]?.ToLowerInvariant() == "list" && includeValue.Count > 0 &&
                         includeValue[0]?.ToLowerInvariant() == "metadata" && timeoutValue.Count > 0 => new OkResult(),
                > 0 when compValue[0]?.ToLowerInvariant() == "properties" && restypeValue.Count > 0 &&
                         restypeValue[0]?.ToLowerInvariant() == "service" => new OkResult(),
                _ => new NotFoundResult()
            };
        }

        public IActionResult CreateContainer(string containerName)
        {
            if (root.Containers.Exists(c => c.Name == containerName))
            {
                return new ConflictResult();
            }
            root.Containers.Add(new Container(containerName));
            return new CreatedResult();
        }

        public string GetBlobs()
        {
            return "Blobs";
        }

        private string GetAllMetadata()
        {
            return string.Empty;
        }
    }
}