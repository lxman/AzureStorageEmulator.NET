using AzureStorageEmulator.NET.Authentication;
using AzureStorageEmulator.NET.Queue;
using AzureStorageEmulator.NET.Queue.Services;
using AzureStorageEmulator.NET.XmlSerialization.Queue;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AzureStorageEmulatorTests.Queue.Services
{
    public class MessageServiceTests
    {
        private const string QueueName = "testQueue";
        private static readonly Mock<IFifoService> MockFifoService = new();
        private static readonly Mock<IAuthenticator> MockAuthenticator = new();
        private static readonly Mock<IQueueSettings> MockQueueSettings = new();
        private static readonly EnumerationResultsSerializer EnumerationResultsSerializer = new();
        private static readonly MessageListSerializer MessageListSerializer = new();
        private readonly MessageService _messageService = new(
            MockFifoService.Object,
            MockAuthenticator.Object,
            EnumerationResultsSerializer,
            MessageListSerializer,
            MockQueueSettings.Object);
        private readonly Mock<HttpRequest> _mockRequest = new();

        public MessageServiceTests()
        {
            MockAuthenticator.Setup(a => a.Authenticate(It.IsAny<HttpRequest>())).Returns(true);
        }

        [Fact]
        public async Task TestCreateQueue()
        {
            _mockRequest.Setup(r => r.Query).Returns(new QueryCollection());
            MockFifoService.Setup(f => f.AddQueue(QueueName)).Returns(true);

            IActionResult result = await _messageService.CreateQueue(QueueName, _mockRequest.Object);

            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, ((StatusCodeResult)result).StatusCode);
        }

        // TODO: Fix this test
        //[Fact]
        //public async Task TestGetMessages()
        //{
        //    const int numOfMessages = 5;
        //    _mockRequest.Setup(r => r.Query).Returns(new QueryCollection());
        //    MockFifoService.Setup(f => f.GetMessages(QueueName, numOfMessages)).Returns([]);

        //    IActionResult result = await _messageService.GetMessages(QueueName, numOfMessages, _mockRequest.Object);

        //    Assert.IsType<ContentResult>(result);
        //    Assert.Equal(200, ((ContentResult)result).StatusCode);
        //}

        [Fact]
        public async Task TestGetAllMessages()
        {
            _mockRequest.Setup(r => r.Query).Returns(new QueryCollection());
            MockFifoService.Setup(f => f.GetAllMessages(QueueName)).Returns([]);

            IActionResult result = await _messageService.GetAllMessages(QueueName, _mockRequest.Object);

            Assert.IsType<ContentResult>(result);
            Assert.Equal(200, ((ContentResult)result).StatusCode);
        }

        // TODO: Fix this test
        //[Fact]
        //public async Task TestPostMessage()
        //{
        //    QueueMessage message = new() { MessageText = "test message" };
        //    const int visibilityTimeout = 10;
        //    const int messageTtl = 100;
        //    const int timeout = 10;
        //    _mockRequest.Setup(r => r.Query).Returns(new QueryCollection());

        //    IActionResult result = await _messageService.PostMessage(QueueName, message, visibilityTimeout, messageTtl, timeout, _mockRequest.Object);

        //    Assert.IsType<ContentResult>(result);
        //    Assert.Equal(201, ((ContentResult)result).StatusCode);
        //}

        [Fact]
        public async Task TestDeleteMessage()
        {
            Guid messageId = Guid.NewGuid();
            const string popReceipt = "popReceipt";
            _mockRequest.Setup(r => r.Query).Returns(new QueryCollection());

            IActionResult result = await _messageService.DeleteMessage(QueueName, messageId, popReceipt, _mockRequest.Object);

            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public void TestDeleteQueue()
        {
            _mockRequest.Setup(r => r.Query).Returns(new QueryCollection());

            IActionResult result = _messageService.DeleteQueue(QueueName, _mockRequest.Object);

            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public void TestDeleteMessages()
        {
            _mockRequest.Setup(r => r.Query).Returns(new QueryCollection());

            IActionResult result = _messageService.DeleteMessages(QueueName, _mockRequest.Object);

            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
        }
    }
}