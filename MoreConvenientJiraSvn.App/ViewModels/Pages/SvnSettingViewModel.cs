using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using MoreConvenientJiraSvn.Core.Enums;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Core.Utils;
using MoreConvenientJiraSvn.Service;
using System.Collections.ObjectModel;
using System.Windows;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class SvnSettingViewModel : ObservableObject, IDisposable
{
    private readonly SettingService _settingService;
    public SnackbarMessageQueue MessageQueue { get; } = new(TimeSpan.FromSeconds(2d));
    public static List<EnumDescription> SvnPathTypes { get; set; } = EnumHelper.GetEnumDescriptions<SvnPathType>();

    [ObservableProperty]
    private SvnConfig _svnConfig = new();

    [ObservableProperty]
    private ObservableCollection<SvnPath> _svnPaths = [];

    [ObservableProperty]
    private string _svnPathCountText = "当前保存了0条Svn路径";

    public SvnSettingViewModel(SettingService settingService)
    {
        _settingService = settingService;

        SvnConfig = _settingService.FindSetting<SvnConfig>() ?? new();
        SvnPaths = [.. _settingService.FindSettings<SvnPath>() ?? []];
        SvnPathCountText = $"当前保存了{SvnPaths.Count}条Svn路径";
    }

    #region command

    [RelayCommand]
    public void SaveSvnConfig()
    {
        _settingService.UpsertSetting(SvnConfig);
    }

    [RelayCommand]
    public void SaveSvnPaths()
    {
        if (SvnPaths.GroupBy(p => p.Path).Any(g => g.Count() > 1))
        {
            ShowMessageSnack($"存在相同的svn路径,请修改");
            return;
        }

        if (SvnPaths.Any(p => string.IsNullOrWhiteSpace(p.Path)))
        {
            ShowMessageSnack($"Svn路径不能为空，请修改");
        }

        _settingService.UpsertSettings(SvnPaths);
        ShowMessageSnack($"保存Svn路径成功!");
        SvnPathCountText = $"当前保存了{SvnPaths.Count}条Svn路径";
    }

    [RelayCommand]
    public void DeleteSvnPath(object parameter)
    {
        if (parameter is not SvnPath path)
        {
            return;
        }

        SvnPaths.Remove(path);
    }

    [RelayCommand]
    public void ShowUserInfoDescription()
    {
        MessageBox.Show("似乎会自动获取本地的svn配置信息，不需要手动输入。。", "真的需要输入吗?");
    }

    #endregion

    private void ShowMessageSnack(string message)
    {
        this.MessageQueue.Enqueue(message, "关闭", () => this.MessageQueue.Clear(), true);
    }

    public void Dispose()
    {
        this.MessageQueue.Dispose();
        GC.SuppressFinalize(this);
    }
}
