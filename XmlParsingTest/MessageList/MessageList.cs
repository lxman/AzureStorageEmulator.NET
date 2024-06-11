using System.Xml.Serialization;

namespace XmlParsingTest.MessageList
{
    [XmlType(TypeName = "QueueMessagesList")]
    public class MessageList
    {
        [XmlElement("QueueMessage", typeof(QueueMessage))]
        public List<QueueMessage?> QueueMessagesList { get; set; } = [];
    }
}