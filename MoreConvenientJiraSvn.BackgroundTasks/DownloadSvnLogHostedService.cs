﻿using LiteDB;
using MoreConvenientJiraSvn.Core.Enums;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Service;

namespace MoreConvenientJiraSvn.BackgroundTask;

public class DownloadSvnLogHostedService : TimedHostedService
{
    private readonly IRepository _repository;

    private readonly SvnService _svnService;
    private readonly LogService _logService;
    private readonly SettingService _settingService;

    public DownloadSvnLogHostedService(IRepository repository, SvnService svnService, LogService logService, SettingService settingService)
        : base(repository, new TimeSpan(9, 30, 0), TimeSpan.FromMinutes(5), 3)
    {
        _repository = repository;

        _svnService = svnService;
        _logService = logService;
        _settingService = settingService;

        var config = _settingService.FindSetting<BackgroundTaskConfig>();
        if (config != null)
        {
            base.RefreshExecuteConfig(config.ExecutionTime - config.ExecutionTime.Date,
                TimeSpan.FromMinutes(config.RetryIntervalMinutes),
                config.MaxRetryCount);
        }
    }

    public override async Task<bool> ExecuteTask()
    {
        BackgroundTaskLog taskLog = new()
        {
            StartTime = DateTime.Now,
            TaskName = nameof(DownloadSvnLogHostedService),
            IsSucccess = false,
        };
        List<BackgroundTaskMessage> taskMessages = [];
        var config = _settingService.FindSetting<BackgroundTaskConfig>();
        var prevDays = config?.SvnLogDownloadPrevDays ?? 1;
        var needAutoRefreshSvnPaths = _settingService.FindSetting<BackgroundTaskConfig>()?.CheckSvnPaths ?? [];
        var svnPaths = _svnService.SvnPaths.Where(p => needAutoRefreshSvnPaths.Contains(p.PathName));
        if (svnPaths.Any())
        {
            // todo:add default date to svn config / use version instead
            var updatePathCount = 0;
            var updateLogTotal = 0;

            foreach (var path in svnPaths)
            {
                var latestLog = _repository.Find<SvnLog>(Query.EQ(nameof(SvnLog.SvnPath), path.Path))
                    .OrderByDescending(log => log.DateTime)
                    .FirstOrDefault();
                var pathBeginTime = latestLog != null ? latestLog.DateTime : DateTime.Today.AddDays(-prevDays);
                var pathEndTime = DateTime.Today.AddDays(1);

                try
                {
                    IEnumerable<SvnLog> logs = await _svnService.GetSvnLogsAsync(path.Path, pathBeginTime, pathEndTime, _svnService.SvnConfig.MaxResultInSingleQuery, path.IsNeedExtractJiraId);

                    updateLogTotal += _repository.Upsert(logs);
                    updatePathCount += 1;

                    taskMessages.Add(new()
                    {
                        Info = $"成功获取SVN日志并保存,路径:{path.Path}({pathBeginTime}->{pathEndTime}) 数量:{updateLogTotal}",
                        Level = InfoLevel.Normal,
                        LogId = taskLog.Id,
                    });
                }
                catch (Exception ex)
                {
                    taskMessages.Add(new()
                    {
                        Info = $"获取SVN日志并保存的过程中出错：路径:{path.Path}({pathBeginTime}->{pathEndTime}) 错误:{ex.Message}",
                        Level = InfoLevel.Error,
                        LogId = taskLog.Id,
                    });
                }
            }

            taskLog.IsSucccess = updatePathCount == svnPaths.Count();
            taskLog.Summary = $"SVN日志更新完成，成功更新了{updatePathCount}个路径";
        }
        else
        {
            taskLog.IsSucccess = true;
            taskLog.Level = InfoLevel.Normal;
            taskLog.Summary = "没有设置需要自动更新的Svn路径，或对应的路径已在Svn配置页面删除";
        }

        _repository.Insert(taskLog);
        _repository.Insert<BackgroundTaskMessage>(taskMessages);

        _logService.LogDebug(taskLog.Summary);

        return taskLog.IsSucccess;
    }
}
