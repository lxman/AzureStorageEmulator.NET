using System.Xml.Serialization;
using AzureStorageEmulator.NET.Queue.Models.MessageResponses;

namespace AzureStorageEmulator.NET.Queue.Models.MessageResponseLists
{
    [XmlType(TypeName = "QueueMessagesList")]
    public class GetMessagesResponseList
    {
        [XmlElement("QueueMessage", typeof(GetMessageResponse))]
        public List<GetMessageResponse?> QueueMessagesList { get; set; } = [];
    }
}