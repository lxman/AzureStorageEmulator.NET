using System.Xml.Serialization;

namespace XmlParsingTest.MessageList
{
    [XmlType(TypeName = "QueueMessagesList")]
    public class MessageList
    {
        public List<QueueMessage?> QueueMessagesList { get; set; } = [];
    }
}