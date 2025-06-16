using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
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

        private const string SchemeName = "mcjs";
        private const string AppName = "MoreConvenientJiraSvn";


        /* 可以通过以下代码创建一个从浏览器打开应用的按钮
        // ==UserScript==
        // @name         WPF App Launcher
        // @namespace    http://your-namespace/
        // @version      1.0
        // @description  Launch WPF app with page navigation
        // @match        *://example.com/*
        // @grant        none
        // ==/UserScript==

        (function() {
            'use strict';
    
            // 创建按钮样式
            const style = document.createElement('style');
            style.innerHTML = `
            .wpf-launcher-btn {
                position: fixed;
                bottom: 20px;
                right: 20px;
                padding: 12px 24px;
                background: #4CAF50;
                color: white;
                border: none;
                border-radius: 4px;
                cursor: pointer;
                box-shadow: 0 4px 8px rgba(0,0,0,0.2);
                z-index: 9999;
            }
            `;
            document.head.appendChild(style);
    
            // 创建按钮
            const btn = document.createElement('button');
            btn.className = 'wpf-launcher-btn';
            btn.textContent = '在应用中打开';
    
            // 点击事件处理
            btn.addEventListener('click', () => {
                // 构建自定义URI
                const targetPage = 'MainPage'; // 要跳转的目标页
                const uri = `mcjs://open/${targetPage}`;
        
                // 使用iframe触发协议
                const iframe = document.createElement('iframe');
                iframe.style.display = 'none';
                iframe.src = uri;
                document.body.appendChild(iframe);
                setTimeout(() => document.body.removeChild(iframe), 100);
        
            });
    
            // 插入按钮
            document.body.appendChild(btn);
        })();
         */
        [RelayCommand]
        public void RegisterUriScheme()
        {
            if(!TryGetExecutablePath(out string exePath))
            {
                MessageBox.Show("无法找到应用的可执行文件，请检查应用是否正确安装。", "错误");
                return;
            }

            using var key = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\Classes\\{SchemeName}");

            // 协议描述
            key.SetValue("", $"URL:{AppName} Protocol");
            key.SetValue("URL Protocol", "");

            // 设置默认图标
            using RegistryKey defaultIcon = key.CreateSubKey("DefaultIcon");
            string iconPath = $"\"{System.Reflection.Assembly.GetCallingAssembly().Location}\",1";
            defaultIcon.SetValue("", iconPath);

            // 设置打开命令
            using RegistryKey commandKey = key.CreateSubKey(@"shell\open\command");
            string appPath = $"\"{exePath}\" \"%1\"";
            commandKey.SetValue("", appPath);

            MessageBox.Show("已成功注册URI协议", "注册成功");
        }

        [RelayCommand]
        public void UnRegisterUriScheme()
        {
            Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{SchemeName}");

            MessageBox.Show("已成功删除URI协议", "删除成功");
        }

        [RelayCommand]
        public void ShowEnableWriteTip()
        {
            MessageBox.Show("启用这个按钮将允许进行以下危险操作!\n - 修改Jira状态\n - 修改Jira问题要素\n\n话说回来，这只是启用了一些不包含删除的增改操作\n能有什么问题呢?", "Are you sure?");
        }

        [RelayCommand]
        public void ShowAddToRegisterUriTip()
        {
            MessageBox.Show("尝试把应用路径添加到当前用户的注册表，以支持从其它路径打开该应用", "添加应用到注册表");
        }

        /// <summary>
        /// 尝试获取路径到当前应用的可执行文件
        /// </summary>
        public static bool TryGetExecutablePath(out string exePath)
        {
            string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            exePath = Path.ChangeExtension(dllPath, ".exe");

            if (File.Exists(exePath))
            {
                return true;
            }

            exePath = Process.GetCurrentProcess()?.MainModule?.FileName??string.Empty;
            if (!string.IsNullOrEmpty(exePath))
            {
                return true;
            }

            return false;
        }
    }
}
