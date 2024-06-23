using System.Globalization;
using AzureStorageEmulator.NET.Blob.Models;
using AzureStorageEmulator.NET.XmlSerialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace AzureStorageEmulator.NET.Blob.Services
{
    public interface IBlobService
    {
        IActionResult GetInfo(HttpContext context);

        IActionResult CreateContainer(string containerName, HttpContext context);

        IActionResult PingContainer(string containerName, HttpContext context);

        string GetBlobs();
    }

    public class BlobService(
        IBlobRoot root,
        IXmlSerializer<Metadata> metadataSerializer) : IBlobService
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
                         includeValue[0]?.ToLowerInvariant() == "metadata" => new OkObjectResult(GetContainersMetadata()),
                > 0 when compValue[0]?.ToLowerInvariant() == "list" && includeValue.Count > 0 &&
                         includeValue[0]?.ToLowerInvariant() == "metadata" && timeoutValue.Count > 0 => new OkResult(),
                > 0 when compValue[0]?.ToLowerInvariant() == "properties" && restypeValue.Count > 0 &&
                         restypeValue[0]?.ToLowerInvariant() == "service" => new OkResult(),
                _ => new NotFoundResult()
            };
        }

        public IActionResult PingContainer(string containerName, HttpContext context)
        {
            StringValues compValue = context.Request.Query["comp"];
            StringValues restypeValue = context.Request.Query["restype"];
            StringValues includeValue = context.Request.Query["include"];
            StringValues timeoutValue = context.Request.Query["timeout"];
            StringValues delimiterValue = context.Request.Query["delimiter"];
            StringValues maxresultsValues = context.Request.Query["maxresults"];
            if (restypeValue.Count > 0 && restypeValue[0]?.ToLowerInvariant() == "container"
                || (compValue.Count > 0 && compValue[0]?.ToLowerInvariant() == "properties"
                                        && restypeValue.Count > 0 && restypeValue[0]?.ToLowerInvariant() == "account"))
            {
                IContainer? c = root.Containers.Find(c => c.Name == containerName);
                if (c is null) return new NotFoundResult();
                context.Response.Headers.Append("etag", $"0x{c.Metadata.Etag:X}");
                context.Response.Headers.Append("last-modified", c.Metadata.LastModified.ToString("ddd, dd MMM yyy HH':'mm':'ss 'GMT'", CultureInfo.InvariantCulture));
                context.Response.Headers.Append("content-length", "0");
                context.Response.Headers.Connection = "keep-alive";
                context.Response.Headers.KeepAlive = "timeout=5";
                context.Response.Headers.Append("x-ms-has-legal-hold", c.Metadata.HasLegalHold.ToString().ToLowerInvariant());
                context.Response.Headers.Append("x-ms-has-immutability-policy", c.Metadata.HasImmutabilityPolicy.ToString().ToLowerInvariant());
                context.Response.Headers.Append("x-ms-lease-state", c.Metadata.LeaseState.ToString().ToLowerInvariant());
                context.Response.Headers.Append("x-ms-lease-status", c.Metadata.LeaseStatus.ToString().ToLowerInvariant());
                return new OkResult();
            }
            if (compValue.Count > 0 && compValue[0]?.ToLowerInvariant() == "list" && includeValue.Count > 0
                && includeValue[0]?.ToLowerInvariant() == "metadata"
                && delimiterValue.Count > 0
                && restypeValue.Count > 0 && restypeValue[0]?.ToLowerInvariant() == "container"
                && maxresultsValues.Count > 0)
            {
                return new OkObjectResult(GetBlobs());
            }
            return new NotFoundResult();
        }

        public IActionResult CreateContainer(string containerName, HttpContext context)
        {
            if (root.Containers.Exists(c => c.Name == containerName))
            {
                return new ConflictResult();
            }
            Container c = new(containerName);
            root.Containers.Add(c);
            context.Response.Headers.Append("etag", $"0x{c.Metadata.Etag:X}");
            context.Response.Headers.Append("last-modified", c.Metadata.LastModified.ToString("ddd, dd MMM yyy HH':'mm':'ss 'GMT'", CultureInfo.InvariantCulture));
            context.Response.Headers.Append("content-length", "0");
            context.Response.Headers.Connection = "keep-alive";
            context.Response.Headers.KeepAlive = "timeout=5";

            return new StatusCodeResult(201);
        }

        public string GetBlobs()
        {
            return "Blobs";
        }

        private string GetContainersMetadata()
        {
            string metadata = string.Empty;
            root.Containers.ForEach(c =>
            {
                metadata += metadataSerializer.Serialize(c.Metadata);
            });
            return metadata;
        }
    }
}