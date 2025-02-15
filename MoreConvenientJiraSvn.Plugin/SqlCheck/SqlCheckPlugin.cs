using Microsoft.Extensions.DependencyInjection;

namespace MoreConvenientJiraSvn.Plugin.SqlCheck;

[Plugin("ZZMiner", "0.0.1")]
public class SqlCheckPlugin : IPlugin
{
    private ServiceProvider? _serviceProvider;

    public PluginInfo PluginInfo => new()
    {
        Author = "ZZMiner",
        Description = "Help to build common sql",
        Name = "SqlCheck",
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
        //SqlCheckViewModel viewModel = new(_serviceProvider);
        //SqlCheckWindow commonSqlWindow = new(viewModel);
        //commonSqlWindow.Show();
    }
}
