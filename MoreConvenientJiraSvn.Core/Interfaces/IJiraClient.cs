
using MoreConvenientJiraSvn.Core.Models;

namespace MoreConvenientJiraSvn.Core.Interfaces;

public interface IJiraClient
{
    bool InitHttpClient(JiraConfig jiraConfig);

    Task<JiraConfig> GetUserInfoAsync(JiraConfig jiraConfig);

    Task<List<JiraFilter>> GetUserFavouriteFilterAsync();

    Task<IssueInfo?> GetIssueAsync(string issueId);

    Task<IssuePageInfo> GetIssuesAsyncByUrl(string searchUrl, int startAt = 0);

    Task<List<JiraTransition>> GetTransitionsByIssueId(string issueId);

}
