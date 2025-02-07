using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.Plugin;
using System.Collections.ObjectModel;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class GetPluginControlViewModel : ObservableObject
{
    private const int PageSize = 4;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _currentPageIndex = 0;

    [ObservableProperty]
    public ObservableCollection<IPlugin>? _plugins;

    [ObservableProperty]
    public ObservableCollection<IPlugin>? _currentPagePlugins;

    public int TotalPages => (int)Math.Ceiling((double)(Plugins?.Count ?? 0) / PageSize);


    public GetPluginControlViewModel()
    {
        Plugins = new(PluginsManager.plugins);
        RefreshCurrentPlugin();
    }

    [RelayCommand]
    private void NextPage()
    {
        CurrentPageIndex = Math.Clamp(CurrentPageIndex - 1, 0, TotalPages - 1);
        RefreshCurrentPlugin();
    }

    [RelayCommand]
    private void PreviousPage()
    {
        CurrentPageIndex = Math.Clamp(CurrentPageIndex + 1, 0, TotalPages - 1);
        RefreshCurrentPlugin();
    }

    [RelayCommand]
    public void RefreshCurrentPlugin()
    {
        if (Plugins != null && Plugins.Count > 0)
        {
            CurrentPagePlugins = new ObservableCollection<IPlugin>(Plugins.Skip((CurrentPageIndex - 1) * PageSize).Take(PageSize));
        }
    }
}
