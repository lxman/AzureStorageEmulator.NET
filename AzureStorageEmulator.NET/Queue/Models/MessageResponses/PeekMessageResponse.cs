namespace AzureStorageEmulator.NET.Queue.Models.MessageResponses
{
    public class PeekMessageResponse
    {
        public Guid MessageId { get; set; }

        public DateTime InsertionTime { get; set; }

        public DateTime ExpirationTime { get; set; }

        public int DequeueCount { get; set; }

        public string MessageText { get; set; } = string.Empty;
    }
}
