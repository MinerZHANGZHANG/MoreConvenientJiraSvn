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
    public string SvnLogTimeRangeText => $"日期:{OldestSvnLog?.DateTime.ToString("yy-MM-dd HH:mm") ?? "无"} -> {NewestSvnLog?.DateTime.ToString("yy-MM-dd HH:mm") ?? "无"}";

    private CancellationTokenSource? _cancellationTokenSource;

    private void InitJiraIssueSvnLogLink()
    {
        _selectedIssueChanged += JiraIssueSvnLogLink_SelectedIssueChanged;
        SelectedIssueSvnLogs.CollectionChanged += SelectedIssueSvnLogs_CollectionChanged;
    }

    private void JiraIssueSvnLogLink_SelectedIssueChanged(object? sender, JiraIssue issue)
    {
        IssueRelatedSvnPaths = _jiraService.GetRelatSvnPath(issue);
        SelectedSvnPath = IssueRelatedSvnPaths.FirstOrDefault();

        _cancellationTokenSource?.Cancel();

        LoadSvnLogByFromLocal();
    }

    private void SelectedIssueSvnLogs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        NewestSvnLog = SelectedIssueSvnLogs?.FirstOrDefault();
        OldestSvnLog = SelectedIssueSvnLogs?.LastOrDefault();
    }

    public void LoadSvnLogByFromLocal()
    {
        if (SelectedJiraIssue == null || SelectedSvnPath == null)
        {
            return;
        }

        SelectedIssueSvnLogs = [.. _jiraService.GetSvnLogByJiraIdLocal(SelectedJiraIssue.IssueKey, SelectedSvnPath.Path).OrderByDescending(log => log.DateTime)];
    }

    [RelayCommand(CanExecute = nameof(CanQuerySvnLog))]
    public async Task LoadSvnLogFromServerAsync()
    {
        if (SelectedJiraIssue == null || SelectedSvnPath == null)
        {
            return;
        }

        _cancellationTokenSource = new();
        SelectedIssueSvnLogs = [.. await _svnService.GetSvnLogsAsync(SelectedSvnPath.Path,
                                                            BeginDate,
                                                            EndDate,
                                                            1000,
                                                            SelectedSvnPath.IsNeedExtractJiraId,
                                                            _cancellationTokenSource.Token)];
    }

}
