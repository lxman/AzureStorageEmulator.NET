namespace AzureStorageEmulator.NET.Common
{
    public class Status
    {
        public bool IsAlive { get; set; } = true;

        public DateTime BuildTime { get; set; } = Assembly_Attributes.Readers.BuildTime.GetBuildTime().ToLocalTime();
    }
}