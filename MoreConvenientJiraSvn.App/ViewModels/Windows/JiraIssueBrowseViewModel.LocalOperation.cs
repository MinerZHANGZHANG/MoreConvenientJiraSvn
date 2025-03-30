using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using Microsoft.Win32;
using MoreConvenientJiraSvn.Core.Models;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class JiraIssueBrowseViewModel
{
    // Local jira info
    [ObservableProperty]
    private JiraIssueLocalInfoSetting _jiraIssueLocalInfoSetting = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LocalJiraOperationText))]
    private JiraIssueLocalInfo? _selectedJiraIssueLocalInfo;

    public string LocalJiraOperationText => string.IsNullOrEmpty(SelectedJiraIssueLocalInfo?.LocalDir) ? "新建Jira文件夹" : "打开Jira文件夹";


    private void InitJiraIssueLocalOperations()
    {
        JiraIssueLocalInfoSetting = _settingService.FindSetting<JiraIssueLocalInfoSetting>() ?? new();

        _selectedIssueChanged += JiraIssueLocalOperations_SelectedIssueChanged;
    }

    private void JiraIssueLocalOperations_SelectedIssueChanged(object? sender, JiraIssue issue)
    {
        SelectedJiraIssueLocalInfo = _repository.FindOne<JiraIssueLocalInfo>(Query.EQ(nameof(JiraIssueLocalInfo.IssueKey), issue.IssueKey));
    }

    [RelayCommand]
    public void SelecetLocalInfoDirectory()
    {
        var folderBrowserDialog = new OpenFolderDialog
        {
            Title = "选择Jira本地文件夹的创建目录"
        };

        var result = folderBrowserDialog.ShowDialog();
        if (result != true)
        {
            return;
        }

        string selectedPath = folderBrowserDialog.FolderName;
        if (string.IsNullOrEmpty(selectedPath))
        {
            return;
        }
        JiraIssueLocalInfoSetting.ParentDir = selectedPath;
        _settingService.UpsertSetting(JiraIssueLocalInfoSetting);

        OnPropertyChanged(nameof(JiraIssueLocalInfoSetting));
    }

    [RelayCommand]
    public void SetUserNameForJiraInfo()
    {
        if (string.IsNullOrEmpty(JiraIssueLocalInfoSetting.UserName))
        {
            return;
        }

        _settingService.UpsertSetting(JiraIssueLocalInfoSetting);
    }

    [RelayCommand(CanExecute = nameof(HasIssueBeSelected))]
    public void CopyCommitText()
    {
        if (SelectedJiraIssue == null)
        {
            return;
        }

        try
        {
            Clipboard.SetText(GetDefaultCommitString(SelectedJiraIssue));
            MessageQueue.Enqueue("复制成功!");
        }
        catch
        {
            MessageQueue.Enqueue("复制失败...请重试");
        }
    }

    private string GetDefaultCommitString(JiraIssue issue)
    {
        StringBuilder commitTextBuilder = new($"版本：{issue.FixVersionsText}\r\n");
        if (!string.IsNullOrEmpty(issue.ParentIssueKey))
        {
            commitTextBuilder.AppendLine($"需求编号：{issue.ParentIssueKey}");
            commitTextBuilder.AppendLine($"内容概要：{issue.ParentIssueSummary}");
            commitTextBuilder.AppendLine($"缺陷编号：{issue.IssueKey}");
            commitTextBuilder.Append($"内容概要：{issue.Summary}");
        }
        else
        {
            commitTextBuilder.AppendLine($"需求编号：{issue.IssueKey}");
            commitTextBuilder.AppendLine($"内容概要：{issue.Summary}");
            commitTextBuilder.AppendLine($"缺陷编号：");
            commitTextBuilder.Append($"内容概要：");
        }

        return commitTextBuilder.ToString();
    }

    [RelayCommand(CanExecute = nameof(HasIssueBeSelected))]
    public void CopyAnnotationText()
    {
        if (SelectedJiraIssue == null)
        {
            return;
        }
        try
        {
            var text = $"// {DateTime.Today:yyyy-MM-dd} {JiraIssueLocalInfoSetting.UserName} {SelectedJiraIssue.IssueKey} {SelectedJiraIssue.Summary}";
            Clipboard.SetText(text);
            MessageQueue.Enqueue("复制成功!");
        }
        catch
        {
            MessageQueue.Enqueue("复制失败...请重试");
        }

    }

    [RelayCommand(CanExecute = nameof(HasIssueBeSelected))]
    public async Task OpenOrCreateJiraIssueDirectoryAsync()
    {
        if (SelectedJiraIssue == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(SelectedJiraIssueLocalInfo?.LocalDir))
        {
            OpenLocalBrowse(SelectedJiraIssueLocalInfo.LocalDir);
        }
        else
        {
            await CreateJiraIssueLocalDirectoryAsync(SelectedJiraIssue);
        }
    }

    private async Task CreateJiraIssueLocalDirectoryAsync(JiraIssue SelectedJiraIssue)
    {
        if (string.IsNullOrEmpty(JiraIssueLocalInfoSetting.ParentDir))
        {
            MessageQueue.Enqueue("请先设置本地Jira信息存储位置");
            return;
        }

        if (!Directory.Exists(JiraIssueLocalInfoSetting.ParentDir))
        {
            MessageQueue.Enqueue("未设置有效的本地Jira信息存储路径");
            return;
        }

        string dirName = $"{SelectedJiraIssue.IssueKey}-{SelectedJiraIssue.Summary}";
        string fullDirName = Path.Combine(JiraIssueLocalInfoSetting.ParentDir, dirName);
        string[] directories = Directory.GetDirectories(JiraIssueLocalInfoSetting.ParentDir);
        if (directories.Contains(dirName))
        {
            MessageQueue.Enqueue($"{JiraIssueLocalInfoSetting.ParentDir}目录下已有一个{dirName}文件夹");

            SelectedJiraIssueLocalInfo = new() { IssueKey = SelectedJiraIssue.IssueKey, LocalDir = fullDirName };
            _repository.Upsert(SelectedJiraIssueLocalInfo);

            return;
        }

        var sameIssueKeyDir = directories.FirstOrDefault(dir => dir.Contains(SelectedJiraIssue.IssueKey));
        if (sameIssueKeyDir != null)
        {
            var boxResult = MessageBox.Show($"{JiraIssueLocalInfoSetting.ParentDir}目录下有一个名为[{sameIssueKeyDir}]的目录，是否把它作为当前问题的本地目录?",
                             "相同编号的目录",
                             MessageBoxButton.YesNo,
                             MessageBoxImage.Question);
            if (boxResult == MessageBoxResult.OK)
            {
                SelectedJiraIssueLocalInfo = new() { IssueKey = SelectedJiraIssue.IssueKey, LocalDir = sameIssueKeyDir };
                _repository.Upsert(SelectedJiraIssueLocalInfo);

                return;
            }
            else if (boxResult != MessageBoxResult.No)
            {
                return;
            }
        }

        DirectoryInfo directoryInfo = Directory.CreateDirectory(fullDirName);

        string commitFileFullName = Path.Combine(fullDirName, $"提交文本-{SelectedJiraIssue.IssueKey}.txt");
        if (!File.Exists(commitFileFullName))
        {
            await File.WriteAllTextAsync(commitFileFullName, GetDefaultCommitString(SelectedJiraIssue));
        }

        string documentFileFullName = Path.Combine(fullDirName, $"{SelectedJiraIssue.IssueKey}_{JiraIssueLocalInfoSetting.UserName}_{SelectedJiraIssue.Summary}.txt");
        if (!File.Exists(documentFileFullName))
        {
            await File.WriteAllTextAsync(documentFileFullName, GetDefaultDocumentString(SelectedJiraIssue));
        }

        directoryInfo.CreateSubdirectory("SQL");
        directoryInfo.CreateSubdirectory("图片示例");

        var attachmentDir = directoryInfo.CreateSubdirectory("附件文档");
        await _jiraService.DownloadIssueAttachmentAsync(SelectedJiraIssue.IssueKey, attachmentDir.FullName);

        SelectedJiraIssueLocalInfo = new() { IssueKey = SelectedJiraIssue.IssueKey, LocalDir = fullDirName };
        _repository.Upsert(SelectedJiraIssueLocalInfo);

        MessageQueue.Enqueue($"创建[{fullDirName}]文件夹成功!");

    }

    private static string GetDefaultDocumentString(JiraIssue SelectedJiraIssue)
    {
        StringBuilder documentTextBuilder = new($"一、需求描述:\r\n");
        documentTextBuilder.AppendLine($"{SelectedJiraIssue.Descrpition}");
        documentTextBuilder.AppendLine();
        documentTextBuilder.AppendLine($"二、设计和解决方案:");
        documentTextBuilder.AppendLine();
        documentTextBuilder.AppendLine($"三、自测场景:");
        documentTextBuilder.AppendLine();
        documentTextBuilder.AppendLine($"四、测试建议:");
        documentTextBuilder.AppendLine();
        documentTextBuilder.AppendLine($"五、相关脚本:");

        return documentTextBuilder.ToString();
    }

    private void OpenLocalBrowse(string dir)
    {
        try
        {
            Process.Start("explorer.exe", dir);
        }
        catch (Exception ex)
        {
            MessageQueue.Enqueue($"打开本地路径失败，{ex.Message}");
        }
    }
}
