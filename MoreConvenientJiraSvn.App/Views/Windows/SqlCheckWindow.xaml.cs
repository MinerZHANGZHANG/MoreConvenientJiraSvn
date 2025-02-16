using MoreConvenientJiraSvn.App.ViewModels;
using System.IO;
using System.Windows;

namespace MoreConvenientJiraSvn.App.Views.Windows;

/// <summary>
/// SqlCheckWindow.xaml 的交互逻辑
/// </summary>
public partial class SqlCheckWindow : Window
{
    private SqlCheckViewModel _viewModel;
    public SqlCheckWindow()
    {
        InitializeComponent();
        this._viewModel = ViewModelsManager.GetViewModel<SqlCheckViewModel>();

        this.DataContext = _viewModel;
        this.Loaded += SqlCheckWindow_Loaded;
        this.Closed += SqlCheckWindow_Closed;
    }

    private void SqlCheckWindow_Closed(object? sender, EventArgs e)
    {
        WindowsManager.RemoveWindow(this);
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
