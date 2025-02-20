using LiteDB;

namespace MoreConvenientJiraSvn.Core.Models;

public record BackgroundTaskConfig
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.Empty;

    // Todo: Split each service
    public bool IsEnableBackgroundTask { get; set; } = true;
    public DateTime ExecutionTime { get; set; } = DateTime.MinValue;
    public int MaxRetryCount { get; set; } = 0;
    public int RetryIntervalMinutes { get; set; } = 5;

    public List<string> CheckJiraFliterNames { get; set; } = [];

    public List<string> CheckSqlDirectoies { get; set; } = [];

    public List<string> CheckSvnPaths { get; set; } = [];
    public int SvnLogDownloadPrevDays { get; set; } = 1;


}

