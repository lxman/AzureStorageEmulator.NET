using System.Xml.Serialization;

namespace AzureStorageEmulator.NET.Queue.Models
{
    public class QueueMessage
    {
        public Guid MessageId { get; set; }

        public DateTime InsertionTime { get; set; } = DateTime.UtcNow;

        public DateTime ExpirationTime => InsertionTime.AddSeconds(TimeToLive);

        public string PopReceipt { get; set; } = string.Empty;

        public DateTime TimeNextVisible => InsertionTime.AddSeconds(VisibilityTimeout);

        public int DequeueCount { get; set; }

        public string MessageText { get; set; } = string.Empty;

        public int VisibilityTimeout { get; set; }

        [XmlIgnore]
        public int TimeToLive { get; set; }

        public bool Visible => TimeNextVisible <= DateTime.UtcNow;

        public bool Expired => ExpirationTime < DateTime.UtcNow;
    }
}