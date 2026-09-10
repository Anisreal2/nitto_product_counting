using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BeevisionSolution.Converters
{
    /// <summary>
    /// Converts zoom percentage (100 = 100%) to scale factor (100 -> 1.0)
    /// </summary>
    public class ZoomToScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double zoom)
            {
                return zoom / 100.0;
            }
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double scale)
            {
                return scale * 100.0;
            }
            return 100.0;
        }
    }
}
