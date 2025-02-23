using LiteDB;
using MoreConvenientJiraSvn.Core.Interfaces;

namespace MoreConvenientJiraSvn.Infrastructure;

public class Repository(LiteDatabase db) : IRepository
{
    private readonly LiteDatabase _db = db;

    public BsonValue Insert<T>(T obj) where T : new()
    {
        var collection = _db.GetCollection<T>();
        return collection.Insert(obj);
    }

    public BsonValue Insert<T>(IEnumerable<T> objs) where T : class
    {
        var collection = _db.GetCollection<T>();
        return collection.Insert(objs);
    }

    public bool Upsert<T>(T obj) where T : new()
    {
        var collection = _db.GetCollection<T>();
        return collection.Upsert(obj);
    }

    public int Upsert<T>(IEnumerable<T> objs) where T : new()
    {
        var collection = _db.GetCollection<T>();
        collection.DeleteAll();
        return collection.Insert(objs);
    }

    public IEnumerable<T> FindAll<T>() where T : new()
    {
        var result = _db.GetCollection<T>().FindAll();
        return result;
    }

    public IEnumerable<T> Find<T>(BsonExpression expression) where T : new()
    {
        var result = _db.GetCollection<T>().Find(expression);
        return result ?? [];
    }

    public T? FindOne<T>(BsonExpression expression) where T : new()
    {
        var result = _db.GetCollection<T>().FindOne(expression);
        return result;
    }

    public bool Delete<T>(BsonValue id) where T : new()
    {
        var result = _db.GetCollection<T>().Delete(id);
        return result;
    }

    public async Task<BsonValue> InsertAsync<T>(T obj) where T : new()
    {
        var collection = _db.GetCollection<T>();
        return await Task.Run(() =>
        {
            try
            {
                _db.BeginTrans();
                var result = collection.Insert(obj);
                _db.Commit();
                return result;
            }
            catch
            {
                _db.Rollback();
                throw;
            }
        });
    }

    public async Task<bool> UpsertAsync<T>(T obj) where T : new()
    {
        var collection = _db.GetCollection<T>();
        return await Task.Run(() =>
        {
            try
            {
                _db.BeginTrans();
                var result = collection.Upsert(obj);
                _db.Commit();
                return result;
            }
            catch
            {
                _db.Rollback();
                throw;
            }
        });
    }

    /// <summary>
    /// Need length less than 24 char
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    //[Obsolete("Use BsonMapper is better")]
    //private ObjectId ConvertToObjectId(string str)
    //{
    //    string cleanedString = Regex.Replace(str, "[^0-9a-fA-F]", "");
    //    cleanedString = cleanedString.PadLeft(24, '0')[..24];
    //    return new ObjectId(cleanedString);
    //}
}
