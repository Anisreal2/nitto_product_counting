using System;
using System.Security.RightsManagement;
using System.Windows.Documents;
using System.Windows.Media;

namespace BeevisionSolution.Utils
{
    class Constant
    {
        public const String strCompanyName = "BVS JSC. (2022.01.09.00)";
        public const string strOutputImageKey = "OutputImage";
        public const string strInputImageKey = "InputImage";
        public const string strInputImage2Key = "InputImage2";
        public const string strParamsKey = "Params";
        public const string strProductIDKey = "ProductID";
        public const string strPcsIndexKey = "PcsIndex";
        public const string strPcsRotateKey = "PcsRotate";
        public const string strResultCodeKey = "ResultCode";
        public const string strSheetIdKey = "SheetID";
        public const string strLotIdKey = "LotID";
        public const string strSpecsKey = "Spec";
        public const string strStatusKey = "Status";
        public const string strCameraSettingKey = "CameraSetting";
        public const string strResultKey = "Result";
        public const string strResult2Key = "Result2";
        public const string strResult3Key = "Result3";

        public const string strResultsKey = "Results";
        public const string strResults1Key = "Results1";
        public const string strRecordKey = "Record";
        public const String strToolBlockEditor = "ToolBlock Editor";
        public const string strLiveView = "LiveView";
        public const string strNoCode = "NoCode";
        public const string strIspDistance = "Distance";
        public const string strAlarmProcess = "Alarm";
        public const string strTrayDirection = "TrayDirection";

        public const string strListDouble = "ListDouble";
        public const string strListInt = "ListInt";
        public const string strListUInt64 = "ListUInt64";
        public const string strCpkOut1 = "CpkOut1";
        public const string strCpkOut2 = "CpkOut2";
        public const string strCpkOut3 = "CpkOut3";
        public const string strCpkOut4 = "CpkOut4";
        public const string strStrOut1 = "StrOut1";
        public const string strStrOut2 = "StrOut2";
        public const string strStrOut3 = "StrOut3";

        public const double gDefaultStepLength = 2.0;
        public const String strPoseNotFound = "000.003,000.003,000.003";

        public const String strDefaultComPort = "COM1";
        public const String strDefaultCameraName = "Camera0";
        public const String strDefaultIspName = "Isp0";
        public const String strDefaultAlignName = "Pick";
        public const String strDefaultJobFile = "Job.vpp";
        public const String strDefaultProfile = "Default";
        public const String strDefaultAppName = "APPLICATION NAME";
        public const String strDefaultTitle = "BEE MACHINE VISION";
        public const String strAutoRunKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        public static Inline MajorSeparator => new Run("  |  ");
        public static Inline MinorSeparator => new Run(", ");
        public static Inline BreakLine => new LineBreak();

        public static readonly Color WatermarkBackground = Color.FromArgb(0xaa, 0x30, 0x30, 0x30);
        public static readonly Color WatermarkForeground = Color.FromArgb(0xff, 0xff, 0xff, 0x00);

        public const string SERVER_SOURCE = "S";
        public const string CLIENT_SOURCE = "C";

        public const string COMMAND_FAILED = "0";
        public const string COMMAND_FAILED_N1 = "-1";
        public const string COMMAND_FAILED_N2 = "-2";
        public const string COMMAND_FAILED_N3 = "-3";
        public const string COMMAND_FAILED_N4 = "-4";
        public const string COMMAND_FAILED_N5 = "-5";
        public const string COMMAND_FAILED_N6 = "-6";
        public const string COMMAND_PENDING = "";
        public const string COMMAND_SUCCESSED_P1 = "1";
        public const string COMMAND_SUCCESSED_P2 = "2";

        public const int dwToolTimeout = 5000; // 5s
        public const int dwMinimumCommandLength = 2; // Valid command is from 2 params
        public const string strUnknownCommand = "Invalid Command";

        public const string XTOK = "XTOK";
        public const string XTNG = "XTNG";
        public const string XTLK = "XTLK";
        public const string XTPI = "XTPI";
        public const string HEBOK = "HEBOK";
        public const string HEBNG = "HEBNG";
        public const string HEEOK = "HEEOK";
        public const string HEENG = "HEENG";
        public const string HEOK = "HEOK";
        public const string HENG = "HENG";
        public const string HXBOK = "HXBOK";
        public const string HXOK = "HXOK";
        public const string HXNG = "HXNG";
        public const string HXBNG = "HXBNG";
        public const string HXEOK = "HXEOK";
        public const string HXENG = "HXENG";
        public const string TTNG = "TTNG";
        public const string TTOK = "TTOK";
        public const string TTRNG = "TTRNG";
        public const string TTROK = "TTROK";
        public const string ISPNG = "ISPNG";
        public const string ISPOK = "ISPOK";

        public const string strHEB = "HEB";
        public const string strHE = "HE";
        public const string strHEE = "HEE";
        public const string strHEB3D = "HEEB3D";
        public const string strHE3D = "HE3D";
        public const string strHEE3D = "HEE3D";
        public const string strXT3D = "XT3D";
        public const string strTT3D = "TT3D";
        public const string strXT = "XT";
        public const string strXTMM = "XTMM";
        public const string strXTMMP = "XTMMP";
        public const string strXTX = "XTX";
        public const string strTT = "TT";
        public const string strTTR = "TTR";
        public const string strXT2 = "XT2";
        public const string strXT2X = "XT2X";
        public const string strInputNG = "InputNG";
        public const string strISP = "ISP";

        public const string TRAINED_FILE_SETTING = "TrainedPoints.json";
        public const string TRAINED_TTM_FILE_SETTING = "TrainedPointsTTM.json";
        public const string CALIB_FILE_SETTING = "Calibs.json";
        public const string CALIB_CAM_MOVING_FILE_SETTING = "CalibsMoving.json";
        public const string JOB_FILE_SETTING = "Jobs.json";
        public const string APP_FILE_SETTING = "app.json";
        public const string PLCS_FILE_SETTING = "PLCs.json";
        public const string LIGHTS_FILE_SETTING = "Lights.json";
        public const string LIGHTS_JOB_FILE_SETTING = "LightJobs.json";
        public const string PLCCAM_FILE_SETTING = "PlcCameras.json";
        public const string SYSTEM_COMMAND_FILE_SETTING = "SystemCommand.json";
        public const string SYSTEM_COMMAND_FILE_SETTING2 = "SystemCommand2.json";
        public const string PLC_SETTING_FILE = "plcsetting.dat";
        public const string PLC_CAMERA_SETTING_FILE = "plccamerasetting.dat";
        public const string PLC_MANAGER_SETTING_FILE = "plcmanagersetting.dat";
        public const string LAYOUT_SETTING_FILE = "layoutsetting.dat";
        public const string RESULT_FILE = "result.dat";
        public const string IO_CONFIG_FILE_SETTING = "io.json";
        public const string IO_JOBS_CONFIG_FILE_SETTING = "ioJobs.json";
        public const string CPK_CONFIG_FILE_SETTING = "CpkConfig.json";

        public const string VISION_MASTER_POSES_FILE_SETTING = "VisionMasterPoses.json";

        public const string VISA_ADDRESS = "TCPIP0::localhost::hislip0::INSTR";

        public static readonly String[] arrSupportedComamnds = { strHEB, strHE, strHEE, strXT, strXTX, strXT2, strXT2X, strTT, strTTR, strISP,strXTMM ,strXTMMP };
    }
}
