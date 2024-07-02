using AzureStorageEmulator.NET.Blob;
using AzureStorageEmulator.NET.Queue;
using AzureStorageEmulator.NET.Table;

namespace AzureStorageEmulator.NET.Common
{
    public interface ISettings
    {
        LogSettings LogSettings { get; set; }

        QueueSettings QueueSettings { get; set; }

        TableSettings TableSettings { get; set; }

        BlobSettings BlobSettings { get; set; }
    }

    public class Settings : ISettings
    {
        public PersistenceSettings PersistenceSettings { get; set; } = new();

        public LogSettings LogSettings { get; set; } = new();

        public QueueSettings QueueSettings { get; set; } = new();

        public TableSettings TableSettings { get; set; } = new();

        public BlobSettings BlobSettings { get; set; } = new();
    }
}