using System.Xml.Serialization;

namespace AzureStorageEmulator.NET.Blob.Models
{
    public interface IContainer
    {
        Metadata Metadata { get; set; }

        string Name { get; set; }

        List<Blob> Blobs { get; set; }
    }

    public class Container(string name) : IContainer
    {
        [XmlElement(ElementName = "Properties")]
        public Metadata Metadata { get; set; } = new();

        public string Name { get; set; } = name;

        public List<Blob> Blobs { get; set; } = [];
    }
}