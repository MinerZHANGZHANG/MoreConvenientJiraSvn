using MoreConvenientJiraSvn.App.ViewModels;
using MoreConvenientJiraSvn.Core.Utils;
using System.Globalization;
using System.Windows.Data;

namespace MoreConvenientJiraSvn.App.Converters;

public class JiraQueryTypeToStringConverter : IValueConverter
{
    public static JiraQueryTypeToStringConverter Instance { get; } = new();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is JiraQueryType queryType)
        {
            return EnumHelper.GetEnumDescription(queryType);
        }
        return JiraQueryType.JiraId;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
