using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonSerializationTest.Converters
{
    internal class DictionaryListObjectJsonConverter : JsonConverter<Dictionary<string, object>>
    {
        public override Dictionary<string, object>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return null;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (KeyValuePair<string, object> pair in value)
            {
                writer.WritePropertyName(pair.Key);
                JsonSerializer.Serialize(writer, pair.Value, options);
            }
            writer.WriteEndObject();
        }
    }
}
