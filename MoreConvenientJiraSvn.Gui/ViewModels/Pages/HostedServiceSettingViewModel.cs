using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class HostedServiceSettingViewModel(SettingService settingService,
    DownloadSvnLogHostedService svnLogHostedService,
    CheckJiraStateHostedService jiraStateHostedService,
    CheckSqlHostedService checkSqlHostedService) : ObservableObject
{
    private readonly SettingService _settingService = settingService;

    private readonly DownloadSvnLogHostedService _downloadLogHostedService = svnLogHostedService;
    private readonly CheckJiraStateHostedService _checkJiraHostedService = jiraStateHostedService;
    private readonly CheckSqlHostedService _checkSqlHostedService = checkSqlHostedService;

    [ObservableProperty]
    private BackgroundTaskConfig _hostedServiceConfig = new();

    public void InitViewModel()
    {
        HostedServiceConfig = _settingService.GetSingleSettingFromDatabase<BackgroundTaskConfig>() ?? new();
    }

    [RelayCommand]
    public void SaveConfig()
    {
        _settingService.InsertOrUpdateSettingIntoDatabase(HostedServiceConfig);
    }
}
