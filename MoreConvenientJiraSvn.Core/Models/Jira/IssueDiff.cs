
namespace MoreConvenientJiraSvn.Core.Models;

public class IssueDiff
{
    public JiraIssue? Old { get; set; }
    public required JiraIssue New { get; set; }
}

