using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
// ReSharper disable ValueParameterNotUsed

namespace AzureStorageEmulator.NET.Queue.Models
{
    public class Queue : IXmlSerializable
    {
        public string Name { get; set; } = string.Empty;

        public List<Metadata>? Metadata { get; set; } = [];

        public bool Blocked { get; set; }

        public int MessageCount { get; set; }

        public Queue()
        {
        }

        public Queue(string name)
        {
            Name = name;
        }

        public Queue(QueueObject queueObject)
        {
            Name = queueObject.Queue.Name;
            Metadata = queueObject.Queue.Metadata;
            Blocked = queueObject.Queue.Blocked;
            MessageCount = queueObject.Messages.Count;
        }

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