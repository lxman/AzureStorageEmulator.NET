using System.Xml.Serialization;

namespace AzureStorageEmulator.NET.Blob.Models
{
    public interface IBlobRoot
    {
        List<IContainer> Containers { get; set; }
    }

    [XmlRoot(ElementName = "EnumerationResults")]
    public class BlobRoot : IBlobRoot
    {
        [XmlAttribute]
        public string ServiceEndpoint { get; set; } = string.Empty;

        public string Prefix { get; set; } = string.Empty;

        public string NextMarker { get; set; } = string.Empty;

        public int MaxResults { get; set; }

        public List<IContainer> Containers { get; set; } = [];
    }
}