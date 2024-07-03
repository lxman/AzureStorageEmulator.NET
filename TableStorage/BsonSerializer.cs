using LiteDB;

namespace TableStorage
{
    public class BsonSerializer
    {
        public static string Serialize(List<BsonDocument> docs)
        {
            List<string> serialized = docs.Select(Serialize).ToList();
            return System.Text.Json.JsonSerializer.Serialize(serialized);
        }

        public static string Serialize(BsonDocument document)
        {
            Dictionary<string, object?> dictionary = [];
            document.Keys.ToList().ForEach(k =>
            {
                switch (document[k].Type)
                {
                    case BsonType.Int32:
                        dictionary.Add(k, document[k].AsInt32);
                        break;

                    case BsonType.Int64:
                        dictionary.Add(k, document[k].AsInt64);
                        break;

                    case BsonType.Double:
                        dictionary.Add(k, document[k].AsDouble);
                        break;

                    case BsonType.Decimal:
                        dictionary.Add(k, document[k].AsDecimal);
                        break;

                    case BsonType.String:
                        dictionary.Add(k, document[k].AsString);
                        break;

                    case BsonType.Binary:
                        dictionary.Add(k, document[k].AsBinary);
                        break;

                    case BsonType.Guid:
                        dictionary.Add(k, document[k].AsGuid);
                        break;

                    case BsonType.Boolean:
                        dictionary.Add(k, document[k].AsBoolean);
                        break;

                    case BsonType.DateTime:
                        dictionary.Add(k, document[k].AsDateTime);
                        break;

                    case BsonType.MinValue:
                        dictionary.Add(k, "-inf");
                        break;

                    case BsonType.Null:
                        dictionary.Add(k, null);
                        break;

                    case BsonType.Document:
                        dictionary.Add(k, Serialize(document[k].AsDocument));
                        break;

                    case BsonType.Array:
                        dictionary.Add(k, document[k].AsArray.Select(a => Serialize(a.AsDocument)).ToList());
                        break;

                    case BsonType.ObjectId:
                        dictionary.Add(k, document[k].AsObjectId);
                        break;

                    case BsonType.MaxValue:
                        dictionary.Add(k, "+inf");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException($"{k}: {document[k].Type}");
                }
            });
            return System.Text.Json.JsonSerializer.Serialize(dictionary);
        }

        public static BsonDocument? Deserialize(string document)
        {
            return JsonSerializer.Deserialize(document) as BsonDocument;
        }
    }
}