using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace MoreConvenientJiraSvn.App
{
    internal static class ViewModelsManager
    {
        private static ServiceProvider? _serviceProvider;
        public static void InitService(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static T GetViewModel<T>() where T : ObservableObject
        {
            T result;
            if (_serviceProvider != null)
            {
                result = _serviceProvider.GetRequiredService<T>();
            }
            else
            {
                throw new InvalidOperationException("ServiceProvicer no sign!");
            }
            return result;
        }
    }
}
