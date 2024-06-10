using System.Xml.Serialization;

namespace XmlTransformer.Queue.Models
{
    [XmlType(TypeName = "QueueMessagesList")]
    public class MessageList
    {
        public List<QueueMessage?> QueueMessagesList { get; set; } = [];
    }
}