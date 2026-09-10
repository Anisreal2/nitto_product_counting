using BeeLib.Math;
using BeevisionSolution.Controller;
using BeevisionSolution.LocalDB;
using BeevisionSolution.Utils;
using BeevisionSolution.Views;
using Cognex.VisionPro;
using DatabaseInterface_MMCV;
using DocumentFormat.OpenXml.Presentation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using static BeevisionSolution.Utils.Common;

namespace BeevisionSolution.Models
{
    public class PlcAlignHeaderModel
    {
        public bool AlignReset_00 { get; set; }
        public bool AlignStartReq_01 { get; set; }
        public bool AlignCompAck_02 { get; set; }
        public bool CmdType_03 { get; set; }
        public bool LightOff_04 { get; set; }
        public bool LightOn_05 { get; set; }
        public bool GrabPos1Req_06 { get; set; }
        public bool GrabPos2Req_07 { get; set; }
        public bool GrabPos3Req_08 { get; set; }
        public bool GrabPos4Req_09 { get; set; }
        public bool CalStartAck_0A { get; set; }
        public bool MoveCmdAck_0B { get; set; }
        public bool MoveDone_0C { get; set; }
        public bool CalEndAck_0D { get; set; }
        public bool Reserve_0E { get; set; }
        public bool Abort_0F { get; set; }

        public void SetData(ushort data) { }
        public ushort GetData() { return 0; }
    }

    public class PlcCommonHeaderModel
    {
        public bool Ready_00 { get; set; }
        public bool Run_01 { get; set; }
        public bool AutoMode_02 { get; set; }
        public bool Error_03 { get; set; }

        public void SetData(ushort data) { }
        public ushort GetData() { return 0; }
    }

    public class VisionAlignHeaderModel
    {
        public bool AlignReadyAck_00 { get; set; }
        public bool AlignStartAck_01 { get; set; }
        public bool AlignComp_02 { get; set; }
        public bool Reserve_03 { get; set; }
        public bool AlignIspOK_04 { get; set; }
        public bool AlignIspNG_05 { get; set; }
        public bool GrabCalPos1_06 { get; set; }
        public bool GrabCalPos2_07 { get; set; }
        public bool GrabCalPos3_08 { get; set; }
        public bool GrabCalPos4_09 { get; set; }
        public bool CalStartReq_0A { get; set; }
        public bool MoveCmdReq_0B { get; set; }
        public bool MoveDoneAck_0C { get; set; }
        public bool CalEndReq_0D { get; set; }
        public bool OpCall_0E { get; set; }
        public bool ErrorState_0F { get; set; }

        public void SetData(ushort data) { }
        public ushort GetData() { return 0; }
    }

    public class VisionCommonHeaderModel
    {
        public bool Ready_00 { get; set; }
        public bool Run_01 { get; set; }
        public bool AutoMode_02 { get; set; }
        public bool Error_03 { get; set; }

        public void SetData(ushort data) { }
        public ushort GetData() { return 0; }
    }

    public class PlcCam
    {
        public int PlcJobId { get; internal set; }
        [JsonIgnore]
        public const int SIZE_OF_DATA_IN_BYTE = 2;

        public event Action<PlcCam, object> TriggerSignal = null;
        public event Action<PlcCam, object, bool, bool, string> HETriggerSignal = null;//isDone, isError, message
        //public event Action<int, string, string, ICogImage, List<object>> OnResult = null;

        [JsonIgnore]
        const int PlcTimeout = 2000;
        [JsonIgnore]
        public ushort ToolId { get; private set; }
        [JsonIgnore]
        public String ToolCode { get; private set; }
        [JsonIgnore]
        public bool KeepRunning { get; internal set; } = true;
        [JsonIgnore]
        //public int JIndex { get; internal set; }//to keep index of buffer list and address list
        public bool IsActive { get; internal set; } = true;
        public bool IsWatcherJob { get; internal set; }
        public bool IsMultiMarks { get; internal set; }
        public bool IsBarcodeOn { get; internal set; }
        public string[] Alias { get; internal set; }
        public bool IsMultiTools { get; internal set; }
        public bool IsMultiToolsCamMoving { get; internal set; }
        public bool UseTwoImageInspection { get; internal set; } = false;
        public bool UseMultiMarkDeltaByResults1 { get; internal set; } = false;
        public bool EnableLengthCheck { get; internal set; } = false;
        public double ExpectedLengthMillimeters { get; internal set; } = 0.0;
        public double LengthToleranceMillimeters { get; internal set; } = 0.0;
        public double LengthOffsetMillimeters { get; internal set; } = 0.0;
        public int PlcJobType { get; internal set; }
        public bool IsMultiTools3 { get; internal set; }
        public bool IsMiJob { get; internal set; }
        public bool IsStart { get; internal set; }
        public bool IsStop { get; internal set; }
        public String PlcName { get; internal set; } = "PLC1";
        //public JobType JobType { get; internal set; } = JobType.TypeInspection;
        public String JobName { get; internal set; } = "Isp1";
        //public string HEJobName { get; internal set; }
        public String Param { get; internal set; }
        public string BarcodeIp { get; internal set; }
        public int PlcHeTimeout { get; internal set; } = 10000;

        public string DisplayName { get; internal set; }
        public int MultiplyBy { get; internal set; } = 10000;

        [JsonIgnore]
        public PlcAlignHeaderModel PlcToolStatus { get; internal set; }
        [JsonIgnore]
        public PlcCommonHeaderModel PlcHeaderStatus { get; internal set; }
        [JsonIgnore]
        public VisionAlignHeaderModel VisionToolStatus { get; internal set; }
        [JsonIgnore]
        public VisionCommonHeaderModel VisionHeaderStatus { get; internal set; }

        public int SizeFromStart { get; internal set; } = 0;//using for reduce configuration these options below, Set to header of next job 200, 400, 600...
        //----------------------- VISION --------------------------
        public int V_Header { get; internal set; } = 0;
        public int V_AlarmCode { get; internal set; } = 2;
        public int V_Revision1_X { get; internal set; } = 5;
        public int V_Revision1_Y { get; internal set; } = 7;
        public int V_Revision1_TH { get; internal set; } = 9;
        public int V_Target1_X { get; internal set; } = 13;
        public int V_Target1_Y { get; internal set; } = 15;
        public int V_Target1_TH { get; internal set; } = 17;
        public int V_Revision2_X { get; internal set; } = 21;
        public int V_Revision2_Y { get; internal set; } = 23;
        public int V_Revision2_TH { get; internal set; } = 25;
        public int V_Target2_X { get; internal set; } = 29;
        public int V_Target2_Y { get; internal set; } = 31;
        public int V_Target2_TH { get; internal set; } = 33;
        public int V_Revision3_X { get; internal set; } = 37;
        public int V_Revision3_Y { get; internal set; } = 39;
        public int V_Revision3_TH { get; internal set; } = 41;

        public int V_Revision4_X { get; internal set; } = 53;
        public int V_Revision4_Y { get; internal set; } = 55;
        public int V_Revision4_TH { get; internal set; } = 57;
        public int V_Offset_X { get; internal set; } = 59;
        public int V_Offset_Y { get; internal set; } = 61;

        public int V_CameraStatus { get; internal set; } = 63;
        public int V_LengthCheckDistance { get; internal set; } = 65;
        public int V_LengthTolerance { get; internal set; } = 67;
        public int V_LengthExpected { get; internal set; } = 69;
        public int V_RetryCount { get; internal set; } = 85;



        public int V_Distance_Left_X { get; internal set; } = 37;
        public int V_Distance_Left_Y { get; internal set; } = 39;
        public int V_Distance_Right_X { get; internal set; } = 41;
        public int V_Distance_Right_Y { get; internal set; } = 43;

        public int V_TrayDirection { get; internal set; } = 45;

        public int V_PM_SCORE1 { get; internal set; } = 71;
        public int V_PM_SCORE2 { get; internal set; } = 73;

        public int V_Isp_Result { get; internal set; } = 75;

        public int V_List_Double { get; internal set; } = 76;
        public int V_List_Double2 { get; internal set; } = 77;
        public int V_List_Double3 { get; internal set; } = 78;
        public int V_List_Double4 { get; internal set; } = 79;
        public int List_Double_Count { get; internal set; } = 0;//list double count

        public int V_List_Int { get; internal set; } = 80;
        public int List_Int_Count { get; internal set; } = 0;//list int count

        public int V_List_UInt64 { get; internal set; } = 84;
        public int List_UInt64_Count { get; internal set; } = 0;//list uint count

        public int V_Str_Out_1 { get; internal set; } = 81;
        public int Str_Out_1_Lenght { get; internal set; } = 0;//lenght of string out1

        public int V_Str_Out_2 { get; internal set; } = 82;
        public int Str_Out_2_Lenght { get; internal set; } = 0;

        public int V_Str_Out_3 { get; internal set; } = 83;
        public int Str_Out_3_Lenght { get; internal set; } = 0;

        //----------------------- PLC --------------------------
        public int Plc_Header { get; internal set; } = 0;
        public int Plc_OptionNo { get; internal set; } = 1;
        public int Plc_UnitNo { get; internal set; } = 2;
        public int Plc_CameraNo { get; internal set; } = 3;
        public int Plc_ZoneNo { get; internal set; } = 4;
        public int Plc_Offset1_X { get; internal set; } = 5;
        public int Plc_Offset1_Y { get; internal set; } = 7;
        public int Plc_Offset2_TH { get; internal set; } = 9;
        public int Plc_Servo_X { get; internal set; } = 13;
        public int Plc_Servo_Y { get; internal set; } = 15;
        public int Plc_Servo_TH { get; internal set; } = 17;
        public int Plc_Teaching1_X { get; internal set; } = 21;
        public int Plc_Teaching1_Y { get; internal set; } = 23;
        public int Plc_Teaching1_TH { get; internal set; } = 25;
        public int Plc_CamPos1_X1 { get; internal set; } = 29;
        public int Plc_CamPos1_X2 { get; internal set; } = 31;
        public int Plc_CamPos1_Y { get; internal set; } = 33;
        public int Plc_Product_ID { get; internal set; } = 35;
        public ushort Product_ID_Lenght { get; internal set; } = 24;
        public int Plc_Lot { get; internal set; } = 40;
        public int Plc_Column { get; internal set; } = 42;
        public int Plc_Row { get; internal set; } = 44;
        public int Plc_Pin { get; internal set; } = 46;
        public int Plc_Pcs_Index { get; internal set; } = 52;
        //----------------------------------------------------
        [JsonIgnore]
        public bool IsOk = false;
        [JsonIgnore]
        public bool IsTool1_OK = false;//for multi tools only
        [JsonIgnore]
        public bool IsTool2_OK = false;//for multi tools only
        [JsonIgnore]
        public bool IsTool3_OK = false;//for multi tools only
        [JsonIgnore]
        public bool IsGetMotion = false; // for motion trigger only
        [JsonIgnore]
        public bool IsLightOff = false;
        [JsonIgnore]
        public double LastCycleTimeSeconds { get; private set; } = double.NaN;
        [JsonIgnore]
        private Object firstInspectionImage;
        [JsonIgnore]
        private string firstInspectionJobName;
        [JsonIgnore]
        private string firstInspectionProductId;

        [JsonIgnore]
        public bool HasFirstInspectionImage
        {
            get { return firstInspectionImage != null; }
        }

        public PlcCam()
        {
            PlcToolStatus = new PlcAlignHeaderModel();
            VisionToolStatus = new VisionAlignHeaderModel();
            PlcHeaderStatus = new PlcCommonHeaderModel();
            VisionHeaderStatus = new VisionCommonHeaderModel();
        }

        public void StoreFirstInspectionImage(Object image, string inspectionJobName, string productId)
        {
            firstInspectionImage = image;
            firstInspectionJobName = inspectionJobName;
            firstInspectionProductId = productId;
        }

        public bool TryTakeFirstInspectionImage(string inspectionJobName, string productId, out Object image)
        {
            image = null;

            if (firstInspectionImage == null)
            {
                return false;
            }

            if (!String.Equals(firstInspectionJobName, inspectionJobName, StringComparison.OrdinalIgnoreCase))
            {
                ClearFirstInspectionImage();
                return false;
            }

            if (!String.IsNullOrEmpty(firstInspectionProductId) &&
                !String.IsNullOrEmpty(productId) &&
                !String.Equals(firstInspectionProductId, productId, StringComparison.OrdinalIgnoreCase))
            {
                ClearFirstInspectionImage();
                return false;
            }

            image = firstInspectionImage;
            ClearFirstInspectionImage();
            return true;
        }

        public void ClearFirstInspectionImage()
        {
            firstInspectionImage = null;
            firstInspectionJobName = null;
            firstInspectionProductId = null;
        }

        public void SetAlignHeader()
        {
        }

        public void VisionAlignHeaderInit()
        {
            VisionToolStatus.SetData(0);
            VisionToolStatus.AlignReadyAck_00 = true;
            VisionToolStatus.AlignStartAck_01 = false;
            SetAlignHeader();
            SetStatusCamera(false);
        }

        public void SetAlignResult(List<Pose> lstPose)
        {
        }

        public void SetAlignResult(Pose pose, double pmScore = -1111)
        {
        }

        public void SetAlignResult(Pose pose, int index, double pmScore = -1111)
        {
        }

        public void SetAlignResult2(Pose pose, double pmScore = -1111)
        {
        }

        /// <summary>Write PM score to V_PM_SCORE2 only (pose registers unchanged).</summary>
        public void SetPmScore2(double pmScore)
        {
        }

        public void SetAlignResult3(Pose pose, double pmScore = -1111)
        {
        }

        public void SetAlignResult4(Pose pose)
        {
        }

        public void SetOffsetResult(Pose pose)
        {
        }

        public void SetAlignResult(Pose pose1, Pose pose2)
        {
        }

        public void SetListDouble(List<Double> list)
        {
        }

        public void SetListDouble2(List<Double> list)
        {
        }

        public void SetListDouble3(List<Double> list)
        {
        }

        public void SetListDouble4(List<Double> list)
        {
        }

        public void SetListInt(List<int> list)
        {
        }

        public void SetListUInt64(List<UInt64> list)
        {
        }

        public void SetStrOut1(string data)
        {
        }

        public void SetStrOut2(string data)
        {
        }

        public void SetStrOut3(string data)
        {
        }

        public void SetMeasurementResult(double leftX, double leftY, double rightX, double rightY)
        {
        }

        public void SetTrayDirection(bool direction)
        {
        }

        public void SetStatusCamera(bool direction)
        {
        }

        public bool SetLengthCheckValues(double lengthCheckDistanceMillimeters, double lengthToleranceMillimeters, double expectedLengthMillimeters)
        {
            int distanceValueForPlc;
            bool distanceIsValid = TryConvertMillimetersToPlcValue(lengthCheckDistanceMillimeters, out distanceValueForPlc);
            int toleranceValueForPlc;
            bool toleranceIsValid = TryConvertMillimetersToPlcValue(lengthToleranceMillimeters, out toleranceValueForPlc);
            int expectedValueForPlc;
            bool expectedIsValid = TryConvertMillimetersToPlcValue(expectedLengthMillimeters, out expectedValueForPlc);
            return distanceIsValid && toleranceIsValid && expectedIsValid;
        }

        public bool SetLengthCheckValues(double lengthCheckDistanceMillimeters, double lengthToleranceMillimeters)
        {
            return SetLengthCheckValues(lengthCheckDistanceMillimeters, lengthToleranceMillimeters, ExpectedLengthMillimeters);
        }

        public void SetRetryCount(int retryCount)
        {
        }

        private bool TryConvertMillimetersToPlcValue(double millimeters, out int valueForPlc)
        {
            valueForPlc = 0;

            if (double.IsNaN(millimeters) ||
                double.IsInfinity(millimeters) ||
                millimeters < 0 ||
                MultiplyBy <= 0)
            {
                return false;
            }

            double scaledValue = millimeters * MultiplyBy;
            if (scaledValue > int.MaxValue)
            {
                return false;
            }

            valueForPlc = (int)Math.Round(scaledValue, MidpointRounding.AwayFromZero);
            return true;
        }

        private bool CanWriteInt32ToPlcBuffer(int byteOffset)
        {
            return true;
        }

        public void SetIspData(object data)
        {
        }

        public PlcAlignHeaderModel GetPlcAlignHeader()
        {
            return PlcToolStatus;
        }

        public PlcCommonHeaderModel GetPlcCommonHeader()
        {
            return PlcHeaderStatus;
        }

        public void SetAlignReady(bool isReady)
        {
            VisionToolStatus.AlignReadyAck_00 = isReady;
            SetAlignHeader();
        }


        public void Abort()
        {
            KeepRunning = false;
        }

        public ushort ClearSize { get; set; } = 20;
        public static event EventHandler LiveJobsStopped;
        public async Task Final()
        {
            if (_isFinalizing || !IsProcessing)
            {
                return;
            }

            _isFinalizing = true;
            try
            {
                GetPlcAlignHeader();
                if (PlcToolStatus.AlignReset_00 || PlcToolStatus.Abort_0F)
                {
                    Info("Received reset/abort signal at start of Final(), aborting...");
                    VisionAlignHeaderInit();
                    IsProcessing = false;
                    return;
                }

                var ProcessingTime = (long)0;

                ProcessingTime = (long)DateTime.Now.Subtract(triggerTime).TotalMilliseconds;
                Info("{0}, Total vision processing time upon recive a trigger. {1} ms", JobName, ProcessingTime);

                //make sure to update result data first

                VisionToolStatus.AlignStartAck_01 = false;//align start ack --> off

                if (IsOk)//update result header
                {
                    VisionToolStatus.AlignIspOK_04 = true;
                }
                else
                {
                    VisionToolStatus.AlignIspNG_05 = true;
                }
                VisionToolStatus.AlignComp_02 = true;//align complete request on
                SetAlignHeader();

                GetPlcAlignHeader();
                if (PlcToolStatus.AlignReset_00 || PlcToolStatus.Abort_0F)
                {
                    Info("Received reset/abort signal after setting header in Final(), aborting...");
                    VisionAlignHeaderInit();
                    IsProcessing = false;
                    return;
                }

                Info("{0}, Waiting for Align Comp Ack On", JobName);
                var timeOut = 0;

                //wait for plc ack result 
                //while (GetPlcAlignHeader().AlignCompAck_02 == true)
                //{
                //    Thread.Sleep(1);
                //    if (!IsProcessing)
                //    {
                //        _isFinalizing = false;
                //        return;
                //    }
                //    if (++timeOut >= PlcTimeout)
                //    {
                //        Bug("{0}, Timeout waiting for Align Comp Ack On", JobName);
                //        break;
                //    }
                //}

                //SpinWait.SpinUntil(() => GetPlcAlignHeader().AlignCompAck_02 == true || !IsProcessing);
                //if (!IsProcessing)
                //{
                //    return;
                //}
                var waitAlignCompAckOnSw = Stopwatch.StartNew();
                SpinWait.SpinUntil(
                    () => GetPlcAlignHeader().AlignCompAck_02 == true
                          || !IsProcessing
                          || PlcToolStatus.AlignReset_00
                          || PlcToolStatus.Abort_0F
                );

                waitAlignCompAckOnSw.Stop();
                Info("{0}, PLC On bit AlignCompAck_02 : {1} ms",
                    JobName,
                    waitAlignCompAckOnSw.ElapsedMilliseconds);

                VisionToolStatus.AlignComp_02 = false;
                VisionToolStatus.AlignIspOK_04 = false;
                VisionToolStatus.AlignIspNG_05 = false;
                SetStatusCamera(false);
                SetAlignHeader();
                Info("{0}, Bit Ready AlignReadyAck_00 On for next cycle", JobName);

                VisionAlignHeaderInit();
                VisionToolStatus.AlignStartAck_01 = false;
                SetAlignHeader();

                ProcessingTime = (long)DateTime.Now.Subtract(triggerTime).TotalMilliseconds;
                LastCycleTimeSeconds = ProcessingTime / 1000.0;
                IsOk = false;
                IsTool1_OK = false;
                IsTool2_OK = false;

                Info("{0}, Finish cycle", JobName);
                Info("{0}, Cycle Time trigger: {1} ms", JobName, ProcessingTime);

                IsProcessing = false;
                _isFinalizing = false;
            }
            catch (Exception ex)
            {
                Bug("Final Exception: {0}", ex.Message);
                Bug(ex.StackTrace);
                IsProcessing = false;
                _isFinalizing = false;
            }
        }

        DateTime triggerTime = DateTime.Now;
        public bool IsProcessing = false;
        private bool _isFinalizing = false; 
        public string GetReadAddress()
        {
            return "";
        }

        public string GetWriteAddress()
        {
            return "";
        }

        public async void Run(Object obj)
        {
        }

        public async void RunHE(Object obj)
        {
        }

        public Motion GetCurrentLotData()
        {
            return new Motion { Lot = 0, Col = 0, Row = 0, Pin = 0 };
        }

        public Pose GetServoCurrentPose()
        {
            return new Pose(0, 0, 0);
        }

        public Pose GetPickUpCurrentPose()
        {
            return new Pose(0, 0, 0);
        }

        public void GetCameraZoneNo(out short cameraNo, out short zoneNo)
        {
            cameraNo = 1;
            zoneNo = 1;
        }

        public int GetPcsIndex()
        {
            return 0;
        }

        public string GetSheetId()
        {
            return "";
        }

        public string GetProductId()
        {
            return "";
        }

        public void SetNextMove(Pose pose)
        {
        }
    }
}
