using MoreConvenientJiraSvn.Core.Enums;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MoreConvenientJiraSvn.Service;

public interface IVersionService
{
    Task<bool> DownloadReleaseAssetAsync(ReleaseAsset releaseAsset, string downloadPath);
    VersionInfo GetCurrentVersionInfoAsync();
    Task<VersionInfo?> GetLatestVersionInfoAsync();
    ReleaseAsset? GetUpdateVersionAssetAsync(VersionInfo currentVersion, VersionInfo latestVersion);
}

public class GitHubVersionService(IRepository repository, LogService logService) : IVersionService
{
    public const string DownloadSourceUrl = $"https://api.github.com/repos/MinerZHANGZHANG/MoreConvenientJiraSvn/releases/latest";

    public VersionInfo GetCurrentVersionInfoAsync()
    {
        var versionInfo = repository.FindOneByOrder<VersionInfo>(nameof(VersionInfo.Version), true);

        if (versionInfo == null)
        {
            versionInfo = new VersionInfo
            {
                Version = "1.0.1",
                Description = string.Empty,
                PublishTime = new DateTime(2025, 4, 1),
                BuildType = IsSelfContained()
                    ? BuildType.Windows_x64_Self_Contained
                    : BuildType.Windows_x64,
                ReleaseInfos = []
            };
            repository.Insert(versionInfo);
        }

        return versionInfo;
    }

    public async Task<VersionInfo?> GetLatestVersionInfoAsync()
    {
        VersionInfo? result = null;
        string errorMessage = string.Empty;
        try
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MoreConvenientJiraSvn/1.0.1");
            HttpResponseMessage response = await client.GetAsync(DownloadSourceUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var release = JsonSerializer.Deserialize<GitHubRelease>(jsonResponse);

                if (release != null)
                {
                    result = new()
                    {
                        Version = release.TagName.TrimStart('v'),
                        Description = release.Body,
                        PublishTime = release.PublishTime,
                        Name = release.Name,
                        ReleaseInfos = [.. release.Assets
                            .Select(asset => new ReleaseAsset
                            {
                                Name = asset.Name,
                                DownloadUrl = asset.BrowserDownloadUrl,
                                Size = asset.Size,
                                BuildType = GetBuildTypeFromName(asset.Name),
                                IsPreRelease = release.IsPreRelease,
                            })]
                    };
                }
                else
                {
                    errorMessage = "can`t parse response to version info.";
                }
            }
            else
            {
                errorMessage = $"request failed: {response.StatusCode} - {response.ReasonPhrase}";
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }

        if (string.IsNullOrEmpty(errorMessage))
        {
            logService.LogInfo($"Get latest version info success: {result?.Version}");
        }
        else
        {
            logService.LogError($"Get latest version info failed: {errorMessage}");
        }

        return result;
    }

    public ReleaseAsset? GetUpdateVersionAssetAsync(VersionInfo currentVersion, VersionInfo latestVersion)
    {
        var release = latestVersion.ReleaseInfos.FirstOrDefault(a => a.BuildType == currentVersion.BuildType);
        if (release == null)
        {
            logService.LogDebug($"Can`t find the asset for current build type({currentVersion.BuildType}).");
            return null;
        }
        if (string.IsNullOrEmpty(release.Name) || string.IsNullOrEmpty(release.DownloadUrl))
        {
            logService.LogDebug($"The asset lack name or download url");
            return null;
        }
        return release;
    }

    public async Task<bool> DownloadReleaseAssetAsync(ReleaseAsset releaseAsset,string downloadPath)
    {
        var downloadAsset = releaseAsset;

        if (string.IsNullOrEmpty(downloadAsset.Name) || string.IsNullOrEmpty(releaseAsset.DownloadUrl))
        {
            logService.LogDebug($"The asset lack name or download url");
            return false;
        }

        try
        {
            using HttpClient client = new();
            if (File.Exists(downloadPath))
            {
                logService.LogDebug($"The asset already downloaded");
                return false;
            }

            HttpResponseMessage assetResponse = await client.GetAsync(releaseAsset.DownloadUrl);
            if (assetResponse.IsSuccessStatusCode)
            {
                using (Stream contentStream = await assetResponse.Content.ReadAsStreamAsync())
                {
                    using FileStream fileStream = new(downloadPath, FileMode.Create);
                    await contentStream.CopyToAsync(fileStream);
                }
                logService.LogDebug($"Download asset {downloadAsset.Name} success");
            }
            else
            {
                logService.LogDebug($"Download asset {downloadAsset.Name} failed: {assetResponse.StatusCode} - {assetResponse.ReasonPhrase}");
                return false;
            }
        }
        catch (Exception ex)
        {
            logService.LogError($"Download asset {downloadAsset.Name} failed: {ex.Message}");
            return false;
        }

        return true;
    }

    #region Private methods

    private static BuildType GetBuildTypeFromName(string assetName)
    {
        BuildType buildType = BuildType.Unknown;
        if (assetName.Contains("win", StringComparison.OrdinalIgnoreCase))
        {
            if (assetName.Contains("x64", StringComparison.OrdinalIgnoreCase))
            {
                if (assetName.Contains("self-contained", StringComparison.OrdinalIgnoreCase))
                {
                    buildType = BuildType.Windows_x64_Self_Contained;

                }
                else
                {
                    buildType = BuildType.Windows_x64;
                }
            }
        }

        return buildType;
    }

    public static bool IsSelfContained()
    {
        string? dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (string.IsNullOrEmpty(dotnetRoot))
        {
            return false;
        }

        string appBaseDir = AppContext.BaseDirectory;
        return dotnetRoot.StartsWith(appBaseDir, StringComparison.OrdinalIgnoreCase);
    }

    public record GitHubRelease
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;

        [JsonPropertyName("prerelease")]
        public bool IsPreRelease { get; set; }

        [JsonPropertyName("published_at")]
        public DateTime PublishTime { get; set; }

        [JsonPropertyName("assets")]
        public GitHubAsset[] Assets { get; set; } = [];

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

    }

    public record GitHubAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = string.Empty;
    }

    #endregion
}
