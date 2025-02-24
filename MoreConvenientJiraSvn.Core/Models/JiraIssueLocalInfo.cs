using LiteDB;

namespace MoreConvenientJiraSvn.Core.Models;

public record JiraIssueLocalInfo
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    public string IssueKey { get; set; } = string.Empty;
    public string LocalDir { get; set; } = string.Empty;
}
