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

        [ObservableProperty]
        private List<JiraFilterItem> _filters = [];

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

            Filters = (await _jiraService.GetCurrentUserFavouriteFilterAsync()).Select(f => new JiraFilterItem() { Name = f.Name }).ToList();
            foreach (var name in Config.NeedAutoRefreshFliterNames)
            {
                var selectedFilter = Filters.FirstOrDefault(f => f.Name == name);
                if (selectedFilter != null)
                {
                    selectedFilter.IsChecked = true;
                }
            }

            OnPropertyChanged(nameof(Config));
            OnPropertyChanged(nameof(Filters));
        }

        public void  UpdateFilter()
        {
            if (Config == null)
            {
                return;
            }
            Config.NeedAutoRefreshFliterNames = Filters.Where(f => f.IsChecked).Select(f => f.Name).ToList();
        }
    }

    public class JiraFilterItem
    {
        public required string Name { get; set; }
        public bool IsChecked { get; set; } = false;
    }
}
