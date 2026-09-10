using System;

namespace BeeMotionModule.Models
{
    /// <summary>
    /// Represents the real-time operational state of a motion axis.
    /// </summary>
    public class AxisState
    {
        public short AxisIndex { get; set; }
        public double ActualPosition { get; set; }        // In user units (e.g. mm)
        public double ActualPositionPulses { get; set; }  // Raw encoder pulses
        public double CommandPosition { get; set; }       // In user units (e.g. mm)
        public double ActualVelocity { get; set; }       // Units/s or Pulses/s
        public int RawStatus { get; set; }
        public ushort StatusWord { get; set; }
        public ushort ControlWord { get; set; }
        
        public bool IsServoOn { get; set; }
        public bool IsBusy { get; set; }
        public bool IsInPosition { get; set; }
        public bool IsHomed { get; set; }
        public bool IsError { get; set; }
        public uint ErrorCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public bool LimitPositive { get; set; }
        public bool LimitNegative { get; set; }
        public bool HomeSensor { get; set; }
        public bool EmergencyStop { get; set; }

        public override string ToString()
        {
            return $"Axis {AxisIndex}: Pos={ActualPosition:F3}, Servo={(IsServoOn ? "ON" : "OFF")}, Busy={IsBusy}, InPos={IsInPosition}, Err={IsError}";
        }
    }
}

