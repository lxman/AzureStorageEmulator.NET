namespace AzureStorageEmulator.NET.Blob.Models
{
    public class Blob
    {
        public string? Name { get; set; }

        public Stream? Data { get; set; }
    }
}
