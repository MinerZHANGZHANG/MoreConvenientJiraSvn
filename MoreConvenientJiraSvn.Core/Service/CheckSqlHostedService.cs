using MoreConvenientJiraSvn.Core.Model;
using MoreConvenientJiraSvn.Core.Utils;

namespace MoreConvenientJiraSvn.Core.Service
{
    public class CheckSqlHostedService(DataService dataService, SettingService settingService, NotificationService notificationService)
    : TimedHostedService(new TimeSpan(9, 30, 0), TimeSpan.FromMinutes(5), 3)
    {
        private readonly DataService _dataService = dataService;
        private readonly SettingService _settingService = settingService;
        private readonly NotificationService _notificationService = notificationService;

        public override async Task<bool> ExecuteTask()
        {
            HostTaskLog hostTaskLog = new()
            {
                DateTime = DateTime.Now,
                TaskServiceName = nameof(CheckSqlHostedService),
                IsSucccess = false,
            };

            var config = _settingService.GetSingleSettingFromDatabase<HostedServiceConfig>();
            if (config == null || config.CheckSqlDirectoies.Count == 0)
            {
                hostTaskLog.Message = "没有配置要检测的Sql目录";
            }
            else
            {
                Dictionary<string, int> viewAlertCountDict = [];
                List<string> fileInfos = [];
                foreach (string dir in config.CheckSqlDirectoies)
                {
                    if (Directory.Exists(dir))
                    {
                        fileInfos.AddRange(Directory.GetFiles(dir, "*.sql"));
                    }
                }

                if (fileInfos.Count == 0)
                {
                    hostTaskLog.Message = "所选Sql目录不存在或没有Sql文件";
                    hostTaskLog.Level = InfoLevel.Warning;
                }
                else
                {
                    List<SqlIssue> tempIssues = [];

                    Parallel.ForEach(fileInfos, file =>
                    {
                        var issues = SqlCheckPipeline.CheckSingleFile(file, viewAlertCountDict);
                        lock (tempIssues)
                        {
                            tempIssues.AddRange(issues);
                        }
                    });

                    hostTaskLog.Message = $"找到{fileInfos.Count}个Sql文件，检测完成，发现{tempIssues.Count}个问题";
                    hostTaskLog.Level = InfoLevel.Warning;
                }

            }

            _dataService.Insert(hostTaskLog);
            _notificationService.ShowNotification("检测Sql已完成", hostTaskLog.Message, EnumHelper.ConvertEnumToIcon(hostTaskLog.Level));

            return await Task.FromResult(hostTaskLog.IsSucccess);
        }

    }
}
