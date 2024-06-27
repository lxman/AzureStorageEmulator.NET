using System.Diagnostics.CodeAnalysis;
using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Services;

namespace AzureStorageEmulatorTests.Queue.Services
{
    [ExcludeFromCodeCoverage]
    public class ConcurrentQueueServiceTests
    {
        private readonly ConcurrentQueueService _service = new();

        private readonly QueueMessage _message = new()
        {
            MessageText = "Hello",
            TimeToLive = Convert.ToInt32(TimeSpan.FromDays(7).TotalSeconds),
            MessageId = Guid.NewGuid(),
            PopReceipt = Guid.NewGuid().ToString()
        };

        [Fact]
        public async Task TestAddQueueAsync()
        {
            bool result = await _service.CreateQueueAsync("testQueue");
            Assert.True(result);

            // Attempt to add a queue with the same name
            result = await _service.CreateQueueAsync("testQueue");
            Assert.False(result);
        }

        [Fact]
        public async Task TestDeleteQueueAsync()
        {
            await _service.CreateQueueAsync("testQueue");
            bool result = await _service.DeleteQueueAsync("testQueue");
            Assert.True(result);

            // Attempt to delete a non-existing queue
            result = await _service.DeleteQueueAsync("nonExistingQueue");
            Assert.False(result);
        }

        [Fact]
        public async Task TestAddMessageAsync()
        {
            await _service.CreateQueueAsync("testQueue");
            bool result = await _service.PutMessageAsync("testQueue", _message);
            Assert.True(result);

            // Attempt to add a message to a non-existing queue
            result = await _service.PutMessageAsync("nonExistingQueue", _message);
            Assert.False(result);
        }

        [Fact]
        public async Task TestGetMessagesAsync()
        {
            await _service.CreateQueueAsync("testQueue");
            await _service.PutMessageAsync("testQueue", _message);

            // Test peeking without removing
            List<QueueMessage>? messages = await _service.GetMessagesAsync("testQueue", 1, peekOnly: true);
            Assert.NotNull(messages);
            Assert.Single(messages);

            messages = await _service.GetMessagesAsync("testQueue", 1);
            Assert.NotNull(messages);
            Assert.Single(messages);
            Assert.Equal("Hello", messages.First().MessageText);

            // Test non-existing queue
            messages = await _service.GetMessagesAsync("nonExistingQueue");
            Assert.Null(messages);
        }

        [Fact]
        public async Task TestDeleteMessageAsync()
        {
            await _service.CreateQueueAsync("testQueue");
            await _service.PutMessageAsync("testQueue", _message);
            QueueMessage? deletedMessage = await _service.DeleteMessageAsync("testQueue", _message.MessageId, _message.PopReceipt);
            Assert.NotNull(deletedMessage);
            Assert.Equal("Hello", deletedMessage.MessageText);

            // Test deleting non-existing message
            deletedMessage = await _service.DeleteMessageAsync("testQueue", Guid.NewGuid(), "fakeReceipt");
            Assert.Null(deletedMessage);
        }

        [Fact]
        public async Task TestClearMessagesAsync()
        {
            await _service.CreateQueueAsync("testQueue");
            await _service.PutMessageAsync("testQueue", _message);
            int statusCode = await _service.ClearMessagesAsync("testQueue");
            Assert.Equal(204, statusCode);

            // Test clearing non-existing queue
            statusCode = await _service.ClearMessagesAsync("nonExistingQueue");
            Assert.Equal(404, statusCode);
        }

        [Fact]
        public async Task TestMessageCountAsync()
        {
            await _service.CreateQueueAsync("testQueue");
            await _service.PutMessageAsync("testQueue", _message);
            int? count = await _service.MessageCountAsync("testQueue");
            Assert.Equal(1, count);

            // Test message count for non-existing queue
            count = await _service.MessageCountAsync("nonExistingQueue");
            Assert.Null(count);
        }

        [Fact]
        public async Task GetQueueMetadataAsync_WithExistingQueue_ReturnsCorrectMetadata()
        {
            const string queueName = "existingQueue";
            await _service.CreateQueueAsync(queueName);
            await _service.PutMessageAsync(queueName, new QueueMessage { MessageText = "Test", TimeToLive = Convert.ToInt32(TimeSpan.FromDays(1).TotalSeconds) });

            AzureStorageEmulator.NET.Queue.Models.Queue? queueMetadata = await _service.GetQueueMetadataAsync(queueName);

            Assert.NotNull(queueMetadata);
            Assert.Equal(queueName, queueMetadata.Name);
            Assert.Equal(1, queueMetadata.MessageCount);
        }

        [Fact]
        public async Task GetQueueMetadataAsync_WithNonExistingQueue_ReturnsNull()
        {
            AzureStorageEmulator.NET.Queue.Models.Queue? queueMetadata = await _service.GetQueueMetadataAsync("nonExistingQueue");
            Assert.Null(queueMetadata);
        }

        [Fact]
        public async Task GetQueuesAsync_WithNoQueues_ReturnsEmptyList()
        {
            List<AzureStorageEmulator.NET.Queue.Models.Queue> queues = await _service.ListQueuesAsync();
            Assert.Empty(queues);
        }

        [Fact]
        public async Task GetQueuesAsync_AfterAddingQueues_ReturnsAllQueuesInOrder()
        {
            await _service.CreateQueueAsync("queue1");
            await _service.CreateQueueAsync("queue2");

            List<AzureStorageEmulator.NET.Queue.Models.Queue> queues = await _service.ListQueuesAsync();

            Assert.Equal(2, queues.Count);
            Assert.Equal("queue1", queues[0].Name);
            Assert.Equal("queue2", queues[1].Name);
        }

        [Fact]
        public async Task GetQueuesAsync_AfterAddingQueuesAndMessages_ExcludesExpiredMessagesFromQueueListing()
        {
            await _service.CreateQueueAsync("queueWithExpiredMessage");
            await _service.PutMessageAsync("queueWithExpiredMessage", new QueueMessage { MessageText = "Expired", TimeToLive = Convert.ToInt32(TimeSpan.FromDays(-1).TotalSeconds) });
            await _service.CreateQueueAsync("queueWithValidMessage");
            await _service.PutMessageAsync("queueWithValidMessage", new QueueMessage { MessageText = "Valid", TimeToLive = Convert.ToInt32(TimeSpan.FromDays(1).TotalSeconds) });

            List<AzureStorageEmulator.NET.Queue.Models.Queue> queues = await _service.ListQueuesAsync();

            Assert.Equal(2, queues.Count);
            Assert.Contains(queues, q => q.Name == "queueWithExpiredMessage");
            Assert.Contains(queues, q => q.Name == "queueWithValidMessage");
        }
    }
}