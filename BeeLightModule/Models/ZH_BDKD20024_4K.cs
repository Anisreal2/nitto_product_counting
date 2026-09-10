using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace BeeLightModule.Models
{
    public class ZH_BDKD20024_4K : LightEthernetControlBase
    {
        public ZH_BDKD20024_4K()
        {
            Initialize();
        }
        public override bool Connect()
        {
            long lRet = -1;
            lRet = HZControllerApi.ThreeDPC_CustomLTSController_InitNetwork(IP, Port);
            return lRet == 0;
        }

        public override bool Disconnect()
        {
            long lRet = HZControllerApi.ThreeDPC_CustomLTSController_InitNetwork(IP, Port);
            return lRet == 0;
        }

        public override int Initialize()
        {
            this.NumOfChannels = 4;
            this.ModelName = "ZH_BDKD20024_4K";

            return 0;
        }

        public override bool LightOffAll()
        {
            return LightOff(0);
        }
        public override bool IsConnected()
        {
            //HZ not implemented this function, so we will workaround by reading indensity
            var intensity = HZControllerApi.ThreeDPC_CustomLTSController_ReadIntensity(0);
            return intensity != -1;
        }

        public override bool LightOff(int _chNo)
        {
            if (!CheckValidChannel(_chNo))
            {
                return false;
            }
            var ret = HZControllerApi.ThreeDPC_CustomLTSController_SetChannelState(_chNo, 0);
            if (ret != 0)
            {
                //retry init the connection and do it again
                Connect();
                ret = HZControllerApi.ThreeDPC_CustomLTSController_SetChannelState(_chNo, 0);
            }
            return ret == 0;
        }

        public override bool LightOn(int _chNo)
        {
            if (!CheckValidChannel(_chNo))
            {
                return false;
            }
            var ret = HZControllerApi.ThreeDPC_CustomLTSController_SetChannelState(_chNo, 1);
            if (ret != 0)
            {
                //retry init the connection and do it again
                Connect();
                ret = HZControllerApi.ThreeDPC_CustomLTSController_SetChannelState(_chNo, 1);
            }
            return ret == 0;
        }

        public override bool LightOn(int _chNo, int _val)
        {
            if (!CheckValidChannel(_chNo))
            {
                return false;
            }
            HZControllerApi.ThreeDPC_CustomLTSController_SetIntensity(_chNo, _val);
            var ret = HZControllerApi.ThreeDPC_CustomLTSController_SetChannelState(_chNo, 1);
            if (ret != 0)
            {
                //retry init the connection and do it again
                Connect();
                HZControllerApi.ThreeDPC_CustomLTSController_SetIntensity(_chNo, _val);
                ret = HZControllerApi.ThreeDPC_CustomLTSController_SetChannelState(_chNo, 1);
            }
            return ret == 0;
        }

        public override bool ReadIntensity(int channel, out int val)
        {
            if (!CheckValidChannel(channel))
            {
                val = -1;
                return false;
            }
            val = HZControllerApi.ThreeDPC_CustomLTSController_ReadIntensity(channel);
            if (val == -1)
            {
                //retry init the connection and do it again
                Connect();
                val = HZControllerApi.ThreeDPC_CustomLTSController_ReadIntensity(channel);
            }
            return val != -1;
        }

        public override bool SetIntensity(int channel, int val)
        {
            if (!CheckValidChannel(channel) || !CheckValidValue(val))
            {
                return false;
            }
            var ret = HZControllerApi.ThreeDPC_CustomLTSController_SetIntensity(channel, val);
            if (ret != 0)
            {
                //retry init the connection and do it again
                Connect();
                ret = HZControllerApi.ThreeDPC_CustomLTSController_SetIntensity(channel, val);
            }
            return ret == 0;
        }
    }
}
