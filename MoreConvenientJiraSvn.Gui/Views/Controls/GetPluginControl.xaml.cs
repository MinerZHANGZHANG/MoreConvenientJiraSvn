using MoreConvenientJiraSvn.App.ViewModels;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.App.Views.Controls;

/// <summary>
/// GetPluginControl.xaml 的交互逻辑
/// </summary>
public partial class GetPluginControl : UserControl
{
    public GetPluginControl()
    {
        DataContext = new GetPluginControlViewModel();
        InitializeComponent();
    }
}
