using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace BeeLightModule.Models
{
    public class OPT_DPA6024E_2 : LightEthernetControlBase
    {
        IntPtr handler = IntPtr.Zero;
        enum TriggerDelayUnit
        {
            us1,
            us10,
            ms1
        }
        enum TriggerWidthUnit
        {
            us1,
            us10,
            ms1,
            ms100
        }

        public OPT_DPA6024E_2()
        {
            Initialize();
        }
        public override bool LightOffAll()
        {
            return LightOff(0);
        }
        public override bool Connect()
        {
            long ret = 1;
            if (!string.IsNullOrEmpty(this.SN))
            {
                ret = OPTControllerAPI.OPTController_CreateEthernetConnectionBySN(SN, out handler);
                if (ret != 0)//fail to connect by SN, try to connect by IP if available
                {
                    if (!string.IsNullOrEmpty(this.IP))
                    {
                        ret = OPTControllerAPI.OPTController_CreateEthernetConnectionByIP(IP, out handler);
                    }
                }
            }
            //if (ret == 0)
            //{
            //    //set trigger time unit
            //    var triggerUnit = TriggerWidthUnit.ms100;
            //    OPTControllerAPI.OPTController_SetTimeUnit(handler, 0, (int)triggerUnit);
            //    //set delay trigger time unit
            //    var triggerDelayUnit = TriggerDelayUnit.ms1;
            //    OPTControllerAPI.OPTController_SetTriggerDelayUnit(handler, 0, (int)triggerDelayUnit);
            //}
            return ret == 0;
        }

        public override bool Disconnect()
        {
            var ret = OPTControllerAPI.OPTController_DestroyEthernetConnection(handler);
            return ret == 0;
        }

        public override int Initialize()
        {
            this.NumOfChannels = 2;
            this.ModelName = "OPT_DPA6024E_2";

            return 0;
        }

        public override bool IsConnected()
        {
            var ret = OPTControllerAPI.OPTController_IsConnect(handler);
            return ret == 0;
        }

        public override bool LightOff(int channel)
        {
            if (!CheckValidChannel(channel))
            {
                return false;
            }
            var ret = OPTControllerAPI.OPTController_TurnOffChannel(handler, channel);
            if(ret != 0)
            {
                //retry init the connection and do it again
                Connect();
                ret = OPTControllerAPI.OPTController_TurnOffChannel(handler, channel);
            }
            return ret == 0;
        }

        public override bool LightOn(int channel)
        {
            if (!CheckValidChannel(channel))
            {
                return false;
            }
            var ret = OPTControllerAPI.OPTController_TurnOnChannel(handler, channel);
            if (ret != 0)
            {
                //retry init the connection and do it again
                Connect();
                ret = OPTControllerAPI.OPTController_TurnOnChannel(handler, channel);
            }
            return ret == 0;
        }
        public override bool LightOn(int channel, int _val)
        {
            if (!CheckValidChannel(channel))
            {
                return false;
            }
            OPTControllerAPI.OPTController_SetIntensity(handler, channel, _val);
            var ret = OPTControllerAPI.OPTController_TurnOnChannel(handler, channel);
            if (ret != 0)
            {
                //retry init the connection and do it again
                Connect();
                OPTControllerAPI.OPTController_SetIntensity(handler, channel, _val);
                ret = OPTControllerAPI.OPTController_TurnOnChannel(handler, channel);
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
            var ret = OPTControllerAPI.OPTController_ReadIntensity(handler, channel, out val);
            if (ret != 0)
            {
                //retry init the connection and do it again
                Connect();
                ret = OPTControllerAPI.OPTController_ReadIntensity(handler, channel, out val);
            }
            return ret == 0;
        }

        public override bool SetIntensity(int channel, int val)
        {
            if (!CheckValidChannel(channel) || !CheckValidValue(val))
            {
                return false;
            }
            var ret = OPTControllerAPI.OPTController_SetIntensity(handler, channel, val);
            if (ret != 0)
            {
                //retry init the connection and do it again
                Connect();
                ret = OPTControllerAPI.OPTController_SetIntensity(handler, channel, val);
            }
            return ret == 0;
        }

    }
}
