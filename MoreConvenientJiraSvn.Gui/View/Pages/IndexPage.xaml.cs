using MoreConvenientJiraSvn.Gui.ViewModel;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.Gui.View.Pages
{
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
}
