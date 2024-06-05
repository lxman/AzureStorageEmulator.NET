using System.Xml.Serialization;

namespace AzureStorageEmulator.NET.Queue.Models
{
    [XmlType(TypeName = "QueueMessage")]
    public class PostQueueMessage
    {
        public string MessageText { get; set; } = string.Empty;
    }
}