using MoreConvenientJiraSvn.App.ViewModels;
using System.Windows;

namespace MoreConvenientJiraSvn.App.Views.Windows;

/// <summary>
/// SvnJiraLinkWindow.xaml 的交互逻辑
/// </summary>
public partial class SvnJiraLinkWindow : Window
{
    private SvnJiraLinkViewModel _viewModel;
    public SvnJiraLinkWindow()
    {
        InitializeComponent();
        this._viewModel = ViewModelsManager.GetViewModel<SvnJiraLinkViewModel>();

        this.DataContext = _viewModel;
        this.Loaded += SvnJiraLinkWindow_Loaded;
        this.Closed += SvnJiraLinkWindow_Closed;
    }

    private void SvnJiraLinkWindow_Closed(object? sender, EventArgs e)
    {
        WindowsManager.RemoveWindow(this);
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
