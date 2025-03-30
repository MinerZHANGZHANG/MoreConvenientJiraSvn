
using MoreConvenientJiraSvn.Core.Models;
using System.IO;

namespace MoreConvenientJiraSvn.Core.Interfaces;

public interface IJiraClient
{
    bool InitHttpClient(JiraConfig jiraConfig);

    Task<JiraConfig> GetUserInfoAsync(JiraConfig jiraConfig, CancellationToken cancellationToken = default);

    Task<List<JiraIssueFilter>> GetUserFavouriteFilterAsync(CancellationToken cancellationToken = default);

    Task<JiraIssue?> GetIssueAsync(string issueId, CancellationToken cancellationToken = default);

    Task<IssuePageInfo> GetIssuesAsyncByUrl(string searchUrl, int startAt = 0, CancellationToken cancellationToken = default);

    Task<IssuePageInfo> GetIssuesAsyncByJql(string jql, int startAt = 0, CancellationToken cancellationToken = default);

    Task<List<JiraTransition>> GetTransitionsByIssueId(string issueId, CancellationToken cancellationToken = default);

    Task<string> GetTransitionFormAsync(string issueId, string transitionId, CancellationToken cancellationToken = default);

    Task<string> TryPostTransitionsAsync(string issueKey, string transitionId, IEnumerable<JiraField> jiraFields, CancellationToken cancellationToken = default);

    Task<int> DownloadIssueAttachmentAsync(string issueKey, string absolutePath, CancellationToken cancellationToken = default);
}
