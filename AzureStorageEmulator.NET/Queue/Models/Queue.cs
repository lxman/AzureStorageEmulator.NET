using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AzureStorageEmulator.NET.Queue.Models
{
    public class Queue : IXmlSerializable
    {
        public string Name { get; set; }

        public List<Metadata>? Metadata { get; set; } = [];

        public bool Blocked { get; set; }

        public int MessageCount { get; set; }

        public XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("Name", Name);
            if (Metadata is not null)
            {
                writer.WriteStartElement("Metadata");
                foreach (Metadata metadata in Metadata)
                {
                    writer.WriteElementString(metadata.Key, metadata.Value);
                }

                writer.WriteEndElement();
            }
            else
            {
                writer.WriteElementString("Metadata", string.Empty);
            }
        }
    }
}