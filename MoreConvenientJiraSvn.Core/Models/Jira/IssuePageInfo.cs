namespace MoreConvenientJiraSvn.Core.Models;

public struct IssuePageInfo
{
    public int StartAt { get; set; }
    public int MaxResults { get; set; }
    public int Total { get; set; }
    public List<JiraIssue> IssueInfos { get; set; }
    public string ErrorMessage { get; set; }
}
