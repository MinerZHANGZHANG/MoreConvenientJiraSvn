using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class MainControlViewModel : ObservableObject
{
    private readonly IRepository _repository;
    [ObservableProperty]
    private ObservableCollection<BackgroundTaskMessage> _pluginMessages;

    public MainControlViewModel(IRepository repository)
    {
        this._repository = repository;
        PluginMessages = [.. _repository.FindAll<BackgroundTaskMessage>()];
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
        PluginMessages = [.. _repository.FindAll<BackgroundTaskMessage>()];
    }
}

