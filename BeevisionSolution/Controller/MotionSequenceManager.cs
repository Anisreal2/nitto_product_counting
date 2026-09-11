using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BeeMotionModule;
using BeeMotionModule.Models;

namespace BeevisionSolution.Controller
{
    public enum SequenceState
    {
        Idle,                   // Vị trí chờ mở kẹp (0mm)
        CheckingReady,          // Kiểm tra servo & an toàn
        WaitingTrigger,         // Chờ nhấn 2 nút trigger IDEC
        ClampingDown,           // Hạ cơ cấu tỳ kẹp phẳng tệp sản phẩm (Leadshine EL7)
        TriggeringVision,       // Bật Backlight, duy trì lực tỳ ổn định
        ProcessingVision,       // VisionPro đếm 100pcs và kiểm tra ngược mặt
        UnclampingUp,           // Nâng cơ cấu tỳ về vị trí mở kẹp
        FinishingCycle,         // Cập nhật kết quả, bật đèn tháp OK/NG
        Error,                  // Báo lỗi
        Stopped,                // Dừng
        // Backward compatibility
        TrayIn = ClampingDown,
        MovingToCapture = ClampingDown,
        CompensatingAndAction = ProcessingVision,
        MovingToEnd = UnclampingUp,
        TrayOut = UnclampingUp
    }

    /// <summary>
    /// Coordinates the automated production cycle for Nitto Product Counting & Reverse Inspection:
    /// Standby -> Two-Hand Trigger -> Clamp Down (Force Control) -> Vision Counting -> Retract Up -> Report.
    /// </summary>
    public class MotionSequenceManager
    {
        private static readonly Lazy<MotionSequenceManager> _instance = new Lazy<MotionSequenceManager>(() => new MotionSequenceManager());
        public static MotionSequenceManager Instance => _instance.Value;

        public IMotionController Motion { get; private set; }
        public SequenceState CurrentState { get; private set; } = SequenceState.Idle;
        public bool IsRunning { get; private set; } = false;
        public bool IsContinuousMode { get; set; } = false;
        public int TotalCycleCount { get; private set; } = 0;
        public double LastCycleTimeMs { get; private set; } = 0;

        // Test Program & Mock Simulation Properties
        public bool IsTestProgramMode { get; set; } = false;
        public string MockVisionResult { get; set; } = "OK"; // "OK", "NG1", "NG2", "NG3"
        public int WatchdogTimeoutMs { get; set; } = 30000;

        public event Action<SequenceState> OnStateChanged;
        public event Action<string> OnLog;
        public event Action<int, double, bool> OnCycleCompleted; // cycleCount, cycleTimeMs, isOk

        private CancellationTokenSource _cts;
        private readonly Stopwatch _cycleStopwatch = new Stopwatch();

        private MotionSequenceManager()
        {
            Motion = new InovanceEcatController();
            Motion.OnLogMessage += (msg) => OnLog?.Invoke(msg);
        }

        public bool Initialize(MotionConfig config)
        {
            Log("[Sequence] Initializing Nitto Motion Control subsystem...");
            bool ok = Motion.Init(config);
            if (ok)
            {
                SetState(SequenceState.Idle);
                Log("[Sequence] Nitto Motion Control system is ready.");
            }
            else
            {
                SetState(SequenceState.Error);
                Log("[Sequence Error] Motion initialization failed.");
            }
            return ok;
        }

        public async Task<bool> StartCycleAsync(bool continuous = false, Func<int, Task<bool>> onVisionJobTrigger = null)
        {
            if (IsRunning)
            {
                Log("[Sequence Warn] Cycle is already running. Ignoring new trigger.");
                return false;
            }

            IsRunning = true;
            IsContinuousMode = continuous;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Log($"[Sequence] Starting Nitto production cycle (Mode: {(continuous ? "Continuous / Auto" : "Single Cycle")}, TestMode: {IsTestProgramMode})...");

            try
            {
                while (IsRunning && !token.IsCancellationRequested)
                {
                    _cycleStopwatch.Restart();
                    bool cycleSuccess = await ExecuteSingleCycleAsync(onVisionJobTrigger, token);
                    _cycleStopwatch.Stop();

                    LastCycleTimeMs = _cycleStopwatch.Elapsed.TotalMilliseconds;
                    if (cycleSuccess) TotalCycleCount++;

                    OnCycleCompleted?.Invoke(TotalCycleCount, LastCycleTimeMs, cycleSuccess);

                    if (!IsContinuousMode || token.IsCancellationRequested || !cycleSuccess)
                    {
                        break;
                    }

                    int dwell = Motion.Config?.DwellTimeMs > 0 ? Motion.Config.DwellTimeMs : 300;
                    await Task.Delay(dwell, token);
                }
            }
            catch (OperationCanceledException)
            {
                Log("[Sequence] Cycle stopped by user request.");
            }
            catch (Exception ex)
            {
                Log($"[Sequence Exception] Cycle error: {ex.Message}");
                SetState(SequenceState.Error);
            }
            finally
            {
                IsRunning = false;
                SetState(SequenceState.Idle);
            }

            return true;
        }

        public Task<bool> StartCycleAsync(bool continuous, Func<Task<bool>> onVisionProcessTrigger)
        {
            return StartCycleAsync(continuous, onVisionProcessTrigger != null ? new Func<int, Task<bool>>(jobId => onVisionProcessTrigger()) : null);
        }

        public async Task<bool> SimulateTriggerAsync()
        {
            Log("[Sequence] Simulating 2-hand trigger -> Starting inspection cycle...");
            return await StartCycleAsync(false);
        }

        private async Task<bool> ExecuteSingleCycleAsync(Func<int, Task<bool>> onVisionJobTrigger, CancellationToken ct)
        {
            short axis = 0;
            var cfg = Motion.Config;

            // STEP 1: Safety & Servo Check
            SetState(SequenceState.CheckingReady);
            if (!Motion.IsMasterOp && !cfg.Simulate)
            {
                Log("[Sequence Error] Leadshine EtherCAT Master not in OP state. Please check network cable and driver.");
                SetState(SequenceState.Error);
                return false;
            }

            var axisSts = Motion.GetAxisState(axis);
            if (!axisSts.IsServoOn)
            {
                Log("[Sequence] Automatically turning Servo ON...");
                if (!Motion.ServoOn(axis))
                {
                    Log("[Sequence Error] Cannot turn Servo ON.");
                    SetState(SequenceState.Error);
                    return false;
                }
                await Task.Delay(150, ct);
            }

            // STEP 2: Waiting for Two-Hand Trigger IDEC buttons (if in Auto Mode and not simulation)
            if (IsContinuousMode && !IsTestProgramMode && !cfg.Simulate)
            {
                SetState(SequenceState.WaitingTrigger);
                Log("[Sequence] Waiting for operator to press both IDEC trigger buttons simultaneously...");

                var triggerWaitStart = DateTime.Now;
                bool triggered = false;

                while (!ct.IsCancellationRequested && IsRunning)
                {
                    bool left = Motion.IsTriggerLeftPressed();
                    bool right = Motion.IsTriggerRightPressed();

                    if (left && right)
                    {
                        Log("[Sequence Trigger] Both IDEC safety buttons pressed -> Starting clamping cycle!");
                        triggered = true;
                        break;
                    }

                    await Task.Delay(20, ct);
                }

                if (!triggered) return false;
            }

            // STEP 3: Clamp Down (Hạ cơ cấu tỳ kẹp phẳng tệp sản phẩm, kiểm soát lực qua Loadcell Bongshin)
            SetState(SequenceState.ClampingDown);
            double clampPos = cfg?.ClampingPosition ?? 80.0;
            double clampSpeed = cfg?.ClampingVelocity ?? 50.0;
            Log($"[Sequence] Clamping down to {clampPos:F2} mm (Monitoring Bongshin loadcell force)...");

            bool clampOk = await Motion.ClampDownAsync(clampPos, clampSpeed, ct);
            if (!clampOk)
            {
                Log("[Sequence Error] Clamping down command failed.");
                SetState(SequenceState.Error);
                return false;
            }

            // STEP 4: Force Dwell & Trigger Vision Backlight
            SetState(SequenceState.TriggeringVision);
            int forceDwell = cfg?.ForceDwellTimeMs > 0 ? cfg.ForceDwellTimeMs : 150;
            await Task.Delay(forceDwell, ct);

            // Turn ON Backlight
            if (cfg?.IO != null)
            {
                Motion.SetDigitalOutput((short)cfg.IO.BacklightDOBit, true);
            }

            // STEP 5: Vision Processing (Đếm số lượng 100 pcs & Kiểm tra ngược mặt)
            SetState(SequenceState.ProcessingVision);
            Log("[Sequence Vision] Triggering 65MP Camera & Cognex VisionPro processing...");

            bool visionOk = true;
            if (IsTestProgramMode)
            {
                await Task.Delay(200, ct);
                visionOk = string.Equals(MockVisionResult, "OK", StringComparison.OrdinalIgnoreCase);
                Log($"[Sequence Test Program] Mock Vision Result = {MockVisionResult} (Success: {visionOk})");
            }
            else if (onVisionJobTrigger != null)
            {
                visionOk = await onVisionJobTrigger.Invoke(0);
            }
            else
            {
                visionOk = await JobController.RunJobByIdAsync(0);
            }

            Log($"[Sequence Vision] Result: {(visionOk ? "OK (Count 100 & Correct Orientation)" : "NG (Quantity Mismatch or Inverted)")}");

            // STEP 6: Unclamp & Retract Up (Nâng trục tỳ mở kẹp về vị trí chờ)
            SetState(SequenceState.UnclampingUp);
            // Turn OFF Backlight
            if (cfg?.IO != null)
            {
                Motion.SetDigitalOutput((short)cfg.IO.BacklightDOBit, false);
            }

            double retractSpeed = cfg?.RetractVelocity ?? 80.0;
            Log($"[Sequence] Retracting press axis to standby position (Speed {retractSpeed:F1} mm/s)...");
            bool retractOk = await Motion.RetractUpAsync(retractSpeed, ct);
            if (!retractOk)
            {
                Log("[Sequence Warning] Retract press axis warning or timeout.");
            }

            // STEP 7: Report & Tower Light Indication
            SetState(SequenceState.FinishingCycle);
            if (cfg?.IO != null)
            {
                if (visionOk)
                {
                    Motion.SetDigitalOutput((short)cfg.IO.TowerLightGreenDOBit, true);
                    Motion.SetDigitalOutput((short)cfg.IO.TowerLightRedDOBit, false);
                    Motion.SetDigitalOutput((short)cfg.IO.TowerBuzzerDOBit, false);
                }
                else
                {
                    Motion.SetDigitalOutput((short)cfg.IO.TowerLightGreenDOBit, false);
                    Motion.SetDigitalOutput((short)cfg.IO.TowerLightRedDOBit, true);
                    Motion.SetDigitalOutput((short)cfg.IO.TowerBuzzerDOBit, true);

                    // Auto turn off buzzer after 1 second
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        Motion.SetDigitalOutput((short)cfg.IO.TowerBuzzerDOBit, false);
                    });
                }
            }

            return visionOk;
        }

        public async Task<bool> MoveToPointAsync(TeachingPoint pt, CancellationToken ct = default)
        {
            if (pt == null || Motion == null) return false;
            Log($"[Teaching] Moving to point '{pt.Name}' ({pt.Position:F2} mm)...");
            bool ok = Motion.MoveAbsolute(pt.AxisIndex, pt.Position, pt.Speed, pt.Acceleration, pt.Acceleration);
            if (ok)
            {
                await Motion.WaitMoveDoneAsync(pt.AxisIndex, 30000, ct);
            }
            return ok;
        }

        public bool TeachCurrentPosition(int pointId, short axis = 0)
        {
            var cfg = Motion?.Config;
            var pt = cfg?.TeachingPoints?.Find(p => p.Id == pointId);
            if (pt == null) return false;

            var sts = Motion.GetAxisState(axis);
            pt.Position = Math.Round(sts.ActualPosition, 3);
            pt.AxisIndex = axis;
            Log($"[Teaching] Đã cập nhật tọa độ điểm '{pt.Name}' = {pt.Position:F3} mm");
            return true;
        }

        public void StopCycle()
        {
            Log("[Sequence] Nhận lệnh dừng chu trình...");
            IsRunning = false;
            _cts?.Cancel();
            Motion.Stop(0);
            SetState(SequenceState.Stopped);
        }

        public void EmergencyStop()
        {
            Log("[Sequence CRITICAL] DỪNG KHẨN CẤP!");
            IsRunning = false;
            _cts?.Cancel();
            Motion.EmergencyStop();
            SetState(SequenceState.Error);
        }

        private void SetState(SequenceState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        private void Log(string msg)
        {
            OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss.fff}] {msg}");
        }
    }
}

