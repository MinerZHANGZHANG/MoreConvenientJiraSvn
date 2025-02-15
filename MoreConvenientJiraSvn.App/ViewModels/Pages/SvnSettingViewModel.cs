using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Service;
using MoreConvenientJiraSvn.Core.Utils;
using System.Collections.ObjectModel;
using System.Windows;
using MoreConvenientJiraSvn.Core.Enums;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class SvnSettingViewModel : ObservableObject
{
    private readonly SettingService _settingService;
    [ObservableProperty]
    private SvnConfig? _svnConfig;

    [ObservableProperty]
    private ObservableCollection<SvnPath> _svnPaths;

    public List<EnumDescription> SvnPathTypes { get; set; } = EnumHelper.GetEnumDescriptions<SvnPathType>();

    public SvnSettingViewModel(SettingService settingService)
    {
        _settingService = settingService;

        SvnConfig = _settingService.FindSetting<SvnConfig>() ?? new();
        SvnPaths = [.. _settingService.FindSettings<SvnPath>() ?? []];
    }

    [RelayCommand]
    public void UpdateConfig()
    {
        if (SvnConfig == null)
        {
            return;
        }
        
        _settingService.UpsertSetting(SvnConfig);
        OnPropertyChanged(nameof(SvnConfig));
    }


    [RelayCommand]
    public void UpdatePath(object parameter)
    {
        if (parameter is SvnPath path)
        {
            if (SvnPaths.Any(p => p.Path == path.Path))
            {
                MessageBox.Show($"已经有一个svn地址为{path.Path}了!");
                return;
            }

            SvnPaths.Add(path);
            _settingService.UpsertSettings(SvnPaths);
            
            MessageBox.Show($"保存{path.Path}成功!");
        }
    }

    [RelayCommand]
    public void DeletePath(object parameter)
    {
        if (parameter is SvnPath path)
        {
            SvnPaths.Remove(path);
            _settingService.UpsertSettings(SvnPaths);

            MessageBox.Show($"删除{path.Path}成功!");
        }
    }

}
