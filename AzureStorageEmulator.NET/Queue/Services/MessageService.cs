using System.Text;
using AzureStorageEmulator.NET.Authentication;
using AzureStorageEmulator.NET.Queue.Models;

namespace AzureStorageEmulator.NET.Queue.Services
{
    public interface IMessageService
    {
        bool Authenticate(HttpRequest request);

        string GetQueues();

        bool AddQueue(string queueName);

        void DeleteQueue(string queueName);

        MessageList AddMessage(string queueName, PostQueueMessage message, int visibilityTimeout, int messageTtl);

        MessageList GetMessages(string queueName, int numOfMessages);

        QueueMessage? GetMessage(string queueName);

        Task<QueueMessage?> DeleteMessage(string queueName, Guid messageId, string popReceipt);
    }

    public class MessageService(IFifoService fifoService, IAuthenticator authenticator) : IMessageService
    {
        public bool Authenticate(HttpRequest request)
        {
            return authenticator.Authenticate(request);
        }

        public string GetQueues()
        {
            List<string> list = fifoService.GetQueues();
            StringBuilder output = new();
            output.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            output.Append("<EnumerationResults ServiceEndpoint=\"http://127.0.0.1:10001/devstoreaccount1\">");
            output.Append("<Prefix/>");
            output.Append("<MaxResults>5000</MaxResults>");
            output.Append("<Queues>");
            list.ForEach(queueName => output.Append($"<Queue><Name>{queueName}</Name><Metadata/></Queue>"));
            output.Append("</Queues>");
            output.Append("<NextMarker/>");
            output.Append("</EnumerationResults>");
            return output.ToString();
        }

        public MessageList GetMessages(string queueName, int numOfMessages)
        {
            if (numOfMessages == 0) numOfMessages = 1;
            List<QueueMessage?>? result = fifoService.GetMessages(queueName, numOfMessages);
            MessageList queueMessageList = new();
            if (result is not null) queueMessageList.QueueMessagesList.AddRange(result);
            return queueMessageList;
        }

        public MessageList AddMessage(string queueName, PostQueueMessage message, int visibilityTimeout, int messageTtl)
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
            return queueMessageList;
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
    }
}