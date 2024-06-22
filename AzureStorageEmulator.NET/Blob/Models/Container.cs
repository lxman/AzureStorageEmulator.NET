using System.Xml.Serialization;

namespace AzureStorageEmulator.NET.Blob.Models
{
    public interface IContainer
    {
        string Name { get; set; }
        List<Folder> Folders { get; set; }
        List<Blob> Blobs { get; set; }
    }

    public class Container(string name) : IContainer
    {
        [XmlElement(ElementName = "Properties")]
        public Metadata Metadata { get; set; } = new();

        public string Name { get; set; } = name;

        public List<Folder> Folders { get; set; } = [];

        public List<Blob> Blobs { get; set; } = [];
    }
}
