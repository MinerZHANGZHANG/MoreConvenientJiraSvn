using MoreConvenientJiraSvn.App.ViewModels;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.App.Views.Controls;

/// <summary>
/// IndexControl.xaml 的交互逻辑
/// </summary>
public partial class MainControl : UserControl
{
    public MainControl()
    {
        DataContext = ViewModelsManager.GetViewModel<MainControlViewModel>();
        InitializeComponent();
    }

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {

    }
}
