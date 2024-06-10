using System.Xml.Serialization;

namespace XmlParsingTest.StorageServiceProperties
{
    public class Metrics
    {
        public string Version { get; set; }

        public bool Enabled { get; set; }

        [XmlElement("IncludeAPIs")]
        public bool IncludeApis { get; set; }

        public RetentionPolicy RetentionPolicy { get; set; }
    }
}
