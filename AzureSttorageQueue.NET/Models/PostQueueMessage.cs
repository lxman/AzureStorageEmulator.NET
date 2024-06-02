using System.Xml.Serialization;

namespace AzureStorageQueue.NET.Models
{
    [XmlType(TypeName = "QueueMessage")]
    public class PostQueueMessage
    {
        public string MessageText { get; set; } = string.Empty;
    }
}