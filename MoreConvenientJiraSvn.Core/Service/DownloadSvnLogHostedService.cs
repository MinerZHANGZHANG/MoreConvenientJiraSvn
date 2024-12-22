using System;
using LiteDB;
using MoreConvenientJiraSvn.Core.Model;

namespace MoreConvenientJiraSvn.Core.Service;

public class DownloadSvnLogHostedService(DataService dataService, SvnService svnService) 
    : TimedHostedService(new TimeSpan(9, 30, 0), TimeSpan.FromMinutes(5), 3)
{
    private readonly DataService _dataService = dataService;
    private readonly SvnService _svnService = svnService;

    public override async Task<bool> ExecuteTask()
    {
        HostTaskLog hostTaskLog = new()
        {
            DateTime = DateTime.Now,
            TaskServiceName = nameof(DownloadSvnLogHostedService),
            IsSucccess = false,
        };


        if (_svnService.Paths.Count > 0
            && (!_svnService.Config?.IsAutoUpdateLogDaily ?? false))
        {
            // todo:add default date to svn config / use version instead
            await Task.Run(() =>
            {
                var count = 0;
                foreach (var path in _svnService.Paths)
                {
                    var isHaveJiraId = path.SvnPathType == SvnPathType.Code || path.SvnPathType == SvnPathType.Document;
                    var latestLog = _dataService.SelectByExpression<SvnLog>(Query.EQ(nameof(SvnLog.SvnPath), path.Path))
                        .OrderByDescending(log => log.DateTime)
                        .FirstOrDefault();
                    var pathBeginTime = latestLog != null ? latestLog.DateTime : DateTime.Today;
                    var pathEndTime = DateTime.Today.AddDays(1);

                    try
                    {
                        var logs = _svnService.GetSvnLogs(path.Path, pathBeginTime, pathEndTime, 500, isNeedExtractJiraId: isHaveJiraId);
                        _dataService.InsertOrUpdateMany(logs);
                        count += logs.Count;
                    }
                    catch (Exception ex)
                    {
                        hostTaskLog.IsSucccess = false;
                        hostTaskLog.Message = $"获取SVN日志并保存的过程中出错：Path:{path.Path}({pathBeginTime}->{pathEndTime} \n Error:{ex.Message}";
                        break;
                    }
                    hostTaskLog.IsSucccess = true;
                    hostTaskLog.Message = $"成功保存日志,数量:{count}";
                }
            });
        }
        else
        {
            hostTaskLog.IsSucccess = false;
            hostTaskLog.Message = "没有开启svn自动更新log，或未添加svn路径";
        }

        _dataService.Insert(hostTaskLog);
        return hostTaskLog.IsSucccess;

    }
}
