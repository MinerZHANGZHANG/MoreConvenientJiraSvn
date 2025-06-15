using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using MoreConvenientJiraSvn.App.Utils;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Service;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class SqlCheckViewModel(SvnService svnService, IRepository repository, IPlSqlIssueChecker plSqlCheckPipeline, SettingService settingService) : ObservableObject
{
    #region Service
    private readonly SvnService _svnService = svnService;
    private readonly IRepository _repository = repository;
    private readonly IPlSqlIssueChecker _plSqlCheckPipeline = plSqlCheckPipeline;
    private readonly SettingService _settingService = settingService;

    #endregion

    #region Property & Field

    public static string DialogHostIdent => "SqlCheckWindowDialogHost";

    public static string[] Encodes => [
        "utf-8",
        "gb2312"
   ];

    [ObservableProperty]
    private SqlCheckSetting _setting = new();

    [ObservableProperty]
    private string _checkStateText = string.Empty;

    [ObservableProperty]
    private float _checkStateProgress = 0;

    [ObservableProperty]
    private ObservableCollection<SqlIssue> _sqlIssues = [];

    #endregion

    public void InitViewModel()
    {
        Setting = _settingService.FindSetting<SqlCheckSetting>() ?? new();
    }

    [RelayCommand]
    public void SaveSetting()
    {
        _settingService.UpsertSetting(Setting);
    }

    [RelayCommand]
    public void SetCheckDir()
    {
        var folderBrowserDialog = new OpenFolderDialog
        {
            Title = "选择放置Sql的文件夹"
        };

        var result = folderBrowserDialog.ShowDialog();
        if (result == true)
        {
            string selectedPath = folderBrowserDialog.FolderName;
            if (!string.IsNullOrEmpty(selectedPath))
            {
                Setting = Setting with { DefaultDir = selectedPath };

                _settingService.UpsertSetting(Setting);
            }
        }
        CheckStateText = string.Empty;
    }

    [RelayCommand]
    public async Task CheckDir()
    {
        if (string.IsNullOrEmpty(Setting.DefaultDir))
        {
            return;
        }
        SqlIssues.Clear();
        CheckStateProgress = 0;
        string[] fileInfos = Directory.GetFiles(Setting.DefaultDir, "*.sql", SearchOption.AllDirectories);
        if (fileInfos.Length == 0)
        {
            MessageBox.Show($"{Setting.DefaultDir}路径下没有.sql文件");
            return;
        }

        // 如果设置了跳过目录名，且当前路径包含该目录名，则跳过
        if (!string.IsNullOrEmpty(Setting.SkipDirectoryName))
        {
            fileInfos = [.. fileInfos.Where(p => !p.Contains(Setting.SkipDirectoryName))];
        }

        CheckStateText = $"找到{fileInfos.Length}个Sql文件，正在检测...";
        float eachRatio = 100f / fileInfos.Length;

        void progressAction(int progress)
        {
            CheckStateProgress = progress * eachRatio;
            CheckStateText = $"正在检测第{progress + 1}个文件，进度：{CheckStateProgress}%";
        }
        List<SqlIssue> tempIssues = await _plSqlCheckPipeline.CheckMultipleFilesAsync(fileInfos, Setting, progressAction);

        CheckStateProgress = 1;
        SqlIssues = [.. tempIssues];
        CheckStateText = $"检测完成，发现{SqlIssues.Count}个问题";
    }

    public List<SqlIssue> CheckFile(string filePath)
    {
        return _plSqlCheckPipeline.CheckSingleFile(filePath, Setting);
    }

    [RelayCommand]
    public void OpenSqlFile(object commandParamter)
    {
        string? filePath = commandParamter?.ToString();
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"打开文件失败:{ex.Message}");
        }

    }

    [RelayCommand]
    public async Task SetCheckFile()
    {
        var fileBrowserDialog = new OpenFileDialog
        {
            Title = "选择Sql文件",
            Multiselect = false,
            DefaultExt = ".sql"
        };

        var result = fileBrowserDialog.ShowDialog();
        if (result == true)
        {
            List<SqlIssue> sqlIssues = [];
            await Task.Run(() =>
            {
                sqlIssues = CheckFile(fileBrowserDialog.FileName);
            });

            if (sqlIssues.Count > 0)
            {
                MessageBox.Show(sqlIssues[0].Message);
            }
        }

    }

    [RelayCommand]
    public async Task DisplayInfo(string message)
    {
        message = message.Replace(@"\n", Environment.NewLine);
        await DialogHost.Show(GenerateControl.DisplayInfoDialog(message), DialogHostIdent);
    }

}



