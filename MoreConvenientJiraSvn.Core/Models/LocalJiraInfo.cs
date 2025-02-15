using LiteDB;

namespace MoreConvenientJiraSvn.Core.Models;

public record LocalJiraInfo
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    public string JiraId { get; set; } = string.Empty;
    public string LocalDir { get; set; } = string.Empty;
}
