using BeeLightModule;
using BeeLightModule.Models;
using BeevisionSolution.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Models
{
    public class LightJob
    {
        private LightEthernetControlBase _lightController;
        private int _controllerId;
        public int ControllerID
        {
            get => _controllerId;
            set
            {
                _controllerId = value;
                _lightController = JobController.GetLightByID(_controllerId);
            }
        }
        public int LJobID { get; set; }
        public string LJobName { get; set; }
        public int Channel { get; set; }
        public int Intensity { get; set; }
        public int DelayBeforeTakeImage { get; set; }
        public int DelayAfterTakeImage { get; set; }
        public bool TurnOn()
        {
            if (_lightController != null)
            {
                return _lightController.LightOn(Channel, Intensity);
            }
            return false;
        }

        public bool TurnOff()
        {
            if (_lightController != null)
            {
                return _lightController.LightOff(Channel);
            }
            return false;
        }
    }
}
