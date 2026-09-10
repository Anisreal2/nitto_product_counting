using BeevisionSolution.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace BeevisionSolution.Converters
{
    /// <summary>
    /// Converts InspectionResult to color brush
    /// </summary>
    public class ResultToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is InspectionResult result)
            {
                switch (result)
                {
                    case InspectionResult.OK:
                        return new SolidColorBrush(Color.FromRgb(0, 200, 0)); // Green
                    case InspectionResult.NG:
                        return new SolidColorBrush(Color.FromRgb(255, 0, 0)); // Red
                    case InspectionResult.None:
                    default:
                        return new SolidColorBrush(Color.FromRgb(180, 180, 180)); // Gray
                }
            }
            return new SolidColorBrush(Color.FromRgb(180, 180, 180));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
