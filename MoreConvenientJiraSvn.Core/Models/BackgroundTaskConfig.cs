using LiteDB;

namespace MoreConvenientJiraSvn.Core.Models;

public record BackgroundTaskConfig
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.Empty;

    // Todo: Split each service
    public bool IsEnableTask { get; set; }
    public DateTime ExecutionTime { get; set; }
    public int RetryIntervalMinutes { get; set; }
    public int MaxRetryCount { get; set; }

    public List<string> CheckSqlDirectoies { get; set; } = [];

    public List<string> NeedAutoRefreshFliterNames { get; set; } = [];
    public List<string> NeedAutoRefreshSvnPaths { get; set; } = [];


}

