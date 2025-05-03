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
            // Сравниваем значение свойства (value) с параметром (parameter)
            // Оба приводим к строке для сравнения
            return value?.ToString()?.Equals(parameter?.ToString(), StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Приводим булево значение обратно к строке из параметра, если true
            // Иначе возвращаем что-то, что не выберет RadioButton (или null)
            if (value is true)
            {
                return parameter?.ToString();
            }
            // Важно: вернуть что-то, что НЕ совпадет ни с одним параметром,
            // чтобы другие RadioButton могли отжаться. null обычно работает.
            // Или можно вернуть специальное значение Binding.DoNothing
            return Avalonia.Data.BindingOperations.DoNothing;
            // или return null;
        }
    }
}