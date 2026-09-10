using Cognex.VisionPro.ToolBlock;

namespace BeevisionSolution.Jobs.CameraHandlers
{
    public interface ICameraHandler
    {
        bool IsInitialized { get; }
        void Init();
        void GrabImage(CogToolBlock tb);
        void SetRuntimeConf(double exp, double gain);
        void Close();
    }
}
