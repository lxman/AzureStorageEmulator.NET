using System.Text.Json.Serialization;

namespace JsonSerializationTest.Table
{
    public class TableName
    {
        [JsonPropertyName("TableName")]
        public string Name { get; set; }
    }
}