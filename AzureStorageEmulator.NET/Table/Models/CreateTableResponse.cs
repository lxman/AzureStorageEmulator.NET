using System.Text.Json.Serialization;

namespace AzureStorageEmulator.NET.Table.Models
{
    public class CreateTableResponse
    {
        [JsonPropertyName("odata.metadata")]
        public string Metadata { get; set; }

        public string TableName { get; set; }
    }
}
