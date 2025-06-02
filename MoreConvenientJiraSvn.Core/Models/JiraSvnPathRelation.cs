using LiteDB;

namespace MoreConvenientJiraSvn.Core.Models;

/// <summary>
/// In my surrounding，use FixVersions and IssueKey link svnPath to jira
/// </summary>
public record JiraSvnPathRelation
{
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();

    public string SvnPath { get; set; } = string.Empty;
    public string IssueId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

}
