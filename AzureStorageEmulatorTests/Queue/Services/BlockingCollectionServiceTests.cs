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
            Assert.Equal(QueueName, _service.GetQueues().First().Name);
        }

        [Fact]
        public void DeleteQueue_ShouldRemoveQueueSuccessfully()
        {
            _service.AddQueue(QueueName);

            _service.DeleteQueueAsync(QueueName);

            Assert.DoesNotContain(new AzureStorageEmulator.NET.Queue.Models.Queue { Name = QueueName }, _service.GetQueues());
        }

        [Fact]
        public void GetQueues_ShouldReturnListOfQueues()
        {
            _service.AddQueue(QueueName);

            List<AzureStorageEmulator.NET.Queue.Models.Queue> queues = _service.GetQueues();

            Assert.NotNull(queues);
            Assert.Equal(QueueName, queues.First().Name);
        }
    }
}