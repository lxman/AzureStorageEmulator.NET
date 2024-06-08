using AzureStorageEmulator.NET.Authentication;
using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace AzureStorageEmulatorTests.Queue.Services
{
    public class MessageServiceTests
    {
        private const string QueueName = "testQueue";
        private static readonly Mock<IFifoService> MockFifoService = new();
        private static readonly Mock<IAuthenticator> MockAuthenticator = new();
        private readonly IMessageService _messageService = new MessageService(MockFifoService.Object, MockAuthenticator.Object);

        [Fact]
        public void AddQueue_ShouldAddQueueSuccessfully()
        {
            MockFifoService.Setup(service => service.AddQueue(It.IsAny<string>())).Returns(true);

            bool result = _messageService.AddQueue(QueueName);

            Assert.True(result);
            MockFifoService.Verify(service => service.AddQueue(QueueName), Times.Once);
        }

        [Fact]
        public void DeleteQueue_ShouldRemoveQueueSuccessfully()
        {
            _messageService.DeleteQueue(QueueName);

            MockFifoService.Verify(service => service.DeleteQueue(QueueName), Times.Once);
        }

        [Fact]
        public void AddMessage_ShouldAddMessageToQueue()
        {
            PostQueueMessage message = new() { MessageText = "testMessage" };

            MessageList result = _messageService.AddMessage(QueueName, message, 0, 0);

            Assert.Single(result.QueueMessagesList);
            MockFifoService.Verify(service => service.AddMessage(QueueName, It.IsAny<QueueMessage>()), Times.Once);
        }

        [Fact]
        public async Task DeleteMessage_ShouldDeleteMessageFromQueue()
        {
            Guid messageId = Guid.NewGuid();
            string popReceipt = Guid.NewGuid().ToString();

            await _messageService.DeleteMessage(QueueName, messageId, popReceipt);

            MockFifoService.Verify(service => service.DeleteMessage(QueueName, messageId, popReceipt), Times.Once);
        }

        [Fact]
        public void GetQueues_ShouldReturnListOfQueues()
        {
            List<string> queues = ["queue1", "queue2", "queue3"];
            MockFifoService.Setup(service => service.GetQueues()).Returns(queues);
            MockAuthenticator.Setup(a => a.Authenticate(It.IsAny<HttpRequest>())).Returns(true);

            string result = _messageService.GetQueues();

            Assert.Equal(
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><EnumerationResults ServiceEndpoint=\"http://127.0.0.1:10001/devstoreaccount1\"><Prefix/><MaxResults>5000</MaxResults><Queues><Queue><Name>queue1</Name><Metadata/></Queue><Queue><Name>queue2</Name><Metadata/></Queue><Queue><Name>queue3</Name><Metadata/></Queue></Queues><NextMarker/></EnumerationResults>",
                result);
            MockFifoService.Verify(service => service.GetQueues(), Times.Once);
        }

        [Fact]
        public void GetMessages_ShouldReturnListOfMessages()
        {
            List<QueueMessage?> messages = [new QueueMessage(), new QueueMessage()];
            MockFifoService.Setup(service => service.GetMessages(QueueName, It.IsAny<int>())).Returns(messages);

            MessageList result = _messageService.GetMessages(QueueName, 2);

            Assert.Equal(2, result.QueueMessagesList.Count);
            MockFifoService.Verify(service => service.GetMessages(QueueName, 2), Times.Once);
        }

        [Fact]
        public void GetMessage_ShouldReturnMessage()
        {
            QueueMessage message = new()
            {
                MessageId = Guid.NewGuid(),
                InsertionTime = DateTime.UtcNow,
                ExpirationTime = DateTime.UtcNow.AddDays(7),
                PopReceipt = Guid.NewGuid().ToString(),
                TimeNextVisible = DateTime.UtcNow.AddSeconds(10),
                DequeueCount = 0,
                MessageText = "testMessage"
            };

            MockFifoService.Setup(service => service.GetMessage(QueueName)).Returns(message);

            QueueMessage result = _messageService.GetMessage(QueueName) ?? new QueueMessage();

            Assert.Equal(message, result);
            MockFifoService.Verify(service => service.GetMessage(QueueName), Times.Once);
        }
    }
}