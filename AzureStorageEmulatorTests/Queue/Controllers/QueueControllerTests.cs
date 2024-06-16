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
            MockMessageService.Setup(x => x.CreateQueueAsync(QueueName, It.IsAny<HttpContext>())).ReturnsAsync(new StatusCodeResult(201));

            IActionResult result = await _controller.CreateQueue(QueueName);

            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task ListQueues_ReturnsOk()
        {
            MockMessageService.Setup(x => x.ListQueuesAsync(It.IsAny<HttpContext>())).ReturnsAsync(new OkObjectResult(new List<AzureStorageEmulator.NET.Queue.Models.Queue>()));

            IActionResult result = await _controller.ListQueues();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteQueue_ReturnsOk()
        {
            MockMessageService.Setup(x => x.DeleteQueueAsync(QueueName, It.IsAny<HttpContext>())).ReturnsAsync(new OkResult());

            IActionResult result = await _controller.DeleteQueue(QueueName);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetAllMessages_ReturnsOk()
        {
            MockMessageService.Setup(x => x.GetQueueMetadataAsync(QueueName, It.IsAny<HttpContext>())).ReturnsAsync(new OkObjectResult(new List<Message>()));

            IActionResult result = await _controller.GetQueueMetadata(QueueName);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task PostMessage_ReturnsCreated()
        {
            MockMessageService.Setup(x => x.PostMessageAsync(QueueName, _message, VisibilityTimeout, MessageTtl, Timeout, It.IsAny<HttpContext>())).ReturnsAsync(new StatusCodeResult(201));

            IActionResult result = await _controller.PostMessage(QueueName, _message, VisibilityTimeout, MessageTtl, Timeout);

            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task DeleteMessage_ReturnsOk()
        {
            MockMessageService.Setup(x => x.DeleteMessageAsync(QueueName, _messageId, PopReceipt, It.IsAny<HttpContext>())).ReturnsAsync(new OkResult());

            IActionResult result = await _controller.DeleteMessage(QueueName, _messageId, PopReceipt);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteMessages_ReturnsOk()
        {
            MockMessageService.Setup(x => x.ClearMessagesAsync(QueueName, It.IsAny<HttpContext>())).ReturnsAsync(new OkResult());

            IActionResult result = await _controller.ClearMessages(QueueName);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task CreateQueue_ProcessingTooLong_ReturnsGatewayTimeout()
        {
            MockMessageService.Setup(x => x.CreateQueueAsync(QueueName, It.IsAny<HttpContext>()))
                              .Returns(Task.Delay(2000).ContinueWith<IActionResult>(_ => new StatusCodeResult(504)));

            IActionResult result = await _controller.CreateQueue(QueueName);

            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(504, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task PostMessage_InspectQueryString_UsesDefaultValues()
        {
            MockMessageService.Setup(x => x.PostMessageAsync(QueueName, _message, 0, 0, 0, It.IsAny<HttpContext>()))
                              .ReturnsAsync(new StatusCodeResult(201));

            IActionResult resultWithDefaults = await _controller.PostMessage(QueueName, _message, 0, 0, 0);

            IActionResult resultWithMethodDefaults = await _controller.PostMessage(QueueName, _message);

            Assert.IsType<StatusCodeResult>(resultWithDefaults);
            Assert.Equal(201, ((StatusCodeResult)resultWithDefaults).StatusCode);
            Assert.IsType<StatusCodeResult>(resultWithMethodDefaults);
            Assert.Equal(201, ((StatusCodeResult)resultWithMethodDefaults).StatusCode);
        }

        [Fact]
        public async Task PostMessage_InspectQueryString_UsesProvidedValues()
        {
            const int customVisibilityTimeout = 30;
            const int customMessageTtl = 120;
            const int customTimeout = 15;

            MockMessageService.Setup(x => x.PostMessageAsync(QueueName, _message, customVisibilityTimeout, customMessageTtl, customTimeout, It.IsAny<HttpContext>()))
                              .ReturnsAsync(new StatusCodeResult(201));

            IActionResult result = await _controller.PostMessage(QueueName, _message, customVisibilityTimeout, customMessageTtl, customTimeout);

            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task GetMessages_ReturnsOk()
        {
            MockMessageService.Setup(x => x.GetMessagesAsync(QueueName, It.IsAny<HttpContext>())).ReturnsAsync(new OkObjectResult(new List<QueueMessage>()));

            IActionResult result = await _controller.GetMessages(QueueName);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetMessages_ReturnsNotFound_WhenQueueDoesNotExist()
        {
            MockMessageService.Setup(x => x.GetMessagesAsync(QueueName, It.IsAny<HttpContext>())).ReturnsAsync(new NotFoundResult());

            IActionResult result = await _controller.GetMessages(QueueName);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}