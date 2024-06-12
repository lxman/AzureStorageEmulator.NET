using System.Text;
using System.Xml;
using System.Xml.Serialization;
using AzureStorageEmulator.NET.Queue.Models;

namespace AzureStorageEmulator.NET.XmlSerialization.Queue
{
    public class MessageListSerializer : IXmlSerializer<MessageList>
    {
        private readonly XmlSerializer _serializer = new(typeof(MessageList));
        private readonly XmlWriter _writer;
        private readonly StringBuilder _output = new();
        private readonly XmlSerializerNamespaces _ns = new();

        public MessageListSerializer()
        {
            XmlWriterSettings settings = new()
            {
                Encoding = Encoding.UTF8,
                ConformanceLevel = ConformanceLevel.Auto
            };
            _writer = XmlWriter.Create(_output, settings);
            _writer.WriteStartDocument(true);
            _ns.Add("", "");
        }

        public string Serialize(MessageList o)
        {
            _serializer.Serialize(_writer, o, _ns);
            _writer.Flush();
            return _output.ToString();
        }

        public MessageList Deserialize(string xml)
        {
            throw new NotImplementedException();
        }
    }
}
