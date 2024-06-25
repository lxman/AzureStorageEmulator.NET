using System.Diagnostics.CodeAnalysis;

namespace TableStorageTests
{
    [ExcludeFromCodeCoverage]
    public class TableStorageTests
    {
        private readonly TableStorage.TableStorage _tableStorage = new();
        private const string TableName = "TestTable";

        [Fact]
        public void ListTables_Initially_ReturnsEmptyList()
        {
            List<string> tables = _tableStorage.QueryTables();
            Assert.Empty(tables);
        }

        [Fact]
        public void CreateTable_CreatesNewTable_TableExistsInList()
        {
            _tableStorage.CreateTable(TableName);
            List<string> tables = _tableStorage.QueryTables();
            Assert.Contains(TableName, tables);
        }

        [Fact]
        public void DeleteTable_RemovesTable_TableNotInList()
        {
            _tableStorage.CreateTable(TableName);
            List<string> tables = _tableStorage.QueryTables();
            Assert.Contains(TableName, tables);
            bool deleteSuccess = _tableStorage.DeleteTable(TableName);
            tables = _tableStorage.QueryTables();
            Assert.True(deleteSuccess);
            Assert.DoesNotContain(TableName, tables);
        }
    }
}