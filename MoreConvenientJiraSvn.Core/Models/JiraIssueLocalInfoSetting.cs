using LiteDB;

namespace MoreConvenientJiraSvn.Core.Models;

public record JiraIssueLocalInfoSetting
{
    public ObjectId Id { get; set; } = ObjectId.Empty;
    public string ParentDir { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}
