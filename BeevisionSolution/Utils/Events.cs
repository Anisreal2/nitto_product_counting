using BeevisionSolution.Models;
using Cognex.VisionPro;
using System.Collections.Generic;
using System.Windows.Documents;

namespace BeevisionSolution.Utils
{
    public delegate void OnOperationModeChanged(OperationMode Mode);
    public delegate void OnCamTakePicture(object img, int camId);
    public delegate void OnVisionReturnData(object Sender, object Result, object Record, int CameraId);
    public delegate void OnVisionReturnDataX(object Sender, object Result, int CameraId);
    public delegate void OnMessageReceived(string msg);
    public delegate void OnMessageResponse(string msg);
    public delegate void OnInitJobDone();
    public delegate void OnEIPResponse(string msg);
    public delegate void OnPlcFired();
    public delegate void OnPlcHeartbeatUpdate(short vl);
    public delegate void OnQueueChanged(int count);

    public delegate void OnHandEyeBegin(string[] Params);
    public delegate void OnHandEye(string[] Params);
    public delegate void OnHandEyeEnd(string[] Params);

    public delegate void OnSettingLoadedDone(object Sender);
    public delegate void OnJobLoadedDone(object sender);
    public delegate void OnJobProcessing(FunctionJob job, string message);
    public delegate void OnJobCompleted(List<object> results, int displayID, ICogImage rawImage, string strCamName, string fileName, bool isOK, bool isSaveOnly = false, double actualDist = double.NaN);
    public delegate void OnImageHandle(List<object> results, int displayID, ICogImage rawImage, string strCamName, double actualDist = double.NaN);
    public delegate void OnProductRetrieveHandle(List<object> results, int displayId, string strCamName, bool isOKn, double actualDist = double.NaN);
}
