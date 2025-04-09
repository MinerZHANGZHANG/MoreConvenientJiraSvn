using MoreConvenientJiraSvn.Core.Enums;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MoreConvenientJiraSvn.App.Converters;

public class LevelToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is InfoLevel level)
        {
            return level switch
            {
                InfoLevel.Normal => Brushes.Green,
                InfoLevel.Warning => Brushes.Orange,
                InfoLevel.Error => Brushes.Red,
                _ => Brushes.Gray
            };
        }

        return Brushes.Gray; // Default color for unknown levels
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
