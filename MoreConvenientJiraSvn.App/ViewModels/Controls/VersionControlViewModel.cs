using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using MdXaml;
using MoreConvenientJiraSvn.App.Utils;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Service;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Documents;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class VersionControlViewModel(IVersionService versionService, LogService logService, Markdown markdownEngine) : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UpdateVersionCommand))]
    [NotifyPropertyChangedFor(nameof(IsNeedUpdate))]
    [NotifyPropertyChangedFor(nameof(CurrentVersionText))]
    private VersionInfo? _currentVersion;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UpdateVersionCommand))]
    [NotifyPropertyChangedFor(nameof(IsNeedUpdate))]
    [NotifyPropertyChangedFor(nameof(LatestVersionText))]
    private VersionInfo? _latestVersion;

    [ObservableProperty]
    private FlowDocument? _updateLogDocument;

    [ObservableProperty]
    private float _downloadProgressValue = 0.0f;

    public string CurrentVersionText => CurrentVersion == null
         ? "未找到"
         : $"{CurrentVersion.Name}({CurrentVersion.Version})";

    public string LatestVersionText => LatestVersion == null
         ? "未找到"
         : $"{LatestVersion.Name}({LatestVersion.Version})";

    public static string AppRepoUrl => @"https://github.com/MinerZHANGZHANG/MoreConvenientJiraSvn";

    public static string UpdateApplictionUrl => Path.Combine(Environment.CurrentDirectory, "UpdateApp.exe");

    public bool IsNeedUpdate => LatestVersion != null && CurrentVersion != null
        && LatestVersion.Version != CurrentVersion.Version;

    public async Task Init()
    {
        CurrentVersion = versionService.GetCurrentVersionInfoAsync();
        LatestVersion = await versionService.GetLatestVersionInfoAsync();
        if (LatestVersion == null)
        {
            UpdateLogDocument = null;
            return;
        }

        UpdateLogDocument = markdownEngine.Transform(LatestVersion.Description);
    }

    [RelayCommand]
    public async Task RefreshLatestVersion()
    {
        LatestVersion = await versionService.GetLatestVersionInfoAsync();
        if (LatestVersion == null)
        {
            UpdateLogDocument = null;
            return;
        }

        UpdateLogDocument = markdownEngine.Transform(LatestVersion.Description);
    }

    [RelayCommand(CanExecute = nameof(IsNeedUpdate))]
    public async Task UpdateVersion()
    {
        if (LatestVersion == null || CurrentVersion == null)
        {
            return;
        }

        ReleaseAsset? releaseAsset = versionService.GetUpdateVersionAssetAsync(CurrentVersion, LatestVersion);
        if (releaseAsset == null)
        {
            await DialogHost.Show(GenerateControl.GetErrorDialog("没有找到更新的版本"));
            return;
        }

        string downloadDirectory = Path.Combine(Environment.CurrentDirectory, "Backup");
        if (!Directory.Exists(downloadDirectory))
        {
            Directory.CreateDirectory(downloadDirectory);
        }

        string downloadPosition = Path.Combine(downloadDirectory, releaseAsset.Name);
        if (File.Exists(downloadPosition))
        {
            FileInfo fileInfo = new(downloadPosition);
            if (fileInfo.Length != releaseAsset.Size)
            {
                var dialogResult = await DialogHost.Show(GenerateControl.GetConfrimDialog($"{downloadPosition}位置已有其它文件，是否要改名该文件"));
                if (dialogResult is bool isRename && isRename)
                {
                    var newFileName = Path.Combine(Environment.CurrentDirectory, "Backup", $"{releaseAsset.Name}_{DateTime.Now:yyyyMMddHHmmss}.bak");
                    fileInfo.MoveTo(newFileName);
                    logService.LogDebug($"Rename file {downloadPosition} to {newFileName} success.");
                }
                else
                {
                    return;
                }
            }
        }
        else
        {
            DownloadProgressValue = 0f;
            bool isDownload;
            string errorMessage = string.Empty;
            try
            {
                isDownload = await versionService.DownloadReleaseAssetAsync(releaseAsset, downloadPosition);
            }
            catch (Exception ex)
            {
                isDownload = false;
                errorMessage = ex.Message;
            }
            if (!isDownload)
            {
                await DialogHost.Show(GenerateControl.GetErrorDialog($"文件下载失败 {errorMessage}"));
                return;
            }
            DownloadProgressValue = 1f;
        }
            
        var confirmResult = await DialogHost.Show(GenerateControl.GetConfrimDialog($"{releaseAsset.Name}下载完成，是否立即关闭应用并更新?"));
        if (confirmResult is bool result && result)
        {
            if (!File.Exists(downloadPosition))
            {
                logService.LogDebug($"The file {downloadPosition} not exist.");
                return;
            }

            string extractDirectoryPath = Path.Combine(Environment.CurrentDirectory, "Backup", $"{Path.GetFileNameWithoutExtension(releaseAsset.Name)}_{DateTime.Now:yyyyMMdd-HHmmss}");
            try
            {
                if (!Directory.Exists(extractDirectoryPath))
                {
                    Directory.CreateDirectory(extractDirectoryPath);
                }
                ZipFile.ExtractToDirectory(downloadPosition, extractDirectoryPath);
            }
            catch (Exception ex)
            {
                logService.LogDebug($"Unzip {downloadPosition} failed:{ex.Message}");
            }
           
            string backupPath = Path.Combine(Environment.CurrentDirectory, "Backup", $"Backup_{System.AppDomain.CurrentDomain.FriendlyName}_{CurrentVersion.Version}_{DateTime.Now:yyyyMMddHHmmss}");
            string arguments = $"{System.AppDomain.CurrentDomain.FriendlyName} {backupPath} {extractDirectoryPath}";

            // Start script to update
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = UpdateApplictionUrl,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            Process.Start(startInfo);

            Application.Current.Shutdown();
        }

    }

}
