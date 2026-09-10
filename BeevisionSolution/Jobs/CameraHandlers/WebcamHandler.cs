//using AForge.Video;
//using AForge.Video.DirectShow;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Drawing;
using static BeevisionSolution.Utils.Common;

namespace BeevisionSolution.Jobs.CameraHandlers
{
    //public class WebcamHandler : ICameraHandler
    //{
    //    private readonly Models.CameraJob _job;
    //    private VideoCaptureDevice wcCamera;
    //    private volatile bool _isGrabImage;

    //    public bool IsInitialized { get; private set; }

    //    private CogToolBlock _pendingTb;

    //    public WebcamHandler(Models.CameraJob job)
    //    {
    //        _job = job;
    //    }

    //    public void Init()
    //    {
    //        try
    //        {
    //            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
    //            if (videoDevices.Count > 0)
    //            {
    //                wcCamera = new VideoCaptureDevice(videoDevices[0].MonikerString);
    //                wcCamera.NewFrame += OnNewFrame;
    //                IsInitialized = true;
    //                Info("Webcam camera is initialized");
    //            }
    //            else
    //            {
    //                Info("Webcam camera is NOT initialized");
    //            }
    //        }
    //        catch (Exception exception)
    //        {
    //            Bug("Webcam camera error: {0}", exception.Message);
    //        }
    //    }

    //    public void GrabImage(CogToolBlock tb)
    //    {
    //        if (tb.Inputs.Contains("InputImage"))
    //            tb.Inputs["InputImage"].Value = null;

    //        _pendingTb = tb;
    //        _isGrabImage = true;
    //        wcCamera.Start();

    //        int timeOut = 100;
    //        while (_isGrabImage && timeOut > 0)
    //        {
    //            System.Threading.Thread.Sleep(50);
    //            timeOut--;
    //        }
    //    }

    //    public void SetRuntimeConf(double exp, double gain) { }

    //    public void Close()
    //    {
    //        if (wcCamera != null && wcCamera.IsRunning)
    //        {
    //            wcCamera.SignalToStop();
    //        }
    //    }

    //    private void OnNewFrame(object sender, NewFrameEventArgs e)
    //    {
    //        Bitmap bm = e.Frame;
    //        ICogImage img = new CogImage24PlanarColor(bm);
    //        Info("Convert image to iCogImage successfully, check image in InputImage of toolblock");
    //        if (_pendingTb != null && _pendingTb.Inputs.Contains("InputImage"))
    //            _pendingTb.Inputs["InputImage"].Value = img;
    //        _isGrabImage = false;
    //        wcCamera.SignalToStop();
    //        bm.Dispose();
    //        GC.Collect();
    //    }
    //}
}
