using MoreConvenientJiraSvn.App.ViewModels;
using MoreConvenientJiraSvn.Core.Enums;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Core.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MoreConvenientJiraSvn.App.Views.Pages;

/// <summary>
/// SvnSettingPage.xaml 的交互逻辑
/// </summary>
public partial class SvnSettingPage : Page
{
    private readonly SvnSettingViewModel _viewModel;
    public SvnSettingPage()
    {
        _viewModel = ViewModelsManager.GetViewModel<SvnSettingViewModel>();
        DataContext = _viewModel;

        InitializeComponent();
    }

    private void TextBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _viewModel.SaveSvnConfig();
        }
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        _viewModel.SaveSvnConfig();
    }
}

public static class EnumResource
{
    public static IEnumerable<EnumDescription> SvnPathTypes => EnumHelper.GetEnumDescriptions<SvnPathType>();
}
