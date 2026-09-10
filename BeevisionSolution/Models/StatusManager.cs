using BeevisionSolution.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Models
{
    public class StatusManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ApplicationStatus _applicationStatus = ApplicationStatus.Manual;
        public ApplicationStatus ApplicationStatus
        {
            get { return _applicationStatus; }
            set
            {
                if (_applicationStatus != value)
                {
                    _applicationStatus = value;
                    Notify();
                    Notify(nameof(StatusText));
                }
            }
        }

        public string StatusText
        {
            get
            {
                switch (_applicationStatus)
                {
                    case ApplicationStatus.AutoRunning:
                        return Common.Settings.CurrentLanguage == (int)Lang.en ?  "AUTO RUNNING" : "CHẠY TỰ ĐỘNG";
                    case ApplicationStatus.Manual:
                        return Common.Settings.CurrentLanguage == (int)Lang.en ? "VISION MANUAL": "CHẠY THỦ CÔNG";
                    case ApplicationStatus.CameraLive:
                        return Common.Settings.CurrentLanguage == (int)Lang.en ? "CAMERA LIVE" : "CAMERA TRỰC TUYẾN";
                    default:
                        return "UNKNOWN";
                }
            }
        }

        private PLCStatus _plcStatus = PLCStatus.Offline;
        public PLCStatus PlcStatus
        {
            get { return _plcStatus; }
            set
            {
                if (_plcStatus != value)
                {
                    _plcStatus = value;
                    Notify();
                    Notify(nameof(PlcStatusText));
                }
            }
        }
        public void RefreshDependentTexts()
        {
            Notify(nameof(StatusText));
            Notify(nameof(PlcStatusText));
            Notify(nameof(PcSurvivalText));
        }
        public string PlcStatusText
        {
            get
            {
                bool isOnline = _plcStatus == PLCStatus.Online;
                bool isEnglish = Common.Settings.CurrentLanguage == (int)Lang.en;

                if (isOnline)
                    return isEnglish ? "ETHERCAT OP" : "ETHERCAT OP";
                else
                    return isEnglish ? "ETHERCAT NOT OP" : "ETHERCAT NOT OP";
            }
        }

        private string _plcModelNo = "-";
        public string PlcModelNo
        {
            get => _plcModelNo;
            set { _plcModelNo = value; Notify(); }
        }


        private string _model = "-";
        public string Model
        {
            get => _model;
            set { _model = value; Notify(); }
        }

        private string _product = "-";
        public string Product
        {
            get => _product;
            set { _product = value; Notify(); }
        }

        private int _sendDataA = 0;
        public int SendDataA
        {
            get => _sendDataA;
            set { _sendDataA = value; Notify(); }
        }

        private int _sendDataB = 0;
        public int SendDataB
        {
            get => _sendDataB;
            set { _sendDataB = value; Notify(); }
        }

        private double _tactTimeA = 0.0;
        public double TactTimeA
        {
            get => _tactTimeA;
            set { _tactTimeA = value; Notify(); }
        }

        private double _tactTimeB = 0.0;
        public double TactTimeB
        {
            get => _tactTimeB;
            set { _tactTimeB = value; Notify(); }
        }
        private string _currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string CurrentDateTime
        {
            get => _currentDateTime;
            set { _currentDateTime = value; Notify(); }
        }

        private bool _pcSurvival = false;
        public bool PcSurvival
        {
            get => _pcSurvival;
            set
            {
                if (_pcSurvival != value)
                {
                    _pcSurvival = value;
                    Notify();
                    Notify(nameof(PcSurvivalText));
                }
            }
        }

        public string PcSurvivalText
        {
            get
            {
                bool isEnglish = Common.Settings.CurrentLanguage == (int)Lang.en;
                return _pcSurvival 
                    ? (isEnglish ? "PC SURVIVAL" : "PC SURVIVAL")
                    : (isEnglish ? "PC SURVIVAL" : "PC SURVIVAL");
            }
        }
    }
}
