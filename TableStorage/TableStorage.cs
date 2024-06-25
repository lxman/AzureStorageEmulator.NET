using LiteDB;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace TableStorage
{
    public interface ITableStorage
    {
        List<string> QueryTables();

        void CreateTable(string tableName);

        bool DeleteTable(string tableName);

        BsonValue InsertEntity(string tableName, BsonDocument document);

        IEnumerable<BsonDocument> GetAll(string tableName);

        IAsyncEnumerable<BsonDocument> QueryFromQueryString(string tableName, string actualQuery, IDictionary<string, object> parameters);
    }

    public class TableStorage : ITableStorage
    {
        private readonly LiteDatabase _db = new("Filename=:memory:");

        public List<string> QueryTables()
        {
            return _db.GetCollectionNames().ToList();
        }

        public void CreateTable(string tableName)
        {
            ILiteCollection<BsonDocument> documents = _db.GetCollection(tableName) ?? throw new Exception("Failed to create table");
            documents.Insert(new BsonDocument());
        }

        public bool DeleteTable(string tableName)
        {
            return _db.DropCollection(tableName);
        }

        public BsonValue InsertEntity(string tableName, BsonDocument document)
        {
            ILiteCollection<BsonDocument> documents = _db.GetCollection(tableName) ?? throw new Exception("Table not found");
            return documents.Insert(document);
        }

        public IEnumerable<BsonDocument> GetAll(string tableName)
        {
            ILiteCollection<BsonDocument> documents = _db.GetCollection(tableName) ?? throw new Exception("Table not found");
            List<BsonDocument> results = documents.FindAll().ToList();
            IEnumerable<BsonDocument> newList = [];
            results.ForEach(r =>
            {
                if (r.ContainsKey("_id")) r.Remove("_id");
            });
            results.ForEach(r =>
            {
                if (r.ContainsKey("Timestamp")) newList = newList.Append(r);
            });
            return newList;
        }

        public async IAsyncEnumerable<BsonDocument> QueryFromQueryString(string tableName, string actualQuery, IDictionary<string, object> parameters)
        {
            string query = actualQuery.Replace("[", string.Empty).Replace("]", string.Empty).Replace('*', '$');
            parameters.Keys.ToList().ForEach(k => query = query.Replace(k, $"'{parameters[k]}'"));
            IBsonDataReader reader = _db.Execute(query, ToBsonDocument(parameters));
            while (reader.Read())
            {
                BsonDocument? cast = reader.Current.AsDocument;
                cast.Remove("_id");
                if (cast.ContainsKey("Timestamp")) yield return cast;
            }
        }

        private static BsonDocument ToBsonDocument(IDictionary<string, object> parameters)
        {
            Dictionary<string, BsonValue> converted = parameters.ToDictionary(kv => kv.Key, kv => new BsonValue(kv.Value));
            return new BsonDocument(converted);
        }
    }
}