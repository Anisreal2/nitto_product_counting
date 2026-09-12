using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeeMotionModule;
using BeeMotionModule.Models;
using BeevisionSolution.Jobs;
using log4net;

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
        public short BrakeDOPin { get; set; } = 1;
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
        /// <summary>
        /// Reads Digital Input state from PCIe IO Card (via IoJobCtrl) or fallback to Motion Card.
        /// </summary>
        public bool GetDigitalInput(int pinNo)
        {
            // Ưu tiên đọc từ Card PCIe IO rời (PCIE-E2I12O16)
            var pcieIo = IoJobCtrl.GetIOcardCtrl();
            if (pcieIo != null && pcieIo.IsInit)
            {
                return pcieIo.GetInputState(pinNo);
            }

            // Nếu không có card PCIe thì đọc từ Card Motion Inovance
            if (Motion != null)
            {
                return Motion.GetDigitalInput((short)pinNo);
            }

            return false;
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
        /// <summary>
        /// Standard Sequence: Release Brake (PCIe DO) -> Clear Emergency/Alarm -> Servo ON
        /// </summary>
        public async Task<bool> EnableServoSequenceAsync(short axis = 0, CancellationToken ct = default)
        {
            if (Motion == null) return false;

            try
            {
                Log($"[Servo Sequence] Step 1: Releasing motor brake via PCIe DO {BrakeDOPin}...");
                var pcieIo = IoJobCtrl.GetIOcardCtrl();
                if (pcieIo != null && pcieIo.IsInit)
                {
                    pcieIo.SetPinOutput(BrakeDOPin, true); // ON chân nhả phanh
                }
                else
                {
                    // Fallback sang motion nếu dùng onboard
                    Motion.SetDigitalOutput(BrakeDOPin, true);
                }
                await Task.Delay(150, ct); // Chờ rơle mở phanh vật lý

                Log($"[Servo Sequence] Step 2: Clearing Emergency & Resetting Driver Alarm for Axis {axis}...");
                Motion.ClearAlarm(axis);
                await Task.Delay(100, ct); // Chờ driver xóa lỗi

                Log($"[Servo Sequence] Step 3: Turning Servo ON for Axis {axis}...");
                bool svOk = Motion.ServoOn(axis);
                if (svOk)
                {
                    Log($"[Servo Sequence] >>> Axis {axis}: Servo ON successfully!");
                    return true;
                }
                else
                {
                    Log($"[Servo Sequence Error] Axis {axis}: Servo ON failed!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log($"[Servo Sequence Exception] Axis {axis}: {ex.Message}");
                return false;
            }
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
        // Default DI pin definitions for two-hand start buttons and Loadcell sensor
        public short StartBtn1DIPin { get; set; } = 4; // DI 4: Left Start button
        public short StartBtn2DIPin { get; set; } = 5; // DI 5: Right Start button
        public short LoadcellDIPin { get; set; } = 6;  // DI 6: Loadcell sensor

        private async Task<bool> ExecuteSingleCycleAsync(Func<int, Task<bool>> onVisionJobTrigger, CancellationToken ct)
        {
            short axis = 0;
            var cfg = Motion.Config;

           
            // STEP 1: Safety & Servo Status Check
            SetState(SequenceState.CheckingReady);
            if (!Motion.IsMasterOp && !cfg.Simulate)
            {
                Log("[Cycle Error] EtherCAT Master is not in OP state (State 6).");
                SetState(SequenceState.Error);
                return false;
            }

            if (!Motion.GetAxisState(axis).IsServoOn)
            {
                bool svOk = await EnableServoSequenceAsync(axis, ct);
                if (!svOk)
                {
                    Log("[Cycle Error] Failed to enable Servo. Cycle aborted.");
                    SetState(SequenceState.Error);
                    return false;
                }
            }


            // STEP 2: Wait for simultaneous two-hand start button press
            Log($"[Cycle] Waiting for operator to press both Start buttons (DI {StartBtn1DIPin} & DI {StartBtn2DIPin})...");
            
            while (!ct.IsCancellationRequested)
            {
                bool btn1 = Motion.GetDigitalInput(StartBtn1DIPin);
                bool btn2 = Motion.GetDigitalInput(StartBtn2DIPin);

                // Both buttons detected
                if (btn1 && btn2)
                {
                    await Task.Delay(30, ct); // Debounce 30ms
                    if (Motion.GetDigitalInput(StartBtn1DIPin) && Motion.GetDigitalInput(StartBtn2DIPin))
                    {
                        Log("[Cycle] >>> Start trigger confirmed! Sequence initiated...");
                        break; // Exit wait loop immediately -> Operator can release buttons now!
                    }
                }
                await Task.Delay(10, ct);
            }

            // STEP 3: Move Servo forward (+) and scan dual stop conditions (Loadcell or Target Position)
            SetState(SequenceState.MovingToCapture);

            // Retrieve parameters from taught point in Teaching Points list
            var teachPt = cfg?.TeachingPoints?.Find(p => p.TriggerVision || p.StepType == "CheckVision");
            double targetPos = teachPt != null ? teachPt.Position : (cfg != null && cfg.CapturePosition > 0 ? cfg.CapturePosition : 100.0);
            double moveSpeed = (teachPt != null && teachPt.Speed > 0) ? teachPt.Speed : (cfg?.Axes?.Count > 0 ? cfg.Axes[0].DefaultProfile.TargetVelocity : 50.0);
            int dwellTime = (teachPt != null && teachPt.DwellTimeMs > 0) ? teachPt.DwellTimeMs : 100;
            int visionJobId = teachPt != null ? teachPt.JobId : 0;

            Log($"[Cycle] Moving Servo towards target: {targetPos:F3} mm | Speed: {moveSpeed:F1} mm/s...");
            Motion.MoveAbsolute(axis, targetPos, moveSpeed);

            bool stoppedByLoadcell = false;
            bool stoppedByPosition = false;

            // High-speed scan loop (5ms) checking either stop condition
            while (!ct.IsCancellationRequested)
            {
                // Condition A: Loadcell sensor input triggered (HIGH)
                if (Motion.GetDigitalInput(LoadcellDIPin))
                {
                    stoppedByLoadcell = true;
                    break;
                }

                // Condition B: Pre-configured target position reached
                var st = Motion.GetAxisState(axis);
                if (st.ActualPosition >= targetPos - 0.05 || st.IsInPosition)
                {
                    stoppedByPosition = true;
                    break;
                }

                if (st.IsError) break;

                await Task.Delay(5, ct);
            }

            // Stop Servo immediately upon meeting either condition
            Motion.Stop(axis);
            var stopSt = Motion.GetAxisState(axis);

            if (stoppedByLoadcell)
            {
                Log($"[Cycle] >>> Servo stopped by Loadcell trigger (DI {LoadcellDIPin} = HIGH) at position: {stopSt.ActualPosition:F3} mm");
            }
            else if (stoppedByPosition)
            {
                Log($"[Cycle] >>> Servo stopped at target position: {stopSt.ActualPosition:F3} mm");
            }

            await Task.Delay(dwellTime, ct); // Dwell delay for mechanical stabilization

            // STEP 4: Trigger VisionPro Inspection (Light control is managed by the Vision Job itself)
            SetState(SequenceState.ProcessingVision);
            Log($"[Cycle] Triggering Vision Job ID {visionJobId}...");

            bool visionOk = true;
            try
            {
                visionOk = await JobController.RunJobByIdAsync(visionJobId);
            }
            catch (Exception ex)
            {
                Log($"[Vision Error] Job {visionJobId} execution failed: {ex.Message}");
                visionOk = false;
            }

            if (visionOk)
            {
                Log("[Cycle Vision] Inspection Result: PASS (OK)");
            }
            else
            {
                Log("[Cycle Vision WARNING] Inspection Result: FAIL (NG) - Defective or inverted product detected!");
            }

            // STEP 5: Return Servo to initial starting position (0.000 mm)
            SetState(SequenceState.MovingToEnd);
            Log("[Cycle] Returning Servo to starting position (0.000 mm)...");

            Motion.MoveAbsolute(axis, 0.0, moveSpeed);
            bool returnOk = await Motion.WaitMoveDoneAsync(axis, 15000, ct);
            if (returnOk)
            {
                Log("[Cycle] Servo returned to 0.000 mm successfully. Cycle completed!");
            }
            else
            {
                Log("[Cycle Error] Timeout while returning Servo to home position!");
                SetState(SequenceState.Error);
                return false;
            }

            // STEP 6: Anti-tie-down protection: Ensure operator releases both buttons before allowing next cycle
            while (Motion.GetDigitalInput(StartBtn1DIPin) || Motion.GetDigitalInput(StartBtn2DIPin))
            {
                await Task.Delay(50, ct);
            }

            SetState(SequenceState.FinishingCycle);
            return visionOk;
        }

        public async Task<bool> MoveToPointAsync(TeachingPoint pt, CancellationToken ct = default)
        {
            if (pt == null || Motion == null) return false;
            Log($"[Teaching] Move To Test point '{pt.Name}' ({pt.Position:F2} mm)...");
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
            Log($"[Teaching] Updated Position '{pt.Name}' = {pt.Position:F3} mm");
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

