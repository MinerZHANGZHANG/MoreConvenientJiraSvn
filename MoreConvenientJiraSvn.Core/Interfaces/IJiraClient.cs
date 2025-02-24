
using MoreConvenientJiraSvn.Core.Models;

namespace MoreConvenientJiraSvn.Core.Interfaces;

public interface IJiraClient
{
    bool InitHttpClient(JiraConfig jiraConfig);

    Task<JiraConfig> GetUserInfoAsync(JiraConfig jiraConfig, CancellationToken cancellationToken = default);

    Task<List<JiraIssueFilter>> GetUserFavouriteFilterAsync(CancellationToken cancellationToken = default);

    Task<JiraIssue?> GetIssueAsync(string issueId, CancellationToken cancellationToken = default);

    Task<IssuePageInfo> GetIssuesAsyncByUrl(string searchUrl, int startAt = 0, CancellationToken cancellationToken = default);

    Task<List<JiraTransition>> GetTransitionsByIssueId(string issueId, CancellationToken cancellationToken = default);

}
