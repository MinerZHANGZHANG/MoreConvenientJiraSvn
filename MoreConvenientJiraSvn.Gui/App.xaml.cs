using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoreConvenientJiraSvn.Core.Service;
using MoreConvenientJiraSvn.Gui.Properties;
using MoreConvenientJiraSvn.Gui.ViewModel;
using System.Windows;
using Application = System.Windows.Application;

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
            services.AddSingleton(new NotificationService(Settings.Default.IconUrl));

            services.AddSingleton<DataService>();
            services.AddSingleton<SettingService>();
            services.AddSingleton<SvnService>();
            services.AddSingleton<JiraService>();

            services.AddHostedService<DownloadSvnLogHostedService>();
            services.AddHostedService<CheckJiraStateHostedService>();

            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<JiraSettingViewModel>();
            services.AddTransient<SvnSettingViewModel>();

            _services = services.BuildServiceProvider(true);

            Settings.Default.LastStartTime = DateTime.Now;
            Settings.Default.Save();

            PluginsManager.InitPlugins(_services);
            ViewModelsManager.InitService(_services);

            this.Exit += App_Exit;
            this.Startup += App_Startup;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private async void App_Startup(object sender, StartupEventArgs e)
        {
            var hostServices = _services.GetServices<IHostedService>();
            foreach (var service in hostServices)
            {
                await service.StartAsync(CancellationToken.None);
            }
        }

        private async void App_Exit(object sender, ExitEventArgs e)
        {
            var hostServices = _services.GetServices<IHostedService>();
            foreach (var service in hostServices)
            {
                await service.StopAsync(CancellationToken.None);
            }

            await _services.DisposeAsync();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);
            MessageBox.Show($"发生了一个未处理的异常: {e.Exception.Message}\n{e.Exception.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                MessageBox.Show("应用程序发生了无法恢复的错误，将会结束运行。", "致命错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Exception ex = (Exception)e.ExceptionObject;
                LogException(ex);
            }
        }

        private void LogException(Exception ex)
        {
            // TODO: Replace it to a more useful log
            System.IO.File.AppendAllText("exceptions.log", $"{DateTime.Now}: {ex}\n");
        }
    }

}
