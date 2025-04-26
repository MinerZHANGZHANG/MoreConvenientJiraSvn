using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;

namespace MoreConvenientJiraSvn.Service;

public class SvnService : IDisposable
{
    private readonly IRepository _repository;
    private readonly ISubversionClient _svnClient;

    private readonly SettingService _settingService;
    private readonly LogService _logService;

    private SvnConfig _svnConfig;
    public SvnConfig SvnConfig => _svnConfig;

    private IEnumerable<SvnPath> _svnPaths;
    public IEnumerable<SvnPath> SvnPaths => _svnPaths;

    public SvnService(IRepository dataService, ISubversionClient svnClient, SettingService settingService, LogService logService)
    {
        _repository = dataService;
        _svnClient = svnClient;

        _settingService = settingService;
        _logService = logService;

        _svnConfig = _settingService.FindSetting<SvnConfig>() ?? new();
        _svnPaths = _settingService.FindSettings<SvnPath>() ?? [];

        _settingService.OnConfigChanged += RefreshSvnClient_OnConfigChanged;

        _svnClient.InitSvnClient(_svnConfig);
    }

    private void RefreshSvnClient_OnConfigChanged(object? sender, ConfigChangedArgs e)
    {
        if (e.Config is SvnConfig config)
        {
            if (!string.IsNullOrEmpty(config.UserName) && !string.IsNullOrEmpty(config.UserPassword))
            {
                _svnClient.InitSvnClient(config);
            }
            _svnConfig = config;
        }
        else if (e.Config is IEnumerable<SvnPath> svnPaths)
        {
            _svnPaths = svnPaths;
        }
    }

    public void Dispose()
    {
        _svnClient.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Operation methods

    // TODO: Group query param
    public async Task<List<SvnLog>> GetSvnLogsAsync(string path, DateTime? beginDate, DateTime? endDate, int maxNumber = 200, bool isNeedExtractJiraId = false, CancellationToken cancellationToken = default)
    {
        _logService.LogDebug($"{nameof(GetSvnLogs)}({path}) [{beginDate}——{endDate}](maxCount:{maxNumber})");

        List<SvnLog> result = await _svnClient.GetSvnLogAsync(path, beginDate ?? DateTime.MinValue, endDate ?? DateTime.Today, maxNumber, isNeedExtractJiraId, cancellationToken);

        return result;
    }

    public async Task<List<SvnLog>> GetSvnLogs(string path, long? beginRevision, long? endRevision, int maxNumber = 200, bool isNeedExtractJiraId = false, CancellationToken cancellationToken = default)
    {
        _logService.LogDebug($"{nameof(GetSvnLogs)}({path}) [{beginRevision}——{endRevision}](maxCount:{maxNumber})");

        List<SvnLog> result = await _svnClient.GetSvnLogAsync(path, beginRevision ?? 0, endRevision ?? long.MaxValue, maxNumber, isNeedExtractJiraId, cancellationToken);

        return result;
    }

    #endregion

}
