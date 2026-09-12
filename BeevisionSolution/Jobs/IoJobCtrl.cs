using BeevisionSolution.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeevisionSolution.Jobs
{
    public static class IoJobCtrl
    {
        static List<IpcIOControl> _lstIOCards;
        static List<IOJobs> _lstIoJobs;
        public static void LoadIoConfig()
        {
            _lstIOCards = Common.GetObjectFromFile<List<IpcIOControl>>(Common.IOConfigFile);
            if (_lstIOCards != null && _lstIOCards.Count > 0)
            {
                Common.Info("Loaded IO Config successfully, {0} Card(s) found!", _lstIOCards.Count);
            }
            else
            {
                Common.Info("Loaded IO Config failed, check file {0}", Common.IOConfigFile);
            }
        }
        public static void LoadIoJobs()
        {
            _lstIoJobs = Common.GetObjectFromFile<List<IOJobs>>(Common.IOJobsConfigFile);
            if (_lstIoJobs != null && _lstIoJobs.Count > 0)
            {
                Common.Info("Loaded IO Jobs successfully, {0} Job(s) found!", _lstIoJobs.Count);
            }
            else
            {
                Common.Info("Loaded IO Job failed, check file {0}", Common.IOJobsConfigFile);
            }
        }

        public static IOJobs GetJobByPin(int pin)
        {
            if (_lstIoJobs != null && _lstIoJobs.Count > 0)
            {
                return _lstIoJobs.FirstOrDefault(p => p.PinTrigger == pin);
            }
            return null;
        }

        private static IpcIOControl _defaultPcieCard;
        public static IpcIOControl GetIOcardCtrl()
        {
            if (_lstIOCards != null && _lstIOCards.Count > 0)
            {
                return _lstIOCards[0];
            }
            if (_defaultPcieCard == null)
            {
                _defaultPcieCard = new IpcIOControl
                {
                    Name = "PCIE-E2I12O16",
                    CardType = "PcieE2I12O16",
                    CardId = 0,
                    InputChannels = 12,
                    OutputChannels = 16
                };
            }
            return _defaultPcieCard;
        }

    }
}
