using LiteDB;

namespace MoreConvenientJiraSvn.Core.Models;

public record JiraIssueFilter
{
    public required string FilterId { get; set; }
    public required string Name { get; set; }
    public required string Jql { get; set; }
    public required string SearchUrl { get; set; }
}
