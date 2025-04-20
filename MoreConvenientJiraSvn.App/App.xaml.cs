using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoreConvenientJiraSvn.App.Properties;
using MoreConvenientJiraSvn.App.ViewModels;
using MoreConvenientJiraSvn.BackgroundTask;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Infrastructure;
using MoreConvenientJiraSvn.Service;
using System.Windows;
using Serilog;
using Serilog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using MoreConvenientJiraSvn.Core.Enums;

namespace MoreConvenientJiraSvn.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _services;

        [System.Runtime.InteropServices.LibraryImport("kernel32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static partial bool AllocConsole();

        public App()
        {
            var services = new ServiceCollection();

            AddLogService(services);

            services.AddSingleton(new LiteDatabase(Settings.Default.DatabaseName));
            services.AddSingleton(new NotificationService(Settings.Default.IconUrl));

            services.AddSingleton<IRepository, Repository>();
            services.AddSingleton<IJiraClient, JiraClient>();
            services.AddSingleton<IPlSqlCheckPipeline, PlSqlCheckPipeline>();
            services.AddSingleton<ISubversionClient, SubversionClient>();
            services.AddSingleton<IHtmlConvert, HtmlConvert>();

            services.AddSingleton<SettingService>();
            services.AddSingleton<SvnService>();
            services.AddSingleton<JiraService>();
            services.AddSingleton<SemanticKernelService>();

            services.AddHostedService<DownloadSvnLogHostedService>();
            services.AddHostedService<CheckJiraStateHostedService>();
            services.AddHostedService<CheckSqlHostedService>();
            
            services.AddSingleton<DownloadSvnLogHostedService>();
            services.AddSingleton<CheckJiraStateHostedService>();
            services.AddSingleton<CheckSqlHostedService>();

            services.AddTransient<JiraSettingViewModel>();
            services.AddTransient<SvnSettingViewModel>();
            services.AddTransient<JiraIssueBrowseViewModel>();
            services.AddTransient<SvnJiraLinkViewModel>();
            services.AddTransient<SqlCheckViewModel>();
            services.AddTransient<MainControlViewModel>();
            services.AddTransient<AppSettingControlViewModel>();
            services.AddTransient<HostedServiceSettingViewModel>();
            services.AddTransient<IssueAIAnalysisViewModel>();

            _services = services.BuildServiceProvider(true);

            Settings.Default.LastStartTime = DateTime.Now;
            Settings.Default.Save();

            ViewModelsManager.InitService(_services);

            this.Exit += App_Exit;
            this.Startup += App_Startup;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void AddLogService(ServiceCollection services)
        {
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Settings.Default.LogFileName, rollingInterval: RollingInterval.Day)
                .WriteTo.Debug();

            if (Settings.Default.LogRemindLevel == (int)LogRemindLevel.Debug)
            {
                AllocConsole();
                logConfig.WriteTo.Console(); // Add console sink only in Debug mode
            }

            Log.Logger = logConfig.CreateLogger();

            services.AddSingleton<ILoggerFactory>(new SerilogLoggerFactory(Log.Logger));
            services.AddSingleton<LogService>();
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            var settingService = _services.GetRequiredService<SettingService>();
            if (settingService.FindSetting<BackgroundTaskConfig>()?.IsEnableBackgroundTask != true)
            {
                return;
            }

            var hostServices = _services.GetServices<IHostedService>();
            foreach (var service in hostServices)
            {
                service.StartAsync(CancellationToken.None);
            }

            var logService = _services.GetRequiredService<LogService>();
            logService.LogInfo("Application started");
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

        private static void LogException(Exception ex)
        {
            System.IO.File.AppendAllText("exceptions.log", $"{DateTime.Now}: {ex}\n{ex.StackTrace}");
        }
    }

}
