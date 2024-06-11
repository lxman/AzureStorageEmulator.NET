using AzureStorageEmulator.NET.Queue.Services;
using XmlTransformer.Queue.Models;

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
            Assert.Equal(QueueName, _service.GetQueues().First().Name);
        }

        [Fact]
        public void DeleteQueue_ShouldRemoveQueueSuccessfully()
        {
            _service.AddQueue(QueueName);

            _service.DeleteQueue(QueueName);

            Assert.DoesNotContain(new XmlTransformer.Queue.Models.Queue { Name = QueueName }, _service.GetQueues());
        }

        // TODO: Fix this test
        //[Fact]
        //public void AddMessage_ShouldAddMessageToQueue()
        //{
        //    _service.AddQueue(QueueName);
        //    QueueMessage message = new() { MessageId = Guid.NewGuid(), PopReceipt = "testReceipt" };

        //    _service.AddMessage(QueueName, message);

        //    List<QueueMessage?>? messages = _service.GetMessages(QueueName, 1);
        //    Assert.NotNull(messages);
        //    Assert.Single(messages);
        //    Assert.Equal(message.MessageId, messages.First()!.MessageId);
        //}

        [Fact]
        public async Task DeleteMessage_ShouldDeleteMessageFromQueue()
        {
            _service.AddQueue(QueueName);
            QueueMessage message = new() { MessageId = Guid.NewGuid(), PopReceipt = "testReceipt" };
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

            List<XmlTransformer.Queue.Models.Queue> queues = _service.GetQueues();

            Assert.NotNull(queues);
            Assert.Equal(QueueName, queues.First().Name);
        }

        // TODO: Fix this test
        //[Fact]
        //public void GetMessages_ShouldReturnListOfMessagesFromQueue()
        //{
        //    _service.AddQueue(QueueName);
        //    QueueMessage message1 = new() { MessageId = Guid.NewGuid(), PopReceipt = "testReceipt1" };
        //    QueueMessage message2 = new() { MessageId = Guid.NewGuid(), PopReceipt = "testReceipt2" };
        //    _service.AddMessage(QueueName, message1);
        //    _service.AddMessage(QueueName, message2);

        //    List<QueueMessage?>? messages = _service.GetMessages(QueueName, 2);

        //    Assert.NotNull(messages);
        //    Assert.Equal(2, messages.Count);
        //    Assert.Contains(message1.MessageId, messages.Select(m => m?.MessageId));
        //    Assert.Contains(message2.MessageId, messages.Select(m => m?.MessageId));
        //}
    }
}