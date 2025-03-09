using MoreConvenientJiraSvn.App.ViewModels;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.App.Views.Pages
{
    /// <summary>
    /// HostedServiceSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class HostedServiceSettingPage : Page
    {
        private readonly HostedServiceSettingViewModel _viewModel;
        public HostedServiceSettingPage()
        {
            _viewModel = ViewModelsManager.GetViewModel<HostedServiceSettingViewModel>();
            DataContext = _viewModel;

            InitializeComponent();
            Loaded += HostedServiceSettingPage_Loaded;
        }

        private void SaveSetting()
        {
            _viewModel?.SaveSetting();
        }

        private async void HostedServiceSettingPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await _viewModel.InitViewModel();
        }

        private void NumericUpDown_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<int> e)
        {
            SaveSetting();
        }

        private void TimePicker_SelectedTimeChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            SaveSetting();
        }

        private void IntervalSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            SaveSetting();
        }

        private void ComboBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveSetting();
        }
    }
}
