namespace AzureStorageEmulator.NET.Queue.Models
{
    public class MessageList
    {
        public List<QueueMessage?> QueueMessagesList { get; set; } = new();
    }
}