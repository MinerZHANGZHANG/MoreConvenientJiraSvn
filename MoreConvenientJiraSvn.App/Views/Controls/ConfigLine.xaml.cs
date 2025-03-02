using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace MoreConvenientJiraSvn.App.Views.Controls;

/// <summary>
/// ConfigLine.xaml 的交互逻辑
/// </summary>
[ContentProperty("InputContent")]
public partial class ConfigLine : UserControl
{
    public ConfigLine()
    {
        InitializeComponent();

    }

    public static readonly DependencyProperty ConfigNameProperty =
        DependencyProperty.Register(nameof(ConfigName), typeof(string), typeof(ConfigLine), new PropertyMetadata("配置名称"));

    public static readonly DependencyProperty ShowConfigDescriptionCommandProperty =
        DependencyProperty.Register(nameof(ShowConfigDescriptionCommand), typeof(ICommand), typeof(ConfigLine), new PropertyMetadata(null));

    public static readonly DependencyProperty InputContentProperty =
        DependencyProperty.Register(nameof(InputContent), typeof(UIElement), typeof(ConfigLine), new PropertyMetadata(null));

    public static readonly DependencyProperty IsButtonVisibleProperty =
        DependencyProperty.Register(nameof(IsButtonVisible), typeof(bool), typeof(ConfigLine), new PropertyMetadata(true));

    public ICommand ShowConfigDescriptionCommand
    {
        get => (ICommand)GetValue(ShowConfigDescriptionCommandProperty);
        set => SetValue(ShowConfigDescriptionCommandProperty, value);
    }

    public string ConfigName
    {
        get => (string)GetValue(ConfigNameProperty);
        set => SetValue(ConfigNameProperty, value);
    }

    public UIElement InputContent
    {
        get { return (UIElement)GetValue(InputContentProperty); }
        set { SetValue(InputContentProperty, value); }
    }

    public bool IsButtonVisible
    {
        get { return (bool)GetValue(IsButtonVisibleProperty); }
        set { SetValue(IsButtonVisibleProperty, value); }
    }
}
