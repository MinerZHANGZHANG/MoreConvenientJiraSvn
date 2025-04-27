using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.Core.Models;
using System.Collections.ObjectModel;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class JiraIssueBrowseViewModel
{
    [ObservableProperty]
    private IEnumerable<SvnPath> _issueRelatedSvnPaths = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadSvnLogFromServerCommand))]
    [NotifyPropertyChangedFor(nameof(CanQuerySvnLog))]
    private SvnPath? _selectedSvnPath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSvnLog))]
    private ObservableCollection<SvnLog> _selectedIssueSvnLogs = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SvnLogVersionRangeText))]
    [NotifyPropertyChangedFor(nameof(SvnLogTimeRangeText))]
    private SvnLog? _newestSvnLog;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SvnLogVersionRangeText))]
    [NotifyPropertyChangedFor(nameof(SvnLogTimeRangeText))]
    private SvnLog? _oldestSvnLog;

    [ObservableProperty]
    private DateTime _beginDate;

    [ObservableProperty]
    private DateTime _endDate;

    public bool CanQuerySvnLog => !string.IsNullOrEmpty(SelectedSvnPath?.Path);

    public bool HasSvnLog => SelectedIssueSvnLogs.Count > 0;

    public string SvnLogVersionRangeText => $"版本:{OldestSvnLog?.Revision.ToString() ?? "无"} -> {NewestSvnLog?.Revision.ToString() ?? "无"}";
    public string SvnLogTimeRangeText => $"现有数据日期:{OldestSvnLog?.DateTime.ToString("yy-MM-dd HH:mm") ?? "无"} -> {NewestSvnLog?.DateTime.ToString("yy-MM-dd HH:mm") ?? "无"}";

    private CancellationTokenSource? _cancellationTokenSource;

    private void InitJiraIssueSvnLogLink()
    {
        _selectedIssueChanged += JiraIssueSvnLogLink_SelectedIssueChanged;
    }

    private void JiraIssueSvnLogLink_SelectedIssueChanged(object? sender, JiraIssue issue)
    {
        IssueRelatedSvnPaths = _jiraService.GetRelatSvnPath(issue);
        SelectedSvnPath = IssueRelatedSvnPaths.FirstOrDefault();

        _cancellationTokenSource?.Cancel();

        LoadSvnLogByFromLocal();
    }

    partial void OnSelectedIssueSvnLogsChanged(ObservableCollection<SvnLog> value)
    {
        NewestSvnLog = SelectedIssueSvnLogs?.FirstOrDefault();
        OldestSvnLog = SelectedIssueSvnLogs?.LastOrDefault();
    }

    public void LoadSvnLogByFromLocal()
    {
        if (SelectedJiraIssue == null || SelectedSvnPath == null)
        {
            SelectedIssueSvnLogs = [];
            return;
        }

        SelectedIssueSvnLogs = [.. _jiraService.GetSvnLogByJiraIdLocal(SelectedJiraIssue.IssueKey, SelectedSvnPath.Path).OrderByDescending(log => log.DateTime)];

        if (SelectedIssueSvnLogs.Any())
        {
            BeginDate = SelectedIssueSvnLogs.Max(l => l.DateTime);
            EndDate = DateTime.Today.AddDays(1);
        }
        else
        {
            BeginDate = DateTime.Today.AddDays(-7);
            EndDate = DateTime.Today.AddDays(1);
        }
    }

    [RelayCommand(CanExecute = nameof(CanQuerySvnLog))]
    public async Task LoadSvnLogFromServerAsync()
    {
        if (SelectedJiraIssue == null || SelectedSvnPath == null)
        {
            return;
        }

        _cancellationTokenSource = new();

        try
        {
            var svnLogs = await _svnService.GetSvnLogsAsync(SelectedSvnPath.Path,
                                                            BeginDate,
                                                            EndDate,
                                                            1000,
                                                            SelectedSvnPath.IsNeedExtractJiraId,
                                                            _cancellationTokenSource.Token);
            _repository.Upsert(svnLogs.AsEnumerable());
            SelectedIssueSvnLogs = [.. SelectedIssueSvnLogs.UnionBy(
                svnLogs.Where(log => log.IssueJiraId == SelectedJiraIssue.IssueId || log.SubIssueJiraId == SelectedJiraIssue.IssueId),
                l => l.Revision)];

            MessageQueue.Enqueue($"刷新Log成功，查找到{svnLogs.Count}条数据");
        }
        catch (Exception ex)
        {
            MessageQueue.Enqueue($"刷新Log失败: {ex.Message}");
        }
    }

}
