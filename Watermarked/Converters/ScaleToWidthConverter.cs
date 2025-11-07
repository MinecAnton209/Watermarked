using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace Watermarked.Converters;

public class ScaleToWidthConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 2 && 
            values[0] is double containerWidth && 
            values[1] is double scalePercentage)
        {
            return containerWidth * (scalePercentage / 100.0);
        }
        
        return 0.0;
    }
}