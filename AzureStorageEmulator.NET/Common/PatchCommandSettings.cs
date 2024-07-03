namespace AzureStorageEmulator.NET.Common
{
    public enum Action
    {
        None,
        Backup,
        Clear
    }

    public class PatchCommandSettings
    {
        public Action Action { get; set; }

        public bool Table { get; set; }

        public bool Queue { get; set; }

        public bool Blob { get; set; }
    }
}
