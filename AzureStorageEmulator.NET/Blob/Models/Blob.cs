using System.Xml.Serialization;
using BlobStorage;

namespace AzureStorageEmulator.NET.Blob.Models
{
    public class Blob
    {
        [XmlElement(ElementName = "Properties")]
        public Metadata Metadata { get; set; } = new();

        [XmlElement(ElementName = "Name")]
        public string? FileSpec { get; set; }

        [XmlIgnore]
        public Stream? Data
        {
            get => _storage?.DownloadFile(FileSpec);
            set
            {
                if (value is not null)
                {
                    _storage?.UploadFile(FileSpec, value);
                }
            }
        }

        private readonly IBlobStorage? _storage;

        public Blob(IBlobStorage storage)
        {
            _storage = storage;
        }

        public Blob() { }
    }
}