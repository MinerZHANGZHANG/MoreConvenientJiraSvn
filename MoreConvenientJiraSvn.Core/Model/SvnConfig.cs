using LiteDB;

namespace MoreConvenientJiraSvn.Core.Model;

public record SvnConfig
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.Empty;
    public string UserName { get; set; } = string.Empty;
    // Yes, no hash, use really value :), take care your data file
    public string UserPassword { get; set; } = string.Empty;
}

public record SvnPath
{
    [BsonId]
    public required ObjectId Id { get; set; } = ObjectId.NewObjectId();
    public required ObjectId SvnConfigId { get; set; } = ObjectId.Empty;
    public required string Path { get; set; } = string.Empty;
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
