using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using FLyC0De.Core.Models;

namespace FLyC0De.App.Converters
{
    public class PluginStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PluginStatus status)
            {
                switch (status)
                {
                    case PluginStatus.Active:
                        return new SolidColorBrush(Color.FromRgb(0, 255, 0)); // Green
                    case PluginStatus.Inactive:
                    case PluginStatus.Locked:
                        return new SolidColorBrush(Color.FromRgb(255, 0, 0)); // Red
                }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
