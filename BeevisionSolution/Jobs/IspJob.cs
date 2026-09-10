using BeevisionSolution.Utils;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.Linq;
using static BeevisionSolution.Utils.Common;
using static BeevisionSolution.Utils.Constant;

namespace BeevisionSolution.Models
{
    [Serializable]
    public class IspJob : FunctionJob
    {
        public bool IncludeInInspectionHistory { get; set; } = true;
        /// <summary>Nhóm gộp bảng Inspection Data (cùng alias → 1 hàng). Rỗng = mỗi job một nhóm riêng.</summary>
        public string InspectionHistoryGroupAlias { get; set; } = "";
        public bool IsGetMotion { get; internal set; }
        public bool IsVidiJob { get; internal set; } = false;
        public IspJob(String strName, String strJobFile)
        {
            this.VisionType = JobType.TypeInspection;
            this.Name = strName;
            this.JobFile = strJobFile;
        }

        public IspJob() : this(strDefaultAlignName, strDefaultJobFile)
        {
        }

        ~IspJob()
        {
            Dispose();
        }
 
    }
}
