using MoreConvenientJiraSvn.Gui.ViewModel;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.Gui.View.Controls
{
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
}
