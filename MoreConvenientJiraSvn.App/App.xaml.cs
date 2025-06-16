using LiteDB;
using MdXaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoreConvenientJiraSvn.App.Properties;
using MoreConvenientJiraSvn.App.ViewModels;
using MoreConvenientJiraSvn.BackgroundTask;
using MoreConvenientJiraSvn.Core.Enums;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Infrastructure;
using MoreConvenientJiraSvn.Service;
using Serilog;
using Serilog.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

            // 获取exe所在目录
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var exeDir = System.IO.Path.GetDirectoryName(exePath);

            // 组合完整路径
            var dbPath = System.IO.Path.Combine(exeDir, Settings.Default.DatabaseName);
            var iconPath = System.IO.Path.Combine(exeDir, Settings.Default.IconUrl);

            services.AddSingleton(new LiteDatabase(dbPath));
            services.AddSingleton(new NotificationService(iconPath));
            services.AddSingleton(new Markdown());

            services.AddSingleton<IRepository, Repository>();
            services.AddSingleton<IJiraClient, JiraClient>();
            services.AddTransient<IPlSqlIssueChecker, PlSqlChecker>();
            services.AddSingleton<ISubversionClient, SubversionClient>();
            services.AddSingleton<IHtmlConvert, HtmlConvert>();

            services.AddSingleton<SettingService>();
            services.AddSingleton<SvnService>();
            services.AddSingleton<JiraService>();
            services.AddSingleton<SemanticKernelService>();
            services.AddSingleton<IVersionService, GitHubVersionService>();

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
            services.AddTransient<VersionControlViewModel>();
            services.AddTransient<HostedServiceSettingViewModel>();
            services.AddTransient<IssueAIAnalysisViewModel>();

            _services = services.BuildServiceProvider(true);

            Settings.Default.LastStartTime = DateTime.Now;
            Settings.Default.Save();

            ViewModelsManager.InitService(_services);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

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

        private async void App_Startup(object sender, StartupEventArgs e)
        {
            var logService = _services.GetRequiredService<LogService>();
            logService.LogInfo("Application started");

            var repository = _services.GetRequiredService<IRepository>();
            repository.InitMapping();
            if (!repository.TryMigrate())
            {
                MessageBox.Show("数据库迁移失败，请检查日志文件。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
            else
            {
                //logService.LogInfo($"Database migration completed successfully");
            }

            var settingService = _services.GetRequiredService<SettingService>();
            if (settingService.FindSetting<BackgroundTaskConfig>()?.IsEnableBackgroundTask != true)
            {
                return;
            }

            var hostServices = _services.GetServices<IHostedService>();
            foreach (var service in hostServices)
            {
                await service.StartAsync(CancellationToken.None);
            }

            if (e.Args.Length > 0 && e.Args[0].StartsWith($"mcjs://"))
            {
                Uri uri = new Uri(e.Args[0]);
                string page = uri.Host; // 获取主机部分
                string query = uri.Query; // 获取查询字符串

                // 根据page值导航到不同页面
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

        private static void LogException(Exception ex)
        {
            System.IO.File.AppendAllText("exceptions.log", $"{Environment.NewLine}{DateTime.Now}: {ex}\n{ex.StackTrace}");
        }
    }

}
