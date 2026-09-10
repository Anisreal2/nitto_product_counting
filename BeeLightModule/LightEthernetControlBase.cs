using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace BeeLightModule
{
    [Serializable]
    public abstract class LightEthernetControlBase : IDisposable
    {
        public bool IsActive { get; set; }
        public int Port { get; set; }
        public string IP { get; set; }
        public string ModelName { get; set; }
        public int IdControl { get; set; }
        public int NumOfChannels { get; set; }
        public string SN { get; set; }
        public int TriggerWidth { get; set; }
        public int DelayTriggerWidth { get; set; }
        public abstract bool Connect();

        public abstract bool Disconnect();


        public void Dispose()
        {
            if (IsConnected())
            {
                Disconnect();
            }
        }
        public abstract int Initialize();

        public abstract bool IsConnected();

        public abstract bool LightOff(int _chNo);
        public abstract bool LightOn(int _chNo);
        public abstract bool LightOn(int _chNo, int _val);

        public abstract bool SetIntensity(int channel, int val);
        public abstract bool ReadIntensity(int channel, out int val);

        public abstract bool LightOffAll();

        protected bool CheckValidChannel(int channel)
        {
            if (channel > this.NumOfChannels || channel < 0)
            {
                return false;
            }
            return true;
        }

        protected bool CheckValidValue(int val)
        {
            if (val > 255 || val < 1)
            {
                return false;
            }
            return true;
        }
    }
}
