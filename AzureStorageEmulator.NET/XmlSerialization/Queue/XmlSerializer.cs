using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace AzureStorageEmulator.NET.XmlSerialization.Queue
{
    public class XmlSerializer<T> : IXmlSerializer<T>
    {
        private readonly XmlSerializer _serializer = new(typeof(T));
        private readonly XmlWriter _writer;
        private readonly StringBuilder _output = new();
        private readonly XmlSerializerNamespaces _ns = new();

        public XmlSerializer()
        {
            XmlWriterSettings settings = new()
            {
                Encoding = Encoding.UTF8,
                ConformanceLevel = ConformanceLevel.Auto,
                Async = true
            };
            _writer = XmlWriter.Create(_output, settings);
            _writer.WriteStartDocument(true);
            _ns.Add("", "");
        }

        public async Task<string> Serialize(T o)
        {
            _serializer.Serialize(_writer, o, _ns);
            await _writer.FlushAsync();
            return _output.Replace("utf-16", "utf-8").ToString();
        }

        public T Deserialize(string xml)
        {
            throw new NotImplementedException();
        }
    }
}
