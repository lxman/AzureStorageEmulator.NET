using System.Xml.Serialization;
using AzureStorageEmulator.NET.Queue.Models.MessageResponses;

namespace AzureStorageEmulator.NET.Queue.Models.MessageResponseLists
{
    [XmlType(TypeName = "QueueMessagesList")]
    public class PeekMessageResponseList
    {
        [XmlElement("QueueMessage", typeof(PeekMessageResponse))]
        public List<PeekMessageResponse?> QueueMessagesList { get; set; } = [];
    }
}
