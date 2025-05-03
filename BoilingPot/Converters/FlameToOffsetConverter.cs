using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BoilingPot.Converters
{
    public class FlameToOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double flameLevel)
            {
                // Нормализуем значение пламени (предполагается, что оно от 0 до 10)
                // к диапазону 0 - 0.5 для Offset
                double normalizedValue = Math.Clamp(flameLevel / 10.0, 0.0, 1.0);
                return 0.5 * normalizedValue; // Значение от 0 до 0.5
            }
            return 0.0; // Значение по умолчанию
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}