namespace AzureStorageQueue.NET.Models
{
    public class MessageList
    {
        public List<QueueMessage?> QueueMessagesList { get; set; } = new();
    }
}