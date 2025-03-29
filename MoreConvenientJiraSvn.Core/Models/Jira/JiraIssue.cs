using LiteDB;
using MoreConvenientJiraSvn.Core.Attributes;
using MoreConvenientJiraSvn.Core.Enums;

namespace MoreConvenientJiraSvn.Core.Models;

public record JiraIssue
{
    #region Main Property

    [BsonId]
    [IssueJsonMapping("id", null, JsonPositionType.Main)]
    public string IssueId { get; set; } = string.Empty;

    [IssueJsonMapping("key", null, JsonPositionType.Main)]
    public string IssueKey { get; set; } = string.Empty;

    [IssueJsonMapping("self", null, JsonPositionType.Main)]
    public string SelfUrl { get; set; } = string.Empty;

    #endregion

    #region Field Property

    [IssueJsonMapping("key", null, JsonPositionType.Parent)]
    public string ParentIssueKey { get; set; } = string.Empty;
    [IssueJsonMapping("id", null, JsonPositionType.Parent)]
    public string ParentIssueId { get; set; } = string.Empty;
    [IssueJsonMapping("self", null, JsonPositionType.Parent)]
    public string ParentIssueUrl { get; set; } = string.Empty;
    [IssueJsonMapping("fields", "summary", JsonPositionType.Parent)]
    public string ParentIssueSummary { get; set; } = string.Empty;
    [IssueJsonMapping("resolution", "name")]
    public string ResolutionName { get; set; } = string.Empty;
    [IssueJsonMapping("creator", "displayName")]
    public string CreatorName { get; set; } = string.Empty;
    [IssueJsonMapping("summary")]
    public string Summary { get; set; } = string.Empty;
    [IssueJsonMapping("status", "name")]
    public string StatusName { get; set; } = string.Empty;
    [IssueJsonMapping("priority", "name")]
    public string PriorityName { get; set; } = string.Empty;
    [IssueJsonMapping("issuetype", "name")]
    public string IssueTypeName { get; set; } = string.Empty;
    [IssueJsonMapping("fixVersions", "name")]
    public List<string> FixVersions { get; set; } = [];
    public string FixVersionsText
    {
        get
        {
            if (FixVersions != null && FixVersions.Count != 0)
            {
                return string.Join(",", FixVersions);
            }
            return string.Empty;
        }
    }
    [IssueJsonMapping("description")]
    public string Descrpition { get; set; } = string.Empty;
    [IssueJsonMapping("resolutiondate")]
    public DateTime ResolutionDate { get; set; }

    #region Custom Property

    [IssueJsonMapping("customfield_12813", "value")]
    public string DeveloperClassName { get; set; } = string.Empty;

    [IssueJsonMapping("customfield_10514", "displayName")]
    public List<string> DeveloperNames { get; set; } = [];
    public string DeveloperNamesString
    {
        get
        {
            if (DeveloperNames != null && DeveloperNames.Any())
            {
                return string.Join(",", DeveloperNames);
            }
            return string.Empty;
        }
    }

    [IssueJsonMapping("customfield_10603", "displayName")]
    public List<string> ReviewerNames { get; set; } = [];
    public string ReviewerNamesString
    {
        get
        {
            if (ReviewerNames != null && ReviewerNames.Any())
            {
                return string.Join(",", ReviewerNames);
            }
            return string.Empty;
        }
    }

    [IssueJsonMapping("customfield_11300", "displayName")]
    public List<string> TesterNames { get; set; } = [];
    public string TesterNamesString
    {
        get
        {
            if (TesterNames != null && TesterNames.Any())
            {
                return string.Join(",", TesterNames);
            }
            return string.Empty;
        }
    }

    [IssueJsonMapping("customfield_13100")]
    public float ExpectedDevelopDuringDays { get; set; }

    [IssueJsonMapping("customfield_12502")]
    public float FactSolveDuringDays { get; set; }

    [IssueJsonMapping("customfield_14022")]
    public string ResultDescription { get; set; } = string.Empty;

    [IssueJsonMapping("customfield_14023")]
    public string SolutionDescription { get; set; } = string.Empty;

    [IssueJsonMapping("customfield_11700")]
    public string TestSuggestion { get; set; } = string.Empty;

    [IssueJsonMapping("customfield_13602", "name")]
    public List<string> MergedVersions { get; set; } = [];

    [IssueJsonMapping("customfield_14216")]
    public string TestPlatformUrl { get; set; } = string.Empty;

    #endregion

    #endregion

    #region jira JSQL

    // use the "subtasks" node instead
    public string ChildrenIssuesJql => $"parent={IssueKey}";
    public string ChildrenIssuesLimitFieldsJql => $"parent={IssueKey}&fields=key,summary,status";

    #endregion


}
