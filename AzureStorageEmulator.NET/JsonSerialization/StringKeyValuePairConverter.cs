using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureStorageEmulator.NET.JsonSerialization
{
    public class StringKeyValuePairConverter : JsonConverter<KeyValuePair<string, string>>
    {
        public override KeyValuePair<string, string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, KeyValuePair<string, string> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(value.Key);
            writer.WriteStringValue(value.Value);
            writer.WriteEndObject();
        }
    }
}
