using System.Text.Json;
using JsonSerializationTest.Converters;
using JsonSerializationTest.Table;

namespace JsonSerializationTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                Converters = { new DictionaryListObjectJsonConverter() }
            };
            ListEntriesResponse response = new()
            {
                Metadata = "http://localhost:10000/$metadata#Tables",
                Objects =
                [
                    new Dictionary<string, object>
                    {
                        { "PartitionKey", "part1" },
                        { "RowKey", "row1" },
                        { "Timestamp", DateTime.UtcNow },
                        { "Age", 32 },
                        {
                            "Properties",
                            new List<KeyValuePair<string, object>>
                            {
                                new("key1", "value1")
                            }
                        }
                    }
                ]
            };
            string json = JsonSerializer.Serialize(response, options);
        }
    }
}