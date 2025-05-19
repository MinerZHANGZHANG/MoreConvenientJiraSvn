using LiteDB;

namespace MoreConvenientJiraSvn.Core.Models;

public record JiraConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime TokenExpiringTime { get; set; }

}

