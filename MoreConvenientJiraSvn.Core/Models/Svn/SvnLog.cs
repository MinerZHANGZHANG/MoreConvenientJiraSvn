using LiteDB;

namespace MoreConvenientJiraSvn.Core.Models;

public record SvnLog
{
    [BsonId]
    public string Id => $"{SvnPath}_{Revision}";

    public string SvnPath { get; set; } = string.Empty;
    public long Revision { get; set; }

    public string Author { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string? Message { get; set; }

    public List<string> ChangedUrls { get; set; } = [];
    public string Operation { get; set; } = string.Empty;

    #region custom property (from message)

    public string? IssueJiraId { get; set; }
    public string? SubIssueJiraId { get; set; }

    #endregion

}
