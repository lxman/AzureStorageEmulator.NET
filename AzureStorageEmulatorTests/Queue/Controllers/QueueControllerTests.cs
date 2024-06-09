using AzureStorageEmulator.NET.Controllers;
using AzureStorageEmulator.NET.Queue;
using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using XmlTransformer.Queue.Models;

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
            MockMessageService.Setup(service => service.Authenticate(It.IsAny<HttpRequest>())).Returns(true);

            IActionResult result = await _controller.CreateQueue(QueueName);

            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task PostMessage_ShouldReturn201_WhenMessageIsPosted()
        {
            PostQueueMessage message = new() { MessageText = "testMessage" };
            MockMessageService
                .Setup(ms => ms.AddMessage(QueueName, It.IsAny<PostQueueMessage>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new MessageList());
            MockMessageService.Setup(service => service.Authenticate(It.IsAny<HttpRequest>())).Returns(true);

            IActionResult result = await _controller.PostMessage(QueueName, message);

            ContentResult contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal(201, contentResult.StatusCode);
        }

        // Add more tests as needed for other methods in QueueController.
    }
}