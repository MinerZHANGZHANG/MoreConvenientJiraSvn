using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    //[ObservableProperty]
    //private IEnumerable<IPlugin> _plugins;

    //[ObservableProperty]
    //[NotifyCanExecuteChangedFor(nameof(OpenPluginViewCommand))]
    //private IPlugin? _selectedPlugin;

    //public bool HasSelectedPlugin => SelectedPlugin != null;

    //public IEnumerable<PluginInfo> PluginInfos => Plugins?.Select(p => p.PluginInfo) ?? [];

    public MainWindowViewModel()
    {
        //Plugins = PluginsManager.plugins;
    }

    [RelayCommand]
    public void OpenPluginView()
    {
        //SelectedPlugin?.OpenWindow();
    }

}
