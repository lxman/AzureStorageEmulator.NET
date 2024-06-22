using AzureStorageEmulator.NET.Blob.Models;
using Microsoft.AspNetCore.Mvc;

namespace AzureStorageEmulator.NET.Blob.Services
{
    public interface IBlobService
    {
        IActionResult CreateContainer(string containerName);

        public string GetBlobs();
    }

    public class BlobService(IBlobRoot root) : IBlobService
    {
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
    }
}