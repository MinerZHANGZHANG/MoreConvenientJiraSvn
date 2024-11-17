using LiteDB;

namespace MoreConvenientJiraSvn.Core.Service
{
    public class SettingService(DataService dataService)
    {
        private readonly DataService _dataService = dataService;
        public Action<object>? OnConfigChange;

        public T? GetSingleSettingFromDatabase<T>() where T : class
        {
            return _dataService.SelectAll<T>().FirstOrDefault();
        }

        public IEnumerable<T>? GetSettingsFromDatabase<T>() where T : class
        {
            return _dataService.SelectAll<T>();
        }

        public void InsertOrUpdateSettingIntoDatabase<T>(T obj) where T : class
        {
            _dataService.InsertOrUpdate<T>(obj);
            OnConfigChange?.Invoke(obj);
        }

        public void InsertOrUpdateSettingIntoDatabase<T>(List<T> objs) where T : class
        {
            _dataService.InsertOrUpdateMany<T>(objs);
            OnConfigChange?.Invoke(objs);
        }

        public bool DeleteSettingToDatabaseById<T>(ObjectId objectId) where T : class
        {
            var result = _dataService.DeleteOneById<T>(objectId);
            OnConfigChange?.Invoke(objectId);
            return result;
        }
    }
}
