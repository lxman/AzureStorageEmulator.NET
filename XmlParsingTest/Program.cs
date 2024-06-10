using System.Text;
using System.Xml;
using System.Xml.Serialization;
using XmlParsingTest.MessageList;

MessageList ml1 = new()
{
    QueueMessagesList =
    [
        new QueueMessage
        {
            MessageId = Guid.NewGuid(),
            InsertionTime = DateTime.Now,
            ExpirationTime = DateTime.Now.AddHours(1),
            PopReceipt = "pop",
            TimeNextVisible = DateTime.Now.AddHours(2),
            DequeueCount = 1,
            MessageText = "Hello, World!"
        }
    ]
};

MessageList ml2 = new()
{
    QueueMessagesList =
    [
        new QueueMessage
        {
            MessageId = Guid.NewGuid(),
            InsertionTime = DateTime.Now,
            ExpirationTime = DateTime.Now.AddHours(1),
            PopReceipt = "pop",
            TimeNextVisible = DateTime.Now.AddHours(2)
        }
    ]
};

Write1();
Write2();
return;

void Write1()
{
    XmlSerializer serializer = new(typeof(MessageList));

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
    serializer.Serialize(writer, ml1, ns);
}

void Write2()
{
    XmlSerializer serializer = new(typeof(MessageList));

    XmlWriterSettings settings = new()
    {
        Indent = true,
        IndentChars = "    ",
        Encoding = Encoding.UTF8
    };
    XmlWriter writer = XmlWriter.Create("output2.xml", settings);
    writer.WriteStartDocument(true);
    XmlSerializerNamespaces ns = new();
    ns.Add("", "");
    serializer.Serialize(writer, ml2, ns);
}