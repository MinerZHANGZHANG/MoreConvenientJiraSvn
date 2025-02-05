using LiteDB;

namespace MoreConvenientJiraSvn.Core.Models;

public record BackgroundTaskConfig
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.Empty;

    public List<string> CheckSqlDirectoies { get; set; } = [];

    public List<string> NeedAutoRefreshFliterNames { get; set; } = [];
}

