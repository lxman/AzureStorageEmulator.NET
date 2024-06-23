using System.Text;
using System.Xml;
using System.Xml.Serialization;
using XmlSerializationTest.BlobEnumeration;

ContainerEnumerationResults results = new()
{
    ContainerName = "aaa",
    MaxResults = 5000,
    Delimiter = "/",
};

XmlSerializer serializer = new(typeof(ContainerEnumerationResults));

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