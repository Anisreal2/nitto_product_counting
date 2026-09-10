using ControllerCSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace BeeLightModule.Models
{
    public class ZH_DCT24200_2K : LightEthernetControlBase
    {
        private ControllerApi Con = new ControllerApi();
        public delegate int StreamSearchCB(string Controller_IP, IntPtr UserVal);

        int StreamCBCon(string Controller_IP, IntPtr UserVal)
        {
            return 0;
        }

        int StreamCBClose(string Controller_IP, IntPtr UserVal)
        {
            return 0;
        }

        public ZH_DCT24200_2K()
        {
            Initialize();
        }
        public override bool Connect()
        {
            long lRet = -1;
            Con.Controller_InitNetwork();

            string[] ArrayIp = new string[1];
            ArrayIp[0] = IP;
            lRet = Con.Controller_ConnectNetwork(ArrayIp, 8234, StreamCBCon, IntPtr.Zero);

            return lRet == 0;
        }

        public override bool Disconnect()
        {
            int lRet = Con.Controller_ReleaseNetwork(StreamCBClose);
            return lRet == 0;
        }

        public override int Initialize()
        {
            this.NumOfChannels = 2;
            this.ModelName = "ZH_DCT24200_2K";

            return 0;
        }

        public override bool IsConnected()
        {
            int intensity = Con.Controller_ReadIntensityLength(IP, 0);
            return intensity != -1;
        }

        public override bool LightOff(int _chNo)
        {
            var result = Con.Controller_TurnOffChannelLength(IP, _chNo, 255);
            return !string.IsNullOrEmpty(result);
        }

        public override bool LightOn(int _chNo)
        {
            var result = Con.Controller_TurnOnChannelLength(IP, _chNo, 255);
            return !string.IsNullOrEmpty(result);
        }

        public override bool LightOn(int _chNo, int _val)
        {
            var result = Con.Controller_TurnOnChannelLength(IP, _chNo, _val);
            return !string.IsNullOrEmpty(result);
        }

        public override bool ReadIntensity(int channel, out int val)
        {
            val = Con.Controller_ReadIntensityLength(IP, 0);
            return val != -1;
        }

        public override bool SetIntensity(int channel, int val)
        {
            var result = Con.Controller_SetIntensityLength(IP, channel, val);
            return !string.IsNullOrEmpty(result);
        }

        public override bool LightOffAll()
        {
            for (int i = 1; i <= this.NumOfChannels; i++)
            {
                var result = Con.Controller_TurnOffChannelLength(IP, i, 255);
            }
            return true;
        }
    }
}
