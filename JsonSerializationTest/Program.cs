using System.Text.Json;
using JsonSerializationTest.Table;

namespace JsonSerializationTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ListTablesResponse listTablesResponse = new()
            {
                Metadata = "metadata",
                Value = [new TableName { Name = "table1" }, new TableName { Name = "table2" }]
            };
            Console.WriteLine(JsonSerializer.Serialize(listTablesResponse));
        }
    }
}
