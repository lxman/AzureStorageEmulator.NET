using AzureStorageEmulator.NET.Queue.Models;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public interface IFifoService
    {
        Task<List<Models.Queue>> ListQueuesAsync();

        Task<bool> CreateQueueAsync(string queueName);

        Task<bool> DeleteQueueAsync(string queueName);

        Task<bool> PutMessageAsync(string queueName, QueueMessage message);

        Task<List<QueueMessage>?> GetMessagesAsync(string queueName, int? numOfMessages = null, bool peekOnly = false);

        Task<Models.Queue?> GetQueueMetadataAsync(string queueName);

        Task<QueueMessage?> DeleteMessageAsync(string queueName, Guid messageId, string popReceipt);

        Task<int> ClearMessagesAsync(string queueName);

        Task<int?> MessageCountAsync(string queueName);
    }
}