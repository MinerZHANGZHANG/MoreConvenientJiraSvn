using Microsoft.Extensions.DependencyInjection;

namespace MoreConvenientJiraSvn.Plugin.CommonSql;

[Plugin("ZZMiner", "0.0.1")]
public class CommonSqlPlugin : IPlugin
{
    private ServiceProvider? _serviceProvider;

    public PluginInfo PluginInfo => new()
    {
        Author = "ZZMiner",
        Description = "Help to build common sql",
        Name = "CommonSql",
        Version = "1.0",
        UpdateTime = DateTime.Now
    };

    public void Initialize(ServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void OpenWindow()
    {
        if (_serviceProvider == null) { return; }
        CommonSqlViewModel viewModel = new(_serviceProvider);
        CommonSqlWindow commonSqlWindow = new(viewModel);
        commonSqlWindow.Show();
    }
}
