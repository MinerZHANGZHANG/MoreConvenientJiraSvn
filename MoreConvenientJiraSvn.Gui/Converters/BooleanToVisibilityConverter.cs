using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MoreConvenientJiraSvn.App.Converters;

[Obsolete("Material nuget already have this")]
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is Visibility visibility) && (visibility == Visibility.Visible);
    }
}
