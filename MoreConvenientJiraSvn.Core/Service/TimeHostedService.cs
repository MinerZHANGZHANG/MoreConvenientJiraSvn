using LiteDB;
using Microsoft.Extensions.Hosting;
using MoreConvenientJiraSvn.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreConvenientJiraSvn.Core.Service
{
    public abstract class TimedHostedService(TimeSpan executionTime, TimeSpan retryInterval, int maxTryCount) : IHostedService, IDisposable
    {
        private Timer? _executionTimer;
        private readonly TimeSpan _executionTime = executionTime;

        private readonly TimeSpan _retryInterval = retryInterval;
        private readonly int _maxTryCount = maxTryCount;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var nextExecution = GetNextExecutionTime(now);
  
            if (now >= nextExecution)
            {
                await ExecuteWithRetry();
                nextExecution = GetNextExecutionTime(DateTime.Now);
            }

            var interval = nextExecution - now;
            _executionTimer = new Timer(OnTimerElapsed, null, interval, Timeout.InfiniteTimeSpan);
        }

        private void OnTimerElapsed(object? state)
        {
            _ = ExecuteWithRetry();
        }

        private DateTime GetNextExecutionTime(DateTime now)
        {
            var nextExecution = DateTime.Today.Add(_executionTime);
            if (now >= nextExecution)
            {
                nextExecution = nextExecution.AddDays(1);
            }
            return nextExecution;
        }

        private async Task ExecuteWithRetry()
        {
            for (int attempt = 1; attempt <= _maxTryCount; attempt++)
            {
                // query log to decide execute?
                //var successLogs = _dataService.SelectByExpression<HostTaskLog>(Query.And(
                //     Query.EQ(nameof(HostTaskLog.TaskServiceName), nameof(DownloadSvnLogHostedService)),
                //     Query.EQ(nameof(HostTaskLog.IsSucccess), true)
                //     ));
                //var lastestLog = successLogs.OrderByDescending(log => log.DateTime)
                //    .FirstOrDefault();

                //if (lastestLog != null && lastestLog.DateTime > DateTime.Today.Add(_executionTime))
                //{
                //    hostTaskLog.IsSucccess = false;
                //    hostTaskLog.Message = $"今天已经执行过一次了，时间：{lastestLog.DateTime}";
                //}

                bool success = await ExecuteTask();

                if (success)
                {
                    return;
                }

                if (attempt < _maxTryCount)
                {
                    await Task.Delay(_retryInterval);
                }
            }
        }

        public abstract Task<bool> ExecuteTask();

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _executionTimer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _executionTimer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
