using MoreConvenientJiraSvn.Core.Enums;

namespace MoreConvenientJiraSvn.Core.Models;

public record SqlIssue
{
    public required string FilePath { get; set; }
    public required string Message { get; set; }
    public InfoLevel Level { get; set; }
}
