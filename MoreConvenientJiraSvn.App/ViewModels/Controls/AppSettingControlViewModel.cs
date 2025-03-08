using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.App.Properties;
using MoreConvenientJiraSvn.Core.Enums;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Core.Utils;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace MoreConvenientJiraSvn.App.ViewModels
{
    public partial class AppSettingControlViewModel : ObservableObject
    {
        public string LocalDataSizeText { get; private set; }

        [ObservableProperty]
        private bool _isEnableWriteOpertion;

        [ObservableProperty]
        private LogRemindLevel _selectedRemindLevel;
        public List<EnumDescription> LogRemindLevels { get; private set; }

        public AppSettingControlViewModel()
        {
            if (File.Exists(Settings.Default.DatabaseName))
            {
                var dbFile = new FileInfo(Settings.Default.DatabaseName);
                LocalDataSizeText = $"{(float)dbFile.Length / 1024 / 1024} MB";
            }
            else
            {
                LocalDataSizeText = $"0 MB";
            }

            IsEnableWriteOpertion = Settings.Default.IsEnableWriteOperation;

            LogRemindLevels = EnumHelper.GetEnumDescriptions<LogRemindLevel>();
            SelectedRemindLevel = (LogRemindLevel)Settings.Default.LogRemindLevel;
        }

        [RelayCommand]
        public void UpdateIsEnableWriteOpertion()
        {
            Settings.Default.IsEnableWriteOperation = IsEnableWriteOpertion;
            Settings.Default.Save();
        }

        public void UpdateLogRemindLevel()
        {
            Settings.Default.LogRemindLevel = (int)SelectedRemindLevel;
            Settings.Default.Save();
        }

        [RelayCommand]
        public void BrowseLocalFile()
        {
            try
            {
                Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        [RelayCommand]
        public void ShowEnableWriteTip()
        {
            MessageBox.Show("启用这个按钮将允许进行以下操作:\r\n - 修改和提交Jira状态");
        }
    }
}
