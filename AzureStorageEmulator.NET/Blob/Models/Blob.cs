using System.Xml.Serialization;

namespace AzureStorageEmulator.NET.Blob.Models
{
    public class Blob
    {
        [XmlElement(ElementName = "Properties")]
        public Metadata Metadata { get; set; } = new();

        public string? Name { get; set; }

        [XmlIgnore]
        public Stream? Data { get; set; }
    }
}
