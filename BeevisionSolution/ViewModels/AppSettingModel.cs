using BeevisionSolution.Utils;
using Cognex.VisionPro;
using System;
using System.Linq;
using static BeevisionSolution.Utils.Common;

namespace BeevisionSolution.ViewModels
{
    class AppSettingModel : PropertyChangedAbstract
    {
        public static event Action SettingUpdated;

        public string AppName
        {
            get => Settings.AppName;
            set
            {
                Settings.AppName = value;
                Settings.Save();
                Notify();
            }
        }

        public string AppTitle
        {
            get => Settings.AppTitle;
            set
            {
                Settings.AppTitle = value;
                Settings.Save();
                Notify();
            }
        }
        public string LineNo
        {
            get => Settings.LineNo;
            set
            {
                Settings.LineNo = value;
                Settings.Save();
                Notify();
            }
        }
        public string UnitName
        {
            get => Settings.UnitName;
            set
            {
                Settings.UnitName = value;
                Settings.Save();
                Notify();
            }
        }
        public string ProcessName
        {
            get => Settings.ProcessName;
            set
            {
                Settings.ProcessName = value;
                Settings.Save();
                Notify();
            }
        }
        public string DirectionName
        {
            get => Settings.DirectionName;
            set
            {
                Settings.DirectionName = value;
                Settings.Save();
                Notify();
            }
        }
        public String SelectedProfile
        {
            get => Settings.CurrentProfile;
            set
            {
                Settings.CurrentProfile = value;
                Settings.Save();
                Notify();
            }
        }

        public bool EnableLogging
        {
            get => Settings.EnableLogging;
            set
            {
                Settings.EnableLogging = value;
                Settings.Save();
                if (value) StartAllLogger();
                else StopAllLogger();
                Notify();
            }
        }

        public bool EnableSaveImageOK
        {
            get => Settings.SaveImageOK;
            set
            {
                Settings.SaveImageOK = value;
                Settings.Save();
                Notify();
            }
        }

        public bool EnableSaveImageNG
        {
            get => Settings.SaveImageNG;
            set
            {
                Settings.SaveImageNG = value;
                Settings.Save();
                Notify();
            }
        }

        public bool SaveOverlayImageOK
        {
            get => Settings.SaveOverlayImageOK;
            set
            {
                Settings.SaveOverlayImageOK = value;
                Settings.Save();
                Notify();
            }
        }

        public bool SaveOverlayImageNG
        {
            get => Settings.SaveOverlayImageNG;
            set
            {
                Settings.SaveOverlayImageNG = value;
                Settings.Save();
                Notify();
            }
        }

        public int OverlayLineWidth
        {
            get => Settings.OverlayLineWidth;
            set
            {
                Settings.OverlayLineWidth = value;
                Settings.Save();
                Notify();
            }
        }

        public bool AutoCleanUp
        {
            get => Settings.AutoCleanUp;
            set
            {
                Settings.AutoCleanUp = value;
                Settings.Save();
                Notify();
            }
        }

        public double DriveSpaceThreshold
        {
            get => Settings.DriveSpaceThreshold;
            set
            {
                Settings.DriveSpaceThreshold = value;
                Settings.Save();
                Notify();
            }
        }

        public bool AllowMelsecScanning
        {
            get => Settings.AllowMelsecScan;
            set
            {
                Settings.AllowMelsecScan = value;
                Settings.Save();
                Notify();
            }
        }

        public bool ShowHandEyeGraphic
        {
            get => Settings.ShowHandEyeGraphic;
            set
            {
                Settings.ShowHandEyeGraphic = value;
                Settings.Save();
                Notify();
            }
        }

        public float ImageFactorOK
        {
            get => Settings.ImageFactorOK;
            set
            {
                Settings.ImageFactorOK = value;
                Settings.Save();
                Notify();
            }
        }

        public float ImageFactorNG
        {
            get => Settings.ImageFactorNG;
            set
            {
                Settings.ImageFactorNG = value;
                Settings.Save();
                Notify();
            }
        }

        public int DaysInHistory
        {
            get => Settings.DaysInHistory;
            set
            {
                Settings.DaysInHistory = value;
                Settings.Save();
                Notify();
            }
        }

        public int ScreeningTimeout
        {
            get => Settings.ScreeningTimeout;
            set
            {
                Settings.ScreeningTimeout = value;
                Settings.Save();
                Notify();
            }
        }

        public int GraphicSize
        {
            get => Settings.GraphicSize;
            set
            {
                Settings.GraphicSize = value;
                Settings.Save();
                Notify();
            }
        }

        public bool AutoLock
        {
            get => Settings.AutoLock;
            set
            {
                Settings.AutoLock = value;
                Notify();
            }
        }

        public int ScanningTime
        {
            get => Settings.MelsecScanningTime;
            set
            {
                Settings.MelsecScanningTime = value;
                Settings.Save();
                Notify();
            }
        }

        public bool EnableLogo
        {
            get => Settings.EnableLogo;
            set
            {
                Settings.EnableLogo = value;
                SettingUpdated?.Invoke();
                Settings.Save();
                Notify();
            }
        }

        public string LogoPath
        {
            get => Settings.LogoPath;
            set
            {
                Settings.LogoPath = value;
                SettingUpdated?.Invoke();
                Settings.Save();
                Notify();
            }
        }

        public string DatamanImagePath
        {
            get => Settings.DatamanImagePath;
            set
            {
                Settings.DatamanImagePath = value;
                SettingUpdated?.Invoke();
                Settings.Save();
                Notify();
            }
        }

        public CogColorConstants CrosshairColor
        {
            get => Settings.CrosshairColor;
            set
            {
                Settings.CrosshairColor = value;
                Settings.Save();
                Notify();
            }
        }

        public string CurrentPort
        {
            get => Settings.CurrentPort;
            set
            {
                Settings.CurrentPort = value;
                Settings.Save();
                Notify();
            }
        }

        public bool IsOnline
        {
            get => Settings.IsOnline;
            set
            {
                Settings.IsOnline = value;
                Settings.Save();
                Notify();
            }
        }

        public Array CrosshairColors => Enum.GetValues(typeof(CogColorConstants)).Cast<CogColorConstants>().ToArray();
    }
}
