using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Service;
using System.Windows;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class JiraSettingViewModel : ObservableObject
{
    private readonly SettingService _settingService;
    private readonly JiraService _jiraService;
    [ObservableProperty]
    private JiraConfig _jiraConfig = new();

    public JiraSettingViewModel(SettingService settingService,JiraService jiraService)
    {
        _settingService = settingService;
        _jiraService = jiraService;

        JiraConfig = _settingService.FindSetting<JiraConfig>() ?? new();
    }

    [RelayCommand]
    public void SaveSetting()
    {
        _settingService.UpsertSetting(JiraConfig);

        JiraConfig = _jiraService.JiraConfig;
    }

    [RelayCommand]
    public void ShowJiraUrlDescription()
    {
        MessageBox.Show("输入Jira网站的地址到这里\n\r类似http://jira.company.com:2025", "Jira路径");
    }

    [RelayCommand]
    public void ShowJiraApiTokenDescription()
    {
        MessageBox.Show("考虑到安全性，使用Jira的ApiToken进行身份认证\n\r在Jira网站右上角打开用户菜单，创建一个新的Token复制到这里", "Jira ApiToken");
    }

    [RelayCommand]
    public void ShowJiraUserInfoDescription()
    {
        MessageBox.Show("正常情况下，输入上方的地址和Token后将会自动带出用户信息\n\r(若没有带出则说明这个程序或者输入有问题)", "Jira用户信息");
    }
}
