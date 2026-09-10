using Cognex.VisionPro.PMAlign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Models
{
    [System.Serializable]
    public class PatternListContainer
    {
        public List<CogPMAlignPattern> Patterns { get; set; } = new List<CogPMAlignPattern>();
    }
}
