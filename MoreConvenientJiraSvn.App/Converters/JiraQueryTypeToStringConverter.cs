using MoreConvenientJiraSvn.App.ViewModels;
using MoreConvenientJiraSvn.Core.Enums;
using MoreConvenientJiraSvn.Core.Utils;
using System.Globalization;
using System.Windows.Data;

namespace MoreConvenientJiraSvn.App.Converters;

public class JiraQueryTypeToStringConverter : IValueConverter
{
    public static JiraQueryTypeToStringConverter Instance { get; } = new();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is JiraIssueQueryType queryType)
        {
            return EnumHelper.GetEnumValueDescription(queryType);
        }
        return JiraIssueQueryType.JiraId;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
