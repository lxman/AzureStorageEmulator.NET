using System.Text.Json.Serialization;

namespace AzureStorageEmulator.NET.Table.Models
{
    public class TableName
    {
        [JsonPropertyName("TableName")]
        public string Name { get; set; }
    }
}