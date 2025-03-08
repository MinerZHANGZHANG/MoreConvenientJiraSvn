using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using SharpSvn;
using System.Text.RegularExpressions;

namespace MoreConvenientJiraSvn.Infrastructure;

public class SubversionClient : ISubversionClient
{
    private SvnClient? _client;
    private SvnConfig? _config;

    public async Task<List<SvnLog>> GetSvnLogAsync(string path, DateTime beginTime, DateTime endTime, int maxNumber = 200, bool isNeedExtractJiraId = false, CancellationToken cancellationToken = default)
    {
        if (_client == null)
        {
            return [];
        }

        var logArgs = new SvnLogArgs()
        {
            Limit = maxNumber,
            Start = beginTime,
            End = endTime,
        };
        var uri = new Uri(path);

        var logEventArgs = await GetSvnLogAsync(uri, logArgs, cancellationToken);
        var result = TransSvnlog(logEventArgs, path, isNeedExtractJiraId);

        return result;
    }

    public async Task<List<SvnLog>> GetSvnLogAsync(string path, long beginRevision, long endRevision, int maxNumber = 200, bool isNeedExtractJiraId = false, CancellationToken cancellationToken = default)
    {
        if (_client == null)
        {
            return [];
        }

        var logArgs = new SvnLogArgs()
        {
            Limit = maxNumber,
            Start = beginRevision,
            End = endRevision,
        };
        var uri = new Uri(path);
        var logEventArgs = await GetSvnLogAsync(uri, logArgs, cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            return [];
        }

        return TransSvnlog(logEventArgs, path, isNeedExtractJiraId);
    }

    public bool InitSvnClient(SvnConfig config)
    {
        _client = new()
        {
            KeepSession = true,
        };

        _config = config;
        if (config != null)
        {
            _client.Authentication.DefaultCredentials = new System.Net.NetworkCredential(config.UserName, config.UserPassword);
        }
        return true;
    }

    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<IEnumerable<SvnLogEventArgs>> GetSvnLogAsync(Uri svnUri, SvnLogArgs svnLogArgs, CancellationToken cancellationToken = default)
    {
        IEnumerable<SvnLogEventArgs> results = [];
        if (_client == null || cancellationToken.IsCancellationRequested)
        {
            return results;
        }

        await Task.Run(() =>
        {
            _client.GetLog(svnUri, svnLogArgs, out var logEventArgs);
            results = logEventArgs;
        }, cancellationToken);

        return results;
    }

    private static List<SvnLog> TransSvnlog(IEnumerable<SvnLogEventArgs> logEventArgs, string svnPath, bool isNeedExtractJiraId = false)
    {
        List<SvnLog> results = [];
        foreach (var item in logEventArgs)
        {

            SvnLog svnLog = new()
            {
                SvnPath = svnPath,

                Author = item.Author ?? string.Empty,
                DateTime = item.Time,
                Message = item.LogMessage ?? string.Empty,
                Revision = item.Revision,

            };

            if (item.ChangedPaths != null && item.ChangedPaths.Count > 0)
            {
                svnLog.ChangedUrls = item.ChangedPaths.Select(s => s.Path)
                                                .ToList();
                svnLog.Operation = string.Join(',', item.ChangedPaths.Select(s => s.Action.ToString())
                                                              .Distinct());
            }
            else
            {
                svnLog.ChangedUrls = [];
                svnLog.Operation = string.Empty;
            }

            if (isNeedExtractJiraId && !string.IsNullOrEmpty(svnLog.Message))
            {
                (svnLog.IssueJiraId, svnLog.SubIssueJiraId) = ExtractJiraId(svnLog.Message);
            }

            results.Add(svnLog);
        }
        return results;
    }

    /// <summary>
    /// According your develop environment
    /// </summary>
    /// <param name="input">log message</param>
    /// <returns>first: issue jiraId, second: sub-issue jiraId</returns>
    private static (string, string) ExtractJiraId(string input)
    {
        string issueId = string.Empty;
        string subIssueId = string.Empty;

        string pattern = @".*?需求编号:(\w+)";
        Match match = Regex.Match(input, pattern);
        if (match.Success)
        {
            issueId = match.Groups[1].Value;
        }

        pattern = @".*?缺陷编号:(\w+)";
        match = Regex.Match(input, pattern);
        if (match.Success)
        {
            subIssueId = match.Groups[1].Value;
        }
        return (issueId, subIssueId);
    }

}
