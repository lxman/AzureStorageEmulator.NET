using System.Xml.Serialization;
using AzureStorageEmulator.NET.Queue.Models.MessageResponses;

namespace AzureStorageEmulator.NET.Queue.Models.MessageResponseLists
{
    [XmlType(TypeName = "QueueMessagesList")]
    public class PutMessageResponseList
    {
        [XmlElement("QueueMessage", typeof(PutMessageResponse))]
        public List<PutMessageResponse?> QueueMessagesList { get; set; } = [];
    }
}
