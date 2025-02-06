using LiteDB;
using MoreConvenientJiraSvn.Core.Enums;

namespace MoreConvenientJiraSvn.Core.Models;

public record BackgroundTaskMessage
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    public string Info { get; set; } = string.Empty;
    public InfoLevel Level { get; set; }
}

