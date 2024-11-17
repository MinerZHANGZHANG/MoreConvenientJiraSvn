using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MoreConvenientJiraSvn.Gui.ViewModel
{
    public partial class MainControlViewModel : ObservableObject
    {
        [RelayCommand]
        public void OpenPluginPage(string pluginName)
        {
            var plugin = PluginsManager.plugins.FirstOrDefault(p => p.PluginInfo.Name == pluginName);
            plugin?.OpenWindow();
        }
    }
}
