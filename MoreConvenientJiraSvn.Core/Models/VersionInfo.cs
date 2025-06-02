using MoreConvenientJiraSvn.Core.Enums;

namespace MoreConvenientJiraSvn.Core.Models;

public record VersionInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime PublishTime { get; set; }
    public BuildType BuildType { get; set; }
    public ReleaseAsset[] ReleaseInfos { get; set; } = [];
}

public record ReleaseAsset
{
    public string Name { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long Size { get; set; }
    public BuildType BuildType { get; set; } = BuildType.Unknown;
    public bool IsPreRelease { get; set; }
}
