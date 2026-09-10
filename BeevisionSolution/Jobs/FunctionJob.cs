using BeevisionSolution.Utils;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using static BeevisionSolution.Utils.Constant;

namespace BeevisionSolution.Models
{
    [Serializable]
    public class FunctionJob : BaseJob
    {
        private static int Timeout = 3000;
        [JsonIgnore]
        public Object InputImage { get; internal set; }
        [JsonIgnore]
        public Object InputImage2 { get; internal set; }
        [JsonIgnore]
        public Object Result { get; internal set; }
        [JsonIgnore]
        public Object Result2 { get; internal set; }//use for multitools, ivb
        //[JsonIgnore]
        //public Object Result3 { get; internal set; }//use for multitools, ivb
        [JsonIgnore]
        public Object Results { get; internal set; }
        [JsonIgnore]
        public Object Results1 { get; internal set; }
        [JsonIgnore]
        public Object Record { get; internal set; }
        [JsonIgnore]
        public Object Distance { get; internal set; }
        [JsonIgnore]
        public Object Alarm { get; internal set; }

        [JsonIgnore]
        public string ProductID { get; internal set; }
        [JsonIgnore]
        public string SheetID { get; internal set; }
        [JsonIgnore]
        public string PcsRotate { get; internal set; }
        [JsonIgnore]
        public Object InputPcsIndex {  get; internal set; }
        [JsonIgnore]
        public Object OutputPcsIndex { get; internal set; }
        [JsonIgnore]
        public Object ResultCode { get; internal set; }
        [JsonIgnore]
        public string LotID { get; internal set; }

        [JsonIgnore]
        public Object Specs { get; internal set; }
        [JsonIgnore]
        public bool Status { get; internal set; }
        [JsonIgnore]
        public Object TrayDirection { get; internal set; }

        [JsonIgnore]
        public Object ListDouble { get; internal set; }
        [JsonIgnore]
        public Object ListInt { get; internal set; }
        [JsonIgnore]
        public Object ListUInt64 { get; internal set; }

        [JsonIgnore]
        public Object CpkOut1 { get; internal set; }
        [JsonIgnore]
        public Object CpkOut2 { get; internal set; }
        [JsonIgnore]
        public Object CpkOut3 { get; internal set; }
        [JsonIgnore]
        public Object CpkOut4 { get; internal set; }
        [JsonIgnore]
        public Object StrOut1 { get; internal set; }
        [JsonIgnore]
        public Object StrOut2 { get; internal set; }
        [JsonIgnore]
        public Object StrOut3 { get; internal set; }


        protected override void ToolBlockRan(object sender, EventArgs e)
        {
            //Bug("ToolBlockRan is fired {0}", Name);
            var tb = sender as CogToolBlock;

            if (tb.Outputs.Contains(strResultKey))
                Result = tb.Outputs[strResultKey].Value;

            if (tb.Outputs.Contains(strResult2Key))
                Result2 = tb.Outputs[strResult2Key].Value;
            if (tb.Outputs.Contains(strResultCodeKey))
                ResultCode = tb.Outputs[strResultCodeKey].Value;

            //if (tb.Outputs.Contains(strResult3Key))
            //    Result3 = tb.Outputs[strResult3Key].Value;

            if (tb.Outputs.Contains(strResultsKey))
                Results = tb.Outputs[strResultsKey].Value;
            if (tb.Outputs.Contains(strResults1Key))
                Results1 = tb.Outputs[strResults1Key].Value;

            if (tb.Outputs.Contains(strRecordKey))
                Record = tb.Outputs[strRecordKey].Value;

            if (tb.Outputs.Contains(strOutputImageKey))
                OutputImage = tb.Outputs[strOutputImageKey].Value;
            else OutputImage = InputImage;

            if (tb.Outputs.Contains(strIspDistance))
                Distance = tb.Outputs[strIspDistance].Value;

            if (tb.Outputs.Contains(strAlarmProcess))
                Alarm = tb.Outputs[strAlarmProcess].Value;

            if (tb.Outputs.Contains(strTrayDirection))
                TrayDirection = tb.Outputs[strTrayDirection].Value;
            if(tb.Outputs.Contains(strPcsIndexKey))
                OutputPcsIndex = tb.Outputs[strPcsIndexKey].Value;

            if (tb.Outputs.Contains(strListDouble))
                ListDouble = tb.Outputs[strListDouble].Value;
            if (tb.Outputs.Contains(strListInt))
                ListInt = tb.Outputs[strListInt].Value;
            if (tb.Outputs.Contains(strListUInt64))
                ListUInt64 = tb.Outputs[strListUInt64].Value;
            if (tb.Outputs.Contains(strStrOut1))
                StrOut1 = tb.Outputs[strStrOut1].Value;
            if (tb.Outputs.Contains(strStrOut2))
                StrOut2 = tb.Outputs[strStrOut2].Value;
            if (tb.Outputs.Contains(strStrOut3))
                StrOut3 = tb.Outputs[strStrOut3].Value;
            if (tb.Outputs.Contains(strCpkOut1))
                CpkOut1 = tb.Outputs[strCpkOut1].Value;
            if (tb.Outputs.Contains(strCpkOut2))
                CpkOut2 = tb.Outputs[strCpkOut2].Value;
            if (tb.Outputs.Contains(strCpkOut3))
                CpkOut3 = tb.Outputs[strCpkOut3].Value;
            if (tb.Outputs.Contains(strCpkOut4))
                CpkOut4 = tb.Outputs[strCpkOut4].Value;

            Available = true;

            if (AllowFireEvent && (null != OnToolBlockRan)) OnToolBlockRan.Invoke(this, Result, Results);
            AllowFireEvent = true;
            RunStatus = tb.RunStatus.Result;
            busyWait.Set();
        }

        public override bool RunTool()
        {
            if (!Available) busyWait.WaitOne(Timeout);
            if ((null != ToolBlock) && Available && Initialized)
            {
                var tb = ToolBlock as CogToolBlock;

                if (tb.Inputs.Contains(strParamsKey))
                    tb.Inputs[strParamsKey].Value = Params;

                if (tb.Inputs.Contains(strInputImageKey))
                    tb.Inputs[strInputImageKey].Value = InputImage;

                if (tb.Inputs.Contains(strInputImage2Key))
                    tb.Inputs[strInputImage2Key].Value = InputImage2;

                //while (!Available) Task.Delay(1);
                busyWait.Reset();

                RunStatus = CogToolResultConstants.Error;
                Available = false;
                Results = null;
                Results1 = null;
                Result = null;
                Result2 = null;
                ResultCode = null;
                //Result3 = null;
                Record = null;
                Distance = null;
                Alarm = false;
                TrayDirection = null;
                OutputPcsIndex = null;

                ListDouble = null;
                ListInt = null;
                StrOut1 = null;
                StrOut2 = null;
                StrOut3 = null;
                CpkOut1 = null;
                CpkOut2 = null;
                CpkOut3 = null;
                CpkOut4 = null;

                if (!string.IsNullOrEmpty(ProductID))
                {
                    if (tb.Inputs.Contains(strProductIDKey))
                    {
                        tb.Inputs[strProductIDKey].Value = ProductID;
                    }
                }
                if (!string.IsNullOrEmpty(SheetID))
                {
                    if (tb.Inputs.Contains(strSheetIdKey))
                    {
                        tb.Inputs[strSheetIdKey].Value = SheetID;
                    }
                }
                if (!string.IsNullOrEmpty(PcsRotate))
                {
                    if (tb.Inputs.Contains(strPcsRotateKey))
                    {
                        tb.Inputs[strPcsRotateKey].Value = PcsRotate;
                    }
                }
                if (!string.IsNullOrEmpty(LotID))
                {
                    if (tb.Inputs.Contains(strLotIdKey))
                    {
                        tb.Inputs[strLotIdKey].Value = LotID;
                    }
                }
                if (InputPcsIndex != null)
                {
                    if (tb.Inputs.Contains(strPcsIndexKey))
                    {
                        tb.Inputs[strPcsIndexKey].Value = InputPcsIndex;
                    }
                }

                try
                {
                    tb.Run();
                }
                catch (Exception ex)
                {
                    //may cause violation error, take time then retry
                    Common.Bug("RunTool exception, Job {0}, message {1}", Name, ex.Message);
                    Common.Bug(ex.StackTrace);
                    Thread.Sleep(200);
                    tb.Run();
                }


                return Available;
            }
            return false;
        }

        AutoResetEvent busyWait = new AutoResetEvent(false);
        public override async Task<bool> RunToolAsync()
        {
            if (!Available) busyWait.WaitOne(Timeout);

            if ((null != ToolBlock) && Available && Initialized)
            {
                var tb = ToolBlock as CogToolBlock;

                if (tb.Inputs.Contains(strParamsKey))
                    tb.Inputs[strParamsKey].Value = Params;

                if (tb.Inputs.Contains(strInputImageKey))
                    tb.Inputs[strInputImageKey].Value = InputImage;

                if (tb.Inputs.Contains(strInputImage2Key))
                    tb.Inputs[strInputImage2Key].Value = InputImage2;

                //while (!Available) await Task.Delay(1);
                busyWait.Reset();

                RunStatus = CogToolResultConstants.Error;
                Available = false;
                Results = null;
                Results1 = null;
                Result = null;
                Result2 = null;
                ResultCode = null;
                //Result3 = null;
                Record = null;
                Distance = null;
                Alarm = false;
                TrayDirection = null;

                ListDouble = null;
                ListInt = null;
                StrOut1 = null;
                StrOut2 = null;
                StrOut3 = null;
                CpkOut1 = null;
                CpkOut2 = null;
                CpkOut3 = null;
                CpkOut4 = null;

                if (!string.IsNullOrEmpty(ProductID))
                {
                    if (tb.Inputs.Contains(strProductIDKey))
                    {
                        tb.Inputs[strProductIDKey].Value = ProductID;
                    }
                }
                try
                {
                    await Task.Run(() =>
                    {
                        lock (ToolBlockLock)
                        {
                            tb.Run();
                        }
                    });
                }
                catch (Exception ex)
                {
                    //may cause violation error, take time then retry
                    Common.Bug("RunTool exception, Job {0}, message {1}", Name, ex.Message);
                    Common.Bug(ex.StackTrace);
                    Thread.Sleep(200);
                    await Task.Run(() =>
                    {
                        lock (ToolBlockLock)
                        {
                            tb.Run();
                        }
                    });
                }
                return Available;
            }
            return false;
        }

        ~FunctionJob()
        {
            Dispose();
        }
        public UInt32 CalibId { get; set; }
        public int PlcJobId { get; set; }
        public bool IsMoving { get; internal set; } = false;
        public bool CoordinateOrigin { get; set; } = false;
        public int LogViewId { get; set; }
        [JsonIgnore]
        public bool IsOpCall { get; set; }
        public int DelayTime { get; set; }
        public bool HaveExtendData { get; set; }
        public int[] LightJobId { get; set; }
    }
}
