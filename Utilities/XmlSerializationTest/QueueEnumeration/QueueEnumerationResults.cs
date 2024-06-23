using System.Xml.Serialization;

namespace XmlParsingTest.QueueEnumeration
{
    [XmlRoot("EnumerationResults")]
    public class QueueEnumerationResults
    {
        [XmlAttribute]
        public string ServiceEndpoint { get; set; } = "http://127.0.0.1:10001/devstoreaccount1";

        public string Prefix { get; set; } = string.Empty;

        public int MaxResults { get; set; }

        public List<SerializableQueue> Queues { get; set; } = [];

        public string NextMarker { get; set; } = string.Empty;
    }
}