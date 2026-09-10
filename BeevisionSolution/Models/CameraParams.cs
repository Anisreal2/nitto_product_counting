using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Models
{
    public class CameraParams
    {
        public uint CameraId { get; set; }
        public double Contrast {  get; set; }
        public double Gain { get; set; }
        public double Exposure { get; set; }
        public double Brightness { get; set; }
    }
}
