using LiteDB;
using System.ComponentModel;

namespace MoreConvenientJiraSvn.Core.Model;

public record SvnConfig
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.Empty;
    public string UserName { get; set; } = string.Empty;
    // Yes, no hash, use really value :), take care your data file
    public string UserPassword { get; set; } = string.Empty;
    public bool IsAutoUpdateLogDaily { get; set; } = false;
}

public record SvnPath
{
    [BsonId]
    public required ObjectId Id { get; set; } = ObjectId.NewObjectId();
    public required ObjectId SvnConfigId { get; set; } = ObjectId.Empty;
    public required string Path { get; set; } = string.Empty;
    public required SvnPathType SvnPathType { get; set; } = SvnPathType.UnKnow;
    public string? LocalPath { get; set; }

    public string PathName
    {
        get
        {
            if (string.IsNullOrEmpty(_pathName))
            {
                return Path;
            }
            return _pathName;
        }
        set
        {
            _pathName = value;
        }
    }
    private string _pathName = string.Empty;
}

public enum SvnPathType
{
    [Description("未知")]
    UnKnow,
    [Description("文档")]
    Document,
    [Description("代码")]
    Code,
}
