using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BeevisionSolution.ViewComponents
{
    /// <summary>
    /// Interaction logic for NumPlaceHolderTextbox.xaml
    /// </summary>
    public partial class NumPlaceHolderTextbox : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public delegate void OnValueChanged(double vl);
        
        // Define the Watermark dependency property
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(string), typeof(NumPlaceHolderTextbox), 
                new PropertyMetadata(string.Empty));
                
        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }
        
        // Define the Value dependency property
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(NumPlaceHolderTextbox), 
                new PropertyMetadata(0.0, ValuePropertyChanged));
                
        private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as NumPlaceHolderTextbox;
            if (control != null)
            {
                control.OnTextboxDataChanged?.Invoke((double)e.NewValue);
            }
        }
        
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        
        public OnValueChanged OnTextboxDataChanged { get; set; }
        
        public NumPlaceHolderTextbox()
        {
            InitializeComponent();
        }
        
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            string number = e.Text;
            if (number.StartsWith("-"))
            {
                number = number.Replace("-", string.Empty);
            }
            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(number);
        }

        private void SearchTermTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            OnTextboxDataChanged?.Invoke(Value);
        }

        protected void Notify([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
