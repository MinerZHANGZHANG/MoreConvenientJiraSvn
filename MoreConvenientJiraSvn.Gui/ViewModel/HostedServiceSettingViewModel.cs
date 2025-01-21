using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.Core.Model;
using MoreConvenientJiraSvn.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreConvenientJiraSvn.Gui.ViewModel
{
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
        private HostedServiceConfig _hostedServiceConfig = new();

        public void InitViewModel()
        {
            HostedServiceConfig = _settingService.GetSingleSettingFromDatabase<HostedServiceConfig>() ?? new();
        }

        [RelayCommand]
        public void SaveConfig()
        {
            _settingService.InsertOrUpdateSettingIntoDatabase(HostedServiceConfig);
        }
    }
}
