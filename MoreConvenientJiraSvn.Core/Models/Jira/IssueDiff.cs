
namespace MoreConvenientJiraSvn.Core.Models;

public class IssueDiff
{
    public IssueInfo? Old { get; set; }
    public required IssueInfo New { get; set; }
}

