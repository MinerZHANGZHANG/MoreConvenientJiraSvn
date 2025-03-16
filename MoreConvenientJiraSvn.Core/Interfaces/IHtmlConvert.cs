using MoreConvenientJiraSvn.Core.Models;

namespace MoreConvenientJiraSvn.Core.Interfaces;

public interface IHtmlConvert
{
    Task<List<JiraField>> ConvertHtmlToJiraFieldsAsync(string htmlFormString, CancellationToken cancellationToken);
}
