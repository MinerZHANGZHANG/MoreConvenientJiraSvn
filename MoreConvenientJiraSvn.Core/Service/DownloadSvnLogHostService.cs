using System;
using System.Timers;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;
using LiteDB;
using MoreConvenientJiraSvn.Core.Model;
using System.Text.Json.Serialization;

namespace MoreConvenientJiraSvn.Core.Service
{

    public class DownloadSvnLogHostService(DataService dataService, SvnService svnService) : IHostedService, IDisposable
    {
        private readonly DataService _dataService = dataService;
        private readonly SvnService _svnService = svnService;

        private Timer? _executeionTimer;
        private readonly TimeSpan _executionTime = new(9, 30, 0); // try execute in 9:30 am everyday
        private readonly TimeSpan _retryInterval = TimeSpan.FromMinutes(30);
        private int _tryCount = 0;
        private readonly int _maxTryCount = 1;


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var nextExecution = DateTime.Today.Add(_executionTime);


            if (now >= nextExecution)
            {
                var isExecuteSuccess = await ExecuteTask();
                if (isExecuteSuccess)
                {
                    nextExecution = DateTime.Today.AddDays(1).Add(_executionTime);
                }
                else
                {
                    nextExecution = DateTime.Now.Add(_retryInterval);
                }
            }

            var interval = nextExecution - now;
            _executeionTimer = new Timer(interval.TotalMilliseconds);
            _executeionTimer.Elapsed += TimerElapsed;
            _executeionTimer.AutoReset = false;
            _executeionTimer.Start();
        }

        private async void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (_executeionTimer == null)
            {
                return;
            }

            var isExecuteSuccess = await ExecuteTask();

            if (isExecuteSuccess)
            {
                var nextExecution = DateTime.Today.AddDays(1).Add(_executionTime);
                var now = DateTime.Now;
                var interval = nextExecution - now;

                _executeionTimer.Interval = interval.TotalMilliseconds;
            }
            else
            {
                _executeionTimer.Interval = _retryInterval.TotalMilliseconds;
            }

            _executeionTimer.Start();
        }

        private async Task<bool> ExecuteTask()
        {
            HostTaskLog hostTaskLog = new()
            {
                DateTime = DateTime.Now,
                TaskServiceName = nameof(DownloadSvnLogHostService),
                IsSucccess = false,
            };
            if (_tryCount < _maxTryCount)
            {
                _tryCount++;
                if (_svnService.Paths.Count > 0
                    && (!_svnService.Config?.IsAutoUpdateLogDaily ?? false))
                {
                    var successLogs = _dataService.SelectByExpression<HostTaskLog>(Query.And(
                                         Query.EQ(nameof(HostTaskLog.TaskServiceName), nameof(DownloadSvnLogHostService)),
                                         Query.EQ(nameof(HostTaskLog.IsSucccess), true)
                                         ));
                    var lastestLog = successLogs.OrderByDescending(log => log.DateTime)
                        .FirstOrDefault();

                    if (lastestLog != null && lastestLog.DateTime > DateTime.Today.Add(_executionTime))
                    {
                        hostTaskLog.IsSucccess = false;
                        hostTaskLog.Message = $"今天已经执行过一次了，时间：{lastestLog.DateTime}";
                    }
                    else
                    {
                        // todo:add default date to svn config / use version instead
                        var beginTime = lastestLog?.DateTime ?? DateTime.Today.AddYears(-20);
                        var endTime = DateTime.Now;

                        await Task.Run(() =>
                        {
                            var count = 0;
                            foreach (var path in _svnService.Paths)
                            {
                                try
                                {
                                    var lastLog = _dataService.SelectByExpression<SvnLog>(Query.EQ(nameof(HostTaskLog.TaskServiceName), nameof(DownloadSvnLogHostService)))
                                        .OrderByDescending(log => log.DateTime)
                                        .FirstOrDefault();
                                    var pathBeginTime = beginTime;
                                    if (lastestLog != null)
                                    {
                                        pathBeginTime = lastestLog.DateTime;
                                    }
                                    bool isHaveJiraId = path.SvnPathType == SvnPathType.Code || path.SvnPathType == SvnPathType.Document;

                                    var logs = _svnService.GetSvnLogs(path.Path, beginTime, endTime, 500, isNeedExtractJiraId: isHaveJiraId);
                                    _dataService.InsertOrUpdateMany(logs);
                                    count += logs.Count;
                                }
                                catch (Exception ex)
                                {
                                    hostTaskLog.IsSucccess = false;
                                    hostTaskLog.Message = $"获取SVN日志并保存的过程中出错：Path:{path.Path}({beginTime}->{endTime} \n Error:{ex.Message}";
                                    break;
                                }
                                hostTaskLog.IsSucccess = true;
                                hostTaskLog.Message = $"成功保存日志,数量:{count}";
                            }
                        });

                    }
                }
                else
                {
                    hostTaskLog.IsSucccess = false;
                    hostTaskLog.Message = "没有配置svn自动更新log或未添加svn路径";
                }
            }
            else
            {
                hostTaskLog.Message = $"当前重试次数({_tryCount})达到上限({_maxTryCount})";
            }

            _dataService.Insert(hostTaskLog);

            return hostTaskLog.IsSucccess;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _executeionTimer?.Stop();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _executeionTimer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
