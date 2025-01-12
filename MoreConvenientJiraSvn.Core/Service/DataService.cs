using LiteDB;

namespace MoreConvenientJiraSvn.Core.Service
{
    public class DataService(LiteDatabase db)
    {
        private readonly LiteDatabase _db = db;

        public int Insert<T>(T obj) where T : class
        {
            var collection = _db.GetCollection<T>();
            return collection.Insert(obj);
        }

        public bool InsertOrUpdate<T>(T obj) where T : class
        {
            var collection = _db.GetCollection<T>();
            return collection.Upsert(obj);
        }

        public int InsertOrUpdateMany<T>(List<T> objs) where T : class
        {
            var collection = _db.GetCollection<T>();
            return collection.Upsert(objs);
        }

        public IEnumerable<T> SelectAll<T>() where T : class
        {
            var result = _db.GetCollection<T>().FindAll();
            return result;
        }

        public IEnumerable<T> SelectByExpression<T>(BsonExpression expression) where T : class
        {
            var result = _db.GetCollection<T>().Find(expression);
            return result ?? [];
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

        /// <summary>
        /// Need length less than 24 char
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static ObjectId ConvertToObjectId(string str)
        {
            string cleanedString = new(str.Where(c => "0123456789abcdefABCDEF".Contains(c)).ToArray());
            if (cleanedString.Length < 24)
            {
                cleanedString = cleanedString.PadLeft(24, '0');
            }
            else if (cleanedString.Length > 24)
            {
                cleanedString = cleanedString[..24]; 
            }

            return new ObjectId(cleanedString);
        }
    }
}
