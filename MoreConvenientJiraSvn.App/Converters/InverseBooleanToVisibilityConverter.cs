using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MoreConvenientJiraSvn.App.Converters;

public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public static InverseBooleanToVisibilityConverter Instance { get; } = new();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool booleanValue)
        {
            return booleanValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility visibility && visibility == Visibility.Visible;
    }
}
