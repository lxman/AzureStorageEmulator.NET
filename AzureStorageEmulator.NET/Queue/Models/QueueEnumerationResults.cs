using System.Xml.Serialization;

namespace AzureStorageEmulator.NET.Queue.Models
{
    [XmlRoot("EnumerationResults")]
    public class QueueEnumerationResults
    {
        [XmlAttribute]
        public string ServiceEndpoint { get; set; } = string.Empty;

        public string Prefix { get; set; } = string.Empty;

        public int MaxResults { get; set; }

        [XmlArray("Queues")]
        [XmlArrayItem("Queue")]
        public List<QueueMetadata> Queues { get; set; } = [];

        public string NextMarker { get; set; } = string.Empty;
    }
}