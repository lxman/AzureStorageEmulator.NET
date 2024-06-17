namespace TableStorage
{
    public interface ITableStorage
    {
        List<string> ListTables();

        void CreateTable(string tableName);

        bool DeleteTable(string tableName);
    }
}