using System.Text.Json.Serialization;

namespace AzureStorageEmulator.NET.Table.Models
{
    public class ListTablesResponse
    {
        [JsonPropertyName("odata.metadata")]
        public string Metadata { get; set; }

        [JsonPropertyName("value")]
        public List<TableName> Value { get; set; }
    }
}