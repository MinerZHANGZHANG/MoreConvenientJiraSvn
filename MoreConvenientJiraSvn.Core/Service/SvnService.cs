using MoreConvenientJiraSvn.Core.Model;
using SharpSvn;

namespace MoreConvenientJiraSvn.Core.Service
{
    public class SvnService : IDisposable
    {
        private readonly SettingService _settingService;
        private SvnClient? _client;

        public SvnConfig? Config { get; private set; }
        public List<SvnPath> Paths { get; private set; } = [];

        public SvnService(SettingService settingService)
        {
            this._settingService = settingService;

            this.Config = _settingService.GetSingleSettingFromDatabase<SvnConfig>();
            this.Paths = _settingService.GetSettingsFromDatabase<SvnPath>()?.ToList() ?? [];
            this.InitSvnClient();
        }

        public void UpdateConfig(SvnConfig config)
        {
            if (!string.IsNullOrEmpty(config.UserName) && !string.IsNullOrEmpty(config.UserPassword))
            {
                InitSvnClient();
            }

            Config = config;
            _settingService.InsertOrUpdateSettingIntoDatabase<SvnConfig>(config);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        private void InitSvnClient()
        {
            _client?.Dispose();

            _client = new()
            {
                KeepSession = true,
            };

            if (Config != null)
            {
                _client.Authentication.DefaultCredentials = new System.Net.NetworkCredential(Config.UserName, Config.UserPassword);
            }
        }


        #region Path manager

        public void InsertOrUpdateSinglePath(SvnPath newPath)
        {
            var oldPath = Paths.Find(p => newPath.Path == p.Path);
            if (oldPath != null)
            {
                oldPath.LocalPath = newPath.LocalPath;
                oldPath.PathName = newPath.PathName;
                _settingService.InsertOrUpdateSettingIntoDatabase(oldPath);
            }
            else
            {
                _settingService.InsertOrUpdateSettingIntoDatabase(newPath);
            }

        }

        public void DeleteSinglePath(string path)
        {
            var oldPath = Paths.Find(p => path == p.Path);
            if (oldPath != null)
            {
                _settingService.DeleteSettingToDatabaseById<SvnPath>(oldPath.Id);
            }
        }

        public void UpdatePathMany(List<SvnPath> paths)
        {
            Paths = paths;
            _settingService.InsertOrUpdateSettingIntoDatabase(paths);
        }

        #endregion

        #region Operation methods

        public List<SvnLog> GetSvnLogs(string path, DateTime? beginDate, DateTime? endDate, int maxNumber = 200)
        {
            List<SvnLog> result = [];

            if (_client == null)
            {
                return result;
            }

            var logArgs = new SvnLogArgs()
            {
                Limit = maxNumber,
                Start = beginDate,
                End = endDate,
            };
            try
            {
                _client.GetLog(new Uri(path), logArgs, out var logEventArgs);
                foreach (var item in logEventArgs)
                {
                    SvnLog svnLog = new()
                    {
                        Author = item.Author,
                        ChangedUrls = item.ChangedPaths.Select(s => s.Path).ToList(),
                        DateTime = item.Time,
                        Message = item.LogMessage,
                        SvnUrl = path,
                        Revision = item.Revision,
                        Operation = string.Join(',', item.ChangedPaths.Select(s => s.Action.ToString()).Distinct())
                    };
                    result.Add(svnLog);
                }
            }
            catch (Exception)
            {


            }
            
            return result;
        }

        #endregion



    }
}
