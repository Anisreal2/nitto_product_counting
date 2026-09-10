using BeevisionSolution.Utils;
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
    public class RoleToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UserRole role)
            {
                switch (role)
                {
                    case UserRole.Master:
                        return Application.Current.TryFindResource("strUserRoleAdmin");
                    case UserRole.Engineer:
                        return Application.Current.TryFindResource("strUserRoleEngineer");
                    case UserRole.Operator:
                    default:
                        return Application.Current.TryFindResource("strUserRoleOperator");
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
