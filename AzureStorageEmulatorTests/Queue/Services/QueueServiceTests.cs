using AzureStorageEmulator.NET.Authentication;
using AzureStorageEmulator.NET.Queue;
using AzureStorageEmulator.NET.Queue.Services;
using AzureStorageEmulator.NET.XmlSerialization.Queue;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AzureStorageEmulatorTests.Queue.Services
{
    public class QueueServiceTests
    {
        private const string QueueName = "testQueue";
        private static readonly Mock<IFifoService> MockFifoService = new();
        private static readonly Mock<IAuthenticator> MockAuthenticator = new();
        private static readonly Mock<IQueueSettings> MockQueueSettings = new();
        private static readonly EnumerationResultsSerializer EnumerationResultsSerializer = new();
        private static readonly MessageListSerializer MessageListSerializer = new();

        private readonly QueueService _queueService = new(
            MockFifoService.Object,
            MockAuthenticator.Object,
            EnumerationResultsSerializer,
            MessageListSerializer,
            MockQueueSettings.Object);

        //private readonly Mock<HttpContext> _mockContext = new();
        private readonly string _requestClientId = Guid.NewGuid().ToString();
        private readonly HttpContext _context = new DefaultHttpContext();

        public QueueServiceTests()
        {
            _context.Request.Headers["x-ms-client-request-id"] = _requestClientId;
            _context.Request.Query = new QueryCollection();
            MockAuthenticator.Setup(a => a.Authenticate(It.IsAny<HttpRequest>())).Returns(true);
        }

        [Fact]
        public async Task TestCreateQueue()
        {
            MockFifoService.Setup(f => f.AddQueue(QueueName)).Returns(true);

            IActionResult result = await _queueService.CreateQueue(QueueName, _context);

            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, ((StatusCodeResult)result).StatusCode);
            Assert.Equal(_requestClientId, _context.Response.Headers["x-ms-client-request-id"]);
        }

        // TODO: Fix this test
        //[Fact]
        //public async Task TestGetMessages()
        //{
        //    const int numOfMessages = 5;
        //    _mockContext.Setup(r => r.Query).Returns(new QueryCollection());
        //    MockFifoService.Setup(f => f.GetMessages(QueueName, numOfMessages)).Returns([]);

        //    IActionResult result = await _queueService.GetMessages(QueueName, numOfMessages, _mockContext.Object);

        //    Assert.IsType<ContentResult>(result);
        //    Assert.Equal(200, ((ContentResult)result).StatusCode);
        //}

        [Fact]
        public async Task TestGetAllMessages()
        {
            MockFifoService.Setup(f => f.GetAllMessages(QueueName)).Returns([]);

            IActionResult result = await _queueService.GetAllMessages(QueueName, _context);

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
        //    _mockContext.Setup(r => r.Query).Returns(new QueryCollection());

        //    IActionResult result = await _queueService.PostMessage(QueueName, message, visibilityTimeout, messageTtl, timeout, _mockContext.Object);

        //    Assert.IsType<ContentResult>(result);
        //    Assert.Equal(201, ((ContentResult)result).StatusCode);
        //}

        [Fact]
        public async Task TestDeleteMessage()
        {
            Guid messageId = Guid.NewGuid();
            const string popReceipt = "popReceipt";

            IActionResult result = await _queueService.DeleteMessage(QueueName, messageId, popReceipt, _context);

            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public void TestDeleteQueue()
        {

            IActionResult result = _queueService.DeleteQueue(QueueName, _context);

            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public void TestDeleteMessages()
        {
            IActionResult result = _queueService.DeleteMessages(QueueName, _context);

            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(204, ((StatusCodeResult)result).StatusCode);
        }
    }
}