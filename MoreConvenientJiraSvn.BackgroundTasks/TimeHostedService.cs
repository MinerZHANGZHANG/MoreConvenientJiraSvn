using Microsoft.Extensions.Hosting;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using Timer = System.Threading.Timer;

namespace MoreConvenientJiraSvn.BackgroundTask;

public abstract class TimedHostedService(IRepository repository, TimeSpan executionTime, TimeSpan retryInterval, int maxTryCount) : IHostedService, IDisposable
{
    private readonly IRepository _repository = repository;

    private Timer? _executionTimer;
    private TimeSpan _executionTime = executionTime;

    private TimeSpan _retryInterval = retryInterval;
    private int _maxTryCount = maxTryCount;

    public bool IsRunning { get; protected set; } = false;

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
        var todayExecutionTime = DateTime.Today.Add(_executionTime);
        if (now >= todayExecutionTime)
        {
            var lastLog = _repository.Find<BackgroundTaskLog>(l => l.StartTime >= todayExecutionTime);
            if (lastLog.Any())
            {
                return GetNextExecutionTime(now);
            }
            else
            {
                return now;
            }
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
            IsRunning = true;
            bool success = await ExecuteTask();
            IsRunning = false;

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

    public virtual void RefreshExecuteConfig(TimeSpan executionTime, TimeSpan retryInterval, int maxTryCount)
    {
        _executionTime = executionTime;
        _retryInterval = retryInterval;
        _maxTryCount = maxTryCount;
    }

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
