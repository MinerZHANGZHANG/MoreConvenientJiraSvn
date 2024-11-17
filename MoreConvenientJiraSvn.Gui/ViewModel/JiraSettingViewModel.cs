using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.Core.Model;
using MoreConvenientJiraSvn.Core.Service;

namespace MoreConvenientJiraSvn.Gui.ViewModel
{
    public partial class JiraSettingViewModel : ObservableObject
    {
        private readonly JiraService _jiraService;

        [ObservableProperty]
        private JiraConfig? _config;


        public JiraSettingViewModel(JiraService jiraService)
        {
            this._jiraService = jiraService;

            this.Config = jiraService.Config;
        }

        [RelayCommand]
        public async Task UpdateConfig()
        {
            if (this.Config == null)
            {
                return;
            }
            await _jiraService.UpdateJiraConfig(this.Config);
            OnPropertyChanged(nameof(Config));
        }

    }
}
