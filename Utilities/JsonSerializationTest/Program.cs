using System.Text.Json;
using LiteDB;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace JsonSerializationTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            JsonSerializerOptions options = new() { WriteIndented = true };

            BsonDocument doc = new()
            {
                ["key"] = "value",
                ["key2"] = 32,
                ["key3"] = true,
                ["key4"] = new BsonDocument { ["nestedKey"] = "nestedValue" },
                ["key5"] = DateTime.UtcNow
            };
            string json = JsonSerializer.Serialize(doc, options);
        }
    }
}