using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using Microsoft.Win32;
using MoreConvenientJiraSvn.BackgroundTask;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Service;
using System.Collections.ObjectModel;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class HostedServiceSettingViewModel(SettingService settingService,
    DownloadSvnLogHostedService svnLogHostedService,
    CheckJiraStateHostedService jiraStateHostedService,
    CheckSqlHostedService checkSqlHostedService,
    JiraService jiraService,
    IRepository repository) : ObservableObject
{
    private readonly SettingService _settingService = settingService;

    private readonly DownloadSvnLogHostedService _downloadLogHostedService = svnLogHostedService;
    private readonly CheckJiraStateHostedService _checkJiraHostedService = jiraStateHostedService;
    private readonly CheckSqlHostedService _checkSqlHostedService = checkSqlHostedService;

    private readonly JiraService _jiraService = jiraService;
    private readonly IRepository _repository = repository;

    [ObservableProperty]
    private BackgroundTaskConfig _hostedServiceConfig = new();

    [ObservableProperty]
    private string _lastCheckJiraExectionTimeText = "None";
    [ObservableProperty]
    private ObservableCollection<CheckComboxItem> _jiraFilterItems = [];

    [ObservableProperty]
    private string _lastCheckSqlExectionTimeText = "None";
    [ObservableProperty]
    private ObservableCollection<string> _checkSqlDirectories = [];
    [ObservableProperty]
    private string? _selectSqlCheckDirectoryPath;

    [ObservableProperty]
    private string _lastDownloadSvnExectionTimeText = "None";
    [ObservableProperty]
    private ObservableCollection<CheckComboxItem> _svnPathItems = [];

    private bool _isInit = false;
    public async Task InitViewModel()
    {
        HostedServiceConfig = _settingService.FindSetting<BackgroundTaskConfig>() ?? new();
        _settingService.OnConfigChanged += SettingService_OnConfigChanged;

        await InitJiraStateCheckSettingAsync();
        InitSqlCheckSetting();
        InitSvnDownloadSetting();

        _isInit = true;
    }

    private void SettingService_OnConfigChanged(object? sender, ConfigChangedArgs e)
    {
        if (e.Config is IEnumerable<SvnPath>)
        {
            InitSvnDownloadSetting();
        }
    }

    private void InitSqlCheckSetting()
    {
        LastCheckSqlExectionTimeText = _repository
            .Find<BackgroundTaskLog>(Query.EQ(nameof(BackgroundTaskLog.TaskName), nameof(CheckSqlHostedService)))
            .Max(l => l.StartTime)
            .ToString("yyyy-MM-dd HH:mm:ss");

        CheckSqlDirectories = [.. HostedServiceConfig.CheckSqlDirectoies];
    }

    private async Task InitJiraStateCheckSettingAsync()
    {
        LastCheckJiraExectionTimeText = _repository
            .Find<BackgroundTaskLog>(Query.EQ(nameof(BackgroundTaskLog.TaskName), nameof(CheckJiraStateHostedService)))
            .Max(l => l.StartTime)
            .ToString("yyyy-MM-dd HH:mm:ss");

        var allFilters = (await _jiraService.GetCurrentUserFavouriteFilterAsync());
        JiraFilterItems = [.. allFilters.Select(f => new CheckComboxItem() { Name = f.Name })];
        foreach (var name in HostedServiceConfig.CheckJiraFliterNames)
        {
            var selectedFilter = JiraFilterItems.FirstOrDefault(f => f.Name == name);
            if (selectedFilter != null)
            {
                selectedFilter.IsChecked = true;
            }
        }
    }

    private void InitSvnDownloadSetting()
    {
        LastDownloadSvnExectionTimeText = _repository
            .Find<BackgroundTaskLog>(Query.EQ(nameof(BackgroundTaskLog.TaskName), nameof(DownloadSvnLogHostedService)))
            .Max(l => l.StartTime)
            .ToString("yyyy-MM-dd HH:mm:ss");

        var svnPaths = _settingService.FindSettings<SvnPath>() ?? [];
        SvnPathItems = [.. svnPaths.Select(p => new CheckComboxItem() { Name = p.PathName })];
        foreach (var path in HostedServiceConfig.CheckSvnPaths)
        {
            var selectedPath = SvnPathItems.FirstOrDefault(i => i.Name == path);
            if (selectedPath != null)
            {
                selectedPath.IsChecked = true;
            }
        }
    }

    [RelayCommand]
    public async Task ExecuteService(string serviceName)
    {
        if (serviceName == nameof(CheckJiraStateHostedService))
        {
            LastCheckJiraExectionTimeText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var isSuccess = await _checkJiraHostedService.ExecuteTask();
        }
        else if (serviceName == nameof(CheckSqlHostedService))
        {
            LastCheckSqlExectionTimeText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var isSuccess = await _checkSqlHostedService.ExecuteTask();
        }
        else if (serviceName == nameof(DownloadSvnLogHostedService))
        {
            LastDownloadSvnExectionTimeText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var isSuccess = await _downloadLogHostedService.ExecuteTask();
        }
    }

    [RelayCommand]
    public void SaveSetting()
    {
        if (!_isInit)
        {
            return;
        }

        HostedServiceConfig.CheckJiraFliterNames = [.. JiraFilterItems.Where(i => i.IsChecked).Select(i => i.Name)];
        HostedServiceConfig.CheckSqlDirectoies = [.. CheckSqlDirectories];
        HostedServiceConfig.CheckSvnPaths = [.. SvnPathItems.Where(i => i.IsChecked).Select(i => i.Name)];

        _settingService.UpsertSetting(HostedServiceConfig);
    }

    [RelayCommand]
    public void SetSqlDirectory()
    {
        var folderBrowserDialog = new OpenFolderDialog
        {
            Title = "选择Sql所在的本地文件夹"
        };

        var result = folderBrowserDialog.ShowDialog();
        if (result == true)
        {
            string selectedPath = folderBrowserDialog.FolderName;
            if (!string.IsNullOrEmpty(selectedPath))
            {
                SelectSqlCheckDirectoryPath = selectedPath;
            }
        }
    }

    [RelayCommand]
    public void AddSqlDirectory()
    {
        if (!string.IsNullOrEmpty(SelectSqlCheckDirectoryPath)
            && !CheckSqlDirectories.Contains(SelectSqlCheckDirectoryPath))
        {
            CheckSqlDirectories.Add(SelectSqlCheckDirectoryPath);

            SaveSetting();
        }
    }

    [RelayCommand]
    public void RemoveSqlDirectory(string directoryPath)
    {
        if (!string.IsNullOrEmpty(directoryPath)
            && CheckSqlDirectories.Contains(directoryPath))
        {
            CheckSqlDirectories.Remove(directoryPath);

            SaveSetting();
        }
    }
}

public class CheckComboxItem
{
    public required string Name { get; set; }
    public bool IsChecked { get; set; } = false;
}