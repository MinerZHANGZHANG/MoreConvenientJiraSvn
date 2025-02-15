using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.BackgroundTask;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class HostedServiceSettingViewModel(SettingService settingService,
    DownloadSvnLogHostedService svnLogHostedService,
    CheckJiraStateHostedService jiraStateHostedService,
    CheckSqlHostedService checkSqlHostedService,
    JiraService jiraService) : ObservableObject
{
    private readonly SettingService _settingService = settingService;

    private readonly DownloadSvnLogHostedService _downloadLogHostedService = svnLogHostedService;
    private readonly CheckJiraStateHostedService _checkJiraHostedService = jiraStateHostedService;
    private readonly CheckSqlHostedService _checkSqlHostedService = checkSqlHostedService;
    private readonly JiraService _jiraService = jiraService;

    [ObservableProperty]
    private BackgroundTaskConfig _hostedServiceConfig = new();

    [ObservableProperty]
    private ObservableCollection<JiraFilterItem> _filters = [];

    public async Task InitViewModel()
    {
        HostedServiceConfig = _settingService.FindSetting<BackgroundTaskConfig>() ?? new();

        var allFilters = (await _jiraService.GetCurrentUserFavouriteFilterAsync());
        Filters = [.. allFilters.Select(f => new JiraFilterItem() { Name = f.Name })];

        foreach (var name in HostedServiceConfig.NeedAutoRefreshFliterNames)
        {
            var selectedFilter = Filters.FirstOrDefault(f => f.Name == name);
            if (selectedFilter != null)
            {
                selectedFilter.IsChecked = true;
            }
        }
    }

    [RelayCommand]
    public void SaveConfig()
    {
        _settingService.UpsertSetting(HostedServiceConfig);
    }
}

public class JiraFilterItem
{
    public required string Name { get; set; }
    public bool IsChecked { get; set; } = false;
}