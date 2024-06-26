﻿using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace AzureStorageEmulator.NET.Blob.Xml
{
    [XmlRoot("EnumerationResults")]
    public class ContainerEnumerationResults
    {
        [Required]
        [XmlAttribute]
        public string ServiceEndpoint { get; set; } = "https://myaccount.blob.core.windows.net";

        [Required]
        [XmlAttribute]
        public string? ContainerName { get; set; }

        public string Prefix { get; set; } = string.Empty;

        public string Marker { get; set; } = string.Empty;

        public int MaxResults { get; set; }

        public string Delimiter { get; set; } = string.Empty;

        public List<Models.Blob> Blobs { get; set; } = [];

        public string NextMarker { get; set; } = string.Empty;
    }
}