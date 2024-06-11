using XmlTransformer.Queue.Models;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public interface IFifoService
    {
        List<XmlTransformer.Queue.Models.Queue> GetQueues();

        bool AddQueue(string queueName);

        void DeleteQueue(string queueName);

        void AddMessage(string queueName, QueueMessage message);

        List<QueueMessage?>? GetMessages(string queueName, int numOfMessages);

        List<QueueMessage?>? GetAllMessages(string queueName);

        QueueMessage? GetMessage(string queueName);

        Task<QueueMessage?> DeleteMessage(string queueName, Guid messageId, string popReceipt);

        void DeleteMessages(string queueName);
    }
}