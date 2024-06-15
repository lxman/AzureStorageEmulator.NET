using System.Text;
using System.Xml;
using System.Xml.Serialization;
using XmlParsingTest.QueueEnumeration;

SerializableQueue serializableQueue = new()
{
    Name = "queue1",
    Metadata =
    [
        new Metadata {Key = "key1", Value = "value1"},
        new Metadata {Key = "key2", Value = "value2"}
    ],
    Blocked = true
};

QueueEnumerationResults results = new()
{
    MaxResults = 5000,
    Queues =
    [
        serializableQueue
    ]
};

XmlSerializer serializer = new(typeof(QueueEnumerationResults));

XmlWriterSettings settings = new()
{
    Indent = true,
    IndentChars = "    ",
    Encoding = Encoding.UTF8
};
XmlWriter writer = XmlWriter.Create("output.xml", settings);
writer.WriteStartDocument(true);
XmlSerializerNamespaces ns = new();
ns.Add("", "");
serializer.Serialize(writer, results, ns);