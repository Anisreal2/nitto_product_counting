using Cognex.VisionPro.ToolBlock;

namespace BeevisionSolution.Jobs.CameraHandlers
{
    public class VppCameraHandler : ICameraHandler
    {
        public bool IsInitialized => true;

        public void Init() { }
        public void GrabImage(CogToolBlock tb) { }
        public void SetRuntimeConf(double exp, double gain) { }
        public void Close() { }
    }
}
