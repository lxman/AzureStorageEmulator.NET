using AzureStorageEmulator.NET.Queue.Models;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public interface IFifoService
    {
        List<Models.Queue> GetQueues();

        bool AddQueue(string queueName);

        Task<bool> DeleteQueueAsync(string queueName);

        Task<bool> AddMessageAsync(string queueName, QueueMessage message);

        Task<List<QueueMessage>?> GetMessagesAsync(string queueName, int? numOfMessages = null, bool peekOnly = false);

        List<QueueMessage>? GetAllMessages(string queueName);

        Task<QueueMessage?> DeleteMessageAsync(string queueName, Guid messageId, string popReceipt);

        void DeleteMessages(string queueName);

        Task<int?> MessageCountAsync(string queueName);
    }
}