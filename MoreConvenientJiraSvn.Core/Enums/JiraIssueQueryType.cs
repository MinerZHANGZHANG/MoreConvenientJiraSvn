using System.ComponentModel;

namespace MoreConvenientJiraSvn.Core.Enums;

public enum JiraIssueQueryType
{
    [Description("Jira编号")]
    JiraId,
    [Description("JQL")]
    Jql,
    [Description("筛选器")]
    Filter,
}
