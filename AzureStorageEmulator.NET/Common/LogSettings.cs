namespace AzureStorageEmulator.NET.Common
{
    public class LogSettings
    {
        public int BatchSeconds { get; set; }

        public bool DetailedLogging { get; set; }

        public bool LogToFrontEnd { get; set; }

        public Uri? LogUrl { get; set; }
    }
}