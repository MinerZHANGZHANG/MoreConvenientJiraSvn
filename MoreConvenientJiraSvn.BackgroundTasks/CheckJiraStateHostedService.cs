using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Core.Utils;


namespace MoreConvenientJiraSvn.Core.Service
{
    public class CheckJiraStateHostedService(DataService dataService, SvnService svnService, JiraService jiraService, NotificationService notificationService)
    : TimedHostedService(new TimeSpan(9, 30, 0), TimeSpan.FromMinutes(5), 3)
    {
        private readonly DataService _dataService = dataService;
        private readonly SvnService _svnService = svnService;
        private readonly JiraService _jiraService = jiraService;
        private readonly NotificationService _notificationService = notificationService;

        public override async Task<bool> ExecuteTask()
        {
            HostTaskLog hostTaskLog = new()
            {
                DateTime = DateTime.Now,
                TaskServiceName = nameof(CheckJiraStateHostedService),
                IsSucccess = false,
            };

            var filters = await _jiraService.GetNeedRefreshFavouriteFilterAsync();
            var messages = new List<PluginMessage>();
            foreach (var filter in filters)
            {
                try
                {
                    var result = await _jiraService.GetIssuesAsyncByFilter(filter);
                    var message = GenerateJiraStatusChangeNotifications(result, filter);
                    messages.AddRange(message);
                }
                catch (Exception ex)
                {
                    hostTaskLog.IsSucccess = false;
                    hostTaskLog.Message = $"获取和检测Jira消息时，发生错误:{ex.Message}";
                }

            }
            hostTaskLog.IsSucccess = true;

            if (messages.Count > 0)
            {
                hostTaskLog.Message = $"过滤器[{string.Join("|", filters.Select(f => f.Name))}]相关的Jira存在{messages.Count}处需要注意的变动，请查看首页";
            }
            else
            {
                hostTaskLog.Message = $"过滤器[{string.Join("|", filters.Select(f => f.Name))}]相关的Jira未发现需要注意的变动";
            }
            if (messages.Any(m => m.Level == InfoLevel.Error))
            {
                hostTaskLog.Level = InfoLevel.Error;
            }
            else if (messages.Any(m => m.Level == InfoLevel.Warning))
            {
                hostTaskLog.Level = InfoLevel.Warning;
            }

            _dataService.InsertOrUpdateMany(messages);
            _dataService.Insert(hostTaskLog);

            _notificationService.ShowNotification("获取和检测jira状态已完成", hostTaskLog.Message, EnumHelper.ConvertEnumToIcon(hostTaskLog.Level));
            
            return hostTaskLog.IsSucccess;
        }

        public List<PluginMessage> GenerateJiraStatusChangeNotifications(List<JiraInfoCompareTuple> jiraInfoCompareTuples, JiraFilter jiraFilter)
        {
            List<PluginMessage> messageList = [];
            for (int i = jiraInfoCompareTuples.Count - 1; i >= 0; i--)
            {
                if (jiraInfoCompareTuples[i].Old == null)
                {
                    PluginMessage message = new()
                    {
                        Info = $"Jira:[{jiraFilter.Name}]过滤器里新增了一条数据，JiraId{jiraInfoCompareTuples[i].New.JiraId}",
                        Level = InfoLevel.Normal,
                        Time = DateTime.Now,
                        SourceName = nameof(CheckJiraStateHostedService),
                    };
                    messageList.Add(message);
                    jiraInfoCompareTuples.RemoveAt(i);
                }
            }

            for (int i = jiraInfoCompareTuples.Count - 1; i >= 0; i--)
            {
                string? oldVersion = jiraInfoCompareTuples[i].Old?.FixVersionNameText;
                string? newVersion = jiraInfoCompareTuples[i].New.FixVersionNameText;
                if (oldVersion != newVersion)
                {
                    PluginMessage message = new()
                    {
                        Info = $"Jira:[{jiraInfoCompareTuples[i].New.JiraId}]的待合并版本发生了改变![{oldVersion}]->[{newVersion}]",
                        Level = InfoLevel.Warning,
                        Time = DateTime.Now,
                        SourceName = nameof(CheckJiraStateHostedService),
                    };
                    messageList.Add(message);
                    jiraInfoCompareTuples.RemoveAt(i);
                }
            }

            for (int i = jiraInfoCompareTuples.Count - 1; i >= 0; i--)
            {
                string? oldState = jiraInfoCompareTuples[i].Old?.StatusName;
                string? newState = jiraInfoCompareTuples[i].New.StatusName;
                if (oldState != newState)
                {
                    PluginMessage message = new()
                    {
                        Info = $"Jira:[{jiraInfoCompareTuples[i].New.JiraId}]的状态发生了改变![{oldState}]->[{newState}]",
                        Level = InfoLevel.Normal,
                        Time = DateTime.Now,
                        SourceName = nameof(CheckJiraStateHostedService),
                    };
                    messageList.Add(message);
                    jiraInfoCompareTuples.RemoveAt(i);
                }
            }

            // Other...

            return messageList;
        }
    }

}