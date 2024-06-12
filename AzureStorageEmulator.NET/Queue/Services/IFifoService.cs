using AzureStorageEmulator.NET.Queue.Models;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public interface IFifoService
    {
        List<Models.Queue> GetQueues();

        bool AddQueue(string queueName);

        void DeleteQueue(string queueName);

        void AddMessage(string queueName, QueueMessage message);

        List<QueueMessage?>? GetMessages(string queueName, int numOfMessages);

        List<QueueMessage?>? GetAllMessages(string queueName);

        Task<QueueMessage?> DeleteMessage(string queueName, Guid messageId, string popReceipt);

        void DeleteMessages(string queueName);

        int MessageCount(string queueName);
    }
}