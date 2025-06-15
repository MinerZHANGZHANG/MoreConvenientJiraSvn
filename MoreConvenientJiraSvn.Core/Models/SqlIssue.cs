using MoreConvenientJiraSvn.Core.Enums;

namespace MoreConvenientJiraSvn.Core.Models;

public record SqlIssue
{
    public required string IssueType { get; set; }
    public required string FilePath { get; set; }
    public required string Message { get; set; }
    public InfoLevel Level { get; set; }

    /// <summary>
    /// 截取消息的前256个字符用于显示
    /// </summary>
    public string DisplayMessage => $"{Message[..Math.Min(256, Message.Length)]}..";
}
