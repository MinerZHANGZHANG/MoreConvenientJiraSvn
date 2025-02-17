using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Core.Utils;
using MoreConvenientJiraSvn.Core.Enums;
using MoreConvenientJiraSvn.Service;
using System.IO;

namespace MoreConvenientJiraSvn.BackgroundTask;

public class CheckSqlHostedService: TimedHostedService
{
    private readonly IRepository _repository;
    private readonly IPlSqlCheckPipeline _plSqlCheckPipeline;

    private readonly SettingService _settingService;
    private readonly LogService _logService;

    public CheckSqlHostedService(IRepository repository, IPlSqlCheckPipeline plSqlCheckPipeline, LogService logService, SettingService settingService)
    : base(new TimeSpan(9, 30, 0), TimeSpan.FromMinutes(5), 3)
    {
        _repository = repository;
        _plSqlCheckPipeline = plSqlCheckPipeline;

        _logService = logService;
        _settingService = settingService;

        var config = _settingService.FindSetting<BackgroundTaskConfig>();
        if (config != null)
        {
            base.RefreshExecuteConfig(config.ExecutionTime - DateTime.Today,
                TimeSpan.FromMinutes(config.RetryIntervalMinutes),
                config.MaxRetryCount);
        }
    }

    public override async Task<bool> ExecuteTask()
    {
        BackgroundTaskLog taskLog = new()
        {
            StartTime = DateTime.Now,
            TaskName = nameof(CheckSqlHostedService),
            IsSucccess = false,
        };
        List<BackgroundTaskMessage> taskMessages = [];

        var config = _settingService.FindSetting<BackgroundTaskConfig>();
        if (config == null || config.CheckSqlDirectoies.Count == 0)
        {
            taskLog.Summary = "没有配置要检测的Sql目录";
            taskLog.Level = InfoLevel.Warning;
            taskLog.IsSucccess = true;
        }
        else
        {
            try
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
                    taskLog.Summary = "所选Sql目录不存在或没有Sql文件";
                    taskLog.Level = InfoLevel.Warning;
                    taskLog.IsSucccess = true;
                }
                else
                {
                    List<SqlIssue> sqlIssues = [];
                    foreach (var file in fileInfos)
                    {
                        var issues = _plSqlCheckPipeline.CheckSingleFile(file, viewAlertCountDict);
                        sqlIssues.AddRange(issues);
                    }

                    taskMessages.AddRange(sqlIssues.Select(i => new BackgroundTaskMessage()
                    {
                        Info = $"[{i.FilePath}]存在问题:{i.Message}",
                        Level = i.Level,
                    }));

                    taskLog.MessageIds = taskMessages.Select(m => m.Id);
                    taskLog.Summary = $"找到{fileInfos.Count}个Sql文件，检测完成，发现{sqlIssues.Count}个问题";
                    taskLog.Level = sqlIssues.Any(i => i.Level == InfoLevel.Error)
                                      ? InfoLevel.Error
                                      : (sqlIssues.Any(i => i.Level == InfoLevel.Warning)
                                         ? InfoLevel.Warning
                                         : InfoLevel.Normal);
                    taskLog.IsSucccess = true;
                }
            }
            catch (Exception ex)
            {
                taskLog.Level = InfoLevel.Error;
                taskLog.Summary = $"获取和检测Sql文件时，发生错误:{ex.Message}";
                taskLog.IsSucccess = false;
            }
        }

        _repository.Insert<BackgroundTaskMessage>(taskMessages);
        _repository.Insert(taskLog);
        _logService.Debug($"检测Sql已完成:{taskLog.Summary}");

        return await Task.FromResult(taskLog.IsSucccess);
    }

}
