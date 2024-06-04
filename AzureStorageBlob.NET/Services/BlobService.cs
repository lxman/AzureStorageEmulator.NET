namespace AzureStorageBlob.NET.Services
{
    public interface IBlobService
    {
        public string GetBlobs();
    }

    public class BlobService : IBlobService
    {
        public string GetBlobs()
        {
            return "Blobs";
        }
    }
}