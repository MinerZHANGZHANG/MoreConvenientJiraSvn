using System.Windows;

namespace MoreConvenientJiraSvn.Plugin.CommonSql
{
    /// <summary>
    /// CommonSqlWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CommonSqlWindow : Window
    {
        private CommonSqlViewModel _viewModel;
        public CommonSqlWindow(CommonSqlViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this._viewModel = viewModel;

            this.Loaded += CommonSqlWindow_Loaded;
        }

        private void CommonSqlWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this._viewModel.InitViewModel();
        }

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this._viewModel.RefreshSvnLog();
        }
    }
}
