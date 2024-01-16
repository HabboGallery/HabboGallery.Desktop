using System.Globalization;

namespace HabboGallery.Desktop.Helpers.Converters;

public class StringInterningConverter : IValueConverter
{
    private readonly string _format;

    public StringInterningConverter(string format)
    {
        _format = format;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.Format(_format, value);
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
