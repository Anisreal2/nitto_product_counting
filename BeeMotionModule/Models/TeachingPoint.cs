using System;
using System.Collections.Generic;

namespace BeeMotionModule.Models
{
    public enum NgAction
    {
        Continue = 0,       // Tiep tuc chu trinh binh thuong
        StopEarly = 1,      // Ngat chu trinh va bao loi ngay lap tuc
        JumpToPoint = 2     // Nhay den mot diem day xu ly NG (VD: Diem xa hang NG)
    }

    /// <summary>
    /// Represents a teaching point with axis position, speed, dwell time, and Vision Job mapping.
    /// </summary>
    public class TeachingPoint
    {
        public int Id { get; set; } = 1;
        public string Name { get; set; } = "Point 1";
        public short AxisIndex { get; set; } = 0;
        public double Position { get; set; } = 0.0;       // Toa do muc tieu (mm)
        public double Speed { get; set; } = 100.0;        // Van toc di chuyen (mm/s)
        public double Acceleration { get; set; } = 500.0; // Gia toc (mm/s^2)
        public int DwellTimeMs { get; set; } = 100;       // Thoi gian dung on dinh (ms)
        
        public bool TriggerVision { get; set; } = false;  // Co kich hoat chup anh tai diem nay khong
        public int JobId { get; set; } = 0;               // ID cua Vision Job / ToolBlock can chay
        
        public NgAction ActionOnNg { get; set; } = NgAction.Continue;
        public int TargetPointIdOnNg { get; set; } = 0;   // ID diem can nhay den neu phat hien NG

        public int SetDoPinOnArrival { get; set; } = -1;  // Kich hoat chan DO nao khi den vi tri (-1 la khong dung)
        public bool DoStateOnArrival { get; set; } = true;

        // Process step coordination (like MotionVision)
        public string StepType { get; set; } = "CheckVision"; // Waiting, CheckVision, TrayIn, TrayOut
        public int StepOrder { get; set; } = 0;
        public double TimeoutMs { get; set; } = 5000.0;

        public TeachingPoint Clone()
        {
            return new TeachingPoint
            {
                Id = this.Id,
                Name = this.Name,
                AxisIndex = this.AxisIndex,
                Position = this.Position,
                Speed = this.Speed,
                Acceleration = this.Acceleration,
                DwellTimeMs = this.DwellTimeMs,
                TriggerVision = this.TriggerVision,
                JobId = this.JobId,
                ActionOnNg = this.ActionOnNg,
                TargetPointIdOnNg = this.TargetPointIdOnNg,
                SetDoPinOnArrival = this.SetDoPinOnArrival,
                DoStateOnArrival = this.DoStateOnArrival,
                StepType = this.StepType,
                StepOrder = this.StepOrder,
                TimeoutMs = this.TimeoutMs
            };
        }
    }
}
