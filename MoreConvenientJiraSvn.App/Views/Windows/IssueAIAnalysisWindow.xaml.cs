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
using System.Windows.Shapes;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using MoreConvenientJiraSvn.App.ViewModels;
using MoreConvenientJiraSvn.Core.Models;

namespace MoreConvenientJiraSvn.App.Views.Windows
{
    /// <summary>
    /// IssueAIAnalysisWindow.xaml 的交互逻辑
    /// </summary>
    public partial class IssueAIAnalysisWindow : Window
    {
        private IssueAIAnalysisViewModel _viewModel;
        public IssueAIAnalysisWindow()
        {
            InitializeComponent();
            this._viewModel = ViewModelsManager.GetViewModel<IssueAIAnalysisViewModel>();

            this.DataContext = _viewModel;
            this.Loaded += IssueAIAnalysisWindow_Loaded;
            this.Closed += IssueAIAnalysisWindow_Closed;
        }

        private async void IssueAIAnalysisWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await this._viewModel.Initialize();
        }

        private void IssueAIAnalysisWindow_Closed(object? sender, EventArgs e)
        {
            WindowsManager.RemoveWindow(this);
        }

        private void PromptComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                if (e.AddedItems[0] is KeyValuePair<string,string> keyValuePair)
                {
                    _viewModel.AIServiceSetting.PromptText = keyValuePair.Value;
                }
            }
        }

        private void DialogHost_DialogClosed(object sender, DialogClosedEventArgs eventArgs)
        {

        }
    }
}
