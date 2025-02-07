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
    private readonly JiraSettingViewModel viewModel;
    public JiraSettingPage()
    {
        viewModel = ViewModelsManager.GetViewModel<JiraSettingViewModel>();
        DataContext = viewModel;
        InitializeComponent();
    }

    private async void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await viewModel.UpdateConfig();
        }
    }

    private async void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        await viewModel.UpdateConfig();
    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
        viewModel.UpdateFilter();
    }
}
