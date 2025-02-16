using MoreConvenientJiraSvn.App.ViewModels;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.App.Views.Pages;

/// <summary>
/// MainPage.xaml 的交互逻辑
/// </summary>
public partial class MainPage : Page
{
    public MainPage()
    {
        DataContext = new MainPageViewModel();
        InitializeComponent();
        ((MainPageViewModel)DataContext).SwitchContent(IndexContent.Index);
    }
}
