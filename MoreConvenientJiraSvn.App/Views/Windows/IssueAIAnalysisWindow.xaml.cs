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

        private void IssueAIAnalysisWindow_Closed(object? sender, EventArgs e)
        {
            
        }

        private void IssueAIAnalysisWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowsManager.RemoveWindow(this);
        }

        // Event handler for selecting a folder.
        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            //using (var dialog = new FolderBrowserDialog())
            //{
            //    DialogResult result = dialog.ShowDialog();
            //    if (result == System.Windows.Forms.DialogResult.OK)
            //    {
            //        // Set folder in ViewModel
            //        if (DataContext is ViewModels.Windows.IssueAIAnalysisViewModel vm)
            //        {
            //            vm.SelectedFolderPath = dialog.SelectedPath;
            //            // Populate FolderFiles based on the selected folder.
            //            try
            //            {
            //                var files = System.IO.Directory.GetFiles(dialog.SelectedPath).Select(System.IO.Path.GetFileName).ToList();
            //                vm.FolderFiles.Clear();
            //                foreach (var file in files)
            //                {
            //                    vm.FolderFiles.Add(file);
            //                }
            //            }
            //            catch { /* Handle exceptions if necessary */ }
            //        }
            //    }
            //}
        }

        // Event handler for multi-select file input in 代码问答 Tab.
        private void SelectCodeFiles_Click(object sender, RoutedEventArgs e)
        {
            //var dlg = new OpenFileDialog
            //{
            //    Multiselect = true,
            //    Filter = "All files (*.*)|*.*"
            //};
            //bool? res = dlg.ShowDialog();
            //if (res == true && DataContext is ViewModels.Windows.IssueAIAnalysisViewModel vm)
            //{
            //    vm.CodeFiles.Clear();
            //    foreach (var file in dlg.FileNames)
            //    {
            //        vm.CodeFiles.Add(file);
            //    }
            //}
        }

        // Event handler for multi-select file input in Sql问答 Tab.
        private void SelectSqlFiles_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "All files (*.*)|*.*"
            };
            bool? res = dlg.ShowDialog();
            //if (res == true && DataContext is ViewModels.Windows.IssueAIAnalysisViewModel vm)
            //{
            //    vm.SqlFiles.Clear();
            //    foreach (var file in dlg.FileNames)
            //    {
            //        vm.SqlFiles.Add(file);
            //    }
            //}
        }

        // Sample method to show a dialog using DialogHost.
        private async void ShowSampleDialog()
        {
            var dialogContent = new System.Windows.Controls.TextBlock { Text = "请选择一个选项" };
            var result = await DialogHost.Show(dialogContent, "MainDialogHost");
            // Process result if needed...
        }

        // New: Remove selected code file from CodeFilesListBox
        private void RemoveCodeFile_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            if (button?.Tag is string file)
            {
                // Remove file from the ViewModel's CodeFiles collection
                dynamic vm = DataContext;
                vm.CodeFiles.Remove(file);
            }
        }

        // New: Remove selected SQL file from SqlFilesListBox
        private void RemoveSqlFile_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            if (button?.Tag is string file)
            {
                dynamic vm = DataContext;
                vm.SqlFiles.Remove(file);
            }
        }

        // New: Send Code query event handler
        private void SendCodeQuery_Click(object sender, RoutedEventArgs e)
        {
            // Implement sending the code input along with attached files.
            // For example, call ViewModel method or service.
            MessageBox.Show("发送代码问答请求...");
        }

        // New: Send SQL query event handler
        private void SendSqlQuery_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("发送SQL问答请求...");
        }

        // New: Send Summary event handler
        private void SendSummary_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("发送改动内容总结请求...");
        }

        // New: Send Jira Issue event handler
        private void SendJiraIssue_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("发送Jira Issue请求...");
        }
    }
}
