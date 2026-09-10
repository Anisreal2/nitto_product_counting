using System;

namespace BeeMotionModule.Models
{
    /// <summary>
    /// Motion profile parameters (Velocity, Acceleration, Deceleration, Jerk/S-Curve)
    /// </summary>
    public class MotionProfile
    {
        public double StartVelocity { get; set; } = 0;
        public double TargetVelocity { get; set; } = 5000;
        public double Acceleration { get; set; } = 500000;
        public double Deceleration { get; set; } = 500000;
        public double Jerk { get; set; } = 1000000; // S-Curve jerk
        public double SmoothStopDec { get; set; } = 1000000;
        public double EmergencyStopDec { get; set; } = 2000000;
    }
}

