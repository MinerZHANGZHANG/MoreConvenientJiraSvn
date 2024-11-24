using System.Windows;

namespace MoreConvenientJiraSvn.Plugin.SvnJiraLink
{
    /// <summary>
    /// SvnJiraLinkWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SvnJiraLinkWindow : Window
    {
        private SvnJiraLinkViewModel _viewModel;
        public SvnJiraLinkWindow(SvnJiraLinkViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this._viewModel = viewModel;

            this.Loaded += SvnJiraLinkWindow_Loaded;
        }

        private void SvnJiraLinkWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this._viewModel.InitViewModel();
        }

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this._viewModel.RefreshSvnLog();
        }
    }
}
