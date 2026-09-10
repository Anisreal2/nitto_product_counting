using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Models
{
    public class VisionBarcode
    {
        public string SheetId { get; set; }
        public string LotNo {  get; set; }
        public int PcsIndex { get; set; }
        public int ResultCode {  get; set; }
    }
}
