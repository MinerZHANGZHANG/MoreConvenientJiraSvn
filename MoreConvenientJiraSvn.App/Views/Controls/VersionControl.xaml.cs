using MoreConvenientJiraSvn.App.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.App.Views.Controls
{
    /// <summary>
    /// VersionControl.xaml 的交互逻辑
    /// </summary>
    public partial class VersionControl : UserControl
    {
        private readonly VersionControlViewModel _viewModel;
        public VersionControl()
        {
            _viewModel = ViewModelsManager.GetViewModel<VersionControlViewModel>();
            DataContext = _viewModel;

            InitializeComponent();
            Loaded += VersionControl_Loaded;
        }

        private async void VersionControl_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.Init();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(VersionControlViewModel.AppRepoUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开网页{ex.Message}");
            }
        }
    }
}
