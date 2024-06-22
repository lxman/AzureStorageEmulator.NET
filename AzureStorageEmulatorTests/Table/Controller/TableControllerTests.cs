using System.Text.Json;
using AzureStorageEmulator.NET.Controllers;
using AzureStorageEmulator.NET.Table.Models;
using AzureStorageEmulator.NET.Table.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AzureStorageEmulatorTests.Table.Controller
{
    public class TableControllerTests
    {
        private readonly Mock<ITableService> _mockTableService;
        private readonly TableController _controller;
        private const string TableName = "TestTable";

        public TableControllerTests()
        {
            _mockTableService = new Mock<ITableService>();
            _controller = new TableController(_mockTableService.Object);
        }

        [Fact]
        public void ListTables_ReturnsExpectedResult()
        {
            _mockTableService.Setup(service => service.ListTables(It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                .Returns(new OkResult());

            IActionResult result = _controller.ListTables();

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void CreateTable_ReturnsSuccess()
        {
            TableNameJson tableName = new() { TableName = "TestTable" };
            _mockTableService.Setup(service => service.CreateTable(tableName.TableName, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                .Returns(new OkResult());

            IActionResult result = _controller.CreateTable(tableName);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void DeleteTable_ReturnsSuccess()
        {
            _mockTableService.Setup(service => service.DeleteTable(TableName, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                .Returns(new OkResult());

            IActionResult result = _controller.DeleteTable(TableName);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void InsertEntity_ReturnsExpectedResult()
        {
            _mockTableService.Setup(service => service.Insert(TableName, It.IsAny<JsonElement>(), It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                .Returns(new OkResult());

            IActionResult result = _controller.Insert(TableName, new JsonElement());

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task QueryTable_ReturnsExpectedResultAsync()
        {
            _mockTableService.Setup(service => service.QueryTable(TableName, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                .ReturnsAsync(new MemoryStream());

            MemoryStream result = await _controller.QueryTable(TableName);

            Assert.IsType<MemoryStream>(result);
        }
    }
}
