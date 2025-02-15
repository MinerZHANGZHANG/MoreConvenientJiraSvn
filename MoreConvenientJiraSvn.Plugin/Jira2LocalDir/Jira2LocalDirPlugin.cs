using Microsoft.Extensions.DependencyInjection;

namespace MoreConvenientJiraSvn.Plugin.Jira2LocalDir;

[Plugin("ZZMiner", "0.0.1")]
public class Jira2LocalDirPlugin : IPlugin
{
    private ServiceProvider? _serviceProvider;

    public PluginInfo PluginInfo => new()
    {
        Author = "ZZMiner",
        Description = "According online jira info create and link to local dir",
        Name = "Jira2LocalDir",
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
        //Jira2LocalDirViewModel viewModel = new(_serviceProvider);
        //Jira2LocalDirWindow jira2LocalDirWindow = new(viewModel);
        //jira2LocalDirWindow.Show();
    }
}
