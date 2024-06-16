using TableStorage;

namespace AzureStorageEmulator.NET.Table.Services
{
    public class TableStorageService(ITableStorage storage) : ITableStorageService
    {
        public List<string> ListTables()
        {
            return storage.ListTables();
        }

        public void CreateTable(string tableName)
        {
            storage.CreateTable(tableName);
        }

        public bool DeleteTable(string tableName)
        {
            return storage.DeleteTable(tableName);
        }
    }
}
