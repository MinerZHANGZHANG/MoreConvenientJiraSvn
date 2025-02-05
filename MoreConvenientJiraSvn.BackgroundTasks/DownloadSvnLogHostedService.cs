using LiteDB;
using MoreConvenientJiraSvn.Core.Models;
using System.Text;

namespace MoreConvenientJiraSvn.Core.Service;

public class DownloadSvnLogHostedService(DataService dataService, SvnService svnService, NotificationService notificationService)
    : TimedHostedService(new TimeSpan(9, 30, 0), TimeSpan.FromMinutes(5), 3)
{
    private readonly DataService _dataService = dataService;
    private readonly SvnService _svnService = svnService;
    private readonly NotificationService _notificationService = notificationService;

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
            var successPathCount = 0;
            var messageBuilder = new StringBuilder();
            await Task.Run(() =>
            {
                var updateLogCount = 0;
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
                        var upsertCount= _dataService.InsertOrUpdateMany(logs);

                        if (upsertCount != logs.Count)
                        {
                            throw new Exception($"获取和保存到数据库的数量Log数不一致!Log({logs.Count})|Database({upsertCount})");
                        }

                        updateLogCount += logs.Count;
                        successPathCount += 1;
                        messageBuilder.AppendLine($"成功获取SVN日志并保存,路径:{path.Path}({pathBeginTime}->{pathEndTime}) 数量:{updateLogCount}");
                    }
                    catch (Exception ex)
                    {
                        messageBuilder.AppendLine($"获取SVN日志并保存的过程中出错：路径:{path.Path}({pathBeginTime}->{pathEndTime}) 错误:{ex.Message}");
                        break;
                    }
                }
                hostTaskLog.IsSucccess = successPathCount == _svnService.Paths.Count;
                hostTaskLog.Message = messageBuilder.ToString();
            });
        }
        else
        {
            hostTaskLog.IsSucccess = false;
            hostTaskLog.Message = "没有开启svn自动更新log，或未添加svn路径";
        }
        _dataService.Insert(hostTaskLog);

        _notificationService.ShowNotification($"后台更新SvnLog结束", hostTaskLog.Message);

        return hostTaskLog.IsSucccess;

    }
}
