using Microsoft.Extensions.DependencyInjection;

namespace MoreConvenientJiraSvn.Plugin.SvnJiraLink;

[Plugin("ZZMiner", "0.0.1")]
public class SvnJiraLinkPlugin : IPlugin
{
    private ServiceProvider? _serviceProvider;

    public PluginInfo PluginInfo => new()
    {
        Author = "ZZMiner",
        Description = "According online jira info create and link to local dir",
        Name = "SvnJiraLink",
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
        SvnJiraLinkViewModel viewModel = new(_serviceProvider);
        SvnJiraLinkWindow svnJiraLinkWindow = new(viewModel);
        svnJiraLinkWindow.Show();
    }
}
