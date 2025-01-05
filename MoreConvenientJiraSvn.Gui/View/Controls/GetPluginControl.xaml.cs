using MoreConvenientJiraSvn.Gui.ViewModel;

namespace MoreConvenientJiraSvn.Gui.View.Controls
{
    /// <summary>
    /// GetPluginControl.xaml 的交互逻辑
    /// </summary>
    public partial class GetPluginControl : System.Windows.Controls.UserControl
    {
        public GetPluginControl()
        {
            DataContext = new GetPluginControlViewModel();
            InitializeComponent();
        }
    }
}
