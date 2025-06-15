using MoreConvenientJiraSvn.Core.Models;

namespace MoreConvenientJiraSvn.Core.Interfaces;

public interface IPlSqlIssueChecker
{
    public List<SqlIssue> SqlIssues { get; }

    public List<SqlIssue> CheckSingleFile(string filePath, SqlCheckSetting sqlCheckSetting);

    public Task<List<SqlIssue>> CheckMultipleFilesAsync(IEnumerable<string> filePaths, SqlCheckSetting sqlCheckSetting, Action<int>? progressAction = null);

}
