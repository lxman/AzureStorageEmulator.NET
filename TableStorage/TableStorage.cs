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

        Task Persist(string location);

        Task Restore(string location);
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
            parameters.Keys.ToList().ForEach(k =>
            {
                if (parameters[k] is DateTime dt)
                {
                    query = query.Replace(k, $"DATETIME('{dt:yyyy-MM-ddTHH:mm:ss.fffffffZ}')");
                    return;
                }
                query = query.Replace(k, $"'{parameters[k]}'");
            });
            IBsonDataReader reader = _db.Execute(query);
            while (reader.Read())
            {
                BsonDocument? cast = reader.Current.AsDocument;
                cast.Remove("_id");
                if (cast.ContainsKey("Timestamp")) yield return cast;
            }
        }

        public async Task Persist(string location)
        {
            throw new NotImplementedException("Table persistence is not yet implemented.");
            Directory.CreateDirectory(Path.Combine(location, "Queue"));
            string saveFilePath = Path.Combine(location, "Queue", "Queues.json");
            _db.GetCollectionNames().ToList().ForEach(n =>
            {
                var collection = _db.GetCollection(n);
                var reader = _db.Execute($"SELECT $ FROM {n}");
                while (reader.Read())
                {

                }
            });
        }

        public async Task Restore(string location)
        {
            throw new NotImplementedException("Table restoration is not yet implemented.");
            string saveFilePath = Path.Combine(location, "Queue", "Queues.json");
            if (!File.Exists(saveFilePath)) return;
            string json = await File.ReadAllTextAsync(saveFilePath);
            //BsonDocument[] documents = JsonSerializer.Deserialize<BsonDocument[]>(json);
            //documents.ToList().ForEach(d =>
            //{
            //    _db.GetCollection(d["TableName"].AsString).Insert(d["Document"]);
            //});
        }
    }
}