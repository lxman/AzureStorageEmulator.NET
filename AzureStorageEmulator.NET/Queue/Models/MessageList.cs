using System.Xml.Serialization;

namespace XmlTransformer.Queue.Models
{
    [XmlType(TypeName = "QueueMessagesList")]
    public class MessageList
    {
        [XmlElement("QueueMessage", typeof(QueueMessage))]
        public List<QueueMessage?> QueueMessagesList { get; set; } = [];
    }
}