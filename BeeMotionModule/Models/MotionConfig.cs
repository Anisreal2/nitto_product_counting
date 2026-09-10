using System;
using System.Collections.Generic;

namespace BeeMotionModule.Models
{
    /// <summary>
    /// Configuration for Motion Hardware and Axis Settings
    /// </summary>
    public class AxisConfig
    {
        public short AxisIndex { get; set; } = 0;
        public string AxisName { get; set; } = "Axis 0";
        public double PulsesPerUnit { get; set; } = 1000.0; // E.g., 1000 pulses per mm
        public double MaxVelocity { get; set; } = 50000;    // Units/s
        public double SoftwareLimitPositive { get; set; } = 1000.0; // mm
        public double SoftwareLimitNegative { get; set; } = -100.0; // mm
        public bool EnableSoftwareLimits { get; set; } = true;
        public MotionProfile DefaultProfile { get; set; } = new MotionProfile();
        public HomingConfig Homing { get; set; } = new HomingConfig();
    }

    public class IOConfig
    {
        // Pneumatic Cylinder
        public int CylinderDOBit { get; set; } = 0;
        public int SensorForwardDIBit { get; set; } = 0;
        public int SensorBackwardDIBit { get; set; } = 1;

        // Vacuum Suction
        public int VacuumDOBit { get; set; } = 1;
        public int VacuumSensorDIBit { get; set; } = 2;

        // System Safety Stop Bit
        public int SystemStopDIBit { get; set; } = 3;
    }

    public class MotionConfig
    {
        public bool EnableMotionControl { get; set; } = true;
        public bool Simulate { get; set; } = false;
        public uint CardId { get; set; } = 0;
        public string ConfigFileSys { get; set; } = "SystemCfg.xml";
        public string ConfigFileDrv { get; set; } = "DriveCfg.xml";
        public int TotalAxes { get; set; } = 1;
        public List<AxisConfig> Axes { get; set; } = new List<AxisConfig>
        {
            new AxisConfig { AxisIndex = 0, AxisName = "Main Conveyor/Axis" }
        };
        public double CapturePosition { get; set; } = 50.0; // mm
        public double EndPosition { get; set; } = 200.0;    // mm
        public int DwellTimeMs { get; set; } = 500;

        public IOConfig IO { get; set; } = new IOConfig();

        public List<TeachingPoint> TeachingPoints { get; set; } = new List<TeachingPoint>
        {
        };
    }
}

