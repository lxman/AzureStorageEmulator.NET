namespace AzureStorageEmulator.NET.Queue
{
    public interface IQueueSettings
    {
        public bool LogGetMessages { get; set; }
    }

    public class QueueSettings : IQueueSettings
    {
        public bool LogGetMessages { get; set; }
    }
}