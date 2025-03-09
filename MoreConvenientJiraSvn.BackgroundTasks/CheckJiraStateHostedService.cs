using MoreConvenientJiraSvn.Core.Enums;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Service;


namespace MoreConvenientJiraSvn.BackgroundTask;

public class CheckJiraStateHostedService : TimedHostedService
{
    private readonly IRepository _repository;

    private readonly JiraService _jiraService;
    private readonly LogService _logService;
    private readonly SettingService _settingService;

    public CheckJiraStateHostedService(IRepository repository, SvnService svnService, JiraService jiraService, LogService logService, SettingService settingService)
    : base(new TimeSpan(9, 30, 0), TimeSpan.FromMinutes(5), 3)
    {
        _repository = repository;

        _jiraService = jiraService;
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
            TaskName = nameof(CheckJiraStateHostedService),
            IsSucccess = false,
        };
        var taskMessages = new List<BackgroundTaskMessage>();

        try
        {
            var filters = await _jiraService.GetNeedRefreshFavouriteFilterAsync();
            foreach (var filter in filters)
            {
                var issueInfos = await _jiraService.GetIssuesDiffByFilterAsync(filter);
                var issueMessages = GetIssueChangeMessages(issueInfos, filter, taskLog);
                taskMessages.AddRange(issueMessages);
            }

            if (taskMessages.Count > 0)
            {
                if (taskMessages.Any(m => m.Level == InfoLevel.Error))
                {
                    taskLog.Level = InfoLevel.Error;
                }
                else if (taskMessages.Any(m => m.Level == InfoLevel.Warning))
                {
                    taskLog.Level = InfoLevel.Warning;
                }

                taskLog.Summary = $"过滤器[{string.Join("|", filters.Select(f => f.Name))}]相关的Jira存在{taskMessages.Count}处需要注意的变动，请查看首页";
            }
            else
            {
                taskLog.Summary = $"过滤器[{string.Join("|", filters.Select(f => f.Name))}]相关的Jira未发现需要注意的变动";
            }

            taskLog.IsSucccess = true;
        }
        catch (Exception ex)
        {
            taskLog.Level = InfoLevel.Error;
            taskLog.Summary = $"获取和检测Jira消息时，发生错误:{ex.Message}";
            taskLog.IsSucccess = false;
        }

        _repository.Insert<BackgroundTaskMessage>(taskMessages);
        _repository.Insert(taskLog);

        _logService.Debug("获取和检测jira状态已完成");

        return taskLog.IsSucccess;
    }

    public static List<BackgroundTaskMessage> GetIssueChangeMessages(List<IssueDiff> IssueDiffs, JiraIssueFilter jiraFilter, BackgroundTaskLog taskLog)
    {
        List<BackgroundTaskMessage> messageList = [];

        // Add issue
        for (int i = IssueDiffs.Count - 1; i >= 0; i--)
        {
            if (IssueDiffs[i].Old == null)
            {
                BackgroundTaskMessage message = new()
                {
                    Info = $"Jira:[{jiraFilter.Name}]过滤器里新增了一条数据，IssueKey{IssueDiffs[i].New.IssueKey}",
                    Level = InfoLevel.Normal,
                    LogId = taskLog.Id
                };
                messageList.Add(message);
                IssueDiffs.RemoveAt(i);
            }
        }

        // Issue's version change
        for (int i = IssueDiffs.Count - 1; i >= 0; i--)
        {
            string? oldVersionText = IssueDiffs[i].Old?.FixVersionsText;
            string? newVersionText = IssueDiffs[i].New.FixVersionsText;
            if (oldVersionText != newVersionText)
            {
                BackgroundTaskMessage message = new()
                {
                    Info = $"Jira:[{IssueDiffs[i].New.IssueKey}]的待合并版本发生了改变![{oldVersionText}]->[{newVersionText}]",
                    Level = InfoLevel.Warning,
                };
                messageList.Add(message);
            }
        }

        // Issue's status change
        for (int i = IssueDiffs.Count - 1; i >= 0; i--)
        {
            string? oldState = IssueDiffs[i].Old?.StatusName;
            string? newState = IssueDiffs[i].New.StatusName;
            if (oldState != newState)
            {
                BackgroundTaskMessage message = new()
                {
                    Info = $"Jira:[{IssueDiffs[i].New.IssueKey}]的状态发生了改变![{oldState}]->[{newState}]",
                    Level = InfoLevel.Normal,
                };
                messageList.Add(message);
            }
        }

        // Todo: Add the property that needs attention as a configuration item

        return messageList;
    }
}