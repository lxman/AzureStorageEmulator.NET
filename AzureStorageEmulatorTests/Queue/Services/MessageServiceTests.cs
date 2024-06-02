using AzureStorageQueue.NET.Models;
using AzureStorageQueue.NET.Services;
using Moq;

namespace AzureStorageEmulatorTests.Queue.Services
{
    public class MessageServiceTests
    {
        private const string QueueName = "testQueue";
        private static readonly Mock<IFifoService> _mockFifoService = new();
        private readonly IMessageService _messageService = new MessageService(_mockFifoService.Object);

        [Fact]
        public void AddQueue_ShouldAddQueueSuccessfully()
        {
            _mockFifoService.Setup(service => service.AddQueue(It.IsAny<string>())).Returns(true);

            bool result = _messageService.AddQueue(QueueName);

            Assert.True(result);
            _mockFifoService.Verify(service => service.AddQueue(QueueName), Times.Once);
        }

        [Fact]
        public void DeleteQueue_ShouldRemoveQueueSuccessfully()
        {
            _messageService.DeleteQueue(QueueName);

            _mockFifoService.Verify(service => service.DeleteQueue(QueueName), Times.Once);
        }

        [Fact]
        public void AddMessage_ShouldAddMessageToQueue()
        {
            var message = new PostQueueMessage { MessageText = "testMessage" };

            MessageList result = _messageService.AddMessage(QueueName, message, 0, 0);

            Assert.Single(result.QueueMessagesList);
            _mockFifoService.Verify(service => service.AddMessage(QueueName, It.IsAny<QueueMessage>()), Times.Once);
        }

        [Fact]
        public async Task DeleteMessage_ShouldDeleteMessageFromQueue()
        {
            var messageId = Guid.NewGuid();
            var popReceipt = Guid.NewGuid().ToString();

            await _messageService.DeleteMessage(QueueName, messageId, popReceipt);

            _mockFifoService.Verify(service => service.DeleteMessage(QueueName, messageId, popReceipt), Times.Once);
        }

        [Fact]
        public void GetQueues_ShouldReturnListOfQueues()
        {
            List<string> queues = ["queue1", "queue2", "queue3"];
            _mockFifoService.Setup(service => service.GetQueues()).Returns(queues);

            string result = _messageService.GetQueues();

            Assert.Equal(
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><QueueMessagesList><EnumerationResults ServiceEndpoint=\"http://127.0.0.1:10001/devstoreaccount1\"><Prefix/><MaxResults>5000</MaxResults><Queues><Queue><Name>queue1</Name><Metadata/></Queue><Queue><Name>queue2</Name><Metadata/></Queue><Queue><Name>queue3</Name><Metadata/></Queue></Queues><NextMarker/></EnumerationResults></QueueMessagesList>",
                result);
            _mockFifoService.Verify(service => service.GetQueues(), Times.Once);
        }

        [Fact]
        public void GetMessages_ShouldReturnListOfMessages()
        {
            List<QueueMessage?> messages = [new QueueMessage(), new QueueMessage()];
            _mockFifoService.Setup(service => service.GetMessages(QueueName, It.IsAny<int>())).Returns(messages);

            MessageList result = _messageService.GetMessages(QueueName, 2);

            Assert.Equal(2, result.QueueMessagesList.Count);
            _mockFifoService.Verify(service => service.GetMessages(QueueName, 2), Times.Once);
        }

        [Fact]
        public void GetMessage_ShouldReturnMessage()
        {
            var message = new QueueMessage
            {
                MessageId = Guid.NewGuid(),
                InsertionTime = DateTime.UtcNow,
                ExpirationTime = DateTime.UtcNow.AddDays(7),
                PopReceipt = Guid.NewGuid().ToString(),
                TimeNextVisible = DateTime.UtcNow.AddSeconds(10),
                DequeueCount = 0,
                MessageText = "testMessage"
            };

            _mockFifoService.Setup(service => service.GetMessage(QueueName)).Returns(message);

            QueueMessage result = _messageService.GetMessage(QueueName) ?? new QueueMessage();

            Assert.Equal(message, result);
            _mockFifoService.Verify(service => service.GetMessage(QueueName), Times.Once);
        }
    }
}
