using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Models
{
    public class StageResult
    {
        public int TotalQuantity { get; set; }
        public int OKQuantity { get; set; }
        public int NGQuantity { get; set; }
    }

    public class ResultModel
    {
        public StageResult StageA { get; set; } = new StageResult();
        public StageResult StageB { get; set; } = new StageResult();
    }
}
