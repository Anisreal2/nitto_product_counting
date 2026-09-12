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
        public string AxisName { get; set; } = "Press Axis (Leadshine EL7)";
        public double PulsesPerUnit { get; set; } = 1000.0; // 1000 xung/mm
        public double MaxVelocity { get; set; } = 100.0;    // mm/s
        public double SoftwareLimitPositive { get; set; } = 150.0; // Hành trình max 150mm
        public double SoftwareLimitNegative { get; set; } = -5.0;  // mm
        public bool EnableSoftwareLimits { get; set; } = true;
        public MotionProfile DefaultProfile { get; set; } = new MotionProfile();
        public HomingConfig Homing { get; set; } = new HomingConfig();
    }

    public class IOConfig
    {
        // Nitto Digital Inputs (DI)
        public int TriggerBtnLeftDIBit { get; set; } = 0;    // Nút nhấn trigger Trái (IDEC YW1L-MF2E10Q4-G)
        public int TriggerBtnRightDIBit { get; set; } = 1;   // Nút nhấn trigger Phải (IDEC YW1L-MF2E10Q4-G)
        public int ForceReachedDIBit { get; set; } = 2;      // Tín hiệu đạt lực tỳ (Bongshin CBFSB-10 + BS-205-35)
        public int SensorHomeUpDIBit { get; set; } = 3;      // Cảm biến quang vị trí trên (Misumi C-MSX674N-2M)
        public int SensorDownLimitDIBit { get; set; } = 4;   // Cảm biến quang giới hạn dưới (Misumi C-MSX674N-2M)
        public int SensorPartPresentDIBit { get; set; } = 5; // Cảm biến có phôi trên Jig (Misumi C-MSX674N-2M)
        public int SystemStopDIBit { get; set; } = 6;        // Nút dừng khẩn cấp (E-Stop)

        // Nitto Digital Outputs (DO)
        public int TowerLightGreenDOBit { get; set; } = 0;   // Đèn tháp Xanh (OK)
        public int TowerLightRedDOBit { get; set; } = 1;     // Đèn tháp Đỏ (NG)
        public int TowerBuzzerDOBit { get; set; } = 2;       // Còi báo lỗi
        public int CameraTriggerDOBit { get; set; } = 3;     // Kích hoạt camera cứng
        public int BacklightDOBit { get; set; } = 4;         // Đèn Backlight trắng

        // Backward compatibility properties
        public int CylinderDOBit { get => TowerLightGreenDOBit; set => TowerLightGreenDOBit = value; }
        public int SensorForwardDIBit { get => ForceReachedDIBit; set => ForceReachedDIBit = value; }
        public int SensorBackwardDIBit { get => SensorHomeUpDIBit; set => SensorHomeUpDIBit = value; }
        public int VacuumDOBit { get => BacklightDOBit; set => BacklightDOBit = value; }
        public int VacuumSensorDIBit { get => SensorPartPresentDIBit; set => SensorPartPresentDIBit = value; }
    }

    public class MotionConfig
    {
        public bool EnableMotionControl { get; set; } = true;
        public bool Simulate { get; set; } = false;
        public uint CardId { get; set; } = 0;
        public string ConfigFileSys { get; set; } = "SystemCfg.xml";
        public string ConfigFileDrv { get; set; } = "DriveCfg.xml";
        public string ConfigDirectory { get; set; } = string.Empty;
        public int TotalAxes { get; set; } = 1;
       // public string ConfigDirectory { get; set; } = string.Empty;
        public List<AxisConfig> Axes { get; set; } = new List<AxisConfig>
        {
            new AxisConfig { AxisIndex = 0, AxisName = "Press Axis (Leadshine EL7)" }
        };

        // Nitto Machine Key Setpoints
        public double StandbyPosition { get; set; } = 0.0;    // Vị trí mở kẹp trên cao (mm)
        public double ClampingPosition { get; set; } = 80.0;  // Vị trí tỳ ép sản phẩm (mm)
        public double ClampingVelocity { get; set; } = 50.0;  // Vận tốc tỳ ép (mm/s)
        public double RetractVelocity { get; set; } = 80.0;   // Vận tốc nâng lên (mm/s)
        public int ForceDwellTimeMs { get; set; } = 150;      // Thời gian duy trì lực ổn định trước khi chụp (ms)
        public int TwoHandSyncTimeMs { get; set; } = 500;     // Cửa sổ thời gian bấm đồng thời 2 nút IDEC (ms)
        public int DwellTimeMs { get; set; } = 200;

        // Legacy compatibility
        public double CapturePosition { get => ClampingPosition; set => ClampingPosition = value; }
        public double EndPosition { get => StandbyPosition; set => StandbyPosition = value; }

        public IOConfig IO { get; set; } = new IOConfig();

        public List<TeachingPoint> TeachingPoints { get; set; } = new List<TeachingPoint>
        {
            new TeachingPoint { Id = 1, Name = "1. Standby / Retract", AxisIndex = 0, Position = 0.0, Speed = 80.0, StepType = "Standby", StepOrder = 1, TriggerVision = false },
            new TeachingPoint { Id = 2, Name = "2. Clamping / Press", AxisIndex = 0, Position = 80.0, Speed = 50.0, StepType = "CheckVision", StepOrder = 2, TriggerVision = true, JobId = 0 }
        };
    }
}

