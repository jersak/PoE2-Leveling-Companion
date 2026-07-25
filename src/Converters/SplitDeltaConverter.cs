using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PoE2LevelingCompanion.Converters;

public sealed class SplitDeltaToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TimeSpan delta)
            return Brushes.Transparent;

        if (delta < TimeSpan.Zero)
            return new SolidColorBrush(Color.FromRgb(0x88, 0xcc, 0x88));
        if (delta > TimeSpan.Zero)
            return new SolidColorBrush(Color.FromRgb(0xcc, 0x55, 0x55));
        return new SolidColorBrush(Color.FromRgb(0xe8, 0xa7, 0x35));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
