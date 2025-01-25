using LiteDB;
using MoreConvenientJiraSvn.Core.Model;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MoreConvenientJiraSvn.Core.Service
{
    public class JiraService
    {
        private readonly SettingService _settingService;
        private readonly DataService _dataService;
        private readonly NotificationService _notificationService;// TODO: Replace to log service
        private HttpClient? _httpClient;
        private readonly Lazy<List<OperationModel>> _operationModels;

        public JiraConfig Config { get; private set; }
        public List<OperationModel> Operations => _operationModels.Value;

        public JiraService(SettingService settingService, DataService dataService, NotificationService notificationService)
        {
            this._settingService = settingService;
            this._dataService = dataService;
            this._notificationService = notificationService;
            this.Config = _settingService.GetSingleSettingFromDatabase<JiraConfig>() ?? new();

            this._operationModels = new(InitializeOperations);
            this.InitHttpClient(Config);
        }

        public async Task UpdateJiraConfig(JiraConfig config)
        {
            if (config.BaseUrl != null && config.ApiToken != null)
            {
                InitHttpClient(config);
                await GetCurrentUserInfoAsync(config);
            }

            Config = config;
            _settingService.InsertOrUpdateSettingIntoDatabase<JiraConfig>(config);
        }

        private void InitHttpClient(JiraConfig config)
        {
            if (!string.IsNullOrEmpty(config.BaseUrl) && !string.IsNullOrEmpty(config.ApiToken))
            {
                try
                {
                    _httpClient = new HttpClient()
                    {
                        BaseAddress = new Uri(config.BaseUrl)
                    };

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiToken);

                    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                catch (Exception)
                {
                    _httpClient = null;
                }
            }
            else
            {
                _httpClient = null;
            }
        }

        #region request methods

        public async Task GetCurrentUserInfoAsync(JiraConfig jiraConfig)
        {
            if (_httpClient == null)
            {
                return;
            }

            jiraConfig.TokenExpringAt = string.Empty;
            jiraConfig.UserName = string.Empty;
            jiraConfig.Email = string.Empty;

            _notificationService.DebugMessage($"{nameof(GetCurrentUserInfoAsync)} [{jiraConfig.BaseUrl}:rest/api/2/myself]");
            var response = await _httpClient.GetAsync($"rest/api/2/myself");
            if (!response.IsSuccessStatusCode)
            {
                return;
            }
            var jsonResponse = await response.Content.ReadAsStringAsync();
            if (jsonResponse != null)
            {
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("name", out var nameElement)
                    && root.TryGetProperty("emailAddress", out var emailElement))
                {
                    jiraConfig.UserName = nameElement.GetString() ?? string.Empty;
                    jiraConfig.Email = emailElement.GetString() ?? string.Empty;
                }
            }

            response = await _httpClient.GetAsync($"rest/pat/latest/tokens");
            if (!response.IsSuccessStatusCode)
            {
                return;
            }
            jsonResponse = await response.Content.ReadAsStringAsync();
            if (jsonResponse != null)
            {
                // get first token node
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement element in doc.RootElement.EnumerateArray())
                    {
                        if (element.TryGetProperty("expiringAt", out var expiringAtElement))
                        {
                            DateTime expiringAt = expiringAtElement.GetDateTime();
                            jiraConfig.TokenExpringAt = expiringAt.ToShortDateString();
                            break;
                        }

                    }
                }
            }
        }

        /// <remarks>Can`t find api, so only require favourite filters</remarks>
        public async Task<List<JiraFilter>> GetCurrentUserFavouriteFilterAsync()
        {
            List<JiraFilter> result = [];
            if (_httpClient == null)
            {
                return result;
            }
            try
            {
                _notificationService.DebugMessage($"{nameof(GetCurrentUserFavouriteFilterAsync)} [rest/api/2/filter/favourite]");
                var response = await _httpClient.GetAsync($"rest/api/2/filter/favourite");

                if (!response.IsSuccessStatusCode)
                {
                    return result;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                if (jsonResponse != null)
                {
                    // get first
                    using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement element in doc.RootElement.EnumerateArray())
                        {
                            var filter = new JiraFilter()
                            {
                                FilterId = element.GetProperty("id").GetString(),
                                Name = element.GetProperty("name").GetString() ?? "Nameless",
                                Jql = element.GetProperty("jql").GetString(),
                                SearchUrl = element.GetProperty("searchUrl").GetString(),
                            };
                            result.Add(filter);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

            return result;
        }

        public async Task<JiraInfo?> GetIssueAsync(string? issueId)
        {
            JiraInfo? result = default;
            if (_httpClient == null || string.IsNullOrEmpty(issueId))
            {
                return result;
            }
            _notificationService.DebugMessage($"{nameof(GetIssueAsync)} [rest/api/2/issue/{issueId}]");
            var response = await _httpClient.GetAsync($"rest/api/2/issue/{issueId}");
            if (!response.IsSuccessStatusCode)
            {
                return result;
            }
            var jsonResponse = await response.Content.ReadAsStringAsync();

            result = new(jsonResponse);
            SaveIssues([result]);
            return result;
        }

        public async Task<List<JiraInfoCompareTuple>> GetIssuesAsyncByFilter(JiraFilter jiraFilter, int maxJiraCount = 200)
        {
            List<JiraInfoCompareTuple> result = [];

            if (_httpClient == null)
            {
                return result;
            }

            // maybe request first,then loop request will be more reliable?

            int stratAt = 0;
            int total = 51;
            while (stratAt < total)
            {
                _notificationService.DebugMessage($"{nameof(GetIssueAsync)} [{jiraFilter.SearchUrl}&startAt={stratAt}]");
                var response = await _httpClient.GetAsync($"{jiraFilter.SearchUrl}&startAt={stratAt}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("startAt", out var startAtElement)
                        && root.TryGetProperty("maxResults", out var maxResultsElement)
                        && root.TryGetProperty("total", out var totalElement)
                        && root.TryGetProperty("issues", out var issuesElement)
                        && issuesElement.ValueKind == JsonValueKind.Array)
                    {
                        var issues = issuesElement.EnumerateArray();
                        foreach (var issue in issues)
                        {
                            // use toString will be faster?
                            var newInfo = new JiraInfo(issue.GetRawText());
                            var oldInfo = _dataService.SelectOneByExpression<JiraInfo>(Query.EQ(nameof(JiraInfo.JiraId), newInfo.JiraId));

                            result.Add(new() { Old = oldInfo, New = newInfo });
                        }

                        stratAt = startAtElement.GetInt32();
                        total = totalElement.GetInt32();
                        stratAt += maxResultsElement.GetInt32();

                        if (result.Count >= maxJiraCount)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            SaveIssues(result.Select(i => i.New).ToList());
            return result;
        }

        public async Task<IEnumerable<JiraFilter>> GetNeedRefreshFavouriteFilterAsync()
        {
            var filters = await GetCurrentUserFavouriteFilterAsync();
            return filters.Where(f => Config.NeedAutoRefreshFliterNames.Contains(f.Name));
        }

        #endregion

        public void SaveIssues(List<JiraInfo> issues)
        {
            foreach (JiraInfo issue in issues)
            {
                var localIssue = _dataService.SelectOneByExpression<JiraInfo>(Query.EQ(nameof(JiraInfo.JiraId), issue.JiraId));
                if (localIssue != null)
                {
                    issue.Id = localIssue.Id;
                }
            }
            _dataService.InsertOrUpdateMany(issues);
        }

        public JiraInfo? GetIssueFromLocalByJiraId(string jiraId)
        {
            return _dataService.SelectOneByExpression<JiraInfo>(Query.EQ(nameof(JiraInfo.JiraId), jiraId));
        }

        #region link svn

        public IEnumerable<SvnLog> GetSvnLogByJiraIdLocal(string jiraId, string path)
        {
            return _dataService.SelectByExpression<SvnLog>(Query.And(
                                                                     Query.EQ(nameof(SvnLog.SvnPath), path),
                                                                     Query.Or(
                                                                        Query.EQ(nameof(SvnLog.IssueJiraId), jiraId),
                                                                        Query.EQ(nameof(SvnLog.SubIssueJiraId), jiraId)
                                                           )));
        }

        public IEnumerable<SvnPath> GetRelatSvnPath(JiraInfo jiraInfo)
        {
            if (jiraInfo.FixVersionsName == null || !jiraInfo.FixVersionsName.Any())
            {
                return [];
            }
            var relations = _dataService.SelectByExpression<JiraSvnPathRelation>(Query.In(nameof(JiraSvnPathRelation.FixVersion), jiraInfo.FixVersionsName.Select(v => new BsonValue(v))));
            var svnPaths = _dataService.SelectByExpression<SvnPath>(Query.In(nameof(SvnPath.Path), relations.Select(v => new BsonValue(v.SvnPath))));
            return svnPaths;
        }

        #endregion

        #region upload

        private List<OperationModel> InitializeOperations()
        {
            #region options

            FieldOption[] uploadStateOpions = [
                new (){ OptionId = "-1", OptionName = "无" },
                new (){ OptionId = "17113", OptionName = "文档" },
                new (){ OptionId = "17114", OptionName = "脚本" },
                new (){ OptionId = "17117", OptionName = "脚本（包含视图、报表）" },
                new (){ OptionId = "17115", OptionName = "文档&脚本" },
                new (){ OptionId = "17118", OptionName = "文档&脚本（包含视图、报表）" },
                new (){ OptionId = "17116", OptionName = "不提交任何内容" },
                ];

            #endregion

            #region fields

            var summaryField = new FieldModel
            {
                FieldName = "概要",
                FieldId = "summary",
                Type = FieldType.TextBox,

                FieldValue = string.Empty,
            };

            var componentsField = new FieldModel
            {
                FieldName = "模块",
                FieldId = "components",
                Type = FieldType.ListBox,

                Options = [],
                SelectedValues = []
            };

            var developDescriptionField = new FieldModel
            {
                FieldName = "开发说明（原因分析/解决方案等）",
                FieldId = "customfield_10910",
                Type = FieldType.BigTextBox,

                FieldValue = string.Empty,
            };

            var testSuggestionField = new FieldModel
            {
                FieldName = "测试建议",
                FieldId = "customfield_11700",
                Type = FieldType.BigTextBox,

                FieldValue = string.Empty,
            };

            var uploadStateField = new FieldModel
            {
                FieldName = "文档/脚本是否提交★",
                FieldId = "customfield_11003",
                Type = FieldType.ComboBox,

                Options = uploadStateOpions,
                SelectedValues = []
            };

            var descriptionField = new FieldModel
            {
                FieldName = "描述",
                FieldId = "description",
                Type = FieldType.BigTextBox,

                FieldValue = string.Empty,
            };

            var expectedHangOverDateField = new FieldModel
            {
                FieldName = "预计移交日期",
                FieldId = "customfield_13202",
                Type = FieldType.DatePicker,

                FieldValue = string.Empty,
            };

            #endregion

            #region operations
            var operations = new List<OperationModel>();

            var updateInfoOperation = new OperationModel
            {
                OperationName = "更新开发要素",
                OperationId = "821"
            };
            updateInfoOperation.Fields.Add(summaryField);
            updateInfoOperation.Fields.Add(componentsField);
            updateInfoOperation.Fields.Add(developDescriptionField);
            updateInfoOperation.Fields.Add(testSuggestionField);
            updateInfoOperation.Fields.Add(uploadStateField);
            updateInfoOperation.Fields.Add(descriptionField);
            updateInfoOperation.Fields.Add(expectedHangOverDateField);

            #endregion

            operations.Add(updateInfoOperation);

            return operations;
        }

        public async Task<List<Transition>> GetTransitionsByIssueId(string issueId)
        {
            List<Transition> results = [];
            if (_httpClient == null || string.IsNullOrEmpty(issueId))
            {
                return results;
            }
            _notificationService.DebugMessage($"{nameof(GetIssueAsync)} [rest/api/2/issue/{issueId}/transitions]");
            var response = await _httpClient.GetAsync($"rest/api/2/issue/{issueId}/transitions");
            if (!response.IsSuccessStatusCode)
            {
                return results;
            }
            var jsonResponse = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(jsonResponse);

            var transitionsElement = doc.RootElement.GetProperty("transitions");
            foreach (var transition in transitionsElement.EnumerateArray())
            {
                Transition result = new()
                {
                    TransitionId = transition.GetProperty("id").GetString() ?? string.Empty,
                    TransitionName = transition.GetProperty("name").GetString() ?? string.Empty,
                };
                results.Add(result);
            }

            return results;
        }

        #endregion
    }

    public struct JiraInfoCompareTuple
    {
        public JiraInfo? Old;
        public JiraInfo New;
    }
}
