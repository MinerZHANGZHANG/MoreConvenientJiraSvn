using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using MoreConvenientJiraSvn.Core.Model;
using MoreConvenientJiraSvn.Core.Service;

namespace MoreConvenientJiraSvn.Plugin.SvnJiraLink;

public partial class SvnJiraLinkViewModel(ServiceProvider serviceProvider) : ObservableObject
{
    #region Service
    private readonly SvnService _svnService = serviceProvider.GetRequiredService<SvnService>();
    private readonly DataService _dataService = serviceProvider.GetRequiredService<DataService>();
    private readonly SettingService _settingService = serviceProvider.GetRequiredService<SettingService>();

    #endregion

    #region Property

    [ObservableProperty]
    private SvnConfig _config = new();

    [ObservableProperty]
    private List<SvnPath> _svnPaths = [];

    [ObservableProperty]
    private SvnPath? _selectedPath;
    [ObservableProperty]
    private List<SvnLog> _selectedSvnLogs = [];
    [ObservableProperty]
    private JiraSvnPathRelation _selectPathRelation = new();

    #endregion

    public void InitViewModel()
    {
        Config = _svnService.Config ?? new();
        SvnPaths = _svnService.Paths;
    }

    public void RefreshSvnLog()
    {
        if (SelectedPath != null)
        {
            SelectedSvnLogs = _dataService.SelectByExpression<SvnLog>(Query.EQ(nameof(SvnLog.SvnPath), SelectedPath.Path)).ToList();
            SelectPathRelation = _dataService.SelectOneByExpression<JiraSvnPathRelation>(Query.EQ(nameof(JiraSvnPathRelation.SvnPath), SelectedPath.Path))
                ?? new() { SvnPath = SelectedPath.Path };
        }
    }

    [RelayCommand]
    public async Task GetLatestSvnLog()
    {
        if (SelectedPath != null)
        {
            DateTime begTime = DateTime.Now.AddYears(-20);
            if (SelectedSvnLogs.Count != 0)
            {
                begTime = SelectedSvnLogs.Select(log => log.DateTime).Max();
            }
            List<SvnLog> addition = [];
            await Task.Run(() =>
            {
                bool isHaveJiraId = SelectedPath.SvnPathType == SvnPathType.Code || SelectedPath.SvnPathType == SvnPathType.Document;

                addition = _svnService.GetSvnLogs(SelectedPath.Path, begTime, DateTime.Now, isNeedExtractJiraId: isHaveJiraId);
            });
            if (addition != null)
            {
                _dataService.InsertOrUpdateMany(addition);
                SelectedSvnLogs = [.. SelectedSvnLogs, .. addition];
            }
        }
    }

    [RelayCommand]
    public void SetRelationVersion()
    {
        if (SelectedPath != null && SelectPathRelation?.FixVersion != null)
        {
            var relation = _dataService.SelectOneByExpression<JiraSvnPathRelation>(Query.EQ(nameof(JiraSvnPathRelation.SvnPath), SelectedPath.Path));
            if (relation != null)
            {
                relation.FixVersion = SelectPathRelation.FixVersion;
            }
            else
            {
                relation = SelectPathRelation;
            }
            _dataService.InsertOrUpdate(relation);
        }
    }

}



