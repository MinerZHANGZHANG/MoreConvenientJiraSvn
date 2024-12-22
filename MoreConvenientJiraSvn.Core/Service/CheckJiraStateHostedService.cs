using LiteDB;
using MoreConvenientJiraSvn.Core.Model;


namespace MoreConvenientJiraSvn.Core.Service
{
    public class CheckJiraStateHostedService(DataService dataService, SvnService svnService, JiraService jiraService)
    : TimedHostedService(new TimeSpan(9, 30, 0), TimeSpan.FromMinutes(5), 3)
    {
        private readonly DataService _dataService = dataService;
        private readonly SvnService _svnService = svnService;
        private readonly JiraService _jiraService = jiraService;

        public override async Task<bool> ExecuteTask()
        {
            HostTaskLog hostTaskLog = new()
            {
                DateTime = DateTime.Now,
                TaskServiceName = nameof(CheckJiraStateHostedService),
                IsSucccess = false,
            };



            _dataService.Insert(hostTaskLog);
            return hostTaskLog.IsSucccess;

        }

        public List<PluginMessage> GetMessage(List<JiraInfoCompareTuple> jiraInfoCompareTuples, JiraFilter jiraFilter)
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

                    jiraInfoCompareTuples.RemoveAt(i);
                }
            }

            // Other...

            return messageList;
        }
    }

}