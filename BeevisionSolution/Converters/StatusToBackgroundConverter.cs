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
    /// Converts ApplicationStatus to background color brush
    /// </summary>
    public class StatusToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ApplicationStatus status)
            {
                switch (status)
                {
                    case ApplicationStatus.Manual:
                        return new SolidColorBrush(Color.FromRgb(0, 128, 0)); // Dark Green
                    case ApplicationStatus.AutoRunning:
                    case ApplicationStatus.RunningAuto:
                        return new SolidColorBrush(Color.FromRgb(255, 102, 0)); // Orange
                    case ApplicationStatus.CameraLive:
                        return new SolidColorBrush(Color.FromRgb(0, 128, 128)); // Teal
                    case ApplicationStatus.Stopped:
                    default:
                        return new SolidColorBrush(Color.FromRgb(60, 60, 60)); // Dark Gray
                }
            }

            // Handle PLC Status
            if (value is PLCStatus plcStatus)
            {
                switch (plcStatus)
                {
                    case PLCStatus.Online:
                        return new SolidColorBrush(Color.FromRgb(0, 128, 0)); // Green
                    case PLCStatus.Offline:
                        return new SolidColorBrush(Color.FromRgb(255, 0, 0)); // Red
                }
            }

            // Handle Boolean (for SurvivalCheck)
            if (value is bool boolValue)
            {
                return boolValue 
                    ? new SolidColorBrush(Color.FromRgb(0, 255, 0)) // Green when true (OK)
                    : new SolidColorBrush(Color.FromRgb(255, 0, 0)); // Red when false (NG)
            }

            return new SolidColorBrush(Color.FromRgb(60, 60, 60));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
