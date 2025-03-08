using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using MoreConvenientJiraSvn.Core.Enums;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Service;
using MoreConvenientJiraSvn.Core.Utils;
using System.Windows;
using MoreConvenientJiraSvn.Core.Interfaces;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class SvnJiraLinkViewModel(SvnService svnService, IRepository repository) : ObservableObject
{
    #region Service
    private readonly SvnService _svnService = svnService;
    private readonly IRepository _repository = repository;

    #endregion

    #region Property

    [ObservableProperty]
    private SvnConfig _config = new();

    [ObservableProperty]
    private ObservableCollection<SvnPath> _svnPaths = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageTipText))]
    [NotifyPropertyChangedFor(nameof(SelectPathTipText))]
    [NotifyPropertyChangedFor(nameof(SelectPathStateText))]
    private SvnPath? _selectedPath;

    [ObservableProperty]
    private ObservableCollection<SvnLog> _selectedPathSvnLogs = [];

    [ObservableProperty]
    private JiraSvnPathRelation _selectPathRelation = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageTipText))]
    private IEnumerable<SvnLog> _currentPageSvnLogs = [];

    public SnackbarMessageQueue SnackbarMessageQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(2d));

    private event EventHandler<SvnPath>? _selectedSvnPathChanged;


    public string PageTipText => $"{_svnLogPaginator?.CurrentPageIndex ?? 0} / {_svnLogPaginator?.GetPagesCount() ?? 0}";
    public string SelectPathTipText => $"SVN路径：{SelectedPath?.FullPathName}";
    public string SelectPathStateText
    {
        get
        {
            if (SelectedPathSvnLogs.Count > 0)
            {
                return $"已下载Log：版本号[{SelectedPathSvnLogs.Min(l => l.Revision)}——{SelectedPathSvnLogs.Max(l => l.Revision)}] | 日期[{SelectedPathSvnLogs.Min(l => l.DateTime)}——{SelectedPathSvnLogs.Max(l => l.DateTime)}]";
            }
            else
            {
                return $"本地未下载该路径的Log";
            }
        }
    }

    [ObservableProperty]
    private DateTime _beginQueryDateTime;
    [ObservableProperty]
    private DateTime _endQueryDateTime;

    private const int _singleQueryMaxLogCount = 65535;

    private PaginationHelper<SvnLog>? _svnLogPaginator;
    private const int _pageSize = 5;

    private CancellationTokenSource? _cancellationTokenSource;
    #endregion

    public void InitViewModel()
    {
        Config = _svnService.SvnConfig ?? new();
        SvnPaths = [.. _svnService.SvnPaths];

        _selectedSvnPathChanged += SvnJiraLinkViewModel_SelectedSvnPathChanged;
    }

    public void InvokeSelectedSvnPathEvent(object? sender)
    {
        if (SelectedPath == null)
        {
            return;
        }
        _selectedSvnPathChanged?.Invoke(sender, SelectedPath);
    }

    private void SvnJiraLinkViewModel_SelectedSvnPathChanged(object? sender, SvnPath e)
    {
        _cancellationTokenSource?.Cancel();

        RefreshSvnLog(e);
    }

    public void RefreshSvnLog(SvnPath svnPath)
    {
        SelectedPathSvnLogs = [.. _repository.Find<SvnLog>(Query.EQ(nameof(SvnLog.SvnPath), svnPath.Path)).OrderByDescending(s => s.DateTime)];
        SelectPathRelation = _repository.FindOne<JiraSvnPathRelation>(Query.EQ(nameof(JiraSvnPathRelation.SvnPath), svnPath.Path)) ?? new();

        _svnLogPaginator = new PaginationHelper<SvnLog>(SelectedPathSvnLogs, _pageSize);
        CurrentPageSvnLogs = _svnLogPaginator.GetCurrentItems();

        if (SelectedPathSvnLogs.Count > 0)
        {
            BeginQueryDateTime = SelectedPathSvnLogs.Max(l => l.DateTime);
            EndQueryDateTime = DateTime.Now;
        }
        else
        {
            BeginQueryDateTime = DateTime.Now.AddDays(-7);
            EndQueryDateTime = DateTime.Now;
        }
    }

    [RelayCommand]
    public async Task QuerySvnLog()
    {
        var queryRangeSpan = EndQueryDateTime - BeginQueryDateTime;
        if (queryRangeSpan <= TimeSpan.FromDays(1))
        {
            MessageBox.Show("请选择大于1天的时间范围");
        }
        if (queryRangeSpan > TimeSpan.FromDays(180))
        {
            MessageBox.Show("请选择小于180天的时间范围进行查询");
            return;
        }

        if (SelectedPath != null)
        {
            IEnumerable<SvnLog> querySvnLog = [];
            _cancellationTokenSource = new();
            await Task.Run(async () =>
            {
                bool isHaveJiraId = SelectedPath.SvnPathType == SvnPathType.Code || SelectedPath.SvnPathType == SvnPathType.Document;
                querySvnLog = await _svnService.GetSvnLogsAsync(SelectedPath.Path, BeginQueryDateTime, EndQueryDateTime, _singleQueryMaxLogCount, isHaveJiraId, _cancellationTokenSource.Token);
            });
            if (querySvnLog != null && querySvnLog.Any())
            {
                _repository.Upsert(querySvnLog);
                SelectedPathSvnLogs = [.. SelectedPathSvnLogs.Union(querySvnLog)];

                _svnLogPaginator = new PaginationHelper<SvnLog>(SelectedPathSvnLogs, _pageSize);
                CurrentPageSvnLogs = _svnLogPaginator.GetCurrentItems();

                ShowMessageSnack($"查询成功，获取数据量{querySvnLog.Count()}");
            }
            else
            {
                ShowMessageSnack($"未查询到数据，请修改范围或稍后重试");
            }
        }
    }

    [RelayCommand]
    public void SetRelationVersion()
    {
        if (SelectedPath != null && SelectPathRelation?.Version != null)
        {
            var relation = _repository.FindOne<JiraSvnPathRelation>(Query.EQ(nameof(JiraSvnPathRelation.SvnPath), SelectedPath.Path));
            if (relation != null)
            {
                relation.Version = SelectPathRelation.Version;
            }
            else
            {
                relation = SelectPathRelation;
            }
            _repository.Upsert(relation);
        }
    }

    [RelayCommand]
    public void NextPage()
    {
        if (_svnLogPaginator != null)
        {
            _svnLogPaginator.CurrentPageIndex += 1;
            CurrentPageSvnLogs = _svnLogPaginator.GetCurrentItems();
        }
    }

    [RelayCommand]
    public void PrevPage()
    {
        if (_svnLogPaginator != null)
        {
            _svnLogPaginator.CurrentPageIndex -= 1;
            CurrentPageSvnLogs = _svnLogPaginator.GetCurrentItems();
        }
    }

    private void ShowMessageSnack(string message)
    {
        SnackbarMessageQueue.Enqueue(message, "关闭", () => SnackbarMessageQueue.Clear(), true);
    }

}
