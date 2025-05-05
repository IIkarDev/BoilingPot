// Converters/StringEqualsConverter.cs
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BoilingPot.Converters // Убедитесь, что пространство имен ваше
{
    public class StringEqualsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString()?.Equals(parameter?.ToString(), StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is true)
            {
                return parameter?.ToString();
            }
            return Avalonia.Data.BindingOperations.DoNothing;
        }
    }
}