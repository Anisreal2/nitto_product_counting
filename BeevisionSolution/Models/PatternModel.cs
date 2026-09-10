using Cognex.VisionPro.PMAlign;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BeevisionSolution.Models
{
    public class PatternModel : INotifyPropertyChanged
    {
        public int PatternIndex { get; set; }
        public bool IsEnabled { get; set; } = true;
        private CogPMAlignPattern _pattern;
        public CogPMAlignPattern Pattern
        {
            get => _pattern;
            set { _pattern = value; OnPropertyChanged(); }
        }
        public BitmapImage PatternImage { get; set; }
        public string FilePath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
