using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Models
{
    public class AlignHeaderItem
    {
        public AlignHeaderItem()
        {

        }

        public int No {  get; set; }
        public string IoType {  get; set; }
        public string Class { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public bool Value { get; set; }
        public string Type { get; set; } 
        public string Address { get; set; }
    }
}
