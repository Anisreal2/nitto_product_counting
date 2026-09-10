using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BeevisionSolution.Models
{
    public class ReaderContentModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private long _deviceId;
        private string _deviceName;
        private string _deviceType;
        private string _deviceSerial;
        private int _devicePort;
        private IPAddress _deviceIp;
        private IPAddress _deviceSubnet;
        private IPAddress _deviceDefault;
        private string _result;
        private StatusMode _status;
        private IsActive _isActiveDisplay;
        private IsActive _isActiveLog;
        private IsCompare _isCompare;
        private bool _isActiveDevice;

        public ReaderContentModel()
        {

        }

        public ReaderContentModel(long deviceId, string deviceName, string deviceType, string deviceSerial, int devicePort, IPAddress deviceIp, IPAddress deviceSubnet, IPAddress deviceDefault, string result, StatusMode status, IsActive isActive, IsCompare isCompare, IsActive isActiveLog, bool isActiveDevice)
        {
            _deviceId = deviceId;
            _deviceName = deviceName;
            _deviceType = deviceType;
            _deviceSerial = deviceSerial;
            _devicePort = devicePort;
            _deviceIp = deviceIp;
            _deviceSubnet = deviceSubnet;
            _deviceDefault = deviceDefault;
            _result = result;
            _status = status;
            _isActiveDisplay = isActive;
            _isCompare = isCompare;
            _isActiveLog = isActiveLog;
            _isActiveDevice = isActiveDevice;
        }

        public long DeviceId
        {
            get
            {
                return _deviceId;
            }
            set
            {
                if (_deviceId != value)
                {
                    _deviceId = value;
                    OnPropertyChanged(nameof(DeviceId));
                }
            }
        }

        public string DeviceName
        {
            get
            {
                return _deviceName;
            }
            set
            {
                if (_deviceName != value)
                {
                    _deviceName = value;
                    OnPropertyChanged(nameof(DeviceName));
                }
            }
        }

        public string DeviceType
        {
            get
            {
                return _deviceType;
            }
            set
            {
                if (_deviceType != value)
                {
                    _deviceType = value;
                    OnPropertyChanged(nameof(DeviceType));
                }
            }
        }

        public string DeviceSerial
        {
            get
            {
                return _deviceSerial;
            }
            set
            {
                if (_deviceSerial != value)
                {
                    _deviceSerial = value;
                    OnPropertyChanged(nameof(DeviceSerial));
                }
            }
        }

        public int DevicePort
        {
            get
            {
                return _devicePort;
            }
            set
            {
                if (_devicePort != value)
                {
                    _devicePort = value;
                    OnPropertyChanged(nameof(DevicePort));
                }
            }
        }

        public IPAddress DeviceIp
        {
            get
            {
                return _deviceIp;
            }
            set
            {
                if (_deviceIp != value)
                {
                    _deviceIp = value;
                    OnPropertyChanged(nameof(DeviceIp));
                }
            }
        }

        public string DeviceIpString
        {
            get
            {
                return DeviceIp.ToString();
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    IPAddress.Parse(value);
                }
            }
        }

        public IPAddress DeviceSubnet
        {
            get
            {
                return _deviceSubnet;
            }
            set
            {
                if (_deviceSubnet != value)
                {
                    _deviceSubnet = value;
                    OnPropertyChanged(nameof(DeviceSubnet));
                }
            }
        }

        public string DeviceSubnetString
        {
            get
            {
                return DeviceSubnet.ToString();
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    IPAddress.Parse(value);
                }
            }
        }

        public IPAddress DeviceDefault
        {
            get
            {
                return _deviceDefault;
            }
            set
            {
                if (_deviceDefault != value)
                {
                    _deviceDefault = value;
                    OnPropertyChanged(nameof(DeviceDefault));
                }
            }
        }

        public string DeviceDefaultString
        {
            get
            {
                return DeviceDefault.ToString();
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    IPAddress.Parse(value);
                }
            }
        }

        public string Result
        {
            get
            {
                return _result;
            }
            set
            {
                if (_result != value)
                {
                    _result = value;
                    OnPropertyChanged(nameof(Result));
                }
            }
        }

        [XmlIgnore]
        public StatusMode Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public IsActive IsActiveDisplay
        {
            get
            {
                return _isActiveDisplay;
            }
            set
            {
                if (_isActiveDisplay != value)
                {
                    _isActiveDisplay = value;
                    OnPropertyChanged(nameof(IsActiveDisplay));
                }
            }
        }

        public IsActive IsActiveLog
        {
            get
            {
                return _isActiveLog;
            }
            set
            {
                if (_isActiveLog != value)
                {
                    _isActiveLog = value;
                    OnPropertyChanged(nameof(IsActiveLog));
                }
            }
        }

        public IsCompare IsCompareResult
        {
            get
            {
                return _isCompare;
            }
            set
            {
                if (_isCompare != value)
                {
                    _isCompare = value;
                    OnPropertyChanged(nameof(IsCompareResult));
                }
            }
        }

        public bool IsActiveDevice
        {
            get
            {
                return _isActiveDevice;
            }
            set
            {
                if (_isActiveDevice != value)
                {
                    _isActiveDevice = value;
                    OnPropertyChanged(nameof(IsActiveDevice));
                }
            }
        }
    }

    public enum StatusMode
    {
        OK = 1,
        NG = 0
    }

    public enum IsActive
    {
        Active = 1,
        Inactive = 0
    }
    public enum IsCompare
    {
        Deny,
        Allow
    }
}
