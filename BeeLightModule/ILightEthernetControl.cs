using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeLightModule
{
    public interface ILightEthernetControl
    {
        int Port { get; set; }
        string IP { get; set; }
        int NumOfChannels { get; set; }
        string ModelName { get; set; }
        int IdControl { get; set; }
        int Initialize();
        bool IsConnected();

        bool Connect();
        bool Disconnect();

        int LightOn(int _chNo, int _val);
        int LightOn(int _chNo);
        int LightOff(int _chNo);

        bool SetIntensity(int channel, int value);
    }
}
