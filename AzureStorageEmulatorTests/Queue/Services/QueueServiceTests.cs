using AzureStorageEmulator.NET.Common.HeaderManagement;
using AzureStorageEmulator.NET.Queue;
using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Services;
using AzureStorageEmulator.NET.XmlSerialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;

namespace AzureStorageEmulatorTests.Queue.Services
{
    public class QueueServiceTests
    {
        private readonly Mock<IFifoService> _fifoServiceMock = new();
        private readonly Mock<IXmlSerializer<MessageList>> _messageListSerializerMock = new();
        private readonly Mock<IXmlSerializer<QueueEnumerationResults>> _queueEnumerationResultsSerializerMock = new();
        private readonly Mock<IHeaderManagement> _headerManagementMock = new();
        private readonly Mock<IQueueSettings> _settingsMock = new();
        private readonly Mock<HttpContext> _contextMock = new();
        private readonly QueueService _queueService;
        private const string QueueName = "testQueue";

        public QueueServiceTests()
        {
            _queueService = new QueueService(
                _fifoServiceMock.Object,
                _messageListSerializerMock.Object,
                _queueEnumerationResultsSerializerMock.Object,
                _headerManagementMock.Object,
                _settingsMock.Object
            );
            _contextMock.SetupGet(r => r.Request.Query).Returns(new QueryCollection());
            _contextMock.SetupGet(c => c.Request.Headers).Returns(new HeaderDictionary());
            _contextMock.SetupGet(r => r.Response).Returns(new DefaultHttpContext().Response);
        }

        [Fact]
        public async Task CreateQueueAsync_Authenticated_Returns201()
        {
            _fifoServiceMock.Setup(f => f.AddQueueAsync(QueueName)).ReturnsAsync(true);

            IActionResult result = await _queueService.CreateQueueAsync(QueueName, _contextMock.Object);

            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task ListQueuesAsync_Authenticated_Returns200()
        {
            _fifoServiceMock.Setup(f => f.GetQueuesAsync()).ReturnsAsync([new AzureStorageEmulator.NET.Queue.Models.Queue { Name = "TestQueue" }]);
            _queueEnumerationResultsSerializerMock.Setup(s => s.Serialize(It.IsAny<QueueEnumerationResults>())).ReturnsAsync("<Queues></Queues>");
            Dictionary<string, StringValues> myQueryString = new([new KeyValuePair<string, StringValues>("comp", "list")]);
            QueryCollection queries = new(myQueryString);
            _contextMock.SetupGet(r => r.Request.Query).Returns(queries);

            IActionResult result = await _queueService.ListQueuesAsync(_contextMock.Object);

            ContentResult contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal(200, contentResult.StatusCode);
            Assert.Equal("application/xml", contentResult.ContentType);
        }

        [Fact]
        public async Task DeleteQueueAsync_Authenticated_Returns204()
        {
            IActionResult result = await _queueService.DeleteQueueAsync(QueueName, _contextMock.Object);

            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(204, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetMessagesAsync_Authenticated_Returns200()
        {
            _fifoServiceMock.Setup(f => f.GetMessagesAsync(QueueName, null, false)).ReturnsAsync([]);
            _messageListSerializerMock.Setup(s => s.Serialize(It.IsAny<MessageList>())).ReturnsAsync("<Messages></Messages>");

            IActionResult result = await _queueService.GetMessagesAsync(QueueName, _contextMock.Object);

            ContentResult contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal(200, contentResult.StatusCode);
            Assert.Equal("application/xml", contentResult.ContentType);
        }

        [Fact]
        public async Task PostMessageAsync_Authenticated_Returns201()
        {
            QueueMessage message = new() { MessageText = "Hello, World!" };
            _messageListSerializerMock.Setup(s => s.Serialize(It.IsAny<MessageList>())).ReturnsAsync("<Message></Message>");

            IActionResult result = await _queueService.PostMessageAsync(QueueName, message, 0, 0, 0, _contextMock.Object);

            ContentResult contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal(201, contentResult.StatusCode);
            Assert.Equal("application/xml", contentResult.ContentType);
        }

        [Fact]
        public async Task DeleteMessageAsync_Authenticated_Returns204()
        {
            Guid messageId = Guid.NewGuid();
            string popReceipt = Guid.NewGuid().ToString();

            IActionResult result = await _queueService.DeleteMessageAsync(QueueName, messageId, popReceipt, _contextMock.Object);

            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(204, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task ClearMessagesAsync_Authenticated_Returns204()
        {
            _fifoServiceMock.Setup(f => f.ClearMessagesAsync(QueueName)).ReturnsAsync(204);

            IActionResult result = await _queueService.ClearMessagesAsync(QueueName, _contextMock.Object);

            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(204, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task MessageCountAsync_Authenticated_Returns200()
        {
            _fifoServiceMock.Setup(f => f.MessageCountAsync(QueueName)).ReturnsAsync(5);

            IActionResult result = await _queueService.MessageCountAsync(QueueName, _contextMock.Object);

            ContentResult contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal(200, contentResult.StatusCode);
            Assert.Equal("application/xml", contentResult.ContentType);
        }
    }
}