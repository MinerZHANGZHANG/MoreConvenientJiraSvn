using MoreConvenientJiraSvn.Gui.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MoreConvenientJiraSvn.Gui.View.Pages
{
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

        private async void TextBox_KeyUp(object sender, KeyEventArgs e)
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
    }
}
