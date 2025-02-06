using Microsoft.Extensions.Hosting;
using Timer = System.Threading.Timer;

namespace MoreConvenientJiraSvn.BackgroundTask;

public abstract class TimedHostedService(TimeSpan executionTime, TimeSpan retryInterval, int maxTryCount) : IHostedService, IDisposable
{
    private Timer? _executionTimer;
    private readonly TimeSpan _executionTime = executionTime;

    private readonly TimeSpan _retryInterval = retryInterval;
    private readonly int _maxTryCount = maxTryCount;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.Now;

        var nextExecution = GetNextExecutionTimeWhenStart(now);
        if (now >= nextExecution)
        {
            await ExecuteWithRetry();
            nextExecution = GetNextExecutionTime(now);
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
        var todayExecution = DateTime.Today.Add(_executionTime);
        if (now >= todayExecution)
        {
            return todayExecution.AddDays(1);
        }
        return todayExecution;
    }

    private DateTime GetNextExecutionTimeWhenStart(DateTime now)
    {
        if (now >= DateTime.Today.Add(_executionTime))
        {
            // query log to decide execute?
            //var successLogs = _repository.SelectByExpression<HostTaskLog>(Query.And(
            //     Query.EQ(nameof(HostTaskLog.TaskServiceName), nameof(this.GetType().Name)),
            //     Query.EQ(nameof(HostTaskLog.IsSucccess), true)
            //     ));
            //var lastestLog = successLogs.OrderByDescending(log => log.DateTime)
            //    .FirstOrDefault();

            //if (lastestLog != null && lastestLog.DateTime > DateTime.Today.Add(_executionTime))
            //{
            //    hostTaskLog.IsSucccess = false;
            //    hostTaskLog.Message = $"今天已经执行过一次了，时间：{lastestLog.DateTime}";
            //}

            return now;
        }
        else
        {
            return GetNextExecutionTime(now);
        }
    }

    private async Task ExecuteWithRetry()
    {
        for (int attempt = 1; attempt <= _maxTryCount; attempt++)
        {
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
