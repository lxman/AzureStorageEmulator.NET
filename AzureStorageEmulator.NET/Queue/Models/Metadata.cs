﻿using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AzureStorageEmulator.NET.Queue.Models
{
    public class Metadata : IXmlSerializable
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        private readonly Guid _id = Guid.NewGuid();

        public XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString(Key, Value);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }
}