using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Service;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class SqlCheckViewModel(SvnService svnService, IRepository repository, IPlSqlCheckPipeline plSqlCheckPipeline, SettingService settingService) : ObservableObject
{
    #region Service
    private readonly SvnService _svnService = svnService;
    private readonly IRepository _repository = repository;
    private readonly IPlSqlCheckPipeline _plSqlCheckPipeline = plSqlCheckPipeline;
    private readonly SettingService _settingService = settingService;

    #endregion

    #region Property & Field

    [ObservableProperty]
    private SqlCheckSetting _setting = new();

    [ObservableProperty]
    private string _checkStateText = string.Empty;

    [ObservableProperty]
    private float _checkStateProgress = 0;

    [ObservableProperty]
    private ObservableCollection<SqlIssue> _sqlIssues = [];

    private Dictionary<string, int> _viewAlertCountDict = [];

    #endregion

    public void InitViewModel()
    {
        Setting = _settingService.FindSetting<SqlCheckSetting>() ?? new();
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
        _viewAlertCountDict = [];
        CheckStateProgress = 0;
        string[] fileInfos = Directory.GetFiles(Setting.DefaultDir, "*.sql");
        if (fileInfos.Length == 0)
        {
            MessageBox.Show($"{Setting.DefaultDir}路径下没有.sql文件");
            return;
        }
        CheckStateText = $"找到{fileInfos.Length}个Sql文件，正在检测...";
        float eachRatio = 100f / fileInfos.Length;
        List<SqlIssue> tempIssues = [];
        await Task.Run(() =>
        {
            Parallel.ForEach(fileInfos, file =>
            {
                var issues = _plSqlCheckPipeline.CheckSingleFile(file, _viewAlertCountDict);
                lock (tempIssues)
                {
                    tempIssues.AddRange(issues);
                }

                CheckStateProgress += eachRatio;
            });
        });

        CheckStateProgress = 1;
        SqlIssues = [.. tempIssues];
        CheckStateText = $"检测完成，发现{SqlIssues.Count}个问题";
    }

    public List<SqlIssue> CheckFile(string filePath)
    {
        _viewAlertCountDict = [];
        return _plSqlCheckPipeline.CheckSingleFile(filePath, _viewAlertCountDict);
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

}



