using LiteDB;

namespace MoreConvenientJiraSvn.Core.Models;

public record SvnJiraLinkSetting
{
    /// <summary>
    /// Global setting
    /// </summary>
    public ObjectId Id { get; set; } = ObjectId.Empty;

}
