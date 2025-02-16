using MoreConvenientJiraSvn.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
