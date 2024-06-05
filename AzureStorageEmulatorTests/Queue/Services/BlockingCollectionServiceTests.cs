using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Services;

namespace AzureStorageEmulatorTests.Queue.Services
{
    public class BlockingCollectionServiceTests
    {
        private const string QueueName = "testQueue";
        private readonly BlockingCollectionService _service = new();

        [Fact]
        public void AddQueue_ShouldAddQueueSuccessfully()
        {
            bool result = _service.AddQueue(QueueName);

            Assert.True(result);
            Assert.Contains(QueueName, _service.GetQueues());
        }

        [Fact]
        public void DeleteQueue_ShouldRemoveQueueSuccessfully()
        {
            _service.AddQueue(QueueName);

            _service.DeleteQueue(QueueName);

            Assert.DoesNotContain(QueueName, _service.GetQueues());
        }

        [Fact]
        public void AddMessage_ShouldAddMessageToQueue()
        {
            _service.AddQueue(QueueName);
            var message = new QueueMessage { MessageId = Guid.NewGuid(), PopReceipt = "testReceipt" };

            _service.AddMessage(QueueName, message);

            List<QueueMessage?>? messages = _service.GetMessages(QueueName, 1);
            Assert.NotNull(messages);
            Assert.Single(messages);
            Assert.Equal(message.MessageId, messages.First().MessageId);
        }

        [Fact]
        public void GetMessage_ShouldReturnMessageFromQueue()
        {
            _service.AddQueue(QueueName);
            var message = new QueueMessage { MessageId = Guid.NewGuid(), PopReceipt = "testReceipt" };
            _service.AddMessage(QueueName, message);

            QueueMessage? result = _service.GetMessage(QueueName);

            Assert.NotNull(result);
            Assert.Equal(message.MessageId, result.MessageId);
        }

        [Fact]
        public async Task DeleteMessage_ShouldDeleteMessageFromQueue()
        {
            _service.AddQueue(QueueName);
            var message = new QueueMessage { MessageId = Guid.NewGuid(), PopReceipt = "testReceipt" };
            _service.AddMessage(QueueName, message);

            QueueMessage? deletedMessage = await _service.DeleteMessage(QueueName, message.MessageId, message.PopReceipt);

            Assert.NotNull(deletedMessage);
            Assert.Equal(message.MessageId, deletedMessage.MessageId);
            Assert.DoesNotContain(deletedMessage, _service.GetMessages(QueueName, 1) ?? []);
        }

        [Fact]
        public void GetQueues_ShouldReturnListOfQueues()
        {
            _service.AddQueue(QueueName);

            List<string> queues = _service.GetQueues();

            Assert.NotNull(queues);
            Assert.Contains(QueueName, queues);
        }

        [Fact]
        public void GetMessages_ShouldReturnListOfMessagesFromQueue()
        {
            _service.AddQueue(QueueName);
            var message1 = new QueueMessage { MessageId = Guid.NewGuid(), PopReceipt = "testReceipt1" };
            var message2 = new QueueMessage { MessageId = Guid.NewGuid(), PopReceipt = "testReceipt2" };
            _service.AddMessage(QueueName, message1);
            _service.AddMessage(QueueName, message2);

            List<QueueMessage?>? messages = _service.GetMessages(QueueName, 2);

            Assert.NotNull(messages);
            Assert.Equal(2, messages.Count);
            Assert.Contains(message1.MessageId, messages.Select(m => m?.MessageId));
            Assert.Contains(message2.MessageId, messages.Select(m => m?.MessageId));
        }
    }
}