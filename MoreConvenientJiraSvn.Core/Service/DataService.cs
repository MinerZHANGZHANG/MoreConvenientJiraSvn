using LiteDB;

namespace MoreConvenientJiraSvn.Core.Service
{
    public class DataService(LiteDatabase db)
    {
        private readonly LiteDatabase _db = db;

        public void Insert<T>(T obj) where T : class
        {
            var collection = _db.GetCollection<T>();
            collection.Insert(obj);
        }

        public void InsertOrUpdate<T>(T obj) where T : class
        {
            var collection = _db.GetCollection<T>();
            collection.Upsert(obj);
        }

        public void InsertOrUpdateMany<T>(List<T> objs) where T : class
        {
            var collection = _db.GetCollection<T>();
            collection.Upsert(objs);
        }

        public IEnumerable<T> SelectAll<T>() where T : class
        {
            var result = _db.GetCollection<T>().FindAll();
            return result;
        }

        public T? SelectByObjectId<T>(ObjectId id) where T : class
        {
            var result = _db.GetCollection<T>().FindById(id);
            return result;
        }

        public T? SelectOneByExpression<T>(BsonExpression expression) where T : class
        {
            var result = _db.GetCollection<T>().FindOne(expression);
            return result;
        }

        public bool DeleteOneById<T>(ObjectId id) where T : class
        {
            var result = _db.GetCollection<T>().Delete(id);
            return result;
        }
    }
}
