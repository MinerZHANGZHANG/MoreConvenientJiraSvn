using MoreConvenientJiraSvn.Core.Utils;
using System.Globalization;
using System.Windows.Data;

namespace MoreConvenientJiraSvn.App.Converters;

public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return string.Empty;
        }
        string enumDescription = EnumHelper.GetEnumValueDescription(targetType, value);

        return enumDescription;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
