using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BeevisionSolution.Utils.Common;

namespace BeevisionSolution.Models
{
    [Serializable]
    public abstract class BaseJob : IDisposable
    {
        private Object _ToolBlock;

        [JsonIgnore]
        private readonly object _toolBlockLock = new object();
        [JsonIgnore]
        protected object ToolBlockLock => _toolBlockLock;
        [JsonIgnore]
        public Object Params { get; internal set; }
        [JsonIgnore]
        public Object ToolBlock
        {
            get => _ToolBlock;
            set
            {
                if (null != _ToolBlock)
                {
                    ((CogToolBlock)_ToolBlock).Ran -= ToolBlockRan;
                    ((CogToolBlock)_ToolBlock).Dispose();
                    MemoryCleanup();
                }

                _ToolBlock = value;
                if (null != _ToolBlock)
                {
                    ((CogToolBlock)_ToolBlock).Ran += ToolBlockRan;
                }
            }
        }
        [JsonIgnore]
        public Object OutputImage { get; internal set; } = null;
        [JsonIgnore]
        public Action<Object> OnToolBlockLoaded;
        [JsonIgnore]
        protected bool disposed = false;
        [JsonIgnore]
        internal Action<Object, Object, Object> OnToolBlockRan;
        [JsonIgnore]
        internal Action<int, string, string, ICogImage, List<object>, bool, double> OnResult = null;
        [JsonIgnore]
        public bool Available { get; protected internal set; } = true;
        [JsonIgnore]
        public CogToolResultConstants RunStatus { get; protected set; } = CogToolResultConstants.Error;
        [JsonIgnore]
        public bool AllowFireEvent { get; internal set; } = true;
        [JsonIgnore]
        public bool Initialized { get; private set; } = false;

        public CameraParams CamSettings { get; internal set; }
        public uint DisplayId { get; internal set; } = 0;
        public JobType VisionType { get; protected set; }
        public String JobFile { get; protected set; }
        public String Name { get; protected set; }
        public List<string> Alias { get; protected set; }

        public virtual bool RunTool() => false;
        public virtual async Task<bool> RunToolAsync() => await Task.FromResult(false);

        public virtual bool Save() => SaveToolBlock(ToolBlock, JobFile);
        protected virtual void ToolBlockRan(object sender, EventArgs e) { }

        public virtual bool IsMe(string strJobName)
        {
            if (Name.Equals(strJobName)) return true;
            return ((null != Alias) && Alias.Contains(strJobName));
        }

        public virtual void Init()
        {
            Info("{0} is initializing...", Name);
            AllowFireEvent = true;
            ToolBlock = LoadToolBlock(JobFile);
            OnToolBlockLoaded?.Invoke(this);
            disposed = false;
            Initialized = true;
        }
        public virtual void SetInput(string inputName, object inputValue)
        {
            var tb = (CogToolBlock)_ToolBlock;
            if (tb.Inputs.Contains(inputName))
                tb.Inputs[inputName].Value = inputValue;
        }
        public virtual object GetOutput(string outputName)
        {
            var tb = (CogToolBlock)_ToolBlock;
            if (tb.Outputs.Contains(outputName))
                return tb.Outputs[outputName].Value;
            return null;
        }
        public virtual void Dispose()
        {
            Info("{0} on Dispose", Name);
            if ((!disposed) && (null != ToolBlock))
            {
                ((CogToolBlock)ToolBlock).Ran -= ToolBlockRan;
                ((CogToolBlock)ToolBlock).Dispose();
                ToolBlock = null;
            }
            Initialized = false;
            disposed = true;
        }

        public virtual void RaiseOnResult(int displayId, string camName, string prefix, ICogImage image, List<object> lstResult, bool isOK, double actualDist = double.NaN)
        {
            OnResult?.Invoke(displayId, camName, prefix, image, lstResult, isOK, actualDist);
        }
    }
}
