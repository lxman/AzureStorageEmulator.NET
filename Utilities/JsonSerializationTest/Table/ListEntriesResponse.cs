using System.Text.Json.Serialization;

namespace JsonSerializationTest.Table
{
    public class ListEntriesResponse : IJsonOnSerializing
    {
        [JsonPropertyName("odata.metadata")]
        public string Metadata { get; set; }

        [JsonPropertyName("value")]
        public List<Dictionary<string, object>> Objects { get; set; }

        public void OnSerializing()
        {
        }
    }
}