using System.IO;
using System.Windows;

namespace MoreConvenientJiraSvn.Plugin.SqlCheck
{
    /// <summary>
    /// SqlCheckWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SqlCheckWindow : Window
    {
        private SqlCheckViewModel _viewModel;
        public SqlCheckWindow(SqlCheckViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this._viewModel = viewModel;
            Loaded += SqlCheckWindow_Loaded;
        }

        private void SqlCheckWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this._viewModel.InitViewModel();
        }

        private void FileSelectArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    _viewModel.CheckFile(Path.GetFileName(files[0]));
                }
            }

        }

        private void TextBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this._viewModel.SetCheckDir();
        }
    }
}
