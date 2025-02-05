using MoreConvenientJiraSvn.Core.Models;

namespace MoreConvenientJiraSvn.Core.Interfaces;

public interface IPlSqlCheckPipeline
{
    public List<SqlIssue> SqlIssues { get; set; }

    public List<SqlIssue> CheckSingleFile(string filePath, Dictionary<string, int> viewAlertCountDict);

}
