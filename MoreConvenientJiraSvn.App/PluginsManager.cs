using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MoreConvenientJiraSvn.App
{
    [Obsolete]
    internal static class PluginsManager
    {
        public static List<Window> windows = [];

        public static void InitPlugins(ServiceProvider serviceProvider)
        {
            //Assembly assembly = Assembly.Load("MoreConvenientJiraSvn.Plugin");
            //IEnumerable<Type> types = assembly.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract);
            //foreach (var type in types)
            //{
            //    if (type == null)
            //    {
            //        continue;
            //    }
            //    var attribute = type.GetCustomAttribute(typeof(PluginAttribute), false);
            //    if (attribute == null)
            //    {
            //        continue;
            //    }
            //    IPlugin? impl = (IPlugin?)Activator.CreateInstance(type);
            //    if (impl == null)
            //    {
            //        continue;
            //    }
            //    impl.Initialize(serviceProvider);
            //    plugins.Add(impl);
            //}
        }
    }
}
