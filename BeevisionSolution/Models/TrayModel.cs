using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Models
{
    public class TrayModel
    {
        public int Id { get; set; }
        public string TrayIdMain { get; set; }
        public string TrayIdSub { get; set; }
        public string LotNumber { get; set; }
        public string TrayProfile { get; set; }
        public int TotalItems { get; set; }
        public Dictionary<int, string> PreData { get; set; } = new Dictionary<int, string>();
        public ObservableCollection<ProductModel> Products { get; set; } = new ObservableCollection<ProductModel>();
    }
}
