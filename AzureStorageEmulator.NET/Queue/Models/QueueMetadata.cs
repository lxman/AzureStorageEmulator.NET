namespace AzureStorageEmulator.NET.Queue.Models
{
    public class QueueMetadata
    {
        public int MessageCount { get; set; }

        public Dictionary<string, string>? Metadata { get; set; }
    }
}