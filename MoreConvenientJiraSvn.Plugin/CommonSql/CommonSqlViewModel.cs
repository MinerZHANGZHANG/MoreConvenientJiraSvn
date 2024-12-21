using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using MoreConvenientJiraSvn.Core.Model;
using MoreConvenientJiraSvn.Core.Service;
using System.Collections.ObjectModel;

namespace MoreConvenientJiraSvn.Plugin.CommonSql;

public partial class CommonSqlViewModel(ServiceProvider serviceProvider) : ObservableObject
{
    #region Service
    private readonly SvnService _svnService = serviceProvider.GetRequiredService<SvnService>();
    private readonly DataService _dataService = serviceProvider.GetRequiredService<DataService>();
    private readonly SettingService _settingService = serviceProvider.GetRequiredService<SettingService>();

    #endregion

    #region Property
    
    [ObservableProperty]
    private List<SqlCreateInfo> _sqlCreateInfos = [];

    [ObservableProperty]
    private List<CategoryGroup> _categoryGroups = [];

    #endregion

    public void InitViewModel()
    {
        
    }

    public void RefreshSvnLog()
    {

    }


    [RelayCommand]
    public async Task GetLatestSvnLog()
    {
       
    }

    //public void SplitSql(string sql)
    //{
    //    var tokens = sql.Split(new[] { ' ', ',', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
    //    Tokens.Clear();
    //    foreach (var token in tokens)
    //    {
    //        Tokens.Add(new SqlToken { Value = token });
    //    }
    //}

    //private void AddToken(SqlToken token)
    //{
    //    if (!SelectedTokens.Contains(token))
    //    {
    //        SelectedTokens.Add(token);
    //    }
    //}

    public class SqlToken
    {
        public string Value { get; set; }
    }


}



