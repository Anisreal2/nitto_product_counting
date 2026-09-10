namespace BeevisionSolution.Models
{
    class LiveItem
    {
        public string ToolTip { get; set; }
        public string Tag { get; set; }
    }
}

#region New Trash
//void DoPlcCamJob(PlcCam obj, MelsecComunicator plc, byte[] Params)
//{
//    var jobId = BitConverter.ToUInt32(Params, 0);
//    AddClientEntry(String.Format("CommandId: {0}", jobId));
//    if (bLoaded)
//    {
//        switch (jobId)
//        {
//            case 1:
//                Info("Do XT command from plc commander");
//                DoPlcXT(obj, plc, Params);
//                break;

//            case 2:
//                Info("Do XT2 command from plc commander");
//                DoPlcXT2X(obj, plc, Params);
//                break;

//            case 3:
//                Info("Do XT Check command from plc commander");
//                DoPlcCheck(obj, plc, Params);
//                break;

//            case 4:
//                Info("Do XT Detect command from plc commander");
//                DoPlcDetect(obj, plc, Params);
//                break;

//            case 5:
//                Info("Do ISP command from plc commander");
//                DoPlcISP(obj, plc, Params);
//                break;

//            default:
//                break;
//        }
//    }
//    else
//    {
//        AddServerEntry("Command Result: -7");
//    }
//}

///*
// 1	Calib
// 2	Pick
// 3	Place
// 4	Check
// 5	Detect
// 6	Tool1 
// 7	Tool2
// 8	isp1
// 9	isp2
// */
//void DoPlcXT(PlcCam obj, MelsecComunicator plc, byte[] Params)
//{
//    const String cmd = "XT";
//    var calibId = BitConverter.ToUInt32(Params, 4);
//    var toolId = BitConverter.ToUInt32(Params, 8);
//    var modeId = BitConverter.ToUInt32(Params, 12);

//    AddClientEntry(String.Format("CalibId: {0}, ToolId: {1}, ModeId: {2}", calibId, toolId, modeId));

//    var alignMode = (AlignMode)modeId;
//    var calib = GetCalibrateById(calibId);
//    var align = (AlignJob)GetAbstractJob(toolId, calib.CameraId);
//    var pCam = (PlcCam)obj;
//    String strResult = "-5";

//    if (null != align)
//    {

//        var trained = GetTrainedPoint(align.Name, calibId);
//        var str = String.Format("{0},{1},{2},{3}", cmd, calibId, align.Name, alignMode);
//        AddClientEntry(str);

//        if (trained.Trained)
//        {
//            //var calib = GetCalibrateById(calibId);
//            var cam = GetCameraJob(calib.CameraId);

//            if ((null != align) && (null != cam) && cam.RunTool(true))
//            {
//                if (cam.RunStatus == CogToolResultConstants.Accept)
//                {
//                    align.InputImage = cam.OutputImage;
//                    align.Params = Params;
//                    align.CamSettings.CameraId = calib.CameraId;

//                    SaveImage(cam.Name, true, str, 0, (ICogImage)cam.OutputImage);

//                    align.RunTool(true);

//                    if ((align.RunStatus == CogToolResultConstants.Accept) && (null != align.Result))
//                    {
//                        var lst = align.Result as List<TPose>;
//                        if ((lst.Count > 0) && (lst[0].Item4))
//                        {
//                            var vPose = new Pose(lst[0].Item1, lst[0].Item2, lst[0].Item3);
//                            var pose = calib.DoMath(trained, vPose, alignMode);

//                            pCam.Result(0, 1);
//                            pCam.Result(1, (float)pose.X);
//                            pCam.Result(2, (float)pose.Y);
//                            pCam.Result(3, (float)Compensation(pose.Th));

//                            strResult = StdFormat(pose);
//                        }
//                        else { pCam.Result(0, 0); strResult = "0"; }
//                    }
//                    else { pCam.Result(0, -1); strResult = "-1"; }
//                }
//                else { pCam.Result(0, -2); strResult = "-2"; }
//            }
//            else { pCam.Result(0, -3); strResult = "-3"; }
//        }
//        else { pCam.Result(0, -4); strResult = "-4"; }
//    }
//    AddServerEntry(String.Format("{0},{1}", cmd, strResult));
//}

//void DoPlcXT2X(PlcCam obj, MelsecComunicator plc, byte[] Params)
//{
//    var calibId = BitConverter.ToUInt32(Params, 4);
//    var toolId = BitConverter.ToUInt32(Params, 8);
//    var modeId = BitConverter.ToUInt32(Params, 12);
//    var calibId1 = BitConverter.ToUInt32(Params, 16);
//    var toolId1 = BitConverter.ToUInt32(Params, 20);
//    var modeId1 = BitConverter.ToUInt32(Params, 24);

//    var lstTrained = new TrainedPoint[2];
//    var lstMode = new AlignMode[2];
//    //var lstCalib = new CalibrateResult[2];

//    AddClientEntry(String.Format("CalibId: {0}, ToolId: {1}, ModeId: {2}, CalibId1: {3}, ToolId1: {4}, ModeId1: {5}", calibId, toolId, modeId, calibId1, toolId1, modeId1));

//    var calib = GetCalibrateById(calibId);

//    Info("null == calib: {0}", null == calib);

//    var align = (AlignJob)GetAbstractJob(toolId, calib.CameraId);
//    var align1 = (AlignJob)GetAbstractJob(toolId1, calib.CameraId);

//    Info("null == align: {0}", null == align);
//    Info("null == align1: {0}", null == align1);

//    lstMode[0] = (AlignMode)modeId;
//    lstMode[1] = (AlignMode)modeId1;
//    if (null != align) lstTrained[0] = GetTrainedPoint(align.Name, calibId);
//    if (null != align1) lstTrained[1] = GetTrainedPoint(align1.Name, calibId);

//    Info("null == lstTrained[0]: {0}", null == lstTrained[0]);
//    Info("null == lstTrained[1]: {0}", null == lstTrained[1]);

//    var pCam = (PlcCam)obj;
//    const String cmd = "XT2X";
//    var str = String.Format("{0},{1},{2},{3},{4},{5},{6}", cmd, calibId, align?.Name, lstMode[0], calibId1, align1?.Name, lstMode[1]);
//    AddClientEntry(str);
//    //AddClientEntry(String.Format("P0: {0}, P1: {1}, P2: {2}, P3: {3}, P4: {4}, P5: {5}", calibId, toolId, modeId, calibId1, toolId1, modeId1));
//    String strResult = "-5";

//    if (((null != lstTrained[0]) && lstTrained[0].Trained) && ((null != lstTrained[1]) && lstTrained[1].Trained))
//    {
//        //var calib = GetCalibrateById(calibId);
//        var cam = GetCameraJob(calib.CameraId);

//        if ((null != align) && (null != cam) && cam.RunTool(true))
//        {
//            if (cam.RunStatus == CogToolResultConstants.Accept)
//            {
//                align.InputImage = cam.OutputImage;
//                align.Params = Params;
//                align.CamSettings.CameraId = calib.CameraId;

//                SaveImage(cam.Name, true, str, 0, (ICogImage)cam.OutputImage);
//                align.RunTool(true);

//                if ((align.RunStatus == CogToolResultConstants.Accept) && (null != align.Result))
//                {
//                    var lst = align.Result as List<TPose>;
//                    var count = 0;
//                    var countx = 0;

//                    if ((lst.Count > 0) && (lst[0].Item4))
//                    {
//                        strResult = "";
//                        foreach (var p in lst)
//                        {
//                            var vPose = new Pose(p.Item1, p.Item2, p.Item3);
//                            var pose = calib.DoMath(lstTrained[0], vPose, lstMode[0]);

//                            countx = count * 7;
//                            pCam.Result(0 + countx, 1);
//                            pCam.Result(1 + countx, (float)pose.X);
//                            pCam.Result(2 + countx, (float)pose.Y);
//                            pCam.Result(3 + countx, (float)Compensation(pose.Th));

//                            if (count > 0) strResult = String.Format("{0},1,{1}", strResult, StdFormat(pose));
//                            else strResult = String.Format("1,{0}", StdFormat(pose));

//                            pose = calib.DoMath(lstTrained[1], vPose, lstMode[1]);
//                            pCam.Result(4 + countx, (float)pose.X);
//                            pCam.Result(5 + countx, (float)pose.Y);
//                            pCam.Result(6 + countx, (float)Compensation(pose.Th));

//                            strResult = String.Format("{0},1,{1}", strResult, StdFormat(pose));
//                            ++count;
//                        }
//                    }
//                    else { pCam.Result(0, 0); strResult = "0"; }
//                }
//                else { pCam.Result(0, -1); strResult = "-1"; }
//            }
//            else { pCam.Result(0, -2); strResult = "-2"; }
//        }
//        else { pCam.Result(0, -3); strResult = "-3"; }
//    }
//    else { pCam.Result(0, -4); strResult = "-4"; }

//    AddServerEntry(String.Format("{0},{1}", cmd, strResult));
//}

//void DoPlcCheck(PlcCam obj, MelsecComunicator plc, byte[] Params)
//{
//    var calibId = BitConverter.ToUInt32(Params, 4);
//    var toolId = BitConverter.ToUInt32(Params, 8);
//    var modeId = BitConverter.ToUInt32(Params, 12);

//    var alignMode = (AlignMode)modeId;
//    var calib = GetCalibrateById(calibId);
//    var align = (AlignJob)GetAbstractJob(toolId, calib.CameraId);
//    var trained = GetTrainedPoint(align.Name, calibId);
//    var pCam = (PlcCam)obj;

//    const String cmd = "Check"; // Get Check Job
//    AddClientEntry(String.Format("XT,{0},{1},{2}", calibId, align.Name, alignMode));
//    String strResult = "-5";

//    if (trained.Trained)
//    {
//        //var calib = GetCalibrateById(calibId);
//        var cam = GetCameraJob(calib.CameraId);

//        if ((null != align) && (null != cam) && cam.RunTool(true))
//        {
//            if (cam.RunStatus == CogToolResultConstants.Accept)
//            {
//                align.InputImage = cam.OutputImage;
//                align.Params = Params;
//                align.CamSettings.CameraId = calib.CameraId;
//                align.RunTool(true);

//                if ((align.RunStatus == CogToolResultConstants.Accept) && (null != align.Result))
//                {
//                    var lst = align.Result as List<TPose>;
//                    if ((lst.Count > 0) && (lst[0].Item4))
//                    {
//                        var vPose = new Pose(lst[0].Item1, lst[0].Item2, lst[0].Item3);
//                        var pose = calib.DoMath(trained, vPose, alignMode);

//                        pCam.Result(0, 1);
//                        pCam.Result(1, (float)pose.X);
//                        pCam.Result(2, (float)pose.Y);
//                        pCam.Result(3, (float)Compensation(pose.Th));

//                        strResult = StdFormat(pose);
//                    }
//                    else { pCam.Result(0, 0); strResult = "0"; }
//                }
//                else { pCam.Result(0, -1); strResult = "-1"; }
//            }
//            else { pCam.Result(0, -2); strResult = "-2"; }
//        }
//        else { pCam.Result(0, -3); strResult = "-3"; }
//    }
//    else { pCam.Result(0, -4); strResult = "-4"; }

//    AddServerEntry(String.Format("{0},{1}", cmd, strResult));
//}

//void DoPlcDetect(PlcCam obj, MelsecComunicator plc, byte[] Params)
//{
//    var calibId = BitConverter.ToUInt32(Params, 4);
//    var toolId = BitConverter.ToUInt32(Params, 8);
//    var modeId = BitConverter.ToUInt32(Params, 12);

//    var alignMode = (AlignMode)modeId;
//    var calib = GetCalibrateById(calibId);
//    var align = (AlignJob)GetAbstractJob(toolId, calib.CameraId);
//    //var trained = GetTrainedPoint(align.Name, calibId);
//    var pCam = (PlcCam)obj;

//    const String cmd = "Detect"; // using detect job
//    AddClientEntry(String.Format("XT,{0},{1},{2}", calibId, align.Name, alignMode));
//    String strResult = "-5";

//    var cam = GetCameraJob(calib.CameraId);

//    if ((null != align) && (null != cam) && cam.RunTool(true))
//    {
//        if (cam.RunStatus == CogToolResultConstants.Accept)
//        {
//            align.InputImage = cam.OutputImage;
//            align.Params = Params;
//            align.CamSettings.CameraId = calib.CameraId;
//            align.RunTool(true);

//            if ((align.RunStatus == CogToolResultConstants.Accept) && (null != align.Result))
//            {
//                var Result = (bool?)align.Result ?? false;

//                if (Result)
//                {
//                    pCam.Result(0, 1);
//                    strResult = COMMAND_SUCCESSED_P1;
//                }
//                else
//                {
//                    pCam.Result(0, 2);
//                    strResult = COMMAND_SUCCESSED_P2;
//                }
//            }
//            else { pCam.Result(0, -1); strResult = "-1"; }
//        }
//        else { pCam.Result(0, -2); strResult = "-2"; }
//    }
//    else { pCam.Result(0, -3); strResult = "-3"; }

//    AddServerEntry(String.Format("{0},{1}", cmd, strResult));
//}

//void DoPlcISP(PlcCam obj, MelsecComunicator plc, byte[] Params)
//{
//    var camId = BitConverter.ToUInt32(Params, 4);
//    var toolId = BitConverter.ToUInt32(Params, 8);
//    var job = GetIspJob(toolId, camId);
//    var pCam = (PlcCam)obj;

//    const String cmd = "ISP";
//    AddClientEntry(String.Format("{0},{1}", cmd, job.Name));
//    String strResult = "-5";

//    if (null != job)
//    {
//        var cam = GetCameraJob(job.CamSettings.CameraId);
//        if ((null != cam) && cam.RunTool(true))
//        {
//            if (cam.RunStatus == CogToolResultConstants.Accept)
//            {
//                job.InputImage = cam.OutputImage;
//                job.RunTool(true);
//                if ((job.RunStatus == CogToolResultConstants.Accept) && (null != job.Results))
//                {
//                    var lst = (List<UInt32>)job.Results;
//                    for (int i = 0; i < lst.Count; i++)
//                        pCam.Result(i, lst[i]);
//                }
//                else pCam.Result(0, 0);
//            }
//            else pCam.Result(0, -1);
//        }
//        else pCam.Result(0, -2);
//    }
//    else pCam.Result(0, -3);

//    AddServerEntry(String.Format("{0},{1}", cmd, strResult));
//}
#endregion

#region Trash
//ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
//ThemeManager.Current.SyncTheme();

//void SetImage(ICogImage cogImage, System.Drawing.Size gridSize)
//{
//    ICogImage bgImage;
//    using (var bm = new Bitmap("D:\\code.bmp")) { bgImage = new CogImage8Grey(bm); }

//    for (int i = 0; i < gridSize.Width; i++)
//    {
//        for (int j = 0; j < gridSize.Height; j++)
//        {
//            //dspGrid.SetImage(bgImage, i, j, false);
//        }
//    }
//    MemoryCleanup();
//}

//Bug("Result: {0}", Result);
//if (null != Result)
//{
//    var l = (List<TPose>)Result;
//    var p = l[0];
//    Bug("{0},{1},{2}", p.Item1, p.Item2, p.Item3);
//}
//Bug("Mainwindow callback OnAlignJobRan, CamId: {0}", dspId);

//foreach (var job in lst)
//{
//    if (job is CameraJob) job.OnToolBlockRan += OnCamJobRan;
//    else if (job is AlignJob) job.OnToolBlockRan += OnAlignJobRan;
//    else job.OnToolBlockRan += OnIspJobRan;
//}

//var lst = GetAllJobs();
//var j1 = lst.FirstOrDefault(j => ((j.VisionType == obj.JobType) && (j.Name.Equals(obj.JobName))));

//if (j1 is AbstractJob)
//{
//    var job = j1 as IspJob;

//    //LogPanel.AddClientEntry(String.Format("ISP,{0}", Job.Name));
//    if (null != job)
//    {
//        var cam = GetCameraJob(job.CamSettings.CameraId);
//        if ((null != cam) && cam.RunTool(true))
//        {
//            job.InputImage = cam.OutputImage;
//            job.RunTool(true);

//            if ((job.RunStatus == CogToolResultConstants.Accept) && (null != job.Result))
//            {
//                if (null != job.Results)
//                {
//                    var l1 = (List<ushort>)job.Results;
//                    for (int i = 0; i < l1.Count; i++)
//                    {
//                        var v = l1[i];
//                        //plc.DataWrite[30 + (i * 2)] = v;
//                    }
//                }
//            }
//        }
//    }
//    //LogPanel.AddServerEntry(String.Format("ISP,{0}", iRet));
//}

//Info("Begin Init Plc");
//if (Settings.AllowMelsecScan)
//{
//    foreach (var p in lstPlc)
//    {
//        if (p.IsActive)
//        {
//            p.Sync();
//            Info("Plc: {0} is actived and started", p.Name);
//        }
//    }

//    foreach (var cam in lstCam)
//    {
//        if (cam.IsActive)
//        {
//            cam.TriggerSignal += DoPlcCamJob;
//            var plc = lstPlc.FirstOrDefault(p => p.Name.Equals(cam.PlcName));
//            new Thread((p) => { cam.Run(p); }) { IsBackground = true }
//                .Start(plc);
//            Info("Plc Job {0} is started", cam.JobName);
//        }
//    }
//}
//Info("Done Plc Init");

//private void btnTakeImage_Click(object sender, RoutedEventArgs e)
//{
//    var cams = GetJobs<CameraJob>();
//    if ((null != cams) && (cams.Count > 0))
//    {
//        var job = cams[0];
//        Bug("Begin take image");
//        job.AllowFireEvent = false;
//        job.RunTool(true);
//        Bug("CameraJob is ran");
//        dspGrid.ForceSetImage((ICogImage)job.OutputImage, 3);
//    }
//}

//private async Task PlcInit()
//{
//    Info("Begin Init Plc");
//    if (Settings.AllowMelsecScan)
//    {
//        foreach (var p in lstPlc)
//        {
//            if (p.IsActive)
//            {
//                p.Sync();
//                Info("Plc: {0} is actived and started", p.Name);
//            }
//        }

//        foreach (var cam in lstCam)
//        {
//            if (cam.IsActive)
//            {
//                cam.TriggerSignal += DoPlcCamJob;
//                var plc = lstPlc.FirstOrDefault(p => p.Name.Equals(cam.PlcName));
//                new Thread((p) => { cam.Run(p); }) { IsBackground = true }
//                    .Start(plc);
//                Info("Plc Job {0} is started", cam.JobName);
//            }
//        }
//    }
//    Info("Done Plc Init");
//}

//private void doISP(string strIspJob)
//{
//    var isp = GetIspJob(strIspJob);
//    var iRet = 0;

//    //LogPanel.AddClientEntry(String.Format("ISP,{0}", strIspJob));
//    if (null != isp)
//    {
//        var cam = GetCameraJob(isp.CamSettings.CameraId);
//        if ((null != cam) && cam.RunTool(true))
//        {
//            isp.InputImage = cam.OutputImage;
//            isp.RunTool(true);

//            if ((null != isp) && (null != isp.Result))
//            {
//                var result = isp.Result as bool? ?? false;
//                if (result) iRet = 1;
//            }
//        }
//    }
//    else iRet = -1;
//    //LogPanel.AddServerEntry(String.Format("ISP,{0}", iRet));
//}

//void AutoCalibTest()
//{
//    bool reverse = false;
//    int width = 5;
//    int w1 = width - 1;
//    var src = new List<CppPoint>();
//    var dst = new List<CppPoint>();

//    var SP0 = new BasicMath.Point(0, 0);
//    var SP1 = new BasicMath.Point(0, w1);
//    var SP2 = new BasicMath.Point(w1, w1);

//    var DP0 = new BasicMath.Point(0, 0);
//    var DP2 = new BasicMath.Point(0, 200);
//    var DP1 = new BasicMath.Point(200, 200);

//    {
//        var minX = Min(SP0.X, SP1.X, SP2.X);
//        var minY = Min(SP0.Y, SP1.Y, SP2.Y);
//        var maxX = Max(SP0.X, SP1.X, SP2.X);
//        var maxY = Max(SP0.Y, SP1.Y, SP2.Y);

//        src.Add(new CppPoint(minX, minY));
//        src.Add(new CppPoint(minX, maxY));
//        src.Add(new CppPoint(maxX, maxY));
//        src.Add(new CppPoint(maxX, minY));
//    }

//    {
//        var minX = Min(DP0.X, DP1.X, DP2.X);
//        var minY = Min(DP0.Y, DP1.Y, DP2.Y);
//        var maxX = Max(DP0.X, DP1.X, DP2.X);
//        var maxY = Max(DP0.Y, DP1.Y, DP2.Y);

//        dst.Add(new CppPoint(minX, minY));
//        dst.Add(new CppPoint(minX, maxY));
//        dst.Add(new CppPoint(maxX, maxY));
//        dst.Add(new CppPoint(maxX, minY));
//    }

//    var dwSize = src.Count;
//    var ptr = Perspective2D(src.ToArray(), dst.ToArray(), out dwSize);

//    if ((IntPtr.Zero != ptr) && (dwSize > 0))
//    {
//        var l = new LinearTransform(ptr);
//        Marshal.FreeHGlobal(ptr);

//        for (int i = 0; i < width * width; i++)
//        {
//            int dwRow = i / width;
//            int dwCol = (i % width);

//            if ((i > 0) && ((i % width) == 0))
//                reverse = !reverse;

//            if (reverse) dwCol = (width - dwCol - 1);
//            //Bug("Row: {0}, Col: {1}", dwRow, dwCol);
//            var p0 = new BasicMath.Point(dwRow, dwCol);
//            var p = l.MapPoint(p0);
//            Bug("P0: {0}, P1: {1}", p0, p);
//        }
//    }
//}

//private BitmapSource CaptureScreen(Visual target, double dpiX, double dpiY)
//{
//    if (target == null)
//    {
//        return null;
//    }
//    Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
//    RenderTargetBitmap rtb = new RenderTargetBitmap((int)(bounds.Width * dpiX / 96.0),
//                                                    (int)(bounds.Height * dpiY / 96.0),
//                                                    dpiX,
//                                                    dpiY,
//                                                    PixelFormats.Pbgra32);
//    DrawingVisual dv = new DrawingVisual();
//    using (DrawingContext ctx = dv.RenderOpen())
//    {
//        VisualBrush vb = new VisualBrush(target);
//        ctx.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
//    }
//    rtb.Render(dv);
//    return rtb;
//}

//var arr = msg.Split(',');
//if (arr.Length > 1)
//{
//    int dwWidth = 0;
//    int dwHeight = 0;
//    int.TryParse(arr[1], out dwWidth);
//    //int.TryParse(arr[2], out dwHeight);
//    //dspGrid.GridSize = new System.Drawing.Size(dwWidth, dwHeight);
//    //SetImage(bgImage, dspGrid.GridSize);
//    lstCamJobs[dwWidth % 3].RunTool();
//}

//private void PlcInit()
//{
//    if (Settings.AllowMelsecScan)
//    {
//        plc = new MelsecComunicator(Settings.MelsecIP, Settings.MelsecPort);
//        plc.Sync();

//        thCam1 = new Thread(() =>
//        {
//            plc.VisionData[cam1CtlAddress] = 0x01;
//            while (plcCommander)
//            {
//                if (((plc.PlcData[cam1CtlAddress] & 0x04) != 0) && ((plc.PlcData[cam1CtlAddress] & 0x08) == 0))// Plc trigger
//                {
//                    plc.VisionData[cam1CtlAddress] |= 0x04; // trigger comfirm
//                    doISP("Isp0");
//                    plc.VisionData[cam1CtlAddress] |= 0x08;
//                }

//                if ((plc.PlcData[cam1CtlAddress] & 0x08) != 0) // result confirm
//                {
//                    plc.VisionData[cam1CtlAddress] &= 0xfff3;
//                }

//                Thread.Sleep(1);
//            }
//        })
//        { IsBackground = true };

//        thCam2 = new Thread(() =>
//        {
//            plc.VisionData[cam2CtlAddress] = 0x01;
//            while (plcCommander)
//            {
//                LogPanel.PlcActive = (plc.PlcData[cam2CtlAddress] & 0x02);

//                if (((plc.PlcData[cam2CtlAddress] & 0x04) != 0) && ((plc.PlcData[cam2CtlAddress] & 0x08) == 0))// Plc trigger
//                {
//                    plc.VisionData[cam2CtlAddress] |= 0x04; // trigger comfirm
//                    doISP("Isp1");
//                    plc.VisionData[cam2CtlAddress] |= 0x08;
//                }

//                if ((plc.PlcData[cam2CtlAddress] & 0x08) != 0) // result confirm
//                {
//                    plc.VisionData[cam2CtlAddress] &= 0xfff3;
//                }

//                Thread.Sleep(1);
//            }
//        })
//        { IsBackground = true };

//        thCam3 = new Thread(() =>
//        {
//            plc.VisionData[cam3CtlAddress] = 0x01;
//            while (plcCommander)
//            {
//                if (((plc.PlcData[cam3CtlAddress] & 0x04) != 0) && ((plc.PlcData[cam3CtlAddress] & 0x08) == 0))// Plc trigger
//                {
//                    plc.VisionData[cam3CtlAddress] |= 0x04; // trigger comfirm
//                    doISP("Isp2");
//                    plc.VisionData[cam3CtlAddress] |= 0x08;
//                }

//                if ((plc.PlcData[cam3CtlAddress] & 0x08) != 0) // result confirm
//                {
//                    plc.VisionData[cam3CtlAddress] &= 0xfff3;
//                }

//                Thread.Sleep(1);
//            }
//        })
//        { IsBackground = true };

//        thCam4 = new Thread(() =>
//        {
//            plc.VisionData[cam4CtlAddress] = 0x01;
//            while (plcCommander)
//            {
//                if (((plc.PlcData[cam4CtlAddress] & 0x04) != 0) && ((plc.PlcData[cam4CtlAddress] & 0x08) == 0))// Plc trigger
//                {
//                    plc.VisionData[cam4CtlAddress] |= 0x04; // trigger comfirm
//                    doISP("Isp3");
//                    plc.VisionData[cam4CtlAddress] |= 0x08;
//                }

//                if ((plc.PlcData[cam4CtlAddress] & 0x08) != 0) // result confirm
//                {
//                    plc.VisionData[cam4CtlAddress] &= 0xfff3;
//                }

//                Thread.Sleep(1);
//            }
//        })
//        { IsBackground = true };
//    }
//    else
//    {
//        plc = new MelsecComunicator(Settings.MelsecIP, Settings.MelsecPort);
//        plc.Sync();

//        thCam1 = new Thread(() =>
//        {
//            var timeOut = 5000;
//            plc.VisionData[cam1CtlAddress] = 0x01;
//            while (plcCommander)
//            {
//                LogPanel.PlcActive = (plc.PlcData[0] & 0x02);

//                // No trigger and cleanup (free time) -> Waiting for trigger
//                if ((plc.PlcData[cam1CtlAddress] & ((ushort)(PlcBitDecription.BIT_AUX_BITS))) == 0)
//                {
//                    LogPanel.TriggerActive = 0; // Waiting for trigger
//                }

//                //Trigger and no cleanup -> trigger signal
//                if (((plc.PlcData[cam1CtlAddress] & ((ushort)PlcBitDecription.BIT_ISP_TRIGGER)) != 0) && ((plc.PlcData[cam1CtlAddress] & ((ushort)PlcBitDecription.BIT_RESULT_CONFIRM)) == 0))// Plc trigger
//                {
//                    LogPanel.TriggerActive = 1; // Trigger confirm and do vision work
//                    doISP("Isp0");
//                    timeOut = 0;
//                    while (((plc.PlcData[cam1CtlAddress] & ((ushort)PlcBitDecription.BIT_ISP_TRIGGER)) != 0) && (++timeOut< PlcTimeout))
//                        Thread.Sleep(1);
//                    LogPanel.TriggerActive = 2; // Have result
//                }

//                // No trigger but have trigger confirm -> cleanup
//                if (((plc.PlcData[cam1CtlAddress] & ((ushort)PlcBitDecription.BIT_ISP_TRIGGER)) == 0) && (plc.PlcData[cam1CtlAddress] & ((ushort)PlcBitDecription.BIT_RESULT_CONFIRM)) != 0) // result confirm
//                {
//                    LogPanel.TriggerActive = 3; // Waiting for received confirm from PLC for do cleanup
//                    timeOut = 0;
//                    while (((plc.PlcData[cam1CtlAddress] & ((ushort)PlcBitDecription.BIT_RESULT_CONFIRM)) != 0) && (++timeOut < PlcTimeout))
//                        Thread.Sleep(1);
//                }

//                Thread.Sleep(1);
//            }
//        })
//        { IsBackground = true };

//        thCam1.Start();
//    }
//}
#endregion