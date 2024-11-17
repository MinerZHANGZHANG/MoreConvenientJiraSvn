using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using MoreConvenientJiraSvn.Core.Service;
using MoreConvenientJiraSvn.Gui.Properties;
using MoreConvenientJiraSvn.Gui.ViewModel;
using System.Windows;

namespace MoreConvenientJiraSvn.Gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _services;

        public App()
        {
            var services = new ServiceCollection();

            services.AddSingleton(new LiteDatabase(Settings.Default.DatabaseName));

            services.AddSingleton<DataService>();
            services.AddSingleton<SettingService>();
            services.AddSingleton<SvnService>();
            services.AddSingleton<JiraService>();

            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<JiraSettingViewModel>();
            services.AddTransient<SvnSettingViewModel>();

            _services = services.BuildServiceProvider(true);

            Settings.Default.LastStartTime = DateTime.Now;
            Settings.Default.Save();

            PluginsManager.InitPlugins(_services);
            ViewModelsManager.InitService(_services);

            this.Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            _services.GetService<SvnService>()?.Dispose();
        }
    }

}
