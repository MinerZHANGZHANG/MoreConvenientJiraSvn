using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MoreConvenientJiraSvn.App.Converters;

public class EnumToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
        {
            return Visibility.Collapsed;
        }

        var enumType = value.GetType();
        if (!enumType.IsEnum)
        {
            return Visibility.Collapsed;
        }

        if (Enum.IsDefined(enumType, value))
        {
            var enumString = parameter.ToString();

            if (string.IsNullOrEmpty(enumString))
            {
                return Visibility.Collapsed;
            }
            var enumValue = Enum.Parse(enumType, enumString);
            return value.Equals(enumValue) ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}