using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Service;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class JiraSettingViewModel : ObservableObject
{
    private readonly SettingService _settingService;
    
    [ObservableProperty]
    private JiraConfig? _config;

    public JiraSettingViewModel(SettingService settingService)
    {
        _settingService = settingService;

        Config = _settingService.FindSetting<JiraConfig>() ?? new();
    }

    [RelayCommand]
    public void UpdateConfig()
    {
        if (Config == null)
        {
            return;
        }
        _settingService.UpsertSetting(Config);

        OnPropertyChanged(nameof(Config));
    }

    public void UpdateFilter()
    {
        if (Config == null)
        {
            return;
        }
    }
}
