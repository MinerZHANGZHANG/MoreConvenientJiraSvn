using MoreConvenientJiraSvn.App.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MoreConvenientJiraSvn.App.Views.Pages;

/// <summary>
/// JiraSettingPage.xaml 的交互逻辑
/// </summary>
public partial class JiraSettingPage : Page
{
    private readonly JiraSettingViewModel _viewModel;
    public JiraSettingPage()
    {
        _viewModel = ViewModelsManager.GetViewModel<JiraSettingViewModel>();
        DataContext = _viewModel;

        InitializeComponent();
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        _viewModel.SaveSetting();
    }

    private void TextBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _viewModel.SaveSetting();
        }
    }
}
