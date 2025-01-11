using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.Core.Model;
using MoreConvenientJiraSvn.Core.Service;

namespace MoreConvenientJiraSvn.Gui.ViewModel
{
    public partial class MainControlViewModel : ObservableObject
    {
        private readonly DataService _dataService;
        [ObservableProperty]
        private ObservableCollection<PluginMessage> _pluginMessages;

        public MainControlViewModel(DataService dataService)
        {
            this._dataService = dataService;
            PluginMessages = [.. _dataService.SelectAll<PluginMessage>()];
        }

        [RelayCommand]
        public void OpenPluginPage(string pluginName)
        {
            var plugin = PluginsManager.plugins.FirstOrDefault(p => p.PluginInfo.Name == pluginName);
            plugin?.OpenWindow();
        }

        [RelayCommand]
        public void RefreshMessage()
        {
            PluginMessages = [.. _dataService.SelectAll<PluginMessage>()];
        }
    }
}

