using LiteDB;

namespace MoreConvenientJiraSvn.Core.Models;

public record LocalJiraSetting
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.Empty;
    public string ParentDir { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}
