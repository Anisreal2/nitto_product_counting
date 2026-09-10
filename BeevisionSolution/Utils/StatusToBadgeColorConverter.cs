using BeevisionSolution.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace BeevisionSolution.Utils
{
    public class StatusToBadgeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProductStatus status)
            {
                switch (status)
                {
                    case ProductStatus.OK:
                        return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                    case ProductStatus.NG:
                        return new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                    case ProductStatus.Processing:
                        return new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Blue
                    case ProductStatus.None:
                    default:
                        return new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Gray
                }
            }
            return new SolidColorBrush(Color.FromRgb(158, 158, 158));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
