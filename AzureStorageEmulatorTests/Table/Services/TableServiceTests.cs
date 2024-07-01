using System.Diagnostics.CodeAnalysis;
using AzureStorageEmulator.NET.Table.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TableStorage;

namespace AzureStorageEmulatorTests.Table.Services
{
    [ExcludeFromCodeCoverage]
    public class TableServiceTests
    {
        private readonly Mock<ITableStorage> _mockStorage;
        private readonly TableService _tableService;
        private readonly DefaultHttpContext _httpContext;

        public TableServiceTests()
        {
            _mockStorage = new Mock<ITableStorage>();
            _tableService = new TableService(_mockStorage.Object);
            _httpContext = new DefaultHttpContext { Request = { Scheme = "http", Host = new HostString("127.0.0.1", 10002), Path = new PathString("/devstoreaccount1/devstoreaccount1") } };
        }

        [Fact]
        public void ListTables_ReturnsNonEmptyList()
        {
            _mockStorage.Setup(s => s.QueryTables()).Returns(["TestTable1", "TestTable2"]);

            IActionResult result = _tableService.QueryTables(_httpContext);

            ContentResult contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal(200, contentResult.StatusCode);
            Assert.Contains("TestTable1", contentResult.Content);
            Assert.Contains("TestTable2", contentResult.Content);
        }

        [Fact]
        public void CreateTable_CreatesTableSuccessfully()
        {
            const string tableName = "NewTable";

            IActionResult result = _tableService.CreateTable(tableName, _httpContext);

            _mockStorage.Verify(s => s.CreateTable(tableName), Times.Once);
            ContentResult contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal(201, contentResult.StatusCode);
            Assert.Contains(tableName, contentResult.Content);
        }

        [Fact]
        public void DeleteTable_DeletesTableSuccessfully()
        {
            const string tableName = "TableToDelete";
            _mockStorage.Setup(s => s.DeleteTable(tableName)).Returns(true);

            IActionResult result = _tableService.DeleteTable(tableName, _httpContext);

            OkResult okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        //[Fact]
        //public void Insert_InsertsDocumentSuccessfully()
        //{
        //    const string tableName = "TableToInsert";
        //    JsonElement document = JsonSerializer.SerializeToElement(JsonNode.Parse("{\"Name\":\"Fred\"}"));

        //    IActionResult result = _tableService.InsertEntity(tableName, document, _httpContext);

        //    _mockStorage.Verify(s => s.InsertEntity(tableName, It.IsAny<BsonDocument>()), Times.Once);
        //    NoContentResult noContentResult = Assert.IsType<NoContentResult>(result);
        //    Assert.Equal(204, noContentResult.StatusCode);
        //}

        //[Fact]
        //public async Task QueryTable_ReturnsDataSuccessfully()
        //{
        //    const string tableName = "TableToQuery";
        //    _mockStorage.Setup(s => s.GetAll(tableName)).Returns([new BsonDocument
        //    {
        //        ["Name"] = "Test",
        //        ["Timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:MM:ss.fffffffZ"),
        //        ["PartitionKey"] = "TestPartition",
        //        ["RowKey"] = "TestRow"
        //    }]);

        //    MemoryStream resultStream = await _tableService.QueryEntities(tableName, _httpContext);

        //    Assert.NotNull(resultStream);
        //    resultStream.Seek(0, SeekOrigin.Begin);
        //    StreamReader reader = new(resultStream);
        //    string resultContent = await reader.ReadToEndAsync();
        //    Assert.Contains("value", resultContent);
        //}
    }
}