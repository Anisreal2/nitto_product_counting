using BeevisionSolution.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BeevisionSolution.Utils
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;

            // Check if inverse is requested
            if (parameter is string param && param.ToLower() == "inverse")
            {
                boolValue = !boolValue;
            }

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool result = visibility == Visibility.Visible;

                if (parameter is string param && param.ToLower() == "inverse")
                {
                    result = !result;
                }

                return result;
            }
            return false;
        }
    }

    public class StatusToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2 ||
                !(values[0] is ProductStatus status) || !(values[1] is bool isSelected))
            {
                return Brushes.DarkCyan;
            }
            if (isSelected)
                return Brushes.Pink; // Highlight selected cell

            switch (status)
            {
                case ProductStatus.OK:
                    return Brushes.LimeGreen;
                case ProductStatus.NG:
                    return Brushes.Red;
                case ProductStatus.Processing:
                    return Brushes.Gold;
                case ProductStatus.PreNG:
                    return Brushes.Coral;
                default:
                    return Brushes.LightGray;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ProductStatus status))
            {
                return Brushes.Black;
            }

            switch (status)
            {
                case ProductStatus.NG:
                    return Brushes.Red;
                case ProductStatus.Processing:
                    return Brushes.Gold;
                case ProductStatus.OK: 
                    return Brushes.LimeGreen;
                case ProductStatus.PreNG:
                    return Brushes.Coral;
                default:
                    return Brushes.LightGray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class IsSelectedToBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is bool isSelected))
            {
                return System.Windows.Media.Brushes.DarkGray;
            }

            if (isSelected)
            {
                return System.Windows.Media.Brushes.Black;
            }
            return System.Windows.Media.Brushes.DarkGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BoolToOkNgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? "OK" : "NG";

            return "NONE";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ProductStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value as ProductStatus? ?? ProductStatus.None;

            switch (status)
            {
                case ProductStatus.None:
                    return "None";
                case ProductStatus.OK:
                    return "OK";
                case ProductStatus.NG:
                    return "NG";
                case ProductStatus.Processing:
                    return "Processing";
                case ProductStatus.PreNG:
                    return "PreNG";
                default:
                    return "None";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value as string;
            if (s == null) return ProductStatus.None;

            switch (s)
            {
                case "None":
                    return ProductStatus.None;
                case "OK":
                    return ProductStatus.OK;
                case "NG":
                    return ProductStatus.NG;
                case "Processing":
                    return ProductStatus.Processing;
                case "PreNG":
                    return ProductStatus.PreNG;
                default:
                    return ProductStatus.None;
            }
        }
    }
    public class StatusToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BrushConverter bc = new BrushConverter();

            // Vision Status
            if (value is ApplicationStatus)
            {
                var v = (ApplicationStatus)value;

                switch (v)
                {
                    case ApplicationStatus.AutoRunning:
                        return (Brush)bc.ConvertFrom("#99FF99");   // xanh nhạt

                    case ApplicationStatus.Manual:
                        return (Brush)bc.ConvertFrom("#005544");   // xanh đậm

                    case ApplicationStatus.CameraLive:
                        return (Brush)bc.ConvertFrom("#99CCFF");   // xanh trời nhạt

                    default:
                        return Brushes.Gray;
                }
            }

            // PLC Status
            if (value is PLCStatus)
            {
                var p = (PLCStatus)value;

                switch (p)
                {
                    case PLCStatus.Online:
                        return (Brush)bc.ConvertFrom("#009966");

                    case PLCStatus.Offline:
                        return (Brush)bc.ConvertFrom("#CC0000");

                    default:
                        return Brushes.DarkGray;
                }
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}