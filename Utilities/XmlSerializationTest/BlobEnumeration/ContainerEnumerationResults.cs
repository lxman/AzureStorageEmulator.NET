﻿using System.Xml.Serialization;

namespace XmlSerializationTest.BlobEnumeration
{
    [XmlRoot("EnumerationResults")]
    public class ContainerEnumerationResults
    {
        [XmlAttribute]
        public string ServiceEndpoint { get; set; } = "https://myaccount.blob.core.windows.net";

        [XmlAttribute]
        public string? ContainerName { get; set; }

        public string Prefix { get; set; } = string.Empty;

        public string Marker { get; set; } = string.Empty;

        public int MaxResults { get; set; }

        public string? Delimiter { get; set; }

        public List<Blob> Blobs { get; set; } = [];

        public string NextMarker { get; set; } = string.Empty;
    }
}