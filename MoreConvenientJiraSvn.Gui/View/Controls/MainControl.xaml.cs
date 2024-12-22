using MoreConvenientJiraSvn.Gui.ViewModel;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.Gui.View.Controls
{
    /// <summary>
    /// IndexControl.xaml 的交互逻辑
    /// </summary>
    public partial class MainControl : System.Windows.Controls.UserControl
    {
        public MainControl()
        {
            DataContext = new MainControlViewModel();
            InitializeComponent();
        }
    }
}
