using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureStorageEmulator.NET.JsonSerialization
{
    public class DictionaryStringObjectJsonConverter : JsonConverter<Dictionary<string, object>>
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
                switch (pair.Value.GetType().Name)
                {
                    case "Boolean":
                        writer.WriteBooleanValue(Convert.ToBoolean(pair.Value));
                        break;
                    case "String":
                        writer.WriteStringValue((string)pair.Value);
                        break;
                    case "DateTime":
                        writer.WriteStringValue(((DateTime)pair.Value).ToString("yyyy-MM-ddThh:mm:ss.fffffffZ"));
                        break;
                    case "Int32":
                        writer.WriteNumberValue((int)pair.Value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"{pair.Value}: {pair.Value.GetType().Name}");
                        break;
                }
            }
            writer.WriteEndObject();
        }
    }
}
