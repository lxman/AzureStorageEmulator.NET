namespace AzureStorageEmulator.NET.Blob
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