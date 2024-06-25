using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using AzureStorageEmulator.NET.Controllers;
using AzureStorageEmulator.NET.Table.Models;
using AzureStorageEmulator.NET.Table.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AzureStorageEmulatorTests.Table.Controller
{
    [ExcludeFromCodeCoverage]
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
            _mockTableService.Setup(service => service.QueryTables(It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
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
            _mockTableService.Setup(service => service.InsertEntity(TableName, It.IsAny<JsonElement>(), It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                .Returns(new OkResult());

            IActionResult result = _controller.InsertEntity(TableName, new JsonElement());

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task QueryTable_ReturnsExpectedResultAsync()
        {
            _mockTableService.Setup(service => service.QueryEntities(TableName, It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()))
                .ReturnsAsync(new MemoryStream());

            MemoryStream result = await _controller.QueryEntities(TableName);

            Assert.IsType<MemoryStream>(result);
        }
    }
}