using LiteDB;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Service;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MoreConvenientJiraSvn.Service
{
    public class JiraService
    {
        private readonly IRepository _repository;
        private readonly IJiraClient _jiraClient;

        private readonly LogService _logService;
        private readonly SettingService _settingService;

        private JiraConfig _jiraConfig;
        public JiraConfig JiraConfig => _jiraConfig;

        public JiraService(IRepository repository, IJiraClient jiraClient, SettingService settingService, LogService logService)
        {
            _repository = repository;
            _jiraClient = jiraClient;

            _settingService = settingService;
            _logService = logService;

            _jiraConfig = _settingService.FindSetting<JiraConfig>() ?? new();
            _settingService.OnConfigChanged += RefreshJiraCilent_OnConfigChanged;

            _jiraClient.InitHttpClient(_jiraConfig);
        }


        public async Task<List<JiraIssueFilter>> GetCurrentUserFavouriteFilterAsync()
        {
            _logService.Debug($"{nameof(GetCurrentUserFavouriteFilterAsync)}");
            List<JiraIssueFilter> result = [];
            try
            {
                result = await _jiraClient.GetUserFavouriteFilterAsync();
            }
            catch (Exception ex)
            {
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
            List<JiraIssue> issues = [];
            int start = 0;
            int total = 1;
            int requestCount = 0;

            while (start < total)
            {
                if (requestCount++ > maxRequestCount)
                {
                    break;
                }

                _logService.Debug($"{nameof(GetIssueAsync)} [{jiraFilter.SearchUrl}&startAt={start}]");
                var issuePageInfo = await _jiraClient.GetIssuesAsyncByUrl(jiraFilter.SearchUrl, start);

                start = issuePageInfo.StartAt + issuePageInfo.MaxResults;
                total = issuePageInfo.Total;

                issues.AddRange(issuePageInfo.IssueInfos);
            }

            return issues;
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
            var relations = _repository.Find<JiraSvnPathRelation>(Query.In(nameof(JiraSvnPathRelation.Version), IssueInfo.FixVersions.Select(v => new BsonValue(v))));
            var svnPaths = _repository.Find<SvnPath>(Query.In(nameof(SvnPath.Path), relations.Select(v => new BsonValue(v.SvnPath))));

            return svnPaths;
        }

        #endregion

        #region upload

        //private List<JiraOperation> InitializeOperations()
        //{
        //    #region options

        //    FieldOption[] uploadStateOpions = [
        //        new (){ OptionId = "-1", OptionName = "无" },
        //        new (){ OptionId = "17113", OptionName = "文档" },
        //        new (){ OptionId = "17114", OptionName = "脚本" },
        //        new (){ OptionId = "17117", OptionName = "脚本（包含视图、报表）" },
        //        new (){ OptionId = "17115", OptionName = "文档&脚本" },
        //        new (){ OptionId = "17118", OptionName = "文档&脚本（包含视图、报表）" },
        //        new (){ OptionId = "17116", OptionName = "不提交任何内容" },
        //        ];

        //    #endregion

        //    #region fields

        //    var summaryField = new JiraFieldModel
        //    {
        //        FieldName = "概要",
        //        FieldId = "summary",
        //        Type = FieldType.TextBox,

        //        FieldValue = string.Empty,
        //    };

        //    var componentsField = new JiraFieldModel
        //    {
        //        FieldName = "模块",
        //        FieldId = "components",
        //        Type = FieldType.ListBox,

        //        Options = [],
        //        SelectedValues = []
        //    };

        //    var developDescriptionField = new JiraFieldModel
        //    {
        //        FieldName = "开发说明（原因分析/解决方案等）",
        //        FieldId = "customfield_10910",
        //        Type = FieldType.BigTextBox,

        //        FieldValue = string.Empty,
        //    };

        //    var testSuggestionField = new JiraFieldModel
        //    {
        //        FieldName = "测试建议",
        //        FieldId = "customfield_11700",
        //        Type = FieldType.BigTextBox,

        //        FieldValue = string.Empty,
        //    };

        //    var uploadStateField = new JiraFieldModel
        //    {
        //        FieldName = "文档/脚本是否提交★",
        //        FieldId = "customfield_11003",
        //        Type = FieldType.ComboBox,

        //        Options = uploadStateOpions,
        //        SelectedValues = []
        //    };

        //    var descriptionField = new JiraFieldModel
        //    {
        //        FieldName = "描述",
        //        FieldId = "description",
        //        Type = FieldType.BigTextBox,

        //        FieldValue = string.Empty,
        //    };

        //    var expectedHangOverDateField = new JiraFieldModel
        //    {
        //        FieldName = "预计移交日期",
        //        FieldId = "customfield_13202",
        //        Type = FieldType.DatePicker,

        //        FieldValue = string.Empty,
        //    };

        //    #endregion

        //    #region operations
        //    var operations = new List<JiraOperation>();

        //    var updateInfoOperation = new JiraOperation
        //    {
        //        OperationName = "更新开发要素",
        //        OperationId = "821"
        //    };
        //    updateInfoOperation.Fields.Add(summaryField);
        //    updateInfoOperation.Fields.Add(componentsField);
        //    updateInfoOperation.Fields.Add(developDescriptionField);
        //    updateInfoOperation.Fields.Add(testSuggestionField);
        //    updateInfoOperation.Fields.Add(uploadStateField);
        //    updateInfoOperation.Fields.Add(descriptionField);
        //    updateInfoOperation.Fields.Add(expectedHangOverDateField);

        //    #endregion

        //    operations.Add(updateInfoOperation);

        //    return operations;
        //}

        //public async Task<List<Transition>> GetTransitionsByIssueId(string issueId)
        //{
        //    List<Transition> results = [];
        //    if (_httpClient == null || string.IsNullOrEmpty(issueId))
        //    {
        //        return results;
        //    }
        //    _logService.DebugMessage($"{nameof(GetIssueAsync)} [rest/api/2/issue/{issueId}/transitions]");
        //    var response = await _httpClient.GetAsync($"rest/api/2/issue/{issueId}/transitions");
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        return results;
        //    }
        //    var jsonResponse = await response.Content.ReadAsStringAsync();

        //    using JsonDocument doc = JsonDocument.Parse(jsonResponse);

        //    var transitionsElement = doc.RootElement.GetProperty("transitions");
        //    foreach (var transition in transitionsElement.EnumerateArray())
        //    {
        //        Transition result = new()
        //        {
        //            TransitionId = transition.GetProperty("id").GetString() ?? string.Empty,
        //            TransitionName = transition.GetProperty("name").GetString() ?? string.Empty,
        //        };
        //        results.Add(result);
        //    }

        //    return results;
        //}

        #endregion
    }
}
