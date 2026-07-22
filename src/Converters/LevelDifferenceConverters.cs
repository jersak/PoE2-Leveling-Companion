using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PoE2LevelingCompanion.Converters;

public sealed class LevelDifferenceToBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not int charLevel || values[1] is not int zoneLevel || zoneLevel == 0)
            return new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));

        int threshold = 3 + charLevel / 16;
        int effectiveDiff = Math.Max(0, Math.Abs(charLevel - zoneLevel) - threshold);

        return effectiveDiff switch
        {
            0 => new SolidColorBrush(Color.FromRgb(0x88, 0xcc, 0x88)),
            <= 3 => new SolidColorBrush(Color.FromRgb(0xe8, 0xa7, 0x35)),
            _ => new SolidColorBrush(Color.FromRgb(0xcc, 0x55, 0x55)),
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class ZoneLevelToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int level && level > 0)
            return System.Windows.Visibility.Visible;
        return System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
