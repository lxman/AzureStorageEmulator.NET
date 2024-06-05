namespace AzureStorageEmulator.NET.Queue
{
    public interface IQueueSettings
    {
        public int Delay { get; set; }

        public bool LogGetMessages { get; set; }
    }

    public class QueueSettings : IQueueSettings
    {
        public int Delay { get; set; }

        public bool LogGetMessages { get; set; }
    }
}