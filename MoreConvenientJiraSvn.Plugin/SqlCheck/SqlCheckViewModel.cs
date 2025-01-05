using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using MoreConvenientJiraSvn.Core.Service;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace MoreConvenientJiraSvn.Plugin.SqlCheck;

public partial class SqlCheckViewModel(ServiceProvider serviceProvider) : ObservableObject
{
    #region Service
    private readonly SvnService _svnService = serviceProvider.GetRequiredService<SvnService>();
    private readonly DataService _dataService = serviceProvider.GetRequiredService<DataService>();
    private readonly SettingService _settingService = serviceProvider.GetRequiredService<SettingService>();

    #endregion

    #region Property

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
        Setting = _settingService.GetSingleSettingFromDatabase<SqlCheckSetting>() ?? new();
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

                _settingService.InsertOrUpdateSettingIntoDatabase(Setting);
            }
        }
    }

    [RelayCommand]
    public async Task CheckDir()
    {
        if (string.IsNullOrEmpty(Setting.DefaultDir))
        {
            return;
        }
        CheckStateProgress = 0;
        string[] fileInfos = Directory.GetFiles(Setting.DefaultDir, "*.sql");
        if (fileInfos.Length == 0)
        {
            MessageBox.Show($"{Setting.DefaultDir}路径下没有.sql文件");
            return;
        }
        float eachRatio = 100f / fileInfos.Length;
        List<SqlIssue> tempIssues = [];
        await Task.Run(() =>
        {
            Parallel.ForEach(fileInfos, file =>
            {
                var issues = CheckSingle(file);
                lock (tempIssues)
                {
                    tempIssues.AddRange(issues);
                }

                CheckStateProgress += eachRatio;
            });
        });

        CheckStateProgress += eachRatio;
        SqlIssues = [.. tempIssues];

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
                sqlIssues = CheckSingle(fileBrowserDialog.FileName);
            });

            if (sqlIssues.Count > 0)
            {
                MessageBox.Show(sqlIssues[0].Message);
            }
        }

    }

    public List<SqlIssue> CheckSingle(string filePath)
    {
        SqlCheckPipeline sqlCheckPipeline = new(filePath);
        sqlCheckPipeline.ReadSqlFile()
                     ?.ClearPrompts()
                     ?.ParserSql()
                     ?.CheckWhereCondition()
                     ?.CheckInsideIfBlock();

        return sqlCheckPipeline.SqlIssues ?? [];
    }
}



