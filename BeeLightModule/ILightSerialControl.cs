using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace BeeLightModule
{
    public interface ILightSerialControl
    {
        bool IsOpenedIF
        {
            get;
            set;
        }

        string PortName
        {
            get;
            set;
        }

        int MaxChannel
        {
            get;
            set;
        }

        int MaxVolume
        {
            get;
            set;
        }

        int BaudRate
        {
            get;
            set;
        }

        Parity pParity
        {
            get;
            set;
        }

        int DataBits
        {
            get;
            set;
        }

        StopBits sStopBits
        {
            get;
            set;
        }

        string StartStringSingle
        {
            get;
            set;
        }

        string StartStringMulti
        {
            get;
            set;
        }

        string EndString
        {
            get;
            set;
        }

        int Initialize();
        int OpenIF();

        int CloseIF();

        int LightOn(Channel _chNo, int _val);

        int LightOff(Channel _chNo);

        int SetController(string _portName, int _baudRate, Parity _parity, int _dataBits, StopBits _stopBits);
    }
}
