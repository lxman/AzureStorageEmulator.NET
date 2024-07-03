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

        void Delete(string location);
    }

    public class TableStorage : ITableStorage
    {
        private static MemoryStream _backing = new();
        private LiteDatabase _db = new(_backing);

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
            Directory.CreateDirectory(Path.Combine(location, "AzureStorageEmulator.NET", "Table"));
            string saveFilePath = GetSavePath(location);
            _db.Checkpoint();
            await File.WriteAllBytesAsync(saveFilePath, _backing.ToArray());
        }

        public async Task Restore(string location)
        {
            string saveFilePath = GetSavePath(location);
            if (!File.Exists(saveFilePath)) return;
            _db.Dispose();
            await _backing.DisposeAsync();
            _backing = new MemoryStream(await File.ReadAllBytesAsync(saveFilePath));
            _db = new LiteDatabase(_backing);
        }

        public void Delete(string location)
        {
            string saveFilePath = GetSavePath(location);
            if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
        }

        private static string GetSavePath(string location) => Path.Combine(location, "AzureStorageEmulator.NET", "Table", "Tables.db");
    }
}