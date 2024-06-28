using System.Diagnostics.CodeAnalysis;
using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Services;
using AzureStorageEmulator.NET.Results;

namespace AzureStorageEmulatorTests.Queue.Services
{
    [ExcludeFromCodeCoverage]
    public class ConcurrentQueueServiceTests
    {
        private readonly CancellationToken _cancellationToken = new();
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
            IMethodResult result = await _service.PutMessageAsync("testQueue", _message, _cancellationToken);
            Assert.IsType<ResultOk>(result);

            // Attempt to add a message to a non-existing queue
            result = await _service.PutMessageAsync("nonExistingQueue", _message, _cancellationToken);
            Assert.IsType<ResultNotFound>(result);
        }

        [Fact]
        public async Task TestGetMessagesAsync()
        {
            await _service.CreateQueueAsync("testQueue");
            await _service.PutMessageAsync("testQueue", _message, _cancellationToken);

            // Test peeking without removing
            (IMethodResult methodResult, List<QueueMessage>? messages) result = await _service.GetMessagesAsync("testQueue", 1, true, _cancellationToken);
            Assert.IsType<ResultOk>(result.methodResult);
            Assert.NotNull(result.messages);
            Assert.Single(result.messages);

            result = await _service.GetMessagesAsync("testQueue", 1);
            Assert.IsType<ResultOk>(result.methodResult);
            Assert.NotNull(result.messages);
            Assert.Single(result.messages);
            Assert.Equal("Hello", result.messages.First().MessageText);

            // Test non-existing queue
            result = await _service.GetMessagesAsync("nonExistingQueue", 1);
            Assert.IsType<ResultNotFound>(result.methodResult);
            Assert.Null(result.messages);
        }

        [Fact]
        public async Task TestDeleteMessageAsync()
        {
            await _service.CreateQueueAsync("testQueue");
            await _service.PutMessageAsync("testQueue", _message, _cancellationToken);
            (IMethodResult methodResult, QueueMessage? deletedMessage) result = await _service.DeleteMessageAsync("testQueue", _message.MessageId, _message.PopReceipt, _cancellationToken);
            Assert.IsType<ResultOk>(result.methodResult);
            Assert.NotNull(result.deletedMessage);
            Assert.Equal("Hello", result.deletedMessage.MessageText);

            // Test deleting non-existing message
            result = await _service.DeleteMessageAsync("testQueue", Guid.NewGuid(), "fakeReceipt", _cancellationToken);
            Assert.IsType<ResultNotFound>(result.methodResult);
            Assert.Null(result.deletedMessage);
        }

        [Fact]
        public async Task TestClearMessagesAsync()
        {
            await _service.CreateQueueAsync("testQueue");
            await _service.PutMessageAsync("testQueue", _message, _cancellationToken);
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
            await _service.PutMessageAsync("testQueue", _message, _cancellationToken);
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
            await _service.PutMessageAsync(queueName, new QueueMessage { MessageText = "Test", TimeToLive = Convert.ToInt32(TimeSpan.FromDays(1).TotalSeconds) }, _cancellationToken);

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
            await _service.PutMessageAsync("queueWithExpiredMessage", new QueueMessage { MessageText = "Expired", TimeToLive = Convert.ToInt32(TimeSpan.FromDays(-1).TotalSeconds) }, _cancellationToken);
            await _service.CreateQueueAsync("queueWithValidMessage");
            await _service.PutMessageAsync("queueWithValidMessage", new QueueMessage { MessageText = "Valid", TimeToLive = Convert.ToInt32(TimeSpan.FromDays(1).TotalSeconds) }, _cancellationToken);

            List<AzureStorageEmulator.NET.Queue.Models.Queue> queues = await _service.ListQueuesAsync();

            Assert.Equal(2, queues.Count);
            Assert.Contains(queues, q => q.Name == "queueWithExpiredMessage");
            Assert.Contains(queues, q => q.Name == "queueWithValidMessage");
        }
    }
}