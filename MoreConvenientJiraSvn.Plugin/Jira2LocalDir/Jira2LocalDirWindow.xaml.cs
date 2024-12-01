using System.Windows;

namespace MoreConvenientJiraSvn.Plugin.Jira2LocalDir
{
    /// <summary>
    /// Jira2LocalDirWindow.xaml 的交互逻辑
    /// </summary>
    public partial class Jira2LocalDirWindow : Window
    {
        private Jira2LocalDirViewModel _viewModel;
        public Jira2LocalDirWindow(Jira2LocalDirViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this._viewModel = viewModel;

            this.Loaded += Jira2LocalDirWindow_Loaded;
        }

        private async void Jira2LocalDirWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await this._viewModel.InitViewModelAsync();
        }

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this._viewModel.RefreshLocalJiraInfo();
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this._viewModel.RefreshSelectPathSvnLog();
        }
    }
}
