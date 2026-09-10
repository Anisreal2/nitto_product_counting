using BeeMotionModule.Models;
using BeevisionSolution.Utils;
using Cognex.VisionPro;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using static BeevisionSolution.Utils.Common;
using static BeevisionSolution.Utils.Constant;

namespace BeevisionSolution.Models
{
    class AppSettings
    {
        public static readonly AppSettings Default = new AppSettings()
        {
            AppTitle = strDefaultTitle,
            AppName = strDefaultAppName,
            CurrentProfile = strDefaultProfile,
            CurrentPort = strDefaultComPort,
            Watermarks = new List<string> { "1. Camera 1", "2. Camera 2"},
            LogViewHeader = new List<string> { "LogView 1", "LogView 2" },
            AllowMelsecScan = false,
            EnableLogging = true,
            SaveImageOK = true,
            SaveImageNG = true,
            OverlayLineWidth = 8,
            SavingImageFormat = ImageFormat.Png,
            ImageFactorOK = 1.0f, /* Scale factor, 1.0 means image is saving without any scale (original size) */
            ImageFactorNG = 1.0f, /* Scale factor, 1.0 means image is saving without any scale (original size) */
            ShowHandEyeGraphic = true,
            GraphicSize = 35,
            AutoCleanUp = false,
            CrosshairColor = CogColorConstants.Blue,
            ScreeningTimeout = 30,
            DisplaySizes = new List<double> { 1.4, 1 }, /* 1st row of screen with 1.4* in height, 2nd row height is 1*, means 1st row 1.4 times bigger than 2nd row */
            DisplayList = new List<int> { 1, 1 },       /* 2 Rows, 1st row have 2 screens, 2nd have 2 screen */
            RootDirectory = BaseStorageDirectory,
            DaysInHistory = 15,
            AutoLock = true,
            MelsecScanningTime = 200,
            SavingImageDirectory = null,
            RootFolder = null,
            LineNo = "",
            UnitName = "",
            ProcessName = "",
            DirectionName = "",
            LogoPath = "",
            DatamanImagePath = "",
            NoOfSavingThread = 1,
            SaveImageQueueMaxCapacity = 80,
            Handeye = new HeSettings(),
            IsVidiRequired = false,
            IsBumjin = false,
            IsDoMotion = false,
            MotionConfig = new MotionConfig(),
            IsOnline = false,
            EnableIOController = false,
            EnableIRaypleEarlyProbe = false,
            EnableWatcherAndIOInit = false,
        };
        public HeSettings Handeye { get; internal set; }
        private String _RootDirectory;
        private int _DaysInHistory;
        private int _MelsecScanningTime;
        private int _ScreeningTimeout;
        private double _DriveSpaceThreshold = 80;
        public int NoOfSavingThread { get; internal set; }
        /// <summary>Max items buffered for async disk save (plain/overlay). Excess producers wait (back-pressure). Clamped 10–500 at runtime.</summary>
        public int SaveImageQueueMaxCapacity { get; internal set; } = 80;
        public string AppName { get; internal set; }
        public string CurrentProfile { get; internal set; }
        public string EquipmentID { get; internal set; } = "CNT_ASSEMBLY_01";
        public string LogoPath { get; internal set; }
        public string DatamanImagePath {  get; internal set; }
        public bool EnableLogo { get; set; } = true;
        public bool AllowMelsecScan { get; internal set; }
        public bool IsLightControl { get; internal set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public CogColorConstants CrosshairColor { get; set; } = CogColorConstants.Blue;
        public string LoggingFormat { get; internal set; } = "%date{yyyy-MM-dd HH:mm:ss.fff},\"%level\",\"%message\"%newline";
        public string CsvFormat { get; internal set; } = "%date{yyyy-MM-dd HH:mm:ss.fff},\"%message\"%newline";
        public bool IsVidiRequired { get; internal set; } = false;
        public bool IsBumjin { get; internal set; } = false;
        public bool IsDoMotion { get; internal set; } = false;
        [JsonIgnore]
        public MotionConfig MotionConfig { get; set; } = new MotionConfig();
        public bool IsOnline { get; internal set; } = false;
        public int ModeNpoint { get; internal set; } = 1;
        public int MelsecScanningTime
        {
            get => (_MelsecScanningTime > 0) && (_MelsecScanningTime < 1000) ? _MelsecScanningTime : 100;
            set => _MelsecScanningTime = value;
        }

        [JsonIgnore]
        public int MelsecDelayTime => (int)(MelsecScanningTime / 2.0f);

        public string RootDirectory
        {
            get => (String.IsNullOrEmpty(_RootDirectory) || !Directory.Exists(_RootDirectory)) ? BaseStorageDirectory : _RootDirectory;
            internal set => _RootDirectory = value;
        }
        public String SavingImageDirectory { get; internal set; }
        public String RootFolder { get; internal set; }
        [JsonIgnore]
        public String LoggingDirectory => String.IsNullOrEmpty(SavingImageDirectory) ? LogsFolder : String.Format(@"{0}\{1}", SavingImageDirectory, CurrentProfile);
        public bool EnableLogging { get; internal set; }
        [JsonIgnore]
        public bool SaveImage => (SaveImageOK || SaveImageNG);
        public bool SaveImageOK { get; internal set; } = true;
        public bool SaveImageNG { get; internal set; } = true;

        [JsonIgnore]
        public bool SaveOverlayImage => (SaveOverlayImageOK || SaveOverlayImageNG);
        public bool SaveOverlayImageOK { get; internal set; } = true;
        public bool SaveOverlayImageNG { get; internal set; } = true;
        private int _overlayLineWidth = 8;
        public int OverlayLineWidth
        {
            get => Math.Max(1, Math.Min(20, _overlayLineWidth));
            internal set => _overlayLineWidth = Math.Max(1, Math.Min(20, value));
        }

        public bool AutoCleanUp { get; internal set; } = false;
        public bool ShowHandEyeGraphic { get; internal set; } = true;
        public bool Simulate { get; internal set; } = false;
        public ImageFormat SavingImageFormat { get; internal set; } = ImageFormat.Png;
        public float ImageFactorOK { get; internal set; } = 1.0f;
        public float ImageFactorNG { get; internal set; } = 1.0f;

        public int TriggerGap { get; internal set; } = 500;
        public int GraphicSize { get; set; } = 35;
        [JsonIgnore]
        public bool AutoLock { get; set; } = true;
        public int ScreeningTimeout
        {
            get => ((_ScreeningTimeout > 0) || (_ScreeningTimeout <= 60)) ? _ScreeningTimeout : 15;
            set => _ScreeningTimeout = value;
        }
        public int DaysInHistory
        {
            get => _DaysInHistory < 0 ? 0 : _DaysInHistory;
            set => _DaysInHistory = value;
        }

        public double DriveSpaceThreshold
        {
            get
            {
                if (_DriveSpaceThreshold < 10) return 10;
                if (_DriveSpaceThreshold > 99) return 99;
                return _DriveSpaceThreshold;
            }
            internal set => _DriveSpaceThreshold = value;
        }
        public List<double> DisplaySizes { get; internal set; }
        public List<int> DisplayList { get; internal set; }
        public List<string> Watermarks { get; internal set; }
        public List<string> LogViewHeader { get; internal set; }
        public string AppTitle { get; internal set; }
        public string LineNo { get; internal set; }
        public string UnitName { get; internal set; }
        public string ProcessName { get; internal set; }
        public string DirectionName { get; internal set; }
        public string CurrentPort { get; internal set; }
        public int AutoBackupTimeHours { get; set; }// Chỉ định thời gian backup tự động trên drive D
        public int SizeLimit { get; set; } // Kích thước đối với giá trị hiệu chỉnh sau khi kiểm tra thị giác (mm)
        public double MotionLimitX { get; set; } = 10; // Giới hạn chuyển động trục X
        public double MotionLimitY { get; set; } = 10; // Giới hạn chuyển động trục Y
        public double MotionLimitTheta { get; set; } = 5; // Giới hạn chuyển động trục Theta
        /// <summary>Bật/tắt khu vực Align History + Result Graph trên ImageView.</summary>
        public bool EnableResultAndGraphView { get; set; } = false;
        /// <summary>Số hàng tối đa Align History + graph (chỉ bộ nhớ phiên; có lưu trong profile settings).</summary>
        public int AlignHistoryMaxRows { get; set; } = 50;
        /// <summary>Thời gian chờ (ms) gộp nhiều IspJob cùng nhóm trước khi bỏ batch không đủ job.</summary>
        public int InspectionHistorySyncTimeoutMs { get; set; } = 2000;
        public bool SaveScreenshotImage { get; set; } // lưu ảnh chụp màn hình
        public bool ModifyScoreLimit { get; set; } // Cho phép hiệu chỉnh ngưỡng điểm
        public double ScoreLimit { get; set; } // Ngưỡng điểm
        public bool UseRetryMode { get; set; } // Sử dụng chế độ thử lại
        public double RetryX { get; set; } // Khoảng cách di chuyển lại trục X
        public double RetryY { get; set; } // Khoảng cách di chuyển lại trục Y
        public double RetryTheta { get; set; } // Khoảng cách di chuyển lại trục Theta
        public bool AutoModelChange { get; set; } // Tự động thay đổi nếu số model không giống nhau sau khi so sánh với số model PLC .
        public int MaxRetries { get; set; } = 2;// Số lần retry tối đa cho MultiMark (tổng cộng 3 lần thử: lần đầu + 2 retry)

        private double? _crossLineScale;

        // Scale riêng cho vạch thước Cross Line.
        public double CrossLineScale
        {
            get
            {
                if (_crossLineScale.HasValue && _crossLineScale.Value > 0)
                {
                    return _crossLineScale.Value;
                }

                if (MeasureDistanceScale > 0)
                {
                    return MeasureDistanceScale;
                }

                return 1.0;
            }
            set => _crossLineScale = value;
        }

        // Chỉ giữ để đọc app.json cũ. Giá trị mới được lưu bằng CrossLineScale.
        public double MeasureDistanceScale { get; set; } = 1.0;
        public bool ShouldSerializeMeasureDistanceScale() => false;

        public bool ShowCenterLine { get; set; } // Line center được biểu thị màu đỏ với point oirigin trên màn hình Main

        /// <summary>Hiển thị nút CPK Calculator trên thanh điều khiển ImageView (mặc định: bật).</summary>
        [DefaultValue(true)]
        public bool ShowCpkCalculatorButton { get; set; } = true;
        public double InspectDelay { get; set; } // Trường này chỉ ra độ trễ trong thời gian tạo ảnh của chương trình thị giác (sau khi nhận tín hiệu tạo ảnh từ PLC).
        public int CurrentLanguage { get; set; } = (int)Lang.en; // Ngôn ngữ hiện tại của ứng dụng
        public int SystemStatus { get; set; } = (int)ApplicationStatus.AutoRunning; // Trạng thái hệ thống
        public bool EnableAutoZipBackup { get; set; } = false; // Bật/tắt tính năng tự động tạo zip backup
        public int ZipBackupIntervalDays { get; set; } = 2; // Khoảng thời gian (ngày) để tạo zip backup một lần
        public bool UseOpCallForMultiMark { get; set; } = true; // Sử dụng OpCall window khi MultiMark fail
        public string OptDirectory { get; set; } = null;
        public bool EnableIOController { get; internal set; } = false;

        /// <summary>
        /// When true, IRayple SDK is enumerated and pre-opened at app startup (before Pylon/Cognex load).
        /// Configure in app.json only; default is false (disabled).
        /// </summary>
        public bool EnableIRaypleEarlyProbe { get; internal set; } = false;

        /// <summary>
        /// When true, WatcherInit and InitIOController run at startup/reload.
        /// Configure in app.json only; default is false (disabled).
        /// EnableIOController still controls GPIO thread inside InitIOController when this is true.
        /// </summary>
        public bool EnableWatcherAndIOInit { get; internal set; } = false;

        public DateTime? LastZipBackupTime { get; set; } // Thời gian lần cuối tạo zip backup

        // Calibration dialog - persist last input values
        public double CalibDialogXOffset { get; set; } = 5.0;
        public double CalibDialogYOffset { get; set; } = 5.0;
        public double CalibDialogWOffset { get; set; } = 1.0;
        public bool CalibDialogUseStepMode2 { get; set; } = false;



        public AppSettings() { }
        public bool Save() => SaveObjectToFile(this, AppConfigFile);
    }
    
    class HeSettings
    {
        public int GridSize { get; internal set; } = 3;// no of steps without angle will be 3*3=9
        public int RotationSteps { get; internal set; } = 2;//Number of steps which has angle
    }
}
