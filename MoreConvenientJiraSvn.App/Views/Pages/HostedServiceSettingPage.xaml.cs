using MoreConvenientJiraSvn.App.ViewModels;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.App.Views.Pages
{
    /// <summary>
    /// HostedServiceSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class HostedServiceSettingPage : Page
    {
        private HostedServiceSettingViewModel _viewModel;
        public HostedServiceSettingPage()
        {
            InitializeComponent();
            _viewModel = ViewModelsManager.GetViewModel<HostedServiceSettingViewModel>();
            DataContext = _viewModel;

            Loaded += HostedServiceSettingPage_Loaded;
        }

        private async void HostedServiceSettingPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await _viewModel.InitViewModel();
        }

        private void NumericUpDown_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<int> e)
        {

        }

        private void TimePicker_SelectedTimeChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<DateTime?> e)
        {

        }

        private void IntervalSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void ComboBox_Selected(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
