using LiteDB;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;

namespace MoreConvenientJiraSvn.Service
{
    public class JiraService
    {
        private readonly IRepository _repository;
        private readonly IJiraClient _jiraClient;
        private readonly IHtmlConvert _htmlConvert;

        private readonly LogService _logService;
        private readonly SettingService _settingService;

        private JiraConfig _jiraConfig;
        public JiraConfig JiraConfig => _jiraConfig;

        public JiraService(IRepository repository, IJiraClient jiraClient, IHtmlConvert htmlConvert, SettingService settingService, LogService logService)
        {
            _repository = repository;
            _jiraClient = jiraClient;
            _htmlConvert = htmlConvert;

            _settingService = settingService;
            _logService = logService;

            _jiraConfig = _settingService.FindSetting<JiraConfig>() ?? new();
            _settingService.OnConfigChanged += RefreshJiraCilent_OnConfigChanged;

            _jiraClient.InitHttpClient(_jiraConfig);
        }


        public async Task<List<JiraIssueFilter>> GetCurrentUserFavouriteFilterAsync()
        {
            _logService.LogDebug($"{nameof(GetCurrentUserFavouriteFilterAsync)}");
            List<JiraIssueFilter> result = [];
            try
            {
                result = await _jiraClient.GetUserFavouriteFilterAsync();
            }
            catch (Exception ex)
            {
                _logService.LogDebug($"Get favourite filter failed! \n\r {ex.Message}");
            }

            return result;
        }

        public async Task<JiraIssue?> GetIssueAsync(string issueId)
        {
            JiraIssue? result = await _jiraClient.GetIssueAsync(issueId);
            if (result != null)
            {
                _repository.Upsert(result);
            }

            return result;
        }

        public async Task<List<JiraIssue>> GetIssuesByFilterAsync(JiraIssueFilter jiraFilter, int maxRequestCount = 200)
        {
            List<JiraIssue> issueInfos = [];
            int start = 0;
            int total = 1;
            int requestCount = 0;

            while (start < total)
            {
                if (requestCount++ > maxRequestCount)
                {
                    break;
                }

                _logService.LogDebug($"{nameof(GetIssueAsync)} [{jiraFilter.SearchUrl}&startAt={start}]");
                var issuePageInfo = await _jiraClient.GetIssuesAsyncByUrl(jiraFilter.SearchUrl, start);

                start = issuePageInfo.StartAt + issuePageInfo.MaxResults;
                total = issuePageInfo.Total;

                issueInfos.AddRange(issuePageInfo.IssueInfos);
            }

            return issueInfos;
        }

        public async Task<List<IssueDiff>> GetIssuesDiffByFilterAsync(JiraIssueFilter jiraFilter, int maxRequestCount = 200)
        {
            List<IssueDiff> issueDiffs = [];
            List<JiraIssue> issueInfos = await GetIssuesByFilterAsync(jiraFilter, maxRequestCount);

            _repository.Upsert(issueInfos);
            foreach (var newIssue in issueInfos)
            {
                var oldIssue = _repository.FindOne<JiraIssue>(Query.EQ(nameof(JiraIssue.IssueId), newIssue.IssueId));
                issueDiffs.Add(new() { Old = oldIssue, New = newIssue });
            }

            return issueDiffs;
        }

        public async Task<IEnumerable<JiraIssueFilter>> GetNeedRefreshFavouriteFilterAsync()
        {
            var filters = await _jiraClient.GetUserFavouriteFilterAsync();
            var backgroundTaskConfig = _settingService.FindSetting<BackgroundTaskConfig>();
            if (backgroundTaskConfig?.CheckJiraFliterNames != null)
            {
                return filters.Where(f => backgroundTaskConfig.CheckJiraFliterNames.Contains(f.Name));
            }
            return [];
        }

        public async Task<List<JiraIssue>> GetIssuesByJqlAsync(string jql, int maxRequestCount = 200, CancellationToken cancellationToken = default)
        {
            List<JiraIssue> issues = [];
            int start = 0;
            int total = 1;
            int requestCount = 0;

            while (start < total)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (requestCount++ > maxRequestCount)
                {
                    break;
                }

                _logService.LogDebug($"{nameof(GetIssuesByJqlAsync)} [{jql}&startAt={start}]");
                try
                {
                    var issuePageInfo = await _jiraClient.GetIssuesAsyncByJql(jql, start, cancellationToken);
                    start = issuePageInfo.StartAt + issuePageInfo.MaxResults;
                    total = issuePageInfo.Total;

                    issues.AddRange(issuePageInfo.IssueInfos);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            return issues;
        }

        private async void RefreshJiraCilent_OnConfigChanged(object? sender, ConfigChangedArgs e)
        {
            if (e.Config is not JiraConfig config)
            {
                return;
            }

            if (config.BaseUrl != null && config.ApiToken != null)
            {
                if (_jiraClient.InitHttpClient(config))
                {
                    config = await _jiraClient.GetUserInfoAsync(config);
                }
            }
            _jiraConfig = config;
        }

        #region link svn

        public IEnumerable<SvnLog> GetSvnLogByJiraIdLocal(string jiraId, string path)
        {
            return _repository.Find<SvnLog>(Query.And(
                                                    Query.EQ(nameof(SvnLog.SvnPath), path),
                                                    Query.Or(
                                                        Query.EQ(nameof(SvnLog.IssueJiraId), jiraId),
                                                        Query.EQ(nameof(SvnLog.SubIssueJiraId), jiraId)
                                            )));
        }

        public IEnumerable<SvnPath> GetRelatSvnPath(JiraIssue IssueInfo)
        {
            if (IssueInfo.FixVersions.Count == 0)
            {
                return [];
            }
            var relations = _repository.Find<JiraSvnPathRelation>(r => IssueInfo.FixVersions.Contains(r.Version));
            var svnPaths = _repository.Find<SvnPath>(Query.In(nameof(SvnPath.Path), relations.Select(v => new BsonValue(v.SvnPath))));

            return svnPaths;
        }

        #endregion

        #region upload

        public async Task<List<JiraTransition>> GetTransitionsByIssueId(string issueId)
        {
            List<JiraTransition> results = await _jiraClient.GetTransitionsByIssueId(issueId);

            return results;
        }

        public async Task<(List<JiraField>, List<JiraField>)> GetFieldInfoFromTransitionAndIssueId(string issueId, JiraTransition jiraTransition, CancellationToken cancellationToken)
        {
            var formString = await _jiraClient.GetTransitionFormAsync(issueId, jiraTransition.TransitionId, cancellationToken);

            var jiraFields = await _htmlConvert.ConvertHtmlToJiraFieldsAsync(formString, cancellationToken);
            var backupFields = await _htmlConvert.ConvertHtmlToJiraFieldsAsync(formString, cancellationToken);

            return (jiraFields, backupFields);
        }

        public async Task<string> TryPostTransitionsAsync(string issueKey, string transitionId, IEnumerable<JiraField> jiraFields, CancellationToken cancellationToken = default)
        {
            _logService.LogDebug($"Change issue[{issueKey}] to [{transitionId}] (fields count:{jiraFields.Count()})");
            return await _jiraClient.TryPostTransitionsAsync(issueKey, transitionId, jiraFields, cancellationToken);
        }

        public async Task<int> DownloadIssueAttachmentAsync(string issueKey, string directoryPath, CancellationToken cancellationToken = default)
        {
            return await _jiraClient.DownloadIssueAttachmentAsync(issueKey, directoryPath, cancellationToken);
        }

        #endregion
    }
}
