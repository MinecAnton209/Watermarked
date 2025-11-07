using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Watermarked.Converters;

public class BoolToTextWrappingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isWrappingEnabled && isWrappingEnabled)
        {
            return TextWrapping.Wrap;
        }
        return TextWrapping.NoWrap;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
