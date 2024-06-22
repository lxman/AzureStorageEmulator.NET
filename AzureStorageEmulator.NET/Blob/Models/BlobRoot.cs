namespace AzureStorageEmulator.NET.Blob.Models
{
    public interface IBlobRoot
    {
        List<IContainer> Containers { get; set; }
    }

    public class BlobRoot : IBlobRoot
    {
        public List<IContainer> Containers { get; set; } = [];
    }
}
