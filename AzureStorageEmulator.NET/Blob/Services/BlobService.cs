using System.Globalization;
using System.Text;
using AzureStorageEmulator.NET.Blob.Models;
using AzureStorageEmulator.NET.Blob.Xml;
using AzureStorageEmulator.NET.Common;
using AzureStorageEmulator.NET.XmlSerialization;
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

        string GetBlobs();
    }

    public class BlobService(
        IBlobRoot root,
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

        public string GetBlobs()
        {
            return "Blobs";
        }

        public async Task Persist(string location)
        {
            return;
            throw new NotImplementedException("Blob persistence");
            Directory.CreateDirectory(Path.Combine(location, "Blob"));
            string saveFilePath = GetSavePath(location);
            await File.WriteAllTextAsync(saveFilePath, GetBlobs());
        }

        public async Task Restore(string location)
        {
            return;
            throw new NotImplementedException("Blob restore");
            string saveFilePath = GetSavePath(location);
            if (!File.Exists(saveFilePath)) return;
            await File.WriteAllTextAsync(Path.Combine(location, "Blob", "Blobs.xml"), GetBlobs());
        }

        public void Delete(string location)
        {
            return;
            throw new NotImplementedException("Blob delete");
            string saveFilePath = GetSavePath(location);
            if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
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

        private static string GetSavePath(string location) => Path.Combine(location, "AzureStorageEmulator.NET", "Blob", "Blobs.json");
    }
}