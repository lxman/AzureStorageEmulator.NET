using System.Text;
using System.Xml;
using System.Xml.Serialization;
using XmlParsingTest;

StorageServiceProperties_20130815 props = new()
{
    Logging = new Logging
    {
        Version = "1.0",
        Delete = true,
        Read = true,
        Write = true,
        RetentionPolicy = new RetentionPolicy
        {
            Enabled = true,
            Days = 7
        }
    },
    HourMetrics = new Metrics
    {
        Version = "1.0",
        Enabled = true,
        IncludeApis = true,
        RetentionPolicy = new RetentionPolicy
        {
            Enabled = true,
            Days = 7
        }
    },
    MinuteMetrics = new Metrics
    {
        Version = "1.0",
        Enabled = true,
        IncludeApis = true,
        RetentionPolicy = new RetentionPolicy
        {
            Enabled = true,
            Days = 7
        }
    },
    Cors = new Cors
    {
        CorsRule = new CorsRule
        {
            AllowedOrigins = "*",
            AllowedMethods = "GET, PUT",
            MaxAgeInSeconds = 500,
            ExposedHeaders = "x-ms-meta-data*, x-ms-meta-target*",
            AllowedHeaders = "x-ms-meta-abc, x-ms-meta-data*, x-ms-meta-target*"
        }
    }
};

XmlSerializer serializer = new(typeof(StorageServiceProperties_20130815));

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
serializer.Serialize(writer, props, ns);