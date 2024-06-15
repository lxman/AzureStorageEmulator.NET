using AzureStorageEmulator.NET.Queue.Models;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public interface IFifoService
    {
        Task<List<Models.Queue>> GetQueues();

        Task<bool> AddQueue(string queueName);

        Task<bool> DeleteQueueAsync(string queueName);

        Task<bool> AddMessageAsync(string queueName, QueueMessage message);

        Task<List<QueueMessage>?> GetMessagesAsync(string queueName, int? numOfMessages = null, bool peekOnly = false);

        Task<Models.Queue?> GetQueueMetadata(string queueName);

        Task<QueueMessage?> DeleteMessageAsync(string queueName, Guid messageId, string popReceipt);

        Task DeleteMessages(string queueName);

        Task<int?> MessageCountAsync(string queueName);
    }
}