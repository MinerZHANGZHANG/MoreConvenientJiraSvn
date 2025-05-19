using LiteDB;
using MoreConvenientJiraSvn.Core.Enums;

namespace MoreConvenientJiraSvn.Core.Models;

public record BackgroundTaskMessage
{
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    public ObjectId? LogId { get; set; }
    public string Info { get; set; } = string.Empty;
    public InfoLevel Level { get; set; }
}

