using LiteDB;
using MoreConvenientJiraSvn.Core.Interfaces;

namespace MoreConvenientJiraSvn.Service;

public class SettingService(IRepository repository)
{
    private readonly IRepository _repository = repository;

    public event EventHandler<ConfigChangedArgs>? OnConfigChanged;

    public T? FindSetting<T>() where T : new()
    {
        return _repository.FindAll<T>().FirstOrDefault();
    }

    public IEnumerable<T>? FindSettings<T>() where T : new()
    {
        return _repository.FindAll<T>();
    }

    public bool UpsertSetting<T>(T obj) where T : new()
    {
        var result = _repository.Upsert<T>(obj);
        OnConfigChanged?.Invoke(this, new(obj));

        return result;
    }

    public int UpsertSettings<T>(IEnumerable<T> objs) where T : new()
    {
        var result = _repository.Upsert<T>(objs);
        OnConfigChanged?.Invoke(this, new(_repository.FindAll<T>()));

        return result;
    }

    //public bool UpsertSettings<T>(T obj) where T : new()
    //{
    //    var result = _repository.Upsert<T>(obj);
    //    OnConfigChanged?.Invoke(this, new(_repository.FindAll<T>()));

    //    return result;
    //}

    public bool DeleteSettings<T>(BsonValue id) where T : new()
    {
        var result = _repository.Delete<T>(id);
        OnConfigChanged?.Invoke(this, new(_repository.FindAll<T>()));

        return result;
    }
}

public class ConfigChangedArgs(object? config) : EventArgs
{
    public object? Config { get; } = config;
}