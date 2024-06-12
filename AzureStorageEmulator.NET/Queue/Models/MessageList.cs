using System.Xml.Serialization;

namespace AzureStorageEmulator.NET.Queue.Models
{
    [XmlType(TypeName = "QueueMessagesList")]
    public class MessageList
    {
        [XmlElement("QueueMessage", typeof(QueueMessage))]
        public List<QueueMessage?> QueueMessagesList { get; set; } = [];
    }
}