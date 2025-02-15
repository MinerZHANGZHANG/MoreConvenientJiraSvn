using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.App.Views.Controls;
using System.Windows;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class IndexViewModel : ObservableObject
{

    public readonly MainControl mainControl = new();

    public readonly GetPluginControl getPluginControl = new();

    [ObservableProperty]
    private UserControl? _currentContent = null;

    [RelayCommand]
    public void SwitchContent(IndexContent indexContent)
    {
        switch (indexContent)
        {
            case IndexContent.Index:
                CurrentContent = mainControl;
                break;
            case IndexContent.Plugin:
                MessageBox.Show("尚未支持");
                //CurrentContent = getPluginControl;
                break;
            case IndexContent.Setting:
            case IndexContent.What:
            case IndexContent.How:
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
