using LiteDB;
using System.Linq.Expressions;

namespace MoreConvenientJiraSvn.Core.Interfaces;

public interface IRepository
{
    BsonValue Insert<T>(T obj) where T : new();

    BsonValue Insert<T>(IEnumerable<T> objs) where T : class;

    bool Upsert<T>(T obj) where T : new();

    int Upsert<T>(IEnumerable<T> objs) where T : new();

    IEnumerable<T> FindAll<T>() where T : new();

    IEnumerable<T> Find<T>(BsonExpression expression) where T : new();
    
    IEnumerable<T> Find<T>(Expression<Func<T, bool>> predicate) where T : new();

    T? FindOne<T>(BsonExpression expression) where T : new();

    bool Delete<T>(BsonValue id) where T : new();

    Task<BsonValue> InsertAsync<T>(T obj) where T : new();

    Task<bool> UpsertAsync<T>(T obj) where T : new();
}
