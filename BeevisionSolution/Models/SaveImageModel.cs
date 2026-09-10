using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Models
{
    public class SaveImageModel
    {
        public Grade Grade { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public Image GraphicImg { get; set; }
        public ICogImage RawImage { get; set; }
        public bool IsRaw { get; set; }
        public bool IsBarcode { get; set; }
    }
}
