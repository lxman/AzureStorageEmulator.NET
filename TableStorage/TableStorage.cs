using LiteDB;

namespace TableStorage
{
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
    }
}
