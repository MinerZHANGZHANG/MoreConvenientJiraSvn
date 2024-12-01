using LiteDB;
using MoreConvenientJiraSvn.Core.Model;
using SharpSvn;
using System.Text.RegularExpressions;

namespace MoreConvenientJiraSvn.Core.Service;

public class SvnService : IDisposable
{
    private readonly SettingService _settingService;
    private readonly DataService _dataService;

    private SvnClient? _client;

    public SvnConfig? Config { get; private set; }
    public List<SvnPath> Paths { get; private set; } = [];

    public SvnService(SettingService settingService, DataService dataService)
    {
        this._settingService = settingService;
        this._dataService = dataService;

        this.Config = _settingService.GetSingleSettingFromDatabase<SvnConfig>();
        this.Paths = _settingService.GetSettingsFromDatabase<SvnPath>()?.ToList() ?? [];
        this.InitSvnClient();
    }

    public void UpdateConfig(SvnConfig config)
    {
        if (!string.IsNullOrEmpty(config.UserName) && !string.IsNullOrEmpty(config.UserPassword))
        {
            InitSvnClient();
        }

        Config = config;
        _settingService.InsertOrUpdateSettingIntoDatabase<SvnConfig>(config);
    }

    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }

    private void InitSvnClient()
    {
        _client?.Dispose();

        _client = new()
        {
            KeepSession = true,
        };

        if (Config != null)
        {
            _client.Authentication.DefaultCredentials = new System.Net.NetworkCredential(Config.UserName, Config.UserPassword);
        }
    }


    #region Path manager

    public void InsertOrUpdateSinglePath(SvnPath newPath)
    {
        var oldPath = Paths.Find(p => newPath.Path == p.Path);
        if (oldPath != null)
        {
            oldPath.LocalPath = newPath.LocalPath;
            oldPath.PathName = newPath.PathName;
            _settingService.InsertOrUpdateSettingIntoDatabase(oldPath);
        }
        else
        {
            _settingService.InsertOrUpdateSettingIntoDatabase(newPath);
        }

    }

    public void DeleteSinglePath(string path)
    {
        var oldPath = Paths.Find(p => path == p.Path);
        if (oldPath != null)
        {
            _settingService.DeleteSettingToDatabaseById<SvnPath>(oldPath.Id);
        }
    }

    public void UpdatePathMany(List<SvnPath> paths)
    {
        Paths = paths;
        _settingService.InsertOrUpdateSettingIntoDatabase(paths);
    }

    #endregion

    #region Operation methods

    public List<SvnLog> GetSvnLogs(string path, DateTime? beginTime, DateTime? endTime, int maxNumber = 200, bool isNeedExtractJiraId = false)
    {
        List<SvnLog> result = [];

        if (_client == null)
        {
            return result;
        }

        var logArgs = new SvnLogArgs()
        {
            Limit = maxNumber,
            Start = beginTime,
            End = endTime,
        };
        try
        {
            _client.GetLog(new Uri(path), logArgs, out var logEventArgs);
            foreach (var item in logEventArgs)
            {
                // the log args only accurate to the day, so filter again
                if (item.Time <= beginTime)
                {
                    continue;
                }

                SvnLog svnLog = new()
                {
                    Author = item.Author ?? string.Empty,
                    DateTime = item.Time,
                    Message = item.LogMessage ?? string.Empty,
                    SvnPath = path,
                    Revision = item.Revision,
                    ChangedUrls = [],
                    Operation = string.Empty
                };

                if (item.ChangedPaths != null && item.ChangedPaths.Count > 0)
                {
                    svnLog.ChangedUrls = item.ChangedPaths.Select(s => s.Path)
                                                    .ToList();
                    svnLog.Operation = string.Join(',', item.ChangedPaths.Select(s => s.Action.ToString())
                                                                  .Distinct());
                }

                if (isNeedExtractJiraId && !string.IsNullOrEmpty(svnLog.Message))
                {
                    (svnLog.IssueJiraId, svnLog.SubIssueJiraId) = ExtractJiraId(svnLog.Message);
                }

                result.Add(svnLog);
            }
        }
        catch (Exception ex)
        {

        }

        return result;
    }

    public List<SvnLog> GetSvnLogs(string path, long? beginRevision, long? endRevision, int maxNumber = 200, bool isNeedExtractJiraId = false)
    {
        List<SvnLog> result = [];

        if (_client == null)
        {
            return result;
        }

        var logArgs = new SvnLogArgs()
        {
            Limit = maxNumber,
            Start = beginRevision ?? 0,
            End = endRevision ?? long.MaxValue,
        };
        try
        {
            _client.GetLog(new Uri(path), logArgs, out var logEventArgs);
            foreach (var item in logEventArgs)
            {
                SvnLog svnLog = new()
                {
                    Author = item.Author ?? string.Empty,
                    DateTime = item.Time,
                    Message = item.LogMessage ?? string.Empty,
                    SvnPath = path,
                    Revision = item.Revision,
                    ChangedUrls = [],
                    Operation = string.Empty
                };

                if (item.ChangedPaths != null && item.ChangedPaths.Count > 0)
                {
                    svnLog.ChangedUrls = item.ChangedPaths.Select(s => s.Path)
                                                    .ToList();
                    svnLog.Operation = string.Join(',', item.ChangedPaths.Select(s => s.Action.ToString())
                                                                  .Distinct());
                }

                if (isNeedExtractJiraId && !string.IsNullOrEmpty(svnLog.Message))
                {
                    (svnLog.IssueJiraId, svnLog.SubIssueJiraId) = ExtractJiraId(svnLog.Message);
                }

                result.Add(svnLog);
            }
        }
        catch (Exception ex)
        {

        }

        return result;
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



    //public List<SvnLog> GetLatestLogs(string path, int maxNumber = 200)
    //{
    //    var result = _dataService.SelectByExpression<SvnLog>(Query.EQ(nameof(SvnLog.SvnPath), SelectedPath.Path))
    //        .OrderByDescending(log=>log.DateTime);
    //}

    #endregion

}
