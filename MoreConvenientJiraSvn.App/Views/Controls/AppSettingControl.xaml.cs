using MoreConvenientJiraSvn.App.ViewModels;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.App.Views.Controls
{
    /// <summary>
    /// AppSettingControl.xaml 的交互逻辑
    /// </summary>
    public partial class AppSettingControl : UserControl
    {
        private AppSettingControlViewModel _viewModel;
        public AppSettingControl()
        {
            _viewModel = ViewModelsManager.GetViewModel<AppSettingControlViewModel>();
            DataContext = _viewModel;

            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.UpdateLogRemindLevel();
        }
    }
}
