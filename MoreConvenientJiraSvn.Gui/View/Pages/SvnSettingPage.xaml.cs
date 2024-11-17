using MoreConvenientJiraSvn.Gui.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MoreConvenientJiraSvn.Gui.View.Pages
{
    /// <summary>
    /// SvnSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SvnSettingPage : Page
    {
        private readonly SvnSettingViewModel viewModel;
        public SvnSettingPage()
        {
            viewModel = ViewModelsManager.GetViewModel<SvnSettingViewModel>();
            DataContext = viewModel;
            InitializeComponent();
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                viewModel.UpdateConfig();
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            viewModel.UpdateConfig();
        }
    }
}
