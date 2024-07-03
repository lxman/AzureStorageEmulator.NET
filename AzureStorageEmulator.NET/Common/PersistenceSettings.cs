namespace AzureStorageEmulator.NET.Common
{
    public class PersistenceSettings
    {
        public Uri RootPath { get; } =
            new(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        public bool ClearTable { get; set; }

        public bool Table { get; set; }

        public bool ClearQueue { get; set; }

        public bool Queue { get; set; }

        public bool ClearBlob { get; set; }

        public bool Blob { get; set; }
    }
}
