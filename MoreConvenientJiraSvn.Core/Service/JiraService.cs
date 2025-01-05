using LiteDB;
using MoreConvenientJiraSvn.Core.Model;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MoreConvenientJiraSvn.Core.Service
{
    public class JiraService
    {
        private readonly SettingService _settingService;
        private readonly DataService _dataService;
        private HttpClient? _httpClient;

        public JiraConfig Config { get; private set; }

        public JiraService(SettingService settingService, DataService dataService)
        {
            this._settingService = settingService;
            this._dataService = dataService;
            this.Config = _settingService.GetSingleSettingFromDatabase<JiraConfig>() ?? new();

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
                                Name = element.GetProperty("name").GetString(),
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

            var response = await _httpClient.GetAsync($"rest/api/2/issue/{issueId}");
            if (!response.IsSuccessStatusCode)
            {
                return result;
            }
            var jsonResponse = await response.Content.ReadAsStringAsync();

            result = new() { JiraId = issueId, JsonResult = jsonResponse };
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
                            var newInfo = new JiraInfo() { JsonResult = issue.GetRawText() };
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
            if (jiraInfo.FixVersionsName == null || jiraInfo.FixVersionsName.Any())
            {
                return [];
            }
            var relations = _dataService.SelectByExpression<JiraSvnPathRelation>(Query.In(nameof(JiraSvnPathRelation.SvnPath), jiraInfo.FixVersionsName.Select(v => new BsonValue(v))));
            var svnPaths = _dataService.SelectByExpression<SvnPath>(Query.In(nameof(SvnPath.Path), relations.Select(v => new BsonValue(v.SvnPath))));
            return svnPaths;
        }

        #endregion
    }

    public struct JiraInfoCompareTuple
    {
        public JiraInfo? Old;
        public JiraInfo New;
    }
}
