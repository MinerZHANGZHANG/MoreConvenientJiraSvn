using MoreConvenientJiraSvn.Plugin.Jira2LocalDir;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MoreConvenientJiraSvn.Plugin.Convert
{
    public class JiraSelectTypeToStringConverter : IValueConverter
    {
        public static JiraSelectTypeToStringConverter Instance = new JiraSelectTypeToStringConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is JiraQueryType queryType)
            {
                return GetEnumDescription(queryType);
            }
            return JiraQueryType.JiraId;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetEnumDescription(JiraQueryType queryType)
        {
            var field = queryType.GetType().GetField(queryType.ToString());
            DescriptionAttribute? attribute = null;
            if (field != null)
            {
                attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            }

            return attribute != null ? attribute.Description : queryType.ToString();
        }
    }

    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public static InverseBooleanToVisibilityConverter Instance = new InverseBooleanToVisibilityConverter();
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
}
