using System.Xml.Serialization;

namespace AzureStorageEmulator.NET.Queue.Models
{
    public class EnumerationResults
    {
        [XmlAttribute]
        public string ServiceEndpoint { get; set; } = "http://127.0.0.1:10001/devstoreaccount1";

        public Prefix Prefix { get; set; } = new();

        public int MaxResults { get; set; }

        public List<Queue> Queues { get; set; } = [];

        public string NextMarker { get; set; } = string.Empty;
    }
}