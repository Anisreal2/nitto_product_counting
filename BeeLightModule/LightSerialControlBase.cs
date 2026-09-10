using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeLightModule
{
    [Serializable]
    public abstract class LightSerialControlBase : ILightSerialControl, IDisposable
    {
        protected const int MAX_BUF = 1024;

        [NonSerialized]
        protected SerialPort srPort;

        protected string PartsName
        {
            get;
            set;
        }

        public byte[] mRead
        {
            get;
            set;
        }

        public byte[] mWrite
        {
            get;
            set;
        }

        public bool IsOpenedIF
        {
            get;
            set;
        }

        public string PortName
        {
            get;
            set;
        }

        public int MaxChannel
        {
            get;
            set;
        }

        public int MaxVolume
        {
            get;
            set;
        }

        public int BaudRate
        {
            get;
            set;
        }

        public Parity pParity
        {
            get;
            set;
        }

        public int DataBits
        {
            get;
            set;
        }

        public StopBits sStopBits
        {
            get;
            set;
        }

        public string StartStringSingle
        {
            get;
            set;
        }

        public string StartStringMulti
        {
            get;
            set;
        }

        public string EndString
        {
            get;
            set;
        }

        public int Initialize()
        {
            try
            {
                srPort = new SerialPort(PortName, BaudRate, pParity, DataBits, sStopBits);
            }
            catch (Exception)
            {
                return 0;
            }
            return 1;
        }

        public LightSerialControlBase(string _portName, int _baudRate, Parity _parity, int _dataBits, StopBits _stopBits)
        {
            mRead = new byte[1024];
            mWrite = new byte[1024];
            IsOpenedIF = false;
            PortName = _portName;
            BaudRate = _baudRate;
            pParity = _parity;
            DataBits = _dataBits;
            sStopBits = _stopBits;
        }

        public int CloseIF()
        {
            try
            {
                srPort.Close();
                IsOpenedIF = false;
            }
            catch (Exception)
            {
                IsOpenedIF = false;
                return 0;
            }
            if (!IsOpenedIF)
            {
                return 1;
            }
            return 0;
        }

        public int OpenIF()
        {
            try
            {
                srPort = new SerialPort(PortName, BaudRate, pParity, DataBits);
                srPort.Open();
                IsOpenedIF = srPort.IsOpen;
            }
            catch (Exception)
            {
                return 0;
            }
            if (IsOpenedIF)
            {
                return 1;
            }
            return 0;
        }

        public int SetController(string _portName, int _baudRate, Parity _parity, int _dataBits, StopBits _stopBits)
        {
            mRead = new byte[1024];
            mWrite = new byte[1024];
            IsOpenedIF = false;
            PortName = _portName;
            BaudRate = _baudRate;
            pParity = _parity;
            DataBits = _dataBits;
            sStopBits = _stopBits;
            return 1;
        }

        public abstract int LightOff(Channel _chNo);

        public abstract int LightOn(Channel _chNo, int _val);

        public void Dispose()
        {
            if (IsOpenedIF)
            {
                CloseIF();
            }
        }
    }
}
