using System.Text.Json.Serialization;

namespace JsonSerializationTest.Table
{
    public class ListTablesResponse
    {
        [JsonPropertyName("odata.metadata")]
        public string Metadata { get; set; }

        [JsonPropertyName("value")]
        public List<TableName> Value { get; set; }
    }
}