using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using MoreConvenientJiraSvn.Core.Model;
using MoreConvenientJiraSvn.Core.Service;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;


namespace MoreConvenientJiraSvn.Plugin.Jira2LocalDir;

public partial class Jira2LocalDirViewModel(ServiceProvider serviceProvider) : ObservableObject
{
    #region Service
    private readonly JiraService _jiraService = serviceProvider.GetRequiredService<JiraService>();
    private readonly SvnService _svnService = serviceProvider.GetRequiredService<SvnService>();
    private readonly DataService _dataService = serviceProvider.GetRequiredService<DataService>();
    private readonly SettingService _settingService = serviceProvider.GetRequiredService<SettingService>();

    #endregion

    #region Field & Property

    // Query jira
    public IReadOnlyList<JiraQueryType> JiraQueryTypes { get; } = Enum.GetValues<JiraQueryType>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUseTextToSearch))]
    private JiraQueryType _selectedJiraQueryType = JiraQueryType.JiraId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(QueryJiraInfoCommand))]
    [NotifyPropertyChangedFor(nameof(HaveQueryText))]
    private string? _jiraQueryText;

    public bool IsUseTextToSearch => SelectedJiraQueryType != JiraQueryType.Filter;
    private bool HaveQueryText => !string.IsNullOrEmpty(JiraQueryText);

    // Jira filter
    [ObservableProperty]
    private JiraFilter? _selectedFilter;
    [ObservableProperty]
    private List<JiraFilter>? _jiraFilters;

    // Jira list
    [ObservableProperty]
    private List<JiraInfo>? _jiraInfoList;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasJiraBeSelected))]
    [NotifyCanExecuteChangedFor(nameof(UseSelectedToCreateLocalJiraCommand))]
    [NotifyCanExecuteChangedFor(nameof(CopyCommitTextCommand))]
    [NotifyCanExecuteChangedFor(nameof(CopyAnnotationTextCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenWebPageCommand))]
    private JiraInfo? _selectedJiraInfo;

    public bool HasJiraBeSelected => SelectedJiraInfo != null;

    // Local jira info
    [ObservableProperty]
    private LocalJiraSetting _localJiraSetting = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LocalJiraOperationText))]
    private LocalJiraInfo? _selectedJiraLocalInfo;

    // Local Svn log

    [ObservableProperty]
    private List<SvnPath> _relatSvnPaths = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanQueryNextLog))]
    [NotifyPropertyChangedFor(nameof(CanQueryPrevLog))]
    [NotifyCanExecuteChangedFor(nameof(DownloadNextSvnLogByAmountCommand))]
    [NotifyCanExecuteChangedFor(nameof(DownloadPrevSvnLogByAmountCommand))]
    [NotifyCanExecuteChangedFor(nameof(DownloadNextSvnLogByDateCommand))]
    [NotifyCanExecuteChangedFor(nameof(DownloadPrevSvnLogByDateCommand))]
    private SvnPath? _selectedSvnPath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSvnLog))]
    [NotifyPropertyChangedFor(nameof(CanQueryNextLog))]
    [NotifyCanExecuteChangedFor(nameof(DownloadNextSvnLogByAmountCommand))]
    [NotifyCanExecuteChangedFor(nameof(DownloadNextSvnLogByDateCommand))]
    private List<SvnLog> _selectedJiraSvnLogs = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SvnLogVersionRangeText))]
    [NotifyPropertyChangedFor(nameof(SvnLogTimeRangeText))]
    private SvnLog? _newestSvnLog;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SvnLogVersionRangeText))]
    [NotifyPropertyChangedFor(nameof(SvnLogTimeRangeText))]
    private SvnLog? _oldestSvnLog;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanQueryNextLog))]
    [NotifyPropertyChangedFor(nameof(CanQueryPrevLog))]
    [NotifyCanExecuteChangedFor(nameof(DownloadNextSvnLogByAmountCommand))]
    [NotifyCanExecuteChangedFor(nameof(DownloadPrevSvnLogByAmountCommand))]
    [NotifyCanExecuteChangedFor(nameof(DownloadNextSvnLogByDateCommand))]
    [NotifyCanExecuteChangedFor(nameof(DownloadPrevSvnLogByDateCommand))]
    private bool _isNotQuerySvnLoging = true;

    public bool HasSvnLog => SelectedJiraSvnLogs.Count > 0;

    public bool CanQueryPrevLog => IsNotQuerySvnLoging && SelectedSvnPath != null;
    public bool CanQueryNextLog => IsNotQuerySvnLoging && SelectedSvnPath != null && HasSvnLog;

    public string SvnLogVersionRangeText => $"版本:{OldestSvnLog?.Revision.ToString() ?? "无"} -> {NewestSvnLog?.Revision.ToString() ?? "无"}";
    public string SvnLogTimeRangeText => $"日期:{OldestSvnLog?.DateTime.ToString("yy-MM-dd HH:mm") ?? "无"} -> {NewestSvnLog?.DateTime.ToString("yy-MM-dd HH:mm") ?? "无"}";

    public string LocalJiraOperationText => string.IsNullOrEmpty(SelectedJiraLocalInfo?.LocalDir) ? "新建Jira文件夹" : "打开Jira文件夹";

    #endregion

    public async Task InitViewModelAsync()
    {
        JiraFilters = await _jiraService.GetCurrentUserFavouriteFilterAsync();
        LocalJiraSetting = _settingService.GetSingleSettingFromDatabase<LocalJiraSetting>() ?? new();

        SelectedSvnPath = RelatSvnPaths.FirstOrDefault();

    }

    #region Command

    [RelayCommand]
    public async Task QueryJiraInfo()
    {
        // need add page func
        List<JiraInfo> tempJiraInfoList = [];
        switch (SelectedJiraQueryType)
        {
            case JiraQueryType.JiraId:
                var jiraInfo = await _jiraService.GetIssueAsync(JiraQueryText);
                if (jiraInfo != null)
                {
                    tempJiraInfoList.Add(jiraInfo);
                }
                break;
            case JiraQueryType.Sql:
                MessageBox.Show("暂不支持");
                break;
            case JiraQueryType.Filter:
                if (SelectedFilter != null)
                {
                    var jiraInfos = await _jiraService.GetIssuesAsyncByFilter(SelectedFilter);
                    tempJiraInfoList.AddRange(jiraInfos.Select(i => i.New));
                }
                break;
            default:
                break;
        }
        JiraInfoList = tempJiraInfoList;
    }

    public void RefreshLocalJiraInfo()
    {
        if (SelectedJiraInfo == null)
        {
            return;
        }

        SelectedJiraLocalInfo = _dataService.SelectOneByExpression<LocalJiraInfo>(BsonExpression.Create($"JiraId = \"{SelectedJiraInfo.JiraId}\""));
    }

    public void RefreshSelectPathSvnLog()
    {
        if (SelectedJiraInfo == null || SelectedSvnPath == null)
        {
            return;
        }

        RelatSvnPaths = _jiraService.GetRelatSvnPath(SelectedJiraInfo).ToList();
        SelectedJiraSvnLogs = [.. _jiraService.GetSvnLogByJiraIdLocal(SelectedJiraInfo.JiraId, SelectedSvnPath.Path).OrderByDescending(log => log.DateTime)];

        NewestSvnLog = SelectedJiraSvnLogs?.FirstOrDefault();
        OldestSvnLog = SelectedJiraSvnLogs?.LastOrDefault();
    }

    [RelayCommand(CanExecute = nameof(HasJiraBeSelected))]
    public void CopyCommitText()
    {
        if (SelectedJiraInfo == null)
        {
            return;
        }
        StringBuilder commitText = new($"版本：{SelectedJiraInfo.FixVersionNameText}\r\n");
        if (!string.IsNullOrEmpty(SelectedJiraInfo.ParentJiraId))
        {
            commitText.AppendLine($"需求编号：{SelectedJiraInfo.ParentJiraId}");
            commitText.AppendLine($"内容概要：{SelectedJiraInfo.ParentSummary}");
            commitText.AppendLine($"缺陷编号：{SelectedJiraInfo.JiraId}");
            commitText.Append($"内容概要：{SelectedJiraInfo.Summary}");
        }
        else
        {
            commitText.AppendLine($"需求编号：{SelectedJiraInfo.JiraId}");
            commitText.AppendLine($"内容概要：{SelectedJiraInfo.Summary}");
            commitText.AppendLine($"缺陷编号：");
            commitText.Append($"内容概要：");
        }

        try
        {
            Clipboard.SetText(commitText.ToString());
            MessageBox.Show("复制成功!");
        }
        catch { }
    }

    [RelayCommand(CanExecute = nameof(HasJiraBeSelected))]
    public void CopyAnnotationText()
    {
        if (SelectedJiraInfo == null)
        {
            return;
        }
        try
        {
            var text = $"{DateTime.Today:yyyy-MM-dd} {LocalJiraSetting.UserName} {SelectedJiraInfo.JiraId} {SelectedJiraInfo.Summary}";
            Clipboard.SetText(text);
            MessageBox.Show("复制成功!");
        }
        catch (Exception)
        {
        }

    }

    [RelayCommand(CanExecute = nameof(HasJiraBeSelected))]
    public void OpenWebPage()
    {
        string? url = SelectedJiraInfo?.SelfUrl;
        if (string.IsNullOrEmpty(url))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"无法打开网页: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(HasJiraBeSelected))]
    public async Task UseSelectedToCreateLocalJiraAsync()
    {
        if (SelectedJiraInfo == null)
        {
            return;
        }

        // open existing jira local directory
        if (!string.IsNullOrEmpty(SelectedJiraLocalInfo?.LocalDir))
        {
            try
            {
                Process.Start("explorer.exe", SelectedJiraLocalInfo.LocalDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return;
        }

        // create new jira local directory
        if (string.IsNullOrEmpty(LocalJiraSetting.ParentDir))
        {
            MessageBox.Show("请先设置本地Jira信息存储位置");
            return;
        }

        // Local path create dir used jira name
        if (Directory.Exists(LocalJiraSetting.ParentDir))
        {
            string dirName = $"{SelectedJiraInfo.JiraId}-{SelectedJiraInfo.Summary}";
            string fullDirName = Path.Combine(LocalJiraSetting.ParentDir, dirName);
            if (Directory.Exists(fullDirName))
            {
                MessageBox.Show($"{LocalJiraSetting.ParentDir}目录下已经有一个{dirName}文件夹了!");
                return;
            }

            DirectoryInfo directoryInfo = Directory.CreateDirectory(fullDirName);

            string commitFileFullName = Path.Combine(fullDirName, $"提交文本-{SelectedJiraInfo.JiraId}.txt");
            if (!File.Exists(commitFileFullName))
            {
                StringBuilder commitText = new($"版本：{SelectedJiraInfo.FixVersionNameText}\r\n");
                if (!string.IsNullOrEmpty(SelectedJiraInfo.ParentJiraId))
                {
                    commitText.AppendLine($"需求编号：{SelectedJiraInfo.ParentJiraId}");
                    commitText.AppendLine($"内容概要：{SelectedJiraInfo.ParentSummary}");
                    commitText.AppendLine($"缺陷编号：{SelectedJiraInfo.JiraId}");
                    commitText.Append($"内容概要：{SelectedJiraInfo.Summary}");
                }
                else
                {
                    commitText.AppendLine($"需求编号：{SelectedJiraInfo.JiraId}");
                    commitText.AppendLine($"内容概要：{SelectedJiraInfo.Summary}");
                    commitText.AppendLine($"缺陷编号：");
                    commitText.Append($"内容概要：");
                }
                await File.WriteAllTextAsync(commitFileFullName, commitText.ToString());
            }

            string documentFileFullName = Path.Combine(fullDirName, $"文档-{SelectedJiraInfo.JiraId}.txt");
            if (!File.Exists(documentFileFullName))
            {
                StringBuilder documentText = new($"一、需求描述:\r\n");
                documentText.AppendLine($"{SelectedJiraInfo.Descrpition}");
                documentText.AppendLine();
                documentText.AppendLine($"二、设计和解决方案:");
                documentText.AppendLine();
                documentText.AppendLine($"三、自测场景:");
                documentText.AppendLine();
                documentText.AppendLine($"四、测试建议:");
                documentText.AppendLine();
                documentText.AppendLine($"五、相关脚本:");

                await File.WriteAllTextAsync(documentFileFullName, documentText.ToString());
            }

            directoryInfo.CreateSubdirectory("SQL");
            directoryInfo.CreateSubdirectory("图片示例");
            directoryInfo.CreateSubdirectory("附加文档");

            // todo:support download extra file

            SelectedJiraLocalInfo = new() { JiraId = SelectedJiraInfo.JiraId, LocalDir = fullDirName };
            _dataService.InsertOrUpdate<LocalJiraInfo>(SelectedJiraLocalInfo);

            MessageBox.Show($"创建[{fullDirName}]文件夹成功!");
        }

    }

    [RelayCommand]
    public void SetLocalDirForJiraInfo()
    {
        var folderBrowserDialog = new OpenFolderDialog
        {
            Title = "选择Jira本地文件夹的创建目录"
        };

        var result = folderBrowserDialog.ShowDialog();
        if (result == true)
        {
            string selectedPath = folderBrowserDialog.FolderName;
            if (!string.IsNullOrEmpty(selectedPath))
            {
                LocalJiraSetting = new() { ParentDir = selectedPath, UserName = LocalJiraSetting.UserName };

                _settingService.InsertOrUpdateSettingIntoDatabase(LocalJiraSetting);

            }
        }
    }

    [RelayCommand]
    public void SetUserNameForJiraInfo()
    {
        if (string.IsNullOrEmpty(LocalJiraSetting.UserName))
        {
            return;
        }
        _settingService.InsertOrUpdateSettingIntoDatabase(LocalJiraSetting);
    }

    // Need cut down code lines
    [RelayCommand(CanExecute = nameof(CanQueryPrevLog))]
    public async Task DownloadPrevSvnLogByDate(object parameter)
    {
        if (SelectedJiraInfo == null || SelectedSvnPath == null
            || !int.TryParse(parameter.ToString(), out int days))
        {
            return;
        }

        IsNotQuerySvnLoging = false;
        await Task.Run(() =>
        {
            if (OldestSvnLog != null)
            {
                _svnService.GetSvnLogs(SelectedSvnPath.Path,
                    OldestSvnLog.DateTime.AddDays(-days),
                    OldestSvnLog.DateTime,
                    300,
                    SelectedSvnPath.IsNeedExtractJiraId);
            }
            else
            {
                _svnService.GetSvnLogs(SelectedSvnPath.Path,
                    DateTime.Now.AddDays(-days),
                    DateTime.Now,
                    300,
                    SelectedSvnPath.IsNeedExtractJiraId);
            }

        });
        IsNotQuerySvnLoging = true;
        RefreshSelectPathSvnLog();
    }

    [RelayCommand(CanExecute = nameof(CanQueryNextLog))]
    public async Task DownloadNextSvnLogByDate(object parameter)
    {
        if (SelectedJiraInfo == null || SelectedSvnPath == null
            || NewestSvnLog == null
            || !int.TryParse(parameter.ToString(), out int days))
        {
            return;
        }

        IsNotQuerySvnLoging = false;
        await Task.Run(() =>
        {
            if (NewestSvnLog != null)
            {
                _svnService.GetSvnLogs(SelectedSvnPath.Path,
                    NewestSvnLog.DateTime,
                    NewestSvnLog.DateTime.AddDays(days),
                    300,
                    SelectedSvnPath.IsNeedExtractJiraId);
            }
        });
        IsNotQuerySvnLoging = true;
        RefreshSelectPathSvnLog();
    }

    [RelayCommand(CanExecute = nameof(CanQueryPrevLog))]
    public async Task DownloadPrevSvnLogByAmount(object parameter)
    {
        if (SelectedJiraInfo == null || SelectedSvnPath == null
            || !int.TryParse(parameter.ToString(), out int amount))
        {
            return;
        }

        IsNotQuerySvnLoging = false;
        await Task.Run(() =>
        {
            if (OldestSvnLog != null)
            {
                _svnService.GetSvnLogs(SelectedSvnPath.Path,
                    0,
                    OldestSvnLog.Revision,
                    amount,
                    SelectedSvnPath.IsNeedExtractJiraId);
            }
            else
            {
                _svnService.GetSvnLogs(SelectedSvnPath.Path,
                    0,
                    long.MaxValue,
                    amount,
                    SelectedSvnPath.IsNeedExtractJiraId);
            }
        });
        IsNotQuerySvnLoging = true;
        RefreshSelectPathSvnLog();
    }

    [RelayCommand(CanExecute = nameof(CanQueryNextLog))]
    public async Task DownloadNextSvnLogByAmount(object parameter)
    {
        if (SelectedJiraInfo == null || SelectedSvnPath == null
            || NewestSvnLog == null
            || !int.TryParse(parameter.ToString(), out int amount))
        {
            return;
        }

        IsNotQuerySvnLoging = false;
        await Task.Run(() =>
        {
            if (NewestSvnLog != null)
            {
                _svnService.GetSvnLogs(SelectedSvnPath.Path,
                    NewestSvnLog.Revision,
                    int.MaxValue,
                    amount,
                    SelectedSvnPath.IsNeedExtractJiraId);
            }

        });
        IsNotQuerySvnLoging = true;
        RefreshSelectPathSvnLog();
    }


    #endregion
}


public enum JiraQueryType
{
    [Description("Jira编号")]
    JiraId,
    [Description("JSQL")]
    Sql,
    [Description("筛选器")]
    Filter,
}



