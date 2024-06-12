namespace AzureStorageEmulator.NET.Queue.Models
{
    public class Queue
    {
        public string Name { get; set; }

        public QueueMetadata Metadata { get; set; } = new();
    }
}
