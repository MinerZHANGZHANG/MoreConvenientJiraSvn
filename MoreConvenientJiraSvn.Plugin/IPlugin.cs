using Microsoft.Extensions.DependencyInjection;

namespace MoreConvenientJiraSvn.Plugin
{
    public interface IPlugin
    {
        void Initialize(ServiceProvider serviceProvider);

        void OpenWindow();

        PluginInfo PluginInfo { get; }
    }
}
