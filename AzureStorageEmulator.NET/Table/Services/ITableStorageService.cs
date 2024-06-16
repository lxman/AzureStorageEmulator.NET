namespace AzureStorageEmulator.NET.Table.Services
{
    public interface ITableStorageService
    {
        List<string> ListTables();
        void CreateTable(string tableName);
        bool DeleteTable(string tableName);
    }
}