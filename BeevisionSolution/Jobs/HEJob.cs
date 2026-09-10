using System;
using static BeevisionSolution.Utils.Constant;

namespace BeevisionSolution.Models
{
    [Serializable]
     public class HEJob : FunctionJob
    {
        public int GridSize { get; set; }
        public int RotationSteps { get; set; }

        public bool IsCamMoving { get; set; }

        public HEJob(String strName, String strJobFile)
        {
            this.VisionType = JobType.TypeHandEye;
            this.Name = strName;
            this.JobFile = strJobFile;
        }

        public HEJob() : this(strDefaultAlignName, strDefaultJobFile)
        {
        }

        ~HEJob()
        {
            Dispose();
        }
    }
}
