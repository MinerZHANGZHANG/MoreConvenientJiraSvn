using MoreConvenientJiraSvn.App.ViewModels;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.App.Views.Pages;

/// <summary>
/// IndexPage.xaml 的交互逻辑
/// </summary>
public partial class IndexPage : Page
{
    public IndexPage()
    {
        DataContext = new IndexViewModel();
        InitializeComponent();
        ((IndexViewModel)DataContext).SwitchContent(IndexContent.Index);
    }
}
