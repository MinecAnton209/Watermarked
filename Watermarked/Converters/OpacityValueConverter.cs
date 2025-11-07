using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Watermarked.Converters
{
    public class OpacityValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return 0m;

            double opacity = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            double percent = opacity * 100.0;

            if (targetType == typeof(decimal))
                return (decimal)percent;

            return percent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return 0.0;

            double percent = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            double opacity = percent / 100.0;

            return opacity;
        }
    }
}