using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Jobs
{
    public class IOJobs
    {
        public string JobName { get; internal set; }
        public int PinTrigger { get; internal set; }

        public int PinOut { get; internal set; } = -1;
        public int PinOutOK { get; internal set; } = 3;
        public int PinOutNG { get; internal set; } = 5;
        public int DelayTime { get; internal set; } = 1000;//milisecond
        public int DelayNGTime { get; internal set; } = 1000;//milisecond

        public int[] LightJobId { get; set; }
    }
}
