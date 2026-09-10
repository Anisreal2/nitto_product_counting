using CaptureCard_Net;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using static BeevisionSolution.Utils.Common;

namespace BeevisionSolution.Jobs.CameraHandlers
{
    public class IRaypleLinescanHandler : ICameraHandler
    {
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", CharSet = CharSet.Ansi)]
        internal static extern void CopyMemory(IntPtr pDst, IntPtr pSrc, int len);

        private readonly Models.CameraJob _job;
        private CardDev card = new CardDev();
        private CamDev cam = new CamDev();
        IMVFGDefine.IMV_FG_INTERFACE_INFO_LIST interfaceList = new IMVFGDefine.IMV_FG_INTERFACE_INFO_LIST();
        IMVFGDefine.IMV_FG_EInterfaceType interfaceTp = IMVFGDefine.IMV_FG_EInterfaceType.typeInterfaceAll;
        IMVFGDefine.IMV_FG_DEVICE_INFO_LIST camListPtr = new IMVFGDefine.IMV_FG_DEVICE_INFO_LIST();
        /// <summary>True khi card này đến từ EarlyProbe (pre-opened). Close() sẽ không close interface để tránh hỏng Reload.</summary>
        private bool _usedEarlyCard;

        public bool IsInitialized { get; private set; }

        public IRaypleLinescanHandler(Models.CameraJob job)
        {
            _job = job;
        }

        public void Init()
        {
            var thread = Thread.CurrentThread;
            Info("[IRayple] Init started - Job:{0}, CamType:{1}, BoardNo:{2}, CamNo:{3}, ThreadId:{4}, Apartment:{5}",
                _job.Name, _job.CamType, _job.BoardNo, _job.CamLSNo, thread.ManagedThreadId, thread.GetApartmentState());

            LogEnvDiagnostics();

            int res = IMVFGDefine.IMV_FG_OK;
            bool usedEarlyCard = false;

            // Ưu tiên dùng CardDev đã được mở từ EarlyProbe trong App.Main (trước khi Pylon/Cognex load).
            // Pylon/Cognex sau khi load làm reset state của IRayple SDK -> EnumInterface trả 0.
            // Giữ handle mở sớm tránh được vấn đề này.
            if (IRaypleEarlyState.Ready && IRaypleEarlyState.Cards.TryGetValue(_job.BoardNo, out var earlyCard) && earlyCard != null)
            {
                card = earlyCard;
                usedEarlyCard = true;
                _usedEarlyCard = true;
                interfaceList = IRaypleEarlyState.InterfaceList;
                Info("[IRayple] Using pre-opened CardDev from EarlyProbe for BoardNo:{0} (interfaces={1})",
                    _job.BoardNo, IRaypleEarlyState.InterfaceCount);
            }
            else
            {
                var swEnum = System.Diagnostics.Stopwatch.StartNew();
                res = CardDev.IMV_FG_EnumInterface((uint)interfaceTp, ref interfaceList);
                swEnum.Stop();
                Info("[IRayple] EnumInterface(typeInterfaceAll) res={0}, nInterfaceNum={1}, elapsed={2}ms",
                    res, interfaceList.nInterfaceNum, swEnum.ElapsedMilliseconds);
                if (res != IMVFGDefine.IMV_FG_OK)
                {
                    Bug("[IRayple] Enum board interface FAILED! errorCode:{0}", res);
                    return;
                }
                if (interfaceList.nInterfaceNum == 0)
                {
                    TryRetryEnumByType(IMVFGDefine.IMV_FG_EInterfaceType.typeCLInterface, "typeCLInterface");
                    TryRetryEnumByType(IMVFGDefine.IMV_FG_EInterfaceType.typeCXPInterface, "typeCXPInterface");

                    if (interfaceList.nInterfaceNum == 0)
                    {
                        Bug("[IRayple] No board device found.");
                        return;
                    }
                }
                Info("[IRayple] Found {0} board interface(s)", interfaceList.nInterfaceNum);

                res = card.IMV_FG_OpenInterface(_job.BoardNo);
                if (res != IMVFGDefine.IMV_FG_OK)
                {
                    Bug("[IRayple] Open board FAILED! errorCode:{0}, BoardNo:{1}", res, _job.BoardNo);
                    _job.Available = false;
                    return;
                }
                Info("[IRayple] Open board OK - BoardNo:{0}", _job.BoardNo);
            }

            // Enum cameras (ưu tiên dùng cache từ EarlyProbe nếu có)
            if (IRaypleEarlyState.Ready && IRaypleEarlyState.DeviceCount > 0)
            {
                camListPtr = IRaypleEarlyState.DeviceList;
                Info("[IRayple] Using cached camera list from EarlyProbe (nDevNum={0})", camListPtr.nDevNum);
            }
            else
            {
                var swCam = System.Diagnostics.Stopwatch.StartNew();
                res = CamDev.IMV_FG_EnumDevices((uint)interfaceTp, ref camListPtr);
                swCam.Stop();
                Info("[IRayple] EnumDevices res={0}, nDevNum={1}, elapsed={2}ms",
                    res, camListPtr.nDevNum, swCam.ElapsedMilliseconds);
                if (res != IMVFGDefine.IMV_FG_OK)
                {
                    Bug("[IRayple] Enum camera devices FAILED! errorCode:{0}", res);
                    return;
                }
                if (camListPtr.nDevNum == 0)
                {
                    Bug("[IRayple] No camera device found.");
                    return;
                }
                Info("[IRayple] Found {0} camera device(s)", camListPtr.nDevNum);
            }

            // Đến đây interface đã mở (hoặc pre-opened), tiếp tục mở camera + start grabbing
            {
                if (usedEarlyCard)
                    Info("[IRayple] Skipped EnumInterface/OpenInterface - using EarlyProbe state");
                res = cam.IMV_FG_OpenDevice(IMVFGDefine.IMV_FG_ECreateHandleMode.IMV_FG_MODE_BY_INDEX, _job.CamLSNo);
                if (res != IMVFGDefine.IMV_FG_OK)
                {
                    Bug("[IRayple] Open camera FAILED! errorCode:{0}, CamNo:{1}", res, _job.CamLSNo);
                    _job.Available = false;
                }
                else
                {
                    Info("[IRayple] Open camera OK - CamNo:{0}", _job.CamLSNo);

                    // 1) Load riêng card + camera .mvcfg (không gộp một file)
                    bool cardCfgLoaded = false;
                    bool camCfgLoaded = false;
                    TryLoadMvcfgPair(out cardCfgLoaded, out camCfgLoaded);
                    if (cardCfgLoaded || camCfgLoaded)
                        MergeDimensionsAfterMvcfgLoad();

                    // 2) Đồng bộ Width/Height: camera -> job -> capture card (bắt buộc trước StartGrabbing)
                    if (!SyncDimensionsToCard(logReadback: true))
                        Bug("[IRayple] SyncDimensionsToCard had errors - check Width/Height");

                    // 3) Manual trigger trên camera chỉ khi chưa load cam .mvcfg
                    if (!camCfgLoaded)
                    {
                        if (_job.CamType == 1)
                        {
                            Info("[IRayple] Setting SoftTrigger config...");
                            SetSoftTriggerConf(applyDimensions: false);
                        }
                        else if (_job.CamType == 2)
                        {
                            Info("[IRayple] Setting LineTrigger config...");
                            SetLineTriggerConf(applyDimensions: false);
                        }
                        SyncDimensionsToCard(logReadback: true);
                    }

                    if (!ValidateCardDimensionsBeforeStart())
                    {
                        Bug("[IRayple] Card Width/Height invalid - abort StartGrabbing");
                        return;
                    }

                    res = IMVFGDefine.IMV_FG_OK;
                    res = card.IMV_FG_StartGrabbing();
                    if (res != IMVFGDefine.IMV_FG_OK)
                    {
                        Bug("[IRayple] StartGrabbing FAILED! errorCode:{0}", res);
                        return;
                    }
                    _job.Available = true;
                    IsInitialized = true;
                    Info("[IRayple] Init COMPLETED - Ready to grab (Width:{0}, Height:{1}, Timeout:{2}ms)", _job.WidthLS, _job.HeightLS, _job.CamLSTimeOut);
                }
            }

            if (!IsInitialized)
                Bug("[IRayple] Init FAILED - Camera is NOT ready");
        }

        private void LogEnvDiagnostics()
        {
            try
            {
                Info("[IRayple] CWD={0}", Environment.CurrentDirectory);
                Info("[IRayple] BaseDir={0}", AppDomain.CurrentDomain.BaseDirectory);

                string sdkVer = "<unknown>";
                try { sdkVer = CardDev.IMV_FG_GetVersion(); }
                catch (Exception ex) { sdkVer = "EX:" + ex.Message; }
                Info("[IRayple] SDK_Version={0}", sdkVer);

                // Liệt kê các native DLL trong BaseDir để biết app có copy đè lên SDK chuẩn không.
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] probes = { "MVFGAPI.dll", "MVFG_GenTL.cti", "MVSDKmd.dll", "MvCameraControl.dll", "ThridLibray.dll", "CLIDelegate.dll", "MVFGSDK_Net.dll" };
                foreach (var p in probes)
                {
                    var full = System.IO.Path.Combine(baseDir, p);
                    Info("[IRayple] NativeProbe {0} exists={1}", p, System.IO.File.Exists(full));
                }
            }
            catch (Exception ex)
            {
                Bug("[IRayple] LogEnvDiagnostics EX: {0}", ex.Message);
            }
        }

        private void TryRetryEnumByType(IMVFGDefine.IMV_FG_EInterfaceType type, string label)
        {
            try
            {
                var tmp = new IMVFGDefine.IMV_FG_INTERFACE_INFO_LIST();
                var sw = System.Diagnostics.Stopwatch.StartNew();
                int r = CardDev.IMV_FG_EnumInterface((uint)type, ref tmp);
                sw.Stop();
                Info("[IRayple] RetryEnumInterface({0}) res={1}, nInterfaceNum={2}, elapsed={3}ms",
                    label, r, tmp.nInterfaceNum, sw.ElapsedMilliseconds);
                if (r == IMVFGDefine.IMV_FG_OK && tmp.nInterfaceNum > 0)
                {
                    interfaceList = tmp;
                    interfaceTp = type;
                    Info("[IRayple] Adopting interface type {0} for subsequent calls.", label);
                }
            }
            catch (Exception ex)
            {
                Bug("[IRayple] RetryEnumInterface({0}) EX: {1}", label, ex.Message);
            }
        }

        public void GrabImage(CogToolBlock tb)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Info("[IRayple] GrabImage started - Job:{0}", _job.Name);

            if (false == cam.IMV_FG_IsDeviceOpen())
            {
                Bug("[IRayple] GrabImage ABORTED - device is not open");
                return;
            }
            if (tb.Inputs.Contains("InputImage"))
                tb.Inputs["InputImage"].Value = null;

            int res = IMVFGDefine.IMV_FG_OK;
            if (_job.CamType == 1)
            {
                Info("[IRayple] Sending SoftwareTrigger...");
                res = card.IMV_FG_ExecuteCommandFeature("TriggerSoftware");
                if (IMVFGDefine.IMV_FG_OK != res)
                {
                    Bug("[IRayple] SoftwareTrigger FAILED! errorCode:{0}", res);
                    return;
                }
                Thread.Sleep((int)_job.CamLSTimeOut);
                Info("[IRayple] SoftwareTrigger done, waited {0}ms", _job.CamLSTimeOut);
            }

            IMVFGDefine.IMV_FG_Frame frame = new IMVFGDefine.IMV_FG_Frame();
            Info("[IRayple] GetFrame waiting (timeout:{0}ms)...", _job.CamLSTimeOut);
            res = card.IMV_FG_GetFrame(ref frame, _job.CamLSTimeOut);
            if (res != IMVFGDefine.IMV_FG_OK)
            {
                Bug("[IRayple] GetFrame FAILED! errorCode:{0}", res);
                res = card.IMV_FG_StopGrabbing();
                if (res != IMVFGDefine.IMV_FG_OK)
                {
                    Bug("[IRayple] StopGrabbing FAILED! errorCode:{0}", res);
                    return;
                }
                GC.Collect();
                return;
            }
            Info("[IRayple] GetFrame OK - {0}x{1}", frame.frameInfo.width, frame.frameInfo.height);

            Bitmap bitmap = null;
            ConvertToBitmap(ref frame, ref bitmap);
            if (bitmap != null)
            {
                ICogImage img = new CogImage8Grey(bitmap);
                if (tb.Inputs.Contains("InputImage"))
                    tb.Inputs["InputImage"].Value = img;
                Info("[IRayple] Image set to InputImage OK");
            }
            else
            {
                Bug("[IRayple] ConvertToBitmap FAILED - bitmap is null");
            }

            res = card.IMV_FG_ReleaseFrame(ref frame);
            if (res != IMVFGDefine.IMV_FG_OK)
            {
                Bug("[IRayple] ReleaseFrame FAILED! errorCode:{0}", res);
                GC.Collect();
                return;
            }

            sw.Stop();
            Info("[IRayple] GrabImage COMPLETED - {0}ms", sw.ElapsedMilliseconds);
            GC.Collect();
        }

        public void SetRuntimeConf(double exp, double gain)
        {
            if (!IsInitialized) return;

            int res = IMVFGDefine.IMV_FG_OK;
            res = cam.IMV_FG_SetDoubleFeatureValue("ExposureTime", exp);
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("set exposuretime fail!");
            }

            res = cam.IMV_FG_SetDoubleFeatureValue("GainRaw", gain);
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("set gain fail!");
            }
        }

        public void Close()
        {
            Info("[IRayple] Closing camera... (usedEarlyCard={0})", _usedEarlyCard);
            var res = card.IMV_FG_StopGrabbing();
            if (res != IMVFGDefine.IMV_FG_OK)
            {
                Bug("[IRayple] StopGrabbing FAILED! errorCode:{0}", res);
            }
            else
            {
                res = cam.IMV_FG_CloseDevice();
                if (IMVFGDefine.IMV_FG_OK != res)
                {
                    Bug("[IRayple] CloseDevice FAILED! errorCode:{0}", res);
                }
                else if (!_usedEarlyCard)
                {
                    // Chỉ close interface khi card do handler tự tạo. Nếu dùng card pre-opened
                    // từ EarlyProbe thì giữ alive để Reload sau còn dùng tiếp.
                    res = card.IMV_FG_CloseInterface();
                    if (IMVFGDefine.IMV_FG_OK != res)
                    {
                        Bug("[IRayple] CloseInterface FAILED! errorCode:{0}", res);
                    }
                }
                else
                {
                    Info("[IRayple] Skip CloseInterface - early-opened card kept alive for Reload");
                }
            }
            IsInitialized = false;
            Info("[IRayple] Close done");
        }

        #region private config methods

        /// <summary>Load card + camera mvcfg riêng: CardIrayCfgPath / CamIrayCfgPath hoặc cardkkk / camkkk.</summary>
        private void TryLoadMvcfgPair(out bool cardLoaded, out bool camLoaded)
        {
            cardLoaded = false;
            camLoaded = false;

            string cardPath = ResolveMvcfgPath(_job.CardIrayCfgPath, "card1.mvcfg", "card");
            string camPath = ResolveMvcfgPath(_job.CamIrayCfgPath, "cam1.mvcfg", "cam");

            if (cardPath != null)
                cardLoaded = LoadDeviceCfg(cardPath, loadOnCard: true);
            else
                Info("[IRayple] No capture-card mvcfg found (set CardIrayCfgPath or card1.mvcfg)");

            if (camPath != null)
                camLoaded = LoadDeviceCfg(camPath, loadOnCard: false);
            else
                Info("[IRayple] No camera mvcfg found (set CamIrayCfgPath or cam1.mvcfg)");

            Info("[IRayple] mvcfg summary: cardLoaded={0}, camLoaded={1}", cardLoaded, camLoaded);
        }

        /// <summary>Tìm file .mvcfg theo path job, profile, BaseDir, tempt.</summary>
        private string ResolveMvcfgPath(string jobPath, string defaultFileName, string role)
        {
            var candidates = new List<string>();
            if (!string.IsNullOrWhiteSpace(jobPath))
                candidates.Add(jobPath);
            candidates.Add(defaultFileName);
            candidates.Add(Path.Combine("tempt", defaultFileName));
            candidates.Add(Path.Combine("Configs", defaultFileName));

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string profileDir = null;
            try { profileDir = Utils.Common.ProfileFolder; } catch { }

            foreach (var raw in candidates)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;

                var tryPaths = new List<string>();
                if (Path.IsPathRooted(raw))
                    tryPaths.Add(raw);
                else
                {
                    tryPaths.Add(Path.Combine(baseDir, raw));
                    if (profileDir != null)
                    {
                        tryPaths.Add(Path.Combine(profileDir, raw));
                        tryPaths.Add(Path.Combine(profileDir, "Configs", raw));
                    }
                }

                foreach (var p in tryPaths)
                {
                    if (File.Exists(p))
                    {
                        Info("[IRayple] Resolved {0} mvcfg: {1}", role, p);
                        return p;
                    }
                }
            }
            return null;
        }

        private bool LoadDeviceCfg(string cfgPath, bool loadOnCard)
        {
            if (!File.Exists(cfgPath))
            {
                Bug("[IRayple] Config file not found: {0}", cfgPath);
                return false;
            }

            string target = loadOnCard ? "capture card (CardDev)" : "camera (CamDev)";
            long fileLen = new FileInfo(cfgPath).Length;
            Info("[IRayple] Loading {0} mvcfg: {1} ({2} bytes)", target, cfgPath, fileLen);

            var errorList = new IMVFGDefine.IMV_FG_ErrorList();
            int res = loadOnCard
                ? card.IMV_FG_LoadDeviceCfg(cfgPath, ref errorList)
                : cam.IMV_FG_LoadDeviceCfg(cfgPath, ref errorList);

            if (res != IMVFGDefine.IMV_FG_OK)
            {
                Bug("[IRayple] {0} LoadDeviceCfg FAILED errorCode:{1} ({2})",
                    target, res, DescribeCfgError(res));
                return false;
            }

            for (uint i = 0; i < errorList.nParamCnt; i++)
                Bug("[IRayple] {0} config param error: {1}", target, errorList.paramNameList[i].str);

            Info("[IRayple] {0} mvcfg loaded OK", target);
            ReadDimensionsIntoJob(preferCardSource: loadOnCard);
            return true;
        }

        private static string DescribeCfgError(int code)
        {
            // Mã thường gặp IRayple FG SDK (có thể khác bản SDK)
            switch (code)
            {
                case -114: return "invalid config / wrong handle (card file loaded on cam?)";
                case -108: return "acquisition state / StartGrabbing not allowed";
                case -101: return "invalid handle";
                default: return "see SDK IMV_FG error table";
            }
        }

        /// <summary>Sau khi load cả card + cam mvcfg: đọc W/H từ cả hai, ưu tiên card cho buffer grab.</summary>
        private void MergeDimensionsAfterMvcfgLoad()
        {
            long camW = _job.WidthLS, camH = _job.HeightLS;
            long cardW = 0, cardH = 0;
            bool gotCamW = cam.IMV_FG_GetIntFeatureValue("Width", ref camW) == IMVFGDefine.IMV_FG_OK;
            bool gotCamH = cam.IMV_FG_GetIntFeatureValue("Height", ref camH) == IMVFGDefine.IMV_FG_OK;
            bool gotCardW = card.IMV_FG_GetIntFeatureValue("Width", ref cardW) == IMVFGDefine.IMV_FG_OK;
            bool gotCardH = card.IMV_FG_GetIntFeatureValue("Height", ref cardH) == IMVFGDefine.IMV_FG_OK;

            Info("[IRayple] After mvcfg — cam {0}x{1}, card {2}x{3}",
                gotCamW ? camW.ToString() : "?",
                gotCamH ? camH.ToString() : "?",
                gotCardW ? cardW.ToString() : "?",
                gotCardH ? cardH.ToString() : "?");

            if (gotCardW) _job.WidthLS = cardW;
            else if (gotCamW) _job.WidthLS = camW;

            if (gotCardH) _job.HeightLS = cardH;
            else if (gotCamH) _job.HeightLS = camH;
        }

        /// <summary>Đọc Width/Height từ card (ưu tiên sau load mvcfg card) rồi camera.</summary>
        private void ReadDimensionsIntoJob(bool preferCardSource)
        {
            long width = _job.WidthLS;
            long height = _job.HeightLS;
            bool gotW = false, gotH = false;

            if (preferCardSource)
            {
                gotW = card.IMV_FG_GetIntFeatureValue("Width", ref width) == IMVFGDefine.IMV_FG_OK;
                gotH = card.IMV_FG_GetIntFeatureValue("Height", ref height) == IMVFGDefine.IMV_FG_OK;
                if (gotW) Info("[IRayple] Card Width={0}", width);
                if (gotH) Info("[IRayple] Card Height={0}", height);
            }

            if (!gotW)
            {
                long cw = width;
                if (cam.IMV_FG_GetIntFeatureValue("Width", ref cw) == IMVFGDefine.IMV_FG_OK)
                {
                    width = cw;
                    gotW = true;
                    Info("[IRayple] Camera Width={0}", width);
                }
            }
            if (!gotH)
            {
                long ch = height;
                if (cam.IMV_FG_GetIntFeatureValue("Height", ref ch) == IMVFGDefine.IMV_FG_OK)
                {
                    height = ch;
                    gotH = true;
                    Info("[IRayple] Camera Height={0}", height);
                }
            }

            if (gotW) _job.WidthLS = width;
            else Bug("[IRayple] Could not read Width, job value: {0}", _job.WidthLS);

            if (gotH) _job.HeightLS = height;
            else Bug("[IRayple] Could not read Height, job value: {0}", _job.HeightLS);
        }

        /// <summary>Đọc W/H từ camera (nếu được), ghi lên capture card, log readback từ card.</summary>
        private bool SyncDimensionsToCard(bool logReadback)
        {
            ReadDimensionsIntoJob(preferCardSource: true);

            if (_job.WidthLS <= 0 || _job.HeightLS <= 0)
            {
                Bug("[IRayple] Invalid job dimensions Width={0}, Height={1}", _job.WidthLS, _job.HeightLS);
                return false;
            }

            int resW = card.IMV_FG_SetIntFeatureValue("Width", _job.WidthLS);
            if (resW != IMVFGDefine.IMV_FG_OK)
                Bug("[IRayple] Set card Width={0} FAILED errorCode:{1}", _job.WidthLS, resW);
            else
                Info("[IRayple] Set card Width={0} OK", _job.WidthLS);

            int resH = card.IMV_FG_SetIntFeatureValue("Height", _job.HeightLS);
            if (resH != IMVFGDefine.IMV_FG_OK)
                Bug("[IRayple] Set card Height={0} FAILED errorCode:{1}", _job.HeightLS, resH);
            else
                Info("[IRayple] Set card Height={0} OK", _job.HeightLS);

            if (logReadback)
                LogCardDimensionsReadback();

            return resW == IMVFGDefine.IMV_FG_OK && resH == IMVFGDefine.IMV_FG_OK;
        }

        private void LogCardDimensionsReadback()
        {
            long cardW = 0, cardH = 0;
            int rW = card.IMV_FG_GetIntFeatureValue("Width", ref cardW);
            int rH = card.IMV_FG_GetIntFeatureValue("Height", ref cardH);
            Info("[IRayple] Card readback Width: res={0}, value={1} (job={2})", rW, cardW, _job.WidthLS);
            Info("[IRayple] Card readback Height: res={0}, value={1} (job={2})", rH, cardH, _job.HeightLS);
        }

        private bool ValidateCardDimensionsBeforeStart()
        {
            long cardW = 0, cardH = 0;
            int rW = card.IMV_FG_GetIntFeatureValue("Width", ref cardW);
            int rH = card.IMV_FG_GetIntFeatureValue("Height", ref cardH);

            if (rW != IMVFGDefine.IMV_FG_OK || rH != IMVFGDefine.IMV_FG_OK)
            {
                Bug("[IRayple] Cannot read card dimensions before StartGrabbing (Width res={0}, Height res={1})", rW, rH);
                return false;
            }

            if (cardW <= 0 || cardH <= 0)
            {
                Bug("[IRayple] Card dimensions invalid: {0}x{1}", cardW, cardH);
                return false;
            }

            if (cardW != _job.WidthLS || cardH != _job.HeightLS)
                Info("[IRayple] Card dimensions {0}x{1} differ from job {2}x{3} (using card values for grab)",
                    cardW, cardH, _job.WidthLS, _job.HeightLS);

            return true;
        }

        private void SetSoftTriggerConf(bool applyDimensions = true)
        {
            int res = IMVFGDefine.IMV_FG_OK;

#if TRIGGERBYBOARD     
            res = card.IMV_FG_SetEnumFeatureSymbol("CC1", "SofwareTrigger");
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("Set triggerSource value failed! ErrorCode: {0}", res);
                return;
            }

            if (false == cam.IMV_FG_IsDeviceOpen())
            {
                Bug("Please open device");
                return;
            }

            res = cam.IMV_FG_SetEnumFeatureSymbol("TriggerSource", "CC1");
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("Set triggerSource value failed! ErrorCode[{0}]", res);
                return;
            }

            double LineDeboucerTime = 0;
            res = cam.IMV_FG_SetDoubleFeatureValue("LineDebouncerTime", LineDeboucerTime);
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("Set triggerSource value failed! ErrorCode[{0}]", res);
                return;
            }

            res = cam.IMV_FG_SetEnumFeatureSymbol("LineSelector", "CC1");
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("Set triggerSource value failed! ErrorCode[{0}]", res);
                return;
            }

            res = cam.IMV_FG_SetEnumFeatureSymbol("TriggerMode", "On");
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("Set triggerSource value failed! ErrorCode[{0}]", res);
                return;
            }
#else
            res = cam.IMV_FG_SetEnumFeatureSymbol("TriggerMode", "On");
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("Set triggerSource value failed! ErrorCode[{0}]", res);
                return;
            }

            res = cam.IMV_FG_SetEnumFeatureSymbol("TriggerSource", "Software");
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("Set triggerSource value failed! ErrorCode[{0}]", res);
                return;
            }
#endif
            if (applyDimensions)
                SetCommonConf();
        }

        private void SetLineTriggerConf(bool applyDimensions = true)
        {
            int res = IMVFGDefine.IMV_FG_OK;

#if TRIGGERBYBOARD
            res = card.IMV_FG_SetEnumFeatureSymbol("CC1", "ExternalTrigger1");
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("Set triggerSource value failed! ErrorCode[{0}]", res);
                return;
            }

            if (false == cam.IMV_FG_IsDeviceOpen())
            {
                Bug("Please open device");
                return;
            }

            res = cam.IMV_FG_SetEnumFeatureSymbol("TriggerSource", "CC1");
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("Set triggerSource value failed! ErrorCode[{0}]", res);
                return;
            }

            double LineDeboucerTime = 0;
            res = cam.IMV_FG_SetDoubleFeatureValue("LineDebouncerTime", LineDeboucerTime);
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("Set triggerSource value failed! ErrorCode[{0}]", res);
                return;
            }

            res = cam.IMV_FG_SetEnumFeatureSymbol("LineSelector", "CC1");
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("Set triggerSource value failed! ErrorCode[{0}]", res);
                return;
            }

            res = cam.IMV_FG_SetEnumFeatureSymbol("TriggerMode", "On");
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("Set triggermode value failed! ErrorCode[{0}]", res);
                return;
            }

            res = cam.IMV_FG_SetEnumFeatureSymbol("TriggerActivation", "RisingEdge");
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("Set triggerActivation value failed! ErrorCode[{0}]", res);
                return;
            }
#else
            res = cam.IMV_FG_SetEnumFeatureSymbol("TriggerMode", "On");
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug(string.Format("Set triggermode value failed! ErrorCode[{0}]", res));
                return;
            }

            res = cam.IMV_FG_SetEnumFeatureSymbol("TriggerSource", "Line1");
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug(string.Format("Set triggermode value failed! ErrorCode[{0}]", res));
                return;
            }
#endif
            if (applyDimensions)
                SetCommonConf();
        }

        private void SetCommonConf()
        {
            int res = IMVFGDefine.IMV_FG_OK;
            res = card.IMV_FG_SetIntFeatureValue("Width", _job.WidthLS);
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("set width fail!");
            }

            res = card.IMV_FG_SetIntFeatureValue("Height", _job.HeightLS);
            if (IMVFGDefine.IMV_FG_OK != res)
            {
                Bug("set height fail!");
            }
        }

        private bool ConvertToBitmap(ref IMVFGDefine.IMV_FG_Frame frame, ref Bitmap bitmap)
        {
            IntPtr pDstRGB;
            BitmapData bmpData;
            Rectangle bitmapRect = new Rectangle();

            var ImgSize = (int)frame.frameInfo.width * (int)frame.frameInfo.height * 3;

            try
            {
                pDstRGB = Marshal.AllocHGlobal(ImgSize);
            }
            catch
            {
                return false;
            }
            if (pDstRGB == IntPtr.Zero)
            {
                return false;
            }

            IMVFGDefine.IMV_FG_PixelConvertParam stPixelConvertParam = new IMVFGDefine.IMV_FG_PixelConvertParam();
            int res = 0;
            stPixelConvertParam.nWidth = frame.frameInfo.width;
            stPixelConvertParam.nHeight = frame.frameInfo.height;
            stPixelConvertParam.ePixelFormat = frame.frameInfo.pixelFormat;
            stPixelConvertParam.pSrcData = frame.pData;
            stPixelConvertParam.nSrcDataLen = frame.frameInfo.size;
            stPixelConvertParam.nPaddingX = frame.frameInfo.paddingX;
            stPixelConvertParam.nPaddingY = frame.frameInfo.paddingY;
            stPixelConvertParam.eBayerDemosaic = IMVFGDefine.IMV_FG_EBayerDemosaic.IMV_FG_DEMOSAIC_NEAREST_NEIGHBOR;
            stPixelConvertParam.eDstPixelFormat = IMVFGDefine.IMV_FG_EPixelType.IMV_FG_PIXEL_TYPE_BGR8;
            stPixelConvertParam.pDstBuf = pDstRGB;
            stPixelConvertParam.nDstBufSize = (uint)ImgSize;

            res = card.IMV_FG_PixelConvert(ref stPixelConvertParam);
            if (res != IMVFGDefine.IMV_FG_OK)
            {
                Console.WriteLine("image convert to BGR failed!");
                return false;
            }
            bitmap = new Bitmap((int)frame.frameInfo.width, (int)frame.frameInfo.height, PixelFormat.Format24bppRgb);

            bitmapRect.Height = bitmap.Height;
            bitmapRect.Width = bitmap.Width;
            bmpData = bitmap.LockBits(bitmapRect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            CopyMemory(bmpData.Scan0, pDstRGB, bmpData.Stride * bitmap.Height);
            bitmap.UnlockBits(bmpData);

            Marshal.FreeHGlobal(pDstRGB);

            return true;
        }

        #endregion
    }
}
