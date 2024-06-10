namespace XmlParsingTest.MessageList
{
    public class QueueMessage
    {
        public Guid MessageId { get; set; }

        public DateTime InsertionTime { get; set; }

        public DateTime ExpirationTime { get; set; }

        public string PopReceipt { get; set; } = string.Empty;

        public DateTime TimeNextVisible { get; set; }

        public int? DequeueCount { get; set; }

        public bool ShouldSerializeDequeueCount() => DequeueCount.HasValue;

        public string? MessageText { get; set; }
    }
}