using AzureStorageEmulator.NET.Authentication;
using AzureStorageEmulator.NET.XmlSerialization;
using XmlTransformer.Queue.Models;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public interface IMessageService
    {
        bool Authenticate(HttpRequest request);

        string GetQueues();

        bool AddQueue(string queueName);

        void DeleteQueue(string queueName);

        string AddMessage(string queueName, QueueMessage message, int visibilityTimeout, int messageTtl, int timeout);

        string GetMessages(string queueName, int numOfMessages);

        string GetAllMessages(string queueName);

        QueueMessage? GetMessage(string queueName);

        Task<QueueMessage?> DeleteMessage(string queueName, Guid messageId, string popReceipt);

        void DeleteMessages(string queueName);
    }

    public class MessageService(IFifoService fifoService,
        IAuthenticator authenticator,
        IXmlSerializer<XmlTransformer.Queue.Models.EnumerationResults> enumerationResultsSerializer,
        IXmlSerializer<MessageList> messageListSerializer) : IMessageService
    {
        public bool Authenticate(HttpRequest request)
        {
            return authenticator.Authenticate(request);
        }

        public string GetQueues()
        {
            XmlTransformer.Queue.Models.EnumerationResults results = new();
            results.Queues.AddRange(fifoService.GetQueues());
            results.MaxResults = 5000;
            return enumerationResultsSerializer.Serialize(results);
        }

        public string GetMessages(string queueName, int numOfMessages)
        {
            if (numOfMessages == 0) numOfMessages = 1;
            List<QueueMessage?>? result = fifoService.GetMessages(queueName, numOfMessages);
            MessageList queueMessageList = new();
            if (result is not null) queueMessageList.QueueMessagesList.AddRange(result);
            return messageListSerializer.Serialize(queueMessageList);
        }

        public string GetAllMessages(string queueName)
        {
            List<QueueMessage?>? result = fifoService.GetAllMessages(queueName);
            MessageList queueMessageList = new();
            if (result is not null) queueMessageList.QueueMessagesList.AddRange(result);
            return messageListSerializer.Serialize(queueMessageList);
        }

        public string AddMessage(string queueName, QueueMessage message, int visibilityTimeout, int messageTtl, int timeout)
        {
            MessageList queueMessageList = new();
            QueueMessage queueMessage = new();
            queueMessageList.QueueMessagesList.Add(queueMessage);
            queueMessage.DequeueCount = 0;
            queueMessage.ExpirationTime = DateTime.UtcNow.AddDays(7);
            queueMessage.InsertionTime = DateTime.UtcNow;
            queueMessage.MessageId = Guid.NewGuid();
            queueMessage.MessageText = message.MessageText;
            queueMessage.PopReceipt = Guid.NewGuid().ToString();
            queueMessage.TimeNextVisible = DateTime.UtcNow.AddSeconds(messageTtl);
            fifoService.AddMessage(queueName, queueMessage);
            return messageListSerializer.Serialize(queueMessageList);
        }

        public bool AddQueue(string queueName)
        {
            return fifoService.AddQueue(queueName);
        }

        public void DeleteQueue(string queueName)
        {
            fifoService.DeleteQueue(queueName);
        }

        public QueueMessage? GetMessage(string queueName)
        {
            return fifoService.GetMessage(queueName);
        }

        public Task<QueueMessage?> DeleteMessage(string queueName, Guid messageId, string popReceipt)
        {
            return fifoService.DeleteMessage(queueName, messageId, popReceipt);
        }

        public void DeleteMessages(string queueName)
        {
            fifoService.DeleteMessages(queueName);
        }
    }
}