using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Services;

namespace AzureStorageEmulatorTests.Queue.Services
{
    public class ConcurrentQueueServiceTests
    {
        private readonly ConcurrentQueueService _service = new();

        private readonly QueueMessage _message = new()
        {
            MessageText = "Hello",
            ExpirationTime = DateTime.UtcNow.AddDays(7),
            MessageId = Guid.NewGuid(),
            PopReceipt = Guid.NewGuid().ToString()
        };

        [Fact]
        public async Task TestAddQueueAsync()
        {
            bool result = await _service.AddQueueAsync("testQueue");
            Assert.True(result);

            // Attempt to add a queue with the same name
            result = await _service.AddQueueAsync("testQueue");
            Assert.False(result);
        }

        [Fact]
        public async Task TestDeleteQueueAsync()
        {
            await _service.AddQueueAsync("testQueue");
            bool result = await _service.DeleteQueueAsync("testQueue");
            Assert.True(result);

            // Attempt to delete a non-existing queue
            result = await _service.DeleteQueueAsync("nonExistingQueue");
            Assert.False(result);
        }

        [Fact]
        public async Task TestAddMessageAsync()
        {
            await _service.AddQueueAsync("testQueue");
            bool result = await _service.AddMessageAsync("testQueue", _message);
            Assert.True(result);

            // Attempt to add a message to a non-existing queue
            result = await _service.AddMessageAsync("nonExistingQueue", _message);
            Assert.False(result);
        }

        [Fact]
        public async Task TestGetMessagesAsync()
        {
            await _service.AddQueueAsync("testQueue");
            await _service.AddMessageAsync("testQueue", _message);

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
            await _service.AddQueueAsync("testQueue");
            await _service.AddMessageAsync("testQueue", _message);
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
            await _service.AddQueueAsync("testQueue");
            await _service.AddMessageAsync("testQueue", _message);
            int statusCode = await _service.ClearMessagesAsync("testQueue");
            Assert.Equal(204, statusCode);

            // Test clearing non-existing queue
            statusCode = await _service.ClearMessagesAsync("nonExistingQueue");
            Assert.Equal(404, statusCode);
        }

        [Fact]
        public async Task TestMessageCountAsync()
        {
            await _service.AddQueueAsync("testQueue");
            await _service.AddMessageAsync("testQueue", _message);
            int? count = await _service.MessageCountAsync("testQueue");
            Assert.Equal(1, count);

            // Test message count for non-existing queue
            count = await _service.MessageCountAsync("nonExistingQueue");
            Assert.Null(count);
        }
    }
}