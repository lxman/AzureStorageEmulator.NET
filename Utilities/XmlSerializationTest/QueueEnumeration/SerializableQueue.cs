using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace XmlSerializationTest.QueueEnumeration
{
    public class SerializableQueue : IXmlSerializable
    {
        public string Name { get; set; }

        public List<Metadata> Metadata { get; set; } = [];

        [XmlIgnore]
        public bool Blocked { get; set; }

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
            writer.WriteStartElement("Metadata");
            foreach (Metadata metadata in Metadata)
            {
                writer.WriteElementString(metadata.Key, metadata.Value);
            }
            writer.WriteEndElement();
        }
    }
}