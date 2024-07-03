using System.Xml.Serialization;

namespace XmlSerializationTest.StorageServiceProperties
{
    [XmlType("StorageServiceProperties")]
    public class StorageServiceProperties_20130815
    {
        [XmlElement("Logging")]
        public Logging? Logging { get; set; }

        [XmlElement("HourMetrics")]
        public Metrics? HourMetrics { get; set; }

        [XmlElement("MinuteMetrics")]
        public Metrics? MinuteMetrics { get; set; }

        [XmlElement("Cors")]
        public Cors? Cors { get; set; }
    }
}