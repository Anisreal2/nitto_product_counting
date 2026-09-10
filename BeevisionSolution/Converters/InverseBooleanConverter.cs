using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace BeevisionSolution.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                // Nếu targetType là Visibility, trả về Visibility
                if (targetType == typeof(Visibility) || targetType == typeof(Visibility?))
                {
                    return !b ? Visibility.Visible : Visibility.Collapsed;
                }
                // Nếu không, trả về boolean (giữ nguyên behavior cũ)
                return !b;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
                return v != Visibility.Visible;
            if (value is bool b)
                return !b;
            return Binding.DoNothing;
        }
    }
}