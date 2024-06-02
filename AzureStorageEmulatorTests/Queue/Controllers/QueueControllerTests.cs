using AzureStorageQueue.NET;
using AzureStorageQueue.NET.Controllers;
using AzureStorageQueue.NET.Models;
using AzureStorageQueue.NET.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AzureStorageEmulatorTests.Queue.Controllers
{
    public class QueueControllerTests
    {
        private static readonly Mock<IMessageService> MockMessageService = new();
        private static readonly Mock<IQueueSettings> MockSettings = new();
        private readonly QueueController _controller = new(MockMessageService.Object, MockSettings.Object);
        private const string QueueName = "testQueue";

        [Fact]
        public async Task CreateQueue_ShouldReturn201_WhenQueueIsCreated()
        {
            MockMessageService.Setup(service => service.AddQueue(It.IsAny<string>())).Returns(true);

            IActionResult result = await _controller.CreateQueue(QueueName);

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task PostMessage_ShouldReturn201_WhenMessageIsPosted()
        {
            var message = new PostQueueMessage { MessageText = "testMessage" };
            MockMessageService
                .Setup(ms => ms.AddMessage(QueueName, It.IsAny<PostQueueMessage>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new MessageList());

            IActionResult result = await _controller.PostMessage(QueueName, message);

            var contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal(201, contentResult.StatusCode);
        }

        // Add more tests as needed for other methods in QueueController.
    }
}