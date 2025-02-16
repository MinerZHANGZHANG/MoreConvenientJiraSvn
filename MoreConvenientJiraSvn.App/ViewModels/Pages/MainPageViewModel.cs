using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.App.Views.Controls;
using System.Windows;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    public readonly MainControl MainControl = new();
    public readonly AppSettingControl AppSettingControl = new();
    //public readonly GetPluginControl getPluginControl = new();
    public readonly IntroduceWhatControl IntroduceWhatControl = new();
    public readonly IntroduceHowControl IntroduceHowControl = new();

    [ObservableProperty]
    private UserControl? _currentContent = null;

    [RelayCommand]
    public void SwitchContent(IndexContent indexContent)
    {
        switch (indexContent)
        {
            case IndexContent.Index:
                CurrentContent = MainControl;
                break;
            case IndexContent.Setting:
                CurrentContent = AppSettingControl;
                break;
            case IndexContent.Plugin:
                //CurrentContent = getPluginControl;
                MessageBox.Show("尚未支持");
                break;
            case IndexContent.What:
                CurrentContent = IntroduceWhatControl;
                break;
            case IndexContent.How:
                CurrentContent = IntroduceHowControl;
                break;
            case IndexContent.Expand:
                MessageBox.Show("尚未支持");
                break;
            default:
                break;
        }
    }
}

public enum IndexContent
{
    Index,
    Setting,
    Plugin,
    What,
    How,
    Expand
}
