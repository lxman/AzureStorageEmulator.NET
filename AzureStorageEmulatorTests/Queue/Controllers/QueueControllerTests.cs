using AzureStorageEmulator.NET.Controllers;
using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;

namespace AzureStorageEmulatorTests.Queue.Controllers
{
    public class QueueControllerTests
    {
        private static readonly Mock<IQueueService> MockMessageService = new();
        private readonly QueueController _controller = new(MockMessageService.Object);
        private const string QueueName = "testQueue";
        private const int NumOfMessages = 5;
        private readonly QueueMessage _message = new() { MessageText = "testMessage" };
        private readonly Guid _messageId = Guid.NewGuid();
        private const int VisibilityTimeout = 60;
        private const int MessageTtl = 3600;
        private const int Timeout = 30;
        private const string PopReceipt = "testPopReceipt";

        [Fact]
        public async Task CreateQueue_ReturnsCreated()
        {
            MockMessageService.Setup(x => x.CreateQueue(QueueName, It.IsAny<HttpRequest>())).ReturnsAsync(new StatusCodeResult(201));

            IActionResult result = await _controller.CreateQueue(QueueName);

            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public void ListQueues_ReturnsOk()
        {
            MockMessageService.Setup(x => x.ListQueues(It.IsAny<HttpRequest>())).Returns(new OkObjectResult(new List<AzureStorageEmulator.NET.Queue.Models.Queue>()));

            IActionResult result = _controller.ListQueues();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void DeleteQueue_ReturnsOk()
        {
            MockMessageService.Setup(x => x.DeleteQueue(QueueName, It.IsAny<HttpRequest>())).Returns(new OkResult());

            IActionResult result = _controller.DeleteQueue(QueueName);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetAllMessages_ReturnsOk()
        {
            MockMessageService.Setup(x => x.GetAllMessages(QueueName, It.IsAny<HttpRequest>())).ReturnsAsync(new OkObjectResult(new List<Message>()));

            IActionResult result = await _controller.GetAllMessages(QueueName);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task PostMessage_ReturnsCreated()
        {
            MockMessageService.Setup(x => x.PostMessage(QueueName, _message, VisibilityTimeout, MessageTtl, Timeout, It.IsAny<HttpRequest>())).ReturnsAsync(new StatusCodeResult(201));

            IActionResult result = await _controller.PostMessage(QueueName, _message, VisibilityTimeout, MessageTtl, Timeout);

            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task DeleteMessage_ReturnsOk()
        {
            MockMessageService.Setup(x => x.DeleteMessage(QueueName, _messageId, PopReceipt, It.IsAny<HttpRequest>())).ReturnsAsync(new OkResult());

            IActionResult result = await _controller.DeleteMessage(QueueName, _messageId, PopReceipt);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteMessages_ReturnsOk()
        {
            MockMessageService.Setup(x => x.DeleteMessages(QueueName, It.IsAny<HttpRequest>())).Returns(new OkResult());

            IActionResult result = _controller.DeleteMessages(QueueName);

            Assert.IsType<OkResult>(result);
        }
    }
}