using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Core.Utils;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MoreConvenientJiraSvn.Infrastructure;

public class JiraClient : IJiraClient
{
    private const string UserInfoUri = "rest/api/2/myself";
    private const string UserTokenUri = "rest/pat/latest/tokens";
    private const string FavouriteFilterUri = $"rest/api/2/filter/favourite";
    private const string IssueUri = $"rest/api/2/issue";
    private JiraConfig? _config;
    private HttpClient? _httpClient;

    public bool InitHttpClient(JiraConfig config)
    {
        if (string.IsNullOrEmpty(config.BaseUrl) || string.IsNullOrEmpty(config.ApiToken))
        {
            _httpClient = null;
            return false;
        }

        try
        {
            if (!Uri.TryCreate(config.BaseUrl, UriKind.Absolute, out var result))
            {
                return false;
            }

            _httpClient = new HttpClient()
            {
                BaseAddress = result
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _config = config;

            return true;
        }
        catch
        {
            throw;
        }
    }

    public async Task<JiraIssue?> GetIssueAsync(string issueId, CancellationToken cancellationToken = default)
    {
        JiraIssue? result = default;
        if (_httpClient == null || string.IsNullOrEmpty(issueId) || cancellationToken.IsCancellationRequested)
        {
            return result;
        }

        var response = await _httpClient.GetAsync($"{IssueUri}/{issueId}", cancellationToken);
        if (!response.IsSuccessStatusCode || cancellationToken.IsCancellationRequested)
        {
            return result;
        }

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        result = IssueInfoConverter.GetIssueInfoFromJson(jsonResponse);

        return result;
    }

    public async Task<IssuePageInfo> GetIssuesAsyncByUrl(string searchUrl, int startAt = 0, CancellationToken cancellationToken = default)
    {
        IssuePageInfo result = new()
        {
            IssueInfos = [],
            MaxResults = 0,
            StartAt = startAt,
            Total = 0
        };
        List<JiraIssue> issues = [];
        StringBuilder errorMsgBuilder = new();

        if (_httpClient == null || string.IsNullOrEmpty(searchUrl) || cancellationToken.IsCancellationRequested)
        {
            return result;
        }

        var response = await _httpClient.GetAsync($"{searchUrl}&startAt={startAt}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        using JsonDocument doc = JsonDocument.Parse(jsonResponse);
        JsonElement root = doc.RootElement;

        if (root.TryGetProperty("startAt", out var startAtElement)
            && root.TryGetProperty("maxResults", out var maxResultsElement)
            && root.TryGetProperty("total", out var totalElement)
            && root.TryGetProperty("issues", out var issuesElement)
            && issuesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var issueElement in issuesElement.EnumerateArray())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return result;
                }

                var jsonString = issueElement.GetRawText();
                if (!IssueInfoConverter.TryGetIssueInfoFromJson(jsonString, out var issue, out var errorMsg) || issue == null)
                {
                    errorMsgBuilder.AppendLine($"{errorMsg}:[{jsonString}]");
                    continue;
                }
                issues.Add(issue);
            }

            result.IssueInfos = issues;
            result.StartAt = startAtElement.GetInt32();
            result.Total = totalElement.GetInt32();
            result.MaxResults = maxResultsElement.GetInt32();
            result.ErrorMessage = errorMsgBuilder.ToString();

            return result;
        }

        result.ErrorMessage = "Failed to extract issue info from json";
        return result;
    }

    public async Task<List<JiraTransition>> GetTransitionsByIssueId(string issueId, CancellationToken cancellationToken = default)
    {
        List<JiraTransition> results = [];
        if (_httpClient == null || string.IsNullOrEmpty(issueId) || cancellationToken.IsCancellationRequested)
        {
            return results;
        }

        var response = await _httpClient.GetAsync($"{IssueUri}/{issueId}/transitions", cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        using JsonDocument doc = JsonDocument.Parse(jsonResponse);

        if (doc.RootElement.TryGetProperty("transitions", out var transitionsElement))
        {
            foreach (var transition in transitionsElement.EnumerateArray())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return results;
                }

                JiraTransition result = new()
                {
                    TransitionId = transition.GetProperty("id").GetString() ?? string.Empty,
                    TransitionName = transition.GetProperty("name").GetString() ?? string.Empty,
                };
                results.Add(result);
            }
        }

        return results;
    }

    public async Task<List<JiraIssueFilter>> GetUserFavouriteFilterAsync(CancellationToken cancellationToken = default)
    {
        List<JiraIssueFilter> results = [];
        if (_httpClient == null)
        {
            return results;
        }

        var response = await _httpClient.GetAsync(FavouriteFilterUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrEmpty(jsonResponse))
        {
            throw new Exception("response is empty.");
        }

        using JsonDocument doc = JsonDocument.Parse(jsonResponse);
        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement element in doc.RootElement.EnumerateArray())
            {
                if(cancellationToken.IsCancellationRequested)
                {
                    return results; 
                }

                var filter = new JiraIssueFilter()
                {
                    FilterId = element.GetProperty("id").GetString() ?? string.Empty,
                    Name = element.GetProperty("name").GetString() ?? "Nameless",
                    Jql = element.GetProperty("jql").GetString() ?? string.Empty,
                    SearchUrl = element.GetProperty("searchUrl").GetString() ?? string.Empty,
                };
                results.Add(filter);
            }
        }

        return results;
    }

    public async Task<JiraConfig> GetUserInfoAsync(JiraConfig jiraConfig, CancellationToken cancellationToken = default)
    {
        if (_httpClient == null)
        {
            return jiraConfig;
        }

        #region get name&email
        string name = string.Empty;
        string email = string.Empty;

        var response = await _httpClient.GetAsync(UserInfoUri);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrEmpty(jsonResponse))
        {
            throw new Exception("response is empty.");
        }

        using JsonDocument userDoc = JsonDocument.Parse(jsonResponse);
        JsonElement root = userDoc.RootElement;

        if (root.TryGetProperty("name", out var nameElement)
            && root.TryGetProperty("emailAddress", out var emailElement))
        {
            name = nameElement.GetString() ?? string.Empty;
            email = emailElement.GetString() ?? string.Empty;
        }

        #endregion

        #region get token expire time
        DateTime tokenExprireTime = DateTime.MinValue;

        response = await _httpClient.GetAsync(UserTokenUri);
        response.EnsureSuccessStatusCode();

        jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrEmpty(jsonResponse))
        {
            throw new Exception("response is empty.");
        }

        // The token used is assumed to be the first
        using JsonDocument tokenDoc = JsonDocument.Parse(jsonResponse);
        if (tokenDoc.RootElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement element in tokenDoc.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("expiringAt", out var expiringAtElement))
                {
                    tokenExprireTime = expiringAtElement.GetDateTime();
                    break;
                }
            }
        }

        #endregion

        return jiraConfig with { UserName = name, Email = email, TokenExpiringTime = tokenExprireTime };
    }

}
