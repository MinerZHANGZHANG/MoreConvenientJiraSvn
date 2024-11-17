using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using MoreConvenientJiraSvn.Core.Model;
using MoreConvenientJiraSvn.Core.Service;

namespace MoreConvenientJiraSvn.Plugin.SvnJiraLink;

public partial class SvnJiraLinkViewModel(ServiceProvider serviceProvider) : ObservableObject
{
    #region Service
    private readonly SvnService _svnService = serviceProvider.GetRequiredService<SvnService>();
    private readonly DataService _dataService = serviceProvider.GetRequiredService<DataService>();
    private readonly SettingService _settingService = serviceProvider.GetRequiredService<SettingService>();

    #endregion

    #region Property

    [ObservableProperty]
    private SvnConfig _config = new();

    [ObservableProperty]
    private List<SvnPath> _svnPaths = [];

    [ObservableProperty]
    private List<SvnLog> _svnLogs = [];

    #endregion


    public void InitViewModel()
    {
        Config = _svnService.Config ?? new();
        SvnPaths = _svnService.Paths;
    }

    [RelayCommand]
    public async Task QuerySvnLog()
    {
        if (SvnPaths.Count > 0)
        {
            await Task.Run(() =>
            {
                SvnLogs = _svnService.GetSvnLogs(SvnPaths.First().Path, null, null);
            });
        }

    }

}



