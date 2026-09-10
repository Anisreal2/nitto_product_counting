using System;
using static BeevisionSolution.Utils.Constant;

namespace BeevisionSolution.Models
{
    [Serializable]
    public class AlignJob : FunctionJob
    {
        public bool IsPreAlign { get; internal set; } = false;
        public bool IsNPointsAlign { get; set; } = false;
        public bool IsReverseNPointsCalculation { get; set; } = false;
        /// <summary>Bật thì kết quả align/multimark (job này) được đẩy lên Align History trên ImageView.</summary>
        public bool IncludeInAlignHistory { get; set; } = true; 
        public double XOffset { get; set; } 
        public double YOffset { get; set; }
        public AlignJob(String strName, String strJobFile)
        {
            this.VisionType = JobType.TypeAlignment;
            this.Name = strName;
            this.JobFile = strJobFile;
        }

        public AlignJob() : this(strDefaultAlignName, strDefaultJobFile)
        {
        }

        ~AlignJob()
        {
            Dispose();
        }
    }
}
