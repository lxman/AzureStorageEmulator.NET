using MongoDB.Bson;

namespace BsonSerialization
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BsonDocument document = new()
            {
                ["name"] = "John Doe",
                ["age"] = 30,
                ["isStudent"] = true
            };
            Dictionary<string, object> dict = [];
            document.Names.ToList().ForEach(name =>
            {
                Console.WriteLine($"{name}: {document[name].BsonType.ToString()}");
                switch (document[name].BsonType.ToString())
                {
                    case "Null":
                        dict[name] = null;
                        break;
                }
            });
            //byte[] json = BsonSerializer.Serialize(document);
            //string jsonStr = Encoding.UTF8.GetString(json);
            //var dotNetObject = BsonTypeMapper.MapToDotNetValue(doc);
            //string jsonStr = JsonSerializer.Serialize(dotNetObject);
        }
    }
}