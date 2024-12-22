using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MoreConvenientJiraSvn.Gui.View.Controls
{
    /// <summary>
    /// PluginCard.xaml 的交互逻辑
    /// </summary>
    public partial class PluginCard : System.Windows.Controls.UserControl
    {
        public PluginCard()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PluginNameProperty =
            DependencyProperty.Register("PluginName", typeof(string), typeof(PluginCard), new PropertyMetadata("插件名称"));

        public string PluginName
        {
            get { return (string)GetValue(PluginNameProperty); }
            set { SetValue(PluginNameProperty, value); }
        }

        public static readonly DependencyProperty PluginVersionProperty =
            DependencyProperty.Register("PluginVersion", typeof(string), typeof(PluginCard), new PropertyMetadata("V1.0"));

        public string PluginVersion
        {
            get { return (string)GetValue(PluginVersionProperty); }
            set { SetValue(PluginVersionProperty, value); }
        }

        public static readonly DependencyProperty PluginDescriptionProperty =
            DependencyProperty.Register("PluginDescription", typeof(string), typeof(PluginCard), new PropertyMetadata("插件描述"));

        public string PluginDescription
        {
            get { return (string)GetValue(PluginDescriptionProperty); }
            set { SetValue(PluginDescriptionProperty, value); }
        }

        public static readonly DependencyProperty PluginImageSourceProperty =
            DependencyProperty.Register("PluginImageSource", typeof(ImageSource), typeof(PluginCard), new PropertyMetadata(null));

        public ImageSource PluginImageSource
        {
            get { return (ImageSource)GetValue(PluginImageSourceProperty); }
            set { SetValue(PluginImageSourceProperty, value); }
        }

        public static readonly DependencyProperty InstallCommandProperty =
            DependencyProperty.Register("InstallCommand", typeof(ICommand), typeof(PluginCard), new PropertyMetadata(null));

        public ICommand InstallCommand
        {
            get { return (ICommand)GetValue(InstallCommandProperty); }
            set { SetValue(InstallCommandProperty, value); }
        }

        public static readonly DependencyProperty UnInstallCommandProperty =
            DependencyProperty.Register("UnInstallCommand", typeof(ICommand), typeof(PluginCard), new PropertyMetadata(null));

        public ICommand UnInstallCommand
        {
            get { return (ICommand)GetValue(UnInstallCommandProperty); }
            set { SetValue(UnInstallCommandProperty, value); }
        }
    }
}
