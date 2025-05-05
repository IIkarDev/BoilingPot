using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BoilingPot.Converters
{
    public class MultiplyDurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan && parameter is string multiplierStr)
            {
                if (double.TryParse(multiplierStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double multiplier))
                {
                    return TimeSpan.FromMilliseconds(timeSpan.TotalMilliseconds * multiplier);
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}