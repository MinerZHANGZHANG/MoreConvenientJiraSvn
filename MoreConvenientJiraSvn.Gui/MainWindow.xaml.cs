using MoreConvenientJiraSvn.Gui.ViewModel;
using System.Windows;

namespace MoreConvenientJiraSvn.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;
        public MainWindow()
        {
            DataContext = ViewModelsManager.GetViewModel<MainWindowViewModel>();
            InitializeComponent();
            InitializeNotifyIcon();
        }

        private void InitializeNotifyIcon()
        {
            // 创建通知区域图标  
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information, // 您可以使用自定义图标  
                Visible = true
            };

            // 添加点击事件  
            _notifyIcon.Click += NotifyIcon_Click;

            // 在应用程序关闭时隐藏托盘图标  
            this.Closing += MainWindow_Closing;
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            // 处理点击事件，您可以选择显示窗口等  
            this.Show();
            this.ShowNotification("测试", "测试消息");
            this.WindowState = WindowState.Normal;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 隐藏而不是关闭窗口  
            e.Cancel = true;
            this.Hide();
        }

        public void ShowNotification(string title, string message)
        {
            // 显示气泡通知  
            _notifyIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
        }

        protected override void OnClosed(EventArgs e)
        {
            // 释放资源  
            _notifyIcon?.Dispose();
            base.OnClosed(e);
        }
    }
}