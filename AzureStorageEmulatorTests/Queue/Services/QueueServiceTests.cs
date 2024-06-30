using System.Diagnostics.CodeAnalysis;
using AzureStorageEmulator.NET.Common;
using AzureStorageEmulator.NET.Queue;
using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Models.MessageResponseLists;
using AzureStorageEmulator.NET.Queue.Services;
using AzureStorageEmulator.NET.Results;
using AzureStorageEmulator.NET.XmlSerialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;

namespace AzureStorageEmulatorTests.Queue.Services
{
    [ExcludeFromCodeCoverage]
    public class QueueServiceTests
    {
        private readonly Mock<IFifoService> _fifoServiceMock = new();
        private readonly Mock<IXmlSerializer<GetMessagesResponseList>> _getMessagesResponseListSerializerMock = new();
        private readonly Mock<IXmlSerializer<PeekMessageResponseList>> _peekMessageResponseListSerializerMock = new();
        private readonly Mock<IXmlSerializer<PutMessageResponseList>> _putMessageResponseListSerializerMock = new();
        private readonly Mock<IXmlSerializer<QueueEnumerationResults>> _queueEnumerationResultsSerializerMock = new();
        private readonly Mock<ISettings> _settingsMock = new();
        private readonly Mock<IHeaderDictionary> _headerDictionaryMock = new();
        private readonly Mock<HttpContext> _httpContextMock = new();
        private readonly QueueService _queueService;
        private const string QueueName = "testQueue";
        private readonly QueueMetadata _queueMetadata = new(QueueName) { MessageCount = 5 };

        public QueueServiceTests()
        {
            _queueService = new QueueService(
                _fifoServiceMock.Object,
                _putMessageResponseListSerializerMock.Object,
                _peekMessageResponseListSerializerMock.Object,
                _getMessagesResponseListSerializerMock.Object,
                _queueEnumerationResultsSerializerMock.Object,
                _settingsMock.Object
            );
            _httpContextMock.SetupGet(r => r.Request.Query).Returns(new QueryCollection());
            _httpContextMock.SetupGet(c => c.Request.Headers).Returns(_headerDictionaryMock.Object);
            _httpContextMock.SetupGet(r => r.Response).Returns(new DefaultHttpContext().Response);
            _httpContextMock.SetupGet(c => c.Response.Headers).Returns(_headerDictionaryMock.Object);
            _httpContextMock.Setup(c => c.Request).Returns(new DefaultHttpContext().Request);
            _settingsMock.Setup(s => s.QueueSettings).Returns(new QueueSettings());
        }

        [Fact]
        public async Task CreateQueueAsync_Authenticated_Returns201()
        {
            _fifoServiceMock.Setup(f => f.CreateQueueAsync(QueueName)).Returns(true);

            IActionResult result = await _queueService.CreateQueueAsync(QueueName, 0, _httpContextMock.Object);

            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task ListQueuesAsync_Authenticated_Returns200()
        {
            _fifoServiceMock.Setup(f =>
                f.ListQueuesAsync(It.IsAny<CancellationToken?>()))
                .ReturnsAsync((new ResultOk(), [new QueueMetadata(new QueueObject(QueueName)) { Name = "TestQueue" }]));
            _queueEnumerationResultsSerializerMock.Setup(s => s.Serialize(It.IsAny<QueueEnumerationResults>())).ReturnsAsync("<Queues></Queues>");
            Dictionary<string, StringValues> myQueryString = new([new KeyValuePair<string, StringValues>("comp", "list")]);
            QueryCollection queries = new(myQueryString);
            _httpContextMock.SetupGet(r => r.Request.Query).Returns(queries);

            IActionResult result = await _queueService.ListQueuesAsync(0, _httpContextMock.Object);

            ContentResult contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal(200, contentResult.StatusCode);
            Assert.Equal("application/xml", contentResult.ContentType);
        }

        [Fact]
        public async Task DeleteQueueAsync_Authenticated_Returns204()
        {
            _fifoServiceMock.Setup(f => f.DeleteQueueAsync(QueueName, It.IsAny<CancellationToken>())).ReturnsAsync(new ResultOk());

            IActionResult result = await _queueService.DeleteQueueAsync(QueueName, 0, _httpContextMock.Object);

            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(204, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetMessagesAsync_Authenticated_Returns200()
        {
            _settingsMock.Setup(s => s.QueueSettings).Returns(new QueueSettings());
            _fifoServiceMock.Setup(f => f.GetMessagesAsync(QueueName, null, false, It.IsAny<CancellationToken>())).ReturnsAsync((new ResultOk(), []));
            _getMessagesResponseListSerializerMock.Setup(s => s.Serialize(It.IsAny<GetMessagesResponseList>())).ReturnsAsync("<Messages></Messages>");

            IActionResult result = await _queueService.GetMessagesAsync(QueueName, 0, _httpContextMock.Object);

            ContentResult contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal(200, contentResult.StatusCode);
            Assert.Equal("application/xml", contentResult.ContentType);
        }

        [Fact]
        public async Task PutMessageAsync_GetMessagesAsync_DeleteMessageAsync_WorkAsExpected()
        {
            QueueMessage message = new() { MessageId = Guid.NewGuid(), MessageText = "Hello, World!", PopReceipt = Guid.NewGuid().ToString() };
            _putMessageResponseListSerializerMock.Setup(s => s.Serialize(It.IsAny<PutMessageResponseList>())).ReturnsAsync("<Messages></Messages>");
            _fifoServiceMock.Setup(f => f.PutMessageAsync(QueueName, It.IsAny<QueueMessage>(), It.IsAny<CancellationToken?>())).ReturnsAsync(new ResultOk());
            _fifoServiceMock.Setup(f => f.GetMessagesAsync(QueueName, null, false, It.IsAny<CancellationToken?>())).ReturnsAsync((new ResultOk(), [message]));
            _getMessagesResponseListSerializerMock.Setup(s => s.Serialize(It.IsAny<GetMessagesResponseList>())).ReturnsAsync("<Messages></Messages>");
            _putMessageResponseListSerializerMock.Setup(s => s.Serialize(It.IsAny<PutMessageResponseList>())).ReturnsAsync("<Messages></Messages>");
            _getMessagesResponseListSerializerMock.Setup(s => s.Serialize(It.IsAny<GetMessagesResponseList>())).ReturnsAsync("<Messages></Messages>");
            _fifoServiceMock.Setup(f => f.DeleteMessageAsync(QueueName, message.MessageId, "incorrectPopReceipt", It.IsAny<CancellationToken>())).ReturnsAsync((new ResultNotFound(), null));
            _fifoServiceMock.Setup(f => f.DeleteMessageAsync(QueueName, message.MessageId, message.PopReceipt, It.IsAny<CancellationToken>())).ReturnsAsync((new ResultOk(), message));

            IActionResult result = await _queueService.PutMessageAsync(QueueName, message, 0, 0, 0, _httpContextMock.Object);

            ContentResult contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal(201, contentResult.StatusCode);
            Assert.Equal("application/xml", contentResult.ContentType);
            Assert.Equal("<Messages></Messages>", contentResult.Content);

            result = await _queueService.GetMessagesAsync(QueueName, 0, _httpContextMock.Object);

            contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal(200, contentResult.StatusCode);
            Assert.Equal("application/xml", contentResult.ContentType);
            Assert.Equal("<Messages></Messages>", contentResult.Content);

            result = await _queueService.DeleteMessageAsync(QueueName, message.MessageId, null, 0, _httpContextMock.Object);

            BadRequestResult badRequestResult = Assert.IsType<BadRequestResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            result = await _queueService.DeleteMessageAsync(QueueName, message.MessageId, "incorrectPopReceipt", 0, _httpContextMock.Object);

            NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);

            result = await _queueService.DeleteMessageAsync(QueueName, message.MessageId, message.PopReceipt, 0, _httpContextMock.Object);

            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(204, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task PostMessageAsync_Authenticated_Returns201()
        {
            QueueMessage message = new() { MessageText = "Hello, World!" };
            _putMessageResponseListSerializerMock.Setup(s => s.Serialize(It.IsAny<PutMessageResponseList>())).ReturnsAsync("<Messages></Messages>");
            _fifoServiceMock.Setup(f => f.PutMessageAsync(QueueName, It.IsAny<QueueMessage>(), It.IsAny<CancellationToken?>())).ReturnsAsync(new ResultOk());

            IActionResult result = await _queueService.PutMessageAsync(QueueName, message, 0, 0, 0, _httpContextMock.Object);

            ContentResult contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal(201, contentResult.StatusCode);
            Assert.Equal("application/xml", contentResult.ContentType);
        }

        [Fact]
        public async Task DeleteMessageAsync_Authenticated_Returns204()
        {
            Guid messageId = Guid.NewGuid();
            string popReceipt = Guid.NewGuid().ToString();
            QueueMessage message = new() { MessageId = messageId, PopReceipt = popReceipt };

            _fifoServiceMock.Setup(s => s.DeleteMessageAsync(QueueName, messageId, popReceipt, It.IsAny<CancellationToken>())).ReturnsAsync((new ResultOk(), message));

            IActionResult result = await _queueService.DeleteMessageAsync(QueueName, messageId, popReceipt, 0, _httpContextMock.Object);

            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(204, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task ClearMessagesAsync_Authenticated_Returns204()
        {
            _fifoServiceMock.Setup(f => f.ClearMessagesAsync(QueueName, It.IsAny<CancellationToken>())).ReturnsAsync(204);

            IActionResult result = await _queueService.ClearMessagesAsync(QueueName, 0, _httpContextMock.Object);

            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(204, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task MessageCountAsync_Authenticated_Returns200()
        {
            _fifoServiceMock.Setup(f => f.MessageCountAsync(QueueName)).ReturnsAsync(5);

            IActionResult result = await _queueService.MessageCountAsync(QueueName, _httpContextMock.Object);

            ContentResult contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal(200, contentResult.StatusCode);
            Assert.Equal("application/xml", contentResult.ContentType);
        }

        [Fact]
        public async Task GetQueueMetadataAsync_QueueExistsWithMetadata_Returns200AndMetadata()
        {
            List<Metadata> metadata = [new Metadata { Key = "key1", Value = "value1" }];
            _queueMetadata.Metadata = metadata;
            _fifoServiceMock.Setup(f => f.GetQueueMetadataAsync(QueueName, null)).ReturnsAsync((new ResultOk(), _queue: _queueMetadata));
            const string expectedMessageCount = "5";
            const string expectedMetadataValue = "value1";
            _headerDictionaryMock.Setup(d => d.Keys).Returns(["x-ms-approximate-messages-count", "x-ms-meta-key1"]);
            _headerDictionaryMock.Setup(d => d["x-ms-approximate-messages-count"]).Returns(new StringValues(expectedMessageCount));
            _headerDictionaryMock.Setup(d => d["x-ms-meta-key1"]).Returns(new StringValues(expectedMetadataValue));

            IActionResult result = await _queueService.GetQueueMetadataAsync(QueueName, 0, _httpContextMock.Object);

            OkResult okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Contains("x-ms-approximate-messages-count", _httpContextMock.Object.Response.Headers.Keys);
            Assert.Equal(expectedMessageCount, _httpContextMock.Object.Response.Headers["x-ms-approximate-messages-count"]);
            Assert.Contains("x-ms-meta-key1", _httpContextMock.Object.Response.Headers.Keys);
            Assert.Equal(expectedMetadataValue, _httpContextMock.Object.Response.Headers["x-ms-meta-key1"]);
        }

        [Fact]
        public async Task GetQueueMetadataAsync_QueueExistsNoMetadata_Returns200()
        {
            _queueMetadata.Metadata = null;
            _fifoServiceMock.Setup(f => f.GetQueueMetadataAsync(QueueName, null)).ReturnsAsync((new ResultOk(), _queue: _queueMetadata));
            _headerDictionaryMock.Setup(d => d["x-ms-approximate-messages-count"]).Returns("5");

            IActionResult result = await _queueService.GetQueueMetadataAsync(QueueName, 0, _httpContextMock.Object);

            OkResult okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("5", _headerDictionaryMock.Object["x-ms-approximate-messages-count"]);
        }

        [Fact]
        public async Task GetQueueMetadataAsync_QueueDoesNotExist_Returns404()
        {
            const string queueName = "nonExistentQueue";
            _fifoServiceMock.Setup(f => f.GetQueueMetadataAsync(queueName, It.IsAny<CancellationToken?>())).ReturnsAsync((new ResultNotFound(), null));

            IActionResult result = await _queueService.GetQueueMetadataAsync(queueName, 0, _httpContextMock.Object);

            NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetQueuesAsync_MissingCompQuery_ReturnsBadRequest()
        {
            Mock<IQueryCollection> queryCollectionMock = new();

            // Simulate a request without the required "comp=list" query parameter
            queryCollectionMock.Setup(q => q.ContainsKey(It.IsAny<string>())).Returns(false);
            _httpContextMock.Setup(r => r.Request.Query).Returns(queryCollectionMock.Object);
            _httpContextMock.Setup(c => c.Request.Query.Keys).Returns(Array.Empty<string>());

            IActionResult result = await _queueService.ListQueuesAsync(0, _httpContextMock.Object);

            Assert.IsType<BadRequestResult>(result);
        }
    }
}