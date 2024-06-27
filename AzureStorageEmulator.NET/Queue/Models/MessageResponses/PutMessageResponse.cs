namespace AzureStorageEmulator.NET.Queue.Models.MessageResponses
{
    public class PutMessageResponse
    {
        public Guid MessageId { get; set; }

        public DateTime InsertionTime { get; set; }

        public DateTime ExpirationTime { get; set; }

        public string PopReceipt { get; set; } = string.Empty;

        public DateTime TimeNextVisible { get; set; }
    }
}
