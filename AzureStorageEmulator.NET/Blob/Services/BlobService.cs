using System.Globalization;
using System.Text;
using AzureStorageEmulator.NET.Blob.Models;
using AzureStorageEmulator.NET.Blob.Xml;
using AzureStorageEmulator.NET.Common;
using AzureStorageEmulator.NET.XmlSerialization;
using BlobStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace AzureStorageEmulator.NET.Blob.Services
{
    public interface IBlobService : IStorageProvider
    {
        IActionResult GetInfo(HttpContext context);

        IActionResult CreateContainer(string containerName, HttpContext context);

        Task<IActionResult> PingContainer(string containerName, HttpContext context);

        Task<MemoryStream> ListContainerContents(string containerName, HttpContext context);

        Task<IActionResult> GetBlobProperties(string container, string fileSpec, HttpContext context);

        Task<IActionResult> PutBlob(string container, string fileSpec, HttpContext context);

        string GetBlobs();
    }

    public class BlobService(
        IBlobRoot root,
        IBlobStorage blobStorage,
        IXmlSerializer<Metadata> metadataSerializer,
        IXmlSerializer<ContainerEnumerationResults> containerEnumerationSerializer) : IBlobService
    {
        public IActionResult GetInfo(HttpContext context)
        {
            StringValues compValue = context.Request.Query["comp"];
            StringValues restypeValue = context.Request.Query["restype"];
            StringValues includeValue = context.Request.Query["include"];
            StringValues timeoutValue = context.Request.Query["timeout"];
            if (restypeValue.Count > 0 && restypeValue[0]?.ToLowerInvariant() == "container")
            {
                return new OkResult();
            }
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
                _ => new BadRequestResult()
            };
        }

        public async Task<IActionResult> PingContainer(string containerName, HttpContext context)
        {
            StringValues compValue = context.Request.Query["comp"];
            StringValues restypeValue = context.Request.Query["restype"];
            if ((restypeValue.Count <= 0 || restypeValue[0]?.ToLowerInvariant() != "container" || compValue.Count != 0)
                && (compValue.Count <= 0 || compValue[0]?.ToLowerInvariant() != "properties"
                                         || restypeValue.Count <= 0 ||
                                         restypeValue[0]?.ToLowerInvariant() != "account"))
            {
                return new BadRequestResult();
            }

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

        public async Task<MemoryStream> ListContainerContents(string containerName, HttpContext context)
        {
            StringValues delimiterValue = context.Request.Query["delimiter"];
            StringValues maxresultsValues = context.Request.Query["maxresults"];
            StringValues includeValue = context.Request.Query["include"];
            StringValues timeoutValue = context.Request.Query["timeout"];
            if (!root.Containers.Exists(c => c.Name == containerName))
            {
                context.Response.StatusCode = 404;
                return new MemoryStream("Container not found"u8.ToArray());
            }
            context.Response.Headers.Connection = "keep-alive";
            context.Response.Headers.KeepAlive = "timeout=5";
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/xml";
            ContainerEnumerationResults results = new()
            {
                ServiceEndpoint =
                    $"{context.Request.Scheme}://{context.Request.Host}/{context.Request.Path.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries)[0]}",
                ContainerName = containerName,
                MaxResults = int.Parse(maxresultsValues[0]!),
                Delimiter = delimiterValue[0] ?? string.Empty
            };
            return new MemoryStream(Encoding.UTF8.GetBytes(await containerEnumerationSerializer.Serialize(results)));
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

        public Task<IActionResult> GetBlobProperties(string container, string fileSpec, HttpContext context)
        {
            IContainer? c = root.Containers.Find(c => c.Name == container);
            if (c is null) return Task.FromResult<IActionResult>(new NotFoundResult());
            Models.Blob? b = c.Blobs.Find(b => b.FileSpec == fileSpec);
            if (b is null) return Task.FromResult<IActionResult>(new NotFoundResult());
            context.Response.Headers.Append("etag", $"0x{b.Metadata.Etag:X}");
            context.Response.Headers.Append("last-modified", b.Metadata.LastModified.ToString("ddd, dd MMM yyy HH':'mm':'ss 'GMT'", CultureInfo.InvariantCulture));
            //context.Response.Headers.Append("content-length", b.Metadata.ContentLength.ToString());
            context.Response.Headers.Connection = "keep-alive";
            context.Response.Headers.KeepAlive = "timeout=5";
            context.Response.Headers.Append("x-ms-has-legal-hold", b.Metadata.HasLegalHold.ToString().ToLowerInvariant());
            context.Response.Headers.Append("x-ms-has-immutability-policy", b.Metadata.HasImmutabilityPolicy.ToString().ToLowerInvariant());
            context.Response.Headers.Append("x-ms-lease-state", b.Metadata.LeaseState.ToString().ToLowerInvariant());
            context.Response.Headers.Append("x-ms-lease-status", b.Metadata.LeaseStatus.ToString().ToLowerInvariant());
            return Task.FromResult<IActionResult>(new OkResult());
        }

        public Task<IActionResult> PutBlob(string container, string fileSpec, HttpContext context)
        {
            root.Containers
                .Find(c => c.Name == container)?
                .Blobs
                .Add(new Models.Blob(blobStorage) { FileSpec = fileSpec, Data = context.Request.BodyReader.AsStream() });
            //context.Response.Headers.Append("etag", $"0x{b.Metadata.Etag:X}");
            //context.Response.Headers.Append("last-modified", b.Metadata.LastModified.ToString("ddd, dd MMM yyy HH':'mm':'ss 'GMT'", CultureInfo.InvariantCulture));
            context.Response.Headers.Append("content-length", "0");
            context.Response.Headers.Connection = "keep-alive";
            context.Response.Headers.KeepAlive = "timeout=5";
            return Task.FromResult<IActionResult>(new StatusCodeResult(201));
        }

        public string GetBlobs()
        {
            return "Blobs";
        }

        public async Task Persist(string location)
        {
            await blobStorage.Persist(location);
        }

        public async Task Restore(string location)
        {
            await blobStorage.Restore(location);
        }

        public void Delete(string location)
        {
            blobStorage.Delete(location);
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