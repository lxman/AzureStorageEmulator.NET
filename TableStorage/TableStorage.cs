using LiteDB;

namespace TableStorage
{
    public interface ITableStorage
    {
        List<string> ListTables();

        void CreateTable(string tableName);

        bool DeleteTable(string tableName);

        BsonValue Insert(string tableName, BsonDocument document);

        IEnumerable<BsonDocument> GetAll(string tableName);
    }

    public class TableStorage : ITableStorage
    {
        private readonly LiteDatabase _db = new("Filename=:memory:");

        public List<string> ListTables()
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

        public BsonValue Insert(string tableName, BsonDocument document)
        {
            ILiteCollection<BsonDocument> documents = _db.GetCollection(tableName) ?? throw new Exception("Table not found");
            return documents.Insert(document);
        }

        public IEnumerable<BsonDocument> GetAll(string tableName)
        {
            ILiteCollection<BsonDocument> documents = _db.GetCollection(tableName) ?? throw new Exception("Table not found");
            return documents.FindAll();
        }
    }
}