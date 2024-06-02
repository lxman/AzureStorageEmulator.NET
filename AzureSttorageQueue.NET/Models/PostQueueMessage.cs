using System.Xml.Serialization;

namespace AzureStorageEmulator.NET.Models
{
    [XmlType(TypeName = "QueueMessage")]
    public class PostQueueMessage
    {
        public string MessageText { get; set; } = string.Empty;
    }
}