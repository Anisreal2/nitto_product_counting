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
        Idle,
        CheckingReady,
        TrayIn,
        MovingToCapture,
        TriggeringVision,
        ProcessingVision,
        CompensatingAndAction,
        MovingToEnd,
        TrayOut,
        FinishingCycle,
        Error,
        Stopped
    }

    /// <summary>
    /// Coordinates the automated production cycle: Motion -> Camera Capture -> Vision Processing -> Offset Compensation / Sorting -> Next Cycle.
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
        public string MockRobotCommand { get; set; } = "Start"; // "Start", "Continue", "INTRAY"
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
            Log("[Sequence] Initializing Motion Control subsystem...");
            bool ok = Motion.Init(config);
            if (ok)
            {
                SetState(SequenceState.Idle);
                Log("[Sequence] Motion Control system is ready.");
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

            Log($"[Sequence] Starting automated cycle (Mode: {(continuous ? "Continuous" : "Single Cycle")}, TestMode: {IsTestProgramMode})...");

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

        // Overload for backward compatibility with single trigger
        public Task<bool> StartCycleAsync(bool continuous, Func<Task<bool>> onVisionProcessTrigger)
        {
            return StartCycleAsync(continuous, onVisionProcessTrigger != null ? new Func<int, Task<bool>>(jobId => onVisionProcessTrigger()) : null);
        }

        private async Task<bool> ExecuteSingleCycleAsync(Func<int, Task<bool>> onVisionJobTrigger, CancellationToken ct)
        {
            short axis = 0;
            var cfg = Motion.Config;

            // STEP 1: Safety & Servo Check
            SetState(SequenceState.CheckingReady);
            if (!Motion.IsMasterOp && !cfg.Simulate)
            {
                Log("[Sequence Error] EtherCAT Master not in OP state. Please check network cable and driver.");
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
                await Task.Delay(200, ct);
            }

            // STEP 2: Multi-Step Action Execution (Teaching Points)
            if (cfg?.TeachingPoints != null && cfg.TeachingPoints.Count > 0)
            {
                bool allPointsOk = true;

                for (int i = 0; i < cfg.TeachingPoints.Count; i++)
                {
                    var pt = cfg.TeachingPoints[i];
                    ct.ThrowIfCancellationRequested();

                    string stepType = string.IsNullOrEmpty(pt.StepType) ? (pt.TriggerVision ? "CheckVision" : "Moving") : pt.StepType;
                    Log($"[Sequence Step {i + 1}/{cfg.TeachingPoints.Count}] Action: '{stepType}' at '{pt.Name}' ({pt.Position:F2} mm)...");

                    // Handle Pneumatic Actions based on StepType
                    if (string.Equals(stepType, "TrayIn", StringComparison.OrdinalIgnoreCase))
                    {
                        SetState(SequenceState.TrayIn);
                        Motion.SetCylinder(true);
                        await Task.Delay(300, ct);
                        Motion.SetVacuum(true);
                        await Task.Delay(200, ct);
                    }
                    else if (string.Equals(stepType, "TrayOut", StringComparison.OrdinalIgnoreCase))
                    {
                        SetState(SequenceState.TrayOut);
                        Motion.SetVacuum(false);
                        await Task.Delay(200, ct);
                        Motion.SetCylinder(false);
                        await Task.Delay(300, ct);
                    }

                    // Move to point position
                    SetState(string.Equals(stepType, "CheckVision", StringComparison.OrdinalIgnoreCase) || pt.TriggerVision 
                        ? SequenceState.MovingToCapture 
                        : SequenceState.MovingToEnd);

                    bool moveOk = Motion.MoveAbsolute(pt.AxisIndex, pt.Position, pt.Speed, pt.Acceleration, pt.Acceleration);
                    if (!moveOk)
                    {
                        Log($"[Sequence Error] Move command to '{pt.Name}' failed.");
                        SetState(SequenceState.Error);
                        return false;
                    }

                    uint waitTimeout = pt.TimeoutMs > 0 ? (uint)pt.TimeoutMs : (uint)WatchdogTimeoutMs;
                    bool reached = await Motion.WaitMoveDoneAsync(pt.AxisIndex, waitTimeout, ct);
                    if (!reached)
                    {
                        Log($"[Sequence Watchdog Error] Timeout waiting for axis to reach '{pt.Name}' ({waitTimeout}ms).");
                        SetState(SequenceState.Error);
                        return false;
                    }

                    // Dwell Time
                    if (pt.DwellTimeMs > 0)
                    {
                        await Task.Delay(pt.DwellTimeMs, ct);
                    }

                    // Set DO Pin if configured
                    if (pt.SetDoPinOnArrival >= 0)
                    {
                        Motion.SetDigitalOutput((short)pt.SetDoPinOnArrival, pt.DoStateOnArrival);
                        Log($"[Sequence IO] Point '{pt.Name}': Set DO Pin {pt.SetDoPinOnArrival} = {(pt.DoStateOnArrival ? "ON" : "OFF")}");
                    }

                    // Trigger Vision if required
                    if (string.Equals(stepType, "CheckVision", StringComparison.OrdinalIgnoreCase) || pt.TriggerVision)
                    {
                        SetState(SequenceState.ProcessingVision);
                        Log($"[Sequence Vision] Triggering Job {pt.JobId} at '{pt.Name}'...");

                        bool jobOk = true;
                        if (IsTestProgramMode)
                        {
                            await Task.Delay(200, ct);
                            jobOk = string.Equals(MockVisionResult, "OK", StringComparison.OrdinalIgnoreCase);
                            Log($"[Sequence Test Program] Mock Vision Result = {MockVisionResult} (Success: {jobOk})");
                        }
                        else if (onVisionJobTrigger != null)
                        {
                            jobOk = await onVisionJobTrigger.Invoke(pt.JobId);
                        }
                        else
                        {
                            jobOk = await JobController.RunJobByIdAsync(pt.JobId);
                        }

                        Log($"[Sequence Vision] Job {pt.JobId} Result: {(jobOk ? "OK" : "NG")}");

                        if (!jobOk)
                        {
                            allPointsOk = false;

                            // Handle NG Branching
                            if (pt.ActionOnNg == NgAction.StopEarly)
                            {
                                Log($"[Sequence NG] Cycle stopped early due to NG at '{pt.Name}' (Action: StopEarly).");
                                SetState(SequenceState.Error);
                                return false;
                            }
                            else if (pt.ActionOnNg == NgAction.JumpToPoint && pt.TargetPointIdOnNg > 0)
                            {
                                var jumpPt = cfg.TeachingPoints.Find(p => p.Id == pt.TargetPointIdOnNg);
                                if (jumpPt != null)
                                {
                                    Log($"[Sequence NG] Jumping to NG reject point '{jumpPt.Name}' ({jumpPt.Position:F2} mm)...");
                                    Motion.MoveAbsolute(jumpPt.AxisIndex, jumpPt.Position, jumpPt.Speed);
                                    await Motion.WaitMoveDoneAsync(jumpPt.AxisIndex, 30000, ct);
                                    if (jumpPt.SetDoPinOnArrival >= 0)
                                    {
                                        Motion.SetDigitalOutput((short)jumpPt.SetDoPinOnArrival, jumpPt.DoStateOnArrival);
                                    }
                                }
                                SetState(SequenceState.FinishingCycle);
                                return false;
                            }
                        }
                    }
                }

                SetState(SequenceState.FinishingCycle);
                return allPointsOk;
            }

            // STEP 3: Fallback for basic capture position
            SetState(SequenceState.MovingToCapture);
            double capturePos = cfg != null ? cfg.CapturePosition : 50.0;
            Log($"[Sequence] Di chuyển trục {axis} tới vị trí chụp ảnh: {capturePos:F2} mm...");
            
            bool fallbackMoveOk = Motion.MoveAbsolute(axis, capturePos);
            if (!fallbackMoveOk)
            {
                Log("[Sequence Error] Lệnh phát chuyển động tới vị trí chụp thất bại.");
                SetState(SequenceState.Error);
                return false;
            }

            bool reachedCapture = await Motion.WaitMoveDoneAsync(axis, 15000, ct);
            if (!reachedCapture)
            {
                Log("[Sequence Error] Hết thời gian chờ trục tới vị trí chụp ảnh.");
                SetState(SequenceState.Error);
                return false;
            }

            SetState(SequenceState.ProcessingVision);
            Log("[Sequence] Kích hoạt chụp ảnh & Xử lý thị giác VisionPro...");
            
            bool visionOk = true;
            if (onVisionJobTrigger != null)
            {
                visionOk = await onVisionJobTrigger.Invoke(0);
            }
            else
            {
                await Task.Delay(100, ct);
            }
            Log($"[Sequence] Kết quả xử lý Vision: {(visionOk ? "OK" : "NG")}");

            SetState(SequenceState.MovingToEnd);
            double endPos = cfg != null ? cfg.EndPosition : 200.0;
            Log($"[Sequence] Di chuyển trục {axis} tới vị trí kết thúc: {endPos:F2} mm...");

            Motion.MoveAbsolute(axis, endPos);
            bool reachedEnd = await Motion.WaitMoveDoneAsync(axis, 15000, ct);
            if (!reachedEnd)
            {
                Log("[Sequence Error] Trục không tới được vị trí kết thúc.");
                SetState(SequenceState.Error);
                return false;
            }

            SetState(SequenceState.FinishingCycle);
            return visionOk;
        }

        public async Task<bool> MoveToPointAsync(TeachingPoint pt, CancellationToken ct = default)
        {
            if (pt == null || Motion == null) return false;
            Log($"[Teaching] Di chuyển thử nghiệm tới điểm '{pt.Name}' ({pt.Position:F2} mm)...");
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

