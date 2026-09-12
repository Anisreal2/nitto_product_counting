using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AForge.Math.Metrics;
using BeeMotionModule.Models;
using BeevisionSolution.Controller;
using BeevisionSolution.Jobs;
using BeevisionSolution.Utils;

namespace BeevisionSolution.Views
{
    public class AlarmRecord
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public string TimeFormatted => Time.ToString("yyyy-MM-dd HH:mm:ss");
        public string AxisName { get; set; } = "Axis 0";
        public uint ErrorCode { get; set; }
        public string CodeHex => $"0x{ErrorCode:X4}";
        public string Message { get; set; } = string.Empty;
        public string StatusText { get; set; } = "ACTIVE";
    }

    public partial class MotionControlView : UserControl
    {
        private short _currentAxis = 0;
        private readonly Queue<string> _logLines = new Queue<string>();
        private const int MaxLogLines = 200;

        public ObservableCollection<IoPinDisplayItem> DiItems { get; set; } = new ObservableCollection<IoPinDisplayItem>();
        public ObservableCollection<IoPinDisplayItem> DoItems { get; set; } = new ObservableCollection<IoPinDisplayItem>();


        private static readonly SolidColorBrush TileOffBrush = new SolidColorBrush(Color.FromRgb(0x25, 0x25, 0x26));
        private static readonly SolidColorBrush TileGreenBrush = new SolidColorBrush(Color.FromRgb(0x2E, 0x8B, 0x57));
        private static readonly SolidColorBrush TileBlueBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
        private static readonly SolidColorBrush TileRedBrush = new SolidColorBrush(Color.FromRgb(0xDC, 0x14, 0x3C));
        private static readonly SolidColorBrush TileOrangeBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x8C, 0x00));
        private static readonly SolidColorBrush TileYellowBrush = new SolidColorBrush(Color.FromRgb(0xD4, 0xAF, 0x37));

        private readonly ObservableCollection<TeachingPoint> _teachingPoints = new ObservableCollection<TeachingPoint>();
        private readonly ObservableCollection<AlarmRecord> _alarmHistory = new ObservableCollection<AlarmRecord>();
        private uint _lastErrorCode = 0;
        private bool _wasInError = false;

        private DispatcherTimer _ioPollingTimer;
        private bool _isAutoMode = false;
        private bool _isLightOn = false;

        public MotionControlView()
        {
            InitializeComponent();
            this.Loaded += MotionControlView_Loaded;
            this.Unloaded += MotionControlView_Unloaded;
        }

        private void MotionControlView_Loaded(object sender, RoutedEventArgs e)
        {
            InitIoList();
            icDigitalInputs.ItemsSource = DiItems;
            icDigitalOutputs.ItemsSource = DoItems;

            var seq = MotionSequenceManager.Instance;
            if (seq.Motion != null)
            {
                seq.Motion.OnAxisStateUpdated += Motion_OnAxisStateUpdated;
                seq.Motion.OnLogMessage += Motion_OnLogMessage;
                seq.OnStateChanged += Seq_OnStateChanged;
                seq.OnCycleCompleted += Seq_OnCycleCompleted;
            }

            if (seq.Motion?.Config != null)
            {
                chkSimulate.IsChecked = seq.Motion.Config.Simulate;
            }

            dgTeachingPoints.ItemsSource = _teachingPoints;
            dgAlarmHistory.ItemsSource = _alarmHistory;

            LoadTeachingPoints();
            LoadMotionConfigToUI();
            UpdateMasterStatusUI();

            // IO Polling Timer (250ms)
            if (_ioPollingTimer == null)
            {
                _ioPollingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
                _ioPollingTimer.Tick += IoPollingTimer_Tick;
            }
            _ioPollingTimer.Start();
        }

        private void MotionControlView_Unloaded(object sender, RoutedEventArgs e)
        {
            var seq = MotionSequenceManager.Instance;
            if (seq.Motion != null)
            {
                seq.Motion.OnAxisStateUpdated -= Motion_OnAxisStateUpdated;
                seq.Motion.OnLogMessage -= Motion_OnLogMessage;
                seq.OnStateChanged -= Seq_OnStateChanged;
                seq.OnCycleCompleted -= Seq_OnCycleCompleted;
            }

            _ioPollingTimer?.Stop();
        }

        #region Axis Selection & Real-time Status
        private void CboAxisSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded || cboAxisSelect == null) return;

            _currentAxis = (short)Math.Max(0, cboAxisSelect.SelectedIndex);
            var motion = MotionSequenceManager.Instance.Motion;
            if (motion != null)
            {
                var sts = motion.GetAxisState(_currentAxis);
                if (sts != null)
                {
                    Motion_OnAxisStateUpdated(_currentAxis, sts);
                }
            }
            LoadMotionConfigToUI();
        }

        private void RbJogMode_Changed(object sender, RoutedEventArgs e)
        {
            if (cboStepDistance == null) return;
            cboStepDistance.Visibility = (rbJogStep?.IsChecked == true) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ChkSimulate_Changed(object sender, RoutedEventArgs e)
        {
            var cfg = MotionSequenceManager.Instance.Motion?.Config ?? new MotionConfig();
            cfg.Simulate = chkSimulate.IsChecked == true;
        }

        private void Motion_OnAxisStateUpdated(short axis, AxisState state)
        {
            if (axis != _currentAxis) return;

            Dispatcher.InvokeAsync(() =>
            {
                if (txtActualPos == null) return;

                txtActualPos.Text = $"{state.ActualPosition:F3} mm";
                txtActualVel.Text = $"{state.ActualVelocity:F1} mm/s";

                // Update Status Matrix Tiles
                tileSvOn.Background = state.IsServoOn ? TileGreenBrush : TileOffBrush;
                tileInp.Background = state.IsInPosition ? TileBlueBrush : TileOffBrush;
                tileHome.Background = state.IsHomed ? TileGreenBrush : TileOffBrush;
                tileLmPos.Background = state.LimitPositive ? TileRedBrush : TileOffBrush;
                tileLmNeg.Background = state.LimitNegative ? TileRedBrush : TileOffBrush;
                tileAlm.Background = state.IsError ? TileRedBrush : TileOffBrush;
                tileEmg.Background = state.EmergencyStop ? TileRedBrush : TileOffBrush;
                tileBusy.Background = state.IsBusy ? TileYellowBrush : TileOffBrush;

                // Update Servo Button Text
                btnServoToggle.Content = state.IsServoOn ? "Servo OFF" : "Servo ON";
                btnServoToggle.Background = state.IsServoOn ? TileRedBrush : TileGreenBrush;

                // Alarm detection
                uint currentErr = (uint)state.RawStatus;
                if (state.IsError)
                {
                    txtAlarmCode.Text = $"0x{currentErr:X4} (Axis Error)";
                    txtAlarmCode.Foreground = TileRedBrush;

                    if (!_wasInError || currentErr != _lastErrorCode)
                    {
                        _alarmHistory.Insert(0, new AlarmRecord
                        {
                            Id = _alarmHistory.Count + 1,
                            Time = DateTime.Now,
                            AxisName = $"Axis {axis}",
                            ErrorCode = currentErr,
                            Message = "Hardware Drive Alarm or Following Error triggered.",
                            StatusText = "ACTIVE"
                        });
                        while (_alarmHistory.Count > MaxLogLines)
                        {
                            _alarmHistory.RemoveAt(_alarmHistory.Count - 1);
                        }
                    }
                    _wasInError = true;
                    _lastErrorCode = currentErr;
                }
                else
                {
                    txtAlarmCode.Text = "0x0000 (Normal / No Error)";
                    txtAlarmCode.Foreground = TileGreenBrush;
                    _wasInError = false;
                }
            });
        }

        private void Seq_OnStateChanged(SequenceState state)
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (txtSeqStateBadge == null) return;
                txtSeqStateBadge.Text = state.ToString().ToUpper();
                txtSeqStateBadge.Foreground = state == SequenceState.Error ? TileRedBrush : (state == SequenceState.Idle ? new SolidColorBrush(Colors.LightGray) : TileGreenBrush);

                var normalBrush = new SolidColorBrush(Color.FromRgb(0x3F, 0x3F, 0x46));
                var activeBrush = TileGreenBrush;

                if (cardStep1 != null) cardStep1.BorderBrush = normalBrush;
                if (cardStep2 != null) cardStep2.BorderBrush = normalBrush;
                if (cardStep3 != null) cardStep3.BorderBrush = normalBrush;
                if (cardStep4 != null) cardStep4.BorderBrush = normalBrush;

                switch (state)
                {
                    case SequenceState.CheckingReady:
                    case SequenceState.TrayIn:
                        if (cardStep1 != null) cardStep1.BorderBrush = activeBrush;
                        if (txtStep1Status != null) txtStep1Status.Text = "RUNNING";
                        break;
                    case SequenceState.MovingToCapture:
                        if (cardStep2 != null) cardStep2.BorderBrush = activeBrush;
                        if (txtStep2Status != null) txtStep2Status.Text = "MOVING";
                        break;
                    case SequenceState.TriggeringVision:
                    case SequenceState.ProcessingVision:
                    case SequenceState.CompensatingAndAction:
                        if (cardStep3 != null) cardStep3.BorderBrush = activeBrush;
                        if (txtStep3Status != null) txtStep3Status.Text = "INSPECT";
                        break;
                    case SequenceState.MovingToEnd:
                    case SequenceState.TrayOut:
                    case SequenceState.FinishingCycle:
                        if (cardStep4 != null) cardStep4.BorderBrush = activeBrush;
                        if (txtStep4Status != null) txtStep4Status.Text = "EJECT";
                        break;
                    case SequenceState.Idle:
                        if (txtStep1Status != null) txtStep1Status.Text = "READY";
                        if (txtStep2Status != null) txtStep2Status.Text = "WAIT";
                        if (txtStep3Status != null) txtStep3Status.Text = "WAIT";
                        if (txtStep4Status != null) txtStep4Status.Text = "WAIT";
                        break;
                }
            });
        }

        private void Seq_OnCycleCompleted(int totalCount, double cycleTimeMs, bool success)
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (txtSeqCycleCount != null) txtSeqCycleCount.Text = totalCount.ToString();
                if (txtSeqCycleTime != null) txtSeqCycleTime.Text = $"{(cycleTimeMs / 1000.0):F2} s";
            });
        }

        private void IoPollingTimer_Tick(object sender, EventArgs e)
        {
            var motion = MotionSequenceManager.Instance.Motion;
            if (motion == null) return;

            bool cylFwd = motion.GetCylinderForwardSensor();
            bool vacSen = motion.GetVacuumSensor();

            // 1. Kiểm tra xem có Card PCIe IO rời hay không
            var pcieIo = IoJobCtrl.GetIOcardCtrl();
            if (pcieIo != null && pcieIo.IsInit)
            {
                // Cập nhật trạng thái DI từ Card PCIe
                for (int i = 0; i < DiItems.Count; i++)
                {
                    int pin = DiItems[i].Pin;
                    if (pin >= 1 && pin <= pcieIo.InputChannels)
                    {
                        DiItems[i].State = pcieIo.GetInputState(pin);
                    }
                }

                // Cập nhật trạng thái DO từ Card PCIe
                for (int i = 0; i < DoItems.Count; i++)
                {
                    int pin = DoItems[i].Pin;
                    if (pin >= 1 && pin <= pcieIo.OutputChannels)
                    {
                        DoItems[i].State = pcieIo.GetOutputState(pin);
                    }
                }
            }
            else
            {
                // Fallback đọc từ Card Inovance nếu Card PCIe chưa bật
                for (int i = 0; i < DiItems.Count; i++)
                {
                    DiItems[i].State = motion.GetDigitalInput((short)DiItems[i].Pin);
                }
                for (int i = 0; i < DoItems.Count; i++)
                {
                    DoItems[i].State = motion.GetDigitalOutput((short)DoItems[i].Pin);
                }
            }
        }
    

            #endregion

            #region Bottom Action Bar Handlers (Like MotionVision)
        private void BtnAutoMode_Click(object sender, RoutedEventArgs e)
        {
            _isAutoMode = true;
            btnAutoMode.Background = TileGreenBrush;
            btnManualMode.Background = TileOffBrush;
            txtModeStatus.Text = "AUTO";
            txtModeStatus.Foreground = TileGreenBrush;

            // Start Cycle in Auto Mode
            MotionSequenceManager.Instance.StartCycleAsync(true);
        }

        private void BtnManualMode_Click(object sender, RoutedEventArgs e)
        {
            _isAutoMode = false;
            btnManualMode.Background = TileGreenBrush;
            btnAutoMode.Background = TileOffBrush;
            txtModeStatus.Text = "MANUAL";
            txtModeStatus.Foreground = TileGreenBrush;

            MotionSequenceManager.Instance.StopCycle();
        }

        private async void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            var motion = MotionSequenceManager.Instance.Motion;
            if (motion == null) return;

            btnHome.IsEnabled = false;
            try
            {
                await motion.HomeAsync(_currentAxis);
            }
            finally
            {
                btnHome.IsEnabled = true;
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            Motion_OnLogMessage($"[Manual] Press RESET -> Clear Alarm and stop Axis {_currentAxis}");
            var motion = MotionSequenceManager.Instance.Motion;
            if (motion != null)
            {
                motion.ClearAlarm(_currentAxis);
                motion.Stop(_currentAxis);
            }
            txtAlarmCode.Text = "0x0000 (Reset)";
            txtAlarmCode.Foreground = TileGreenBrush;
            Motion_OnLogMessage("[System] Reset command executed.");
        }


        private async void BtnServoToggle_Click(object sender, RoutedEventArgs e)
        {
            var motion = MotionSequenceManager.Instance.Motion;
            if (motion == null) return;

            var sts = motion.GetAxisState(_currentAxis);
            if (sts == null) return;

            if (sts.IsServoOn)
            {
                // Khi tắt Servo: Tắt Servo rồi đóng lại phanh cơ (tắt DO)
                motion.ServoOff(_currentAxis);
                var pcieIo = IoJobCtrl.GetIOcardCtrl();
                if (pcieIo != null && pcieIo.IsInit)
                {
                    pcieIo.SetPinOutput(MotionSequenceManager.Instance.BrakeDOPin, false);
                }
                Motion_OnLogMessage($"[Manual] Axis {_currentAxis}: Servo OFF & Brake Locked.");
            }
            else
            {
                // Khi bật Servo: Phải đi qua chuỗi Nhả phanh PCIe -> Tắt Emergency -> Servo ON
                Motion_OnLogMessage($"[Manual] Axis {_currentAxis}: Enabling Servo (Release Brake -> Clear Emg -> Servo ON)...");
                bool ok = await MotionSequenceManager.Instance.EnableServoSequenceAsync(_currentAxis);
                if (!ok)
                {
                    Motion_OnLogMessage($"[Manual Alarm] Axis {_currentAxis}: Enable Servo Failed!");
                }
            }
        }


        private async void BtnTriggerCam_Click(object sender, RoutedEventArgs e)
        {
            Motion_OnLogMessage("[Vision] Manual Camera Trigger...");
            await JobController.RunJobByIdAsync(0);
        }
        #endregion

        #region Manual Jog & Move
        private void BtnJogPos_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            StartJog(1);
        }

        private void BtnJogPos_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (rbJogContinuous.IsChecked == true) StopJog();
        }

        private void BtnJogNeg_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            StartJog(-1);
        }

        private void BtnJogNeg_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (rbJogContinuous.IsChecked == true) StopJog();
        }

        private void StartJog(int direction)
        {
            var motion = MotionSequenceManager.Instance.Motion;
            if (motion == null) return;

            if (!double.TryParse(txtJogSpeed.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double speed) || speed <= 0)
            {
                speed = 50;
            }

            if (rbJogStep.IsChecked == true)
            {
                double stepDist = 1.0;
                if (cboStepDistance.SelectedItem is ComboBoxItem item)
                {
                    string str = item.Content.ToString().Replace("mm", "").Trim();
                    double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out stepDist);
                }
                Motion_OnLogMessage($"[Manual Move] Press JOG {(direction > 0 ? "+ (UP)" : "- (Down)")} {stepDist} mm | Speed: {speed} mm/s (Axis {_currentAxis})");
                motion.MoveRelative(_currentAxis, direction * stepDist, speed);
            }
            else
            {
                double stepDist = 1.0;
                Motion_OnLogMessage($"[Manual] Press JOG {(direction > 0 ? "+ (UP)" : "- (Down)")} {stepDist} mm | Speed: {speed} mm/s (Axis {_currentAxis})");
                motion.MoveJog(_currentAxis, direction * speed);
            }
        }

        private void StopJog()
        {
            var motion = MotionSequenceManager.Instance.Motion;
            if (motion == null) return;
            Motion_OnLogMessage($"[Manual] Threw JOG -> Stop Axis {_currentAxis}");
            motion.Stop(_currentAxis);
        }

        private void BtnMoveAbs_Click(object sender, RoutedEventArgs e)
        {
            var motion = MotionSequenceManager.Instance.Motion;
            if (motion == null) return;

            if (double.TryParse(txtTargetPos.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double targetPos))
            {
                double.TryParse(txtJogSpeed.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double speed);
                if (speed <= 0) speed = 50;
                Motion_OnLogMessage($"[Manual] Press ABS MOVE -> Move to Abs Pos: {targetPos:F3} mm | Speed: {speed} mm/s (Axis {_currentAxis})");
                motion.MoveAbsolute(_currentAxis, targetPos, speed);
            }
        }

        private void BtnMoveRel_Click(object sender, RoutedEventArgs e)
        {
            var motion = MotionSequenceManager.Instance.Motion;
            if (motion == null) return;

            if (double.TryParse(txtTargetPos.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double dist))
            {
                double.TryParse(txtJogSpeed.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double speed);
                if (speed <= 0) speed = 50;
                Motion_OnLogMessage($"[Manual] Press REL MOVE -> Move relative: {dist:F3} mm | Speed: {speed} mm/s (Axis {_currentAxis})");
                motion.MoveRelative(_currentAxis, dist, speed);
            }
        }
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            Motion_OnLogMessage($"[Manual] Press Stop -> STOP ALL");
            MotionSequenceManager.Instance.StopCycle();
            var motion = MotionSequenceManager.Instance.Motion;
            if (motion != null)
            {
                for (short i = 0; i < (motion.Config?.TotalAxes ?? 1); i++)
                {
                    motion.Stop(i);
                }
            }
        }

        private void BtnToggleCylinder_Click(object sender, RoutedEventArgs e)
        {
            var motion = MotionSequenceManager.Instance.Motion;
            if (motion == null) return;
            motion.SetCylinder(!motion.IsCylinderForward);
        }

        private void BtnToggleVacuum_Click(object sender, RoutedEventArgs e)
        {
            var motion = MotionSequenceManager.Instance.Motion;
            if (motion == null) return;
            motion.SetVacuum(!motion.IsVacuumOn);
        }

        private void BtnLightToggle_Click(object sender, RoutedEventArgs e)
        {
            _isLightOn = !_isLightOn;
            try
            {
                var lights = JobController.GetAllLights();
                if (lights != null && lights.Count > 0)
                {
                    foreach (var light in lights)
                    {
                        if (_isLightOn)
                        {
                            light.LightOn(1, 200);
                        }
                        else
                        {
                            light.LightOff(1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Motion_OnLogMessage($"[Light Error] {ex.Message}");
            }

            if (tileLight != null)
            {
                tileLight.Background = _isLightOn ? TileGreenBrush : TileOffBrush;
            }
            Motion_OnLogMessage($"[Light] All Light Controllers {(_isLightOn ? "Turned ON" : "Turned OFF")}");
        }
        #endregion

        #region Teaching Points
        private void LoadTeachingPoints(MotionConfig specificConfig = null)
        {
            _teachingPoints.Clear();
            var cfg = specificConfig ?? MotionSequenceManager.Instance.Motion?.Config;
            if (cfg?.TeachingPoints != null)
            {
                foreach (var pt in cfg.TeachingPoints)
                {
                    _teachingPoints.Add(pt);
                }
            }

            if (_teachingPoints.Count == 0)
            {
                _teachingPoints.Add(new TeachingPoint { Id = 1, Name = "Tray In Pick", AxisIndex = 0, Position = 0.0, Speed = 100, StepType = "TrayIn", StepOrder = 1, TriggerVision = false });
                _teachingPoints.Add(new TeachingPoint { Id = 2, Name = "Capture View 1", AxisIndex = 0, Position = 50.0, Speed = 200, StepType = "CheckVision", StepOrder = 2, TriggerVision = true, JobId = 0 });
                _teachingPoints.Add(new TeachingPoint { Id = 3, Name = "Tray Out Place", AxisIndex = 0, Position = 200.0, Speed = 200, StepType = "TrayOut", StepOrder = 3, TriggerVision = false });
            }
        }

        private void BtnTeachCurrent_Click(object sender, RoutedEventArgs e)
        {
            if (dgTeachingPoints.SelectedItem is TeachingPoint selectedPt)
            {
                var sts = MotionSequenceManager.Instance.Motion?.GetAxisState(_currentAxis);
                if (sts != null)
                {
                    selectedPt.Position = Math.Round(sts.ActualPosition, 3);
                    selectedPt.AxisIndex = _currentAxis;
                    dgTeachingPoints.Items.Refresh();
                    Motion_OnLogMessage($"[Teaching] Point '{selectedPt.Name}' position updated to {selectedPt.Position:F3} mm");
                }
            }
        }

        private async void BtnRunToPoint_Click(object sender, RoutedEventArgs e)
        {
            if (dgTeachingPoints.SelectedItem is TeachingPoint selectedPt)
            {
                btnRunToPoint.IsEnabled = false;
                try
                {
                    await MotionSequenceManager.Instance.MoveToPointAsync(selectedPt);
                }
                finally
                {
                    btnRunToPoint.IsEnabled = true;
                }
            }
        }

        private void BtnAddPoint_Click(object sender, RoutedEventArgs e)
        {
            int nextId = _teachingPoints.Count > 0 ? _teachingPoints.Max(p => p.Id) + 1 : 1;
            var sts = MotionSequenceManager.Instance.Motion?.GetAxisState(_currentAxis);
            double curPos = sts != null ? Math.Round(sts.ActualPosition, 3) : 0.0;

            _teachingPoints.Add(new TeachingPoint
            {
                Id = nextId,
                Name = $"Point {nextId}",
                AxisIndex = _currentAxis,
                Position = curPos,
                Speed = 100,
                StepType = "CheckVision",
                StepOrder = nextId,
                TriggerVision = false
            });
        }

        private void BtnDeletePoint_Click(object sender, RoutedEventArgs e)
        {
            if (dgTeachingPoints.SelectedItem is TeachingPoint selectedPt)
            {
                _teachingPoints.Remove(selectedPt);
            }
        }

        private void BtnSavePoints_Click(object sender, RoutedEventArgs e)
        {
            var cfg = MotionSequenceManager.Instance.Motion?.Config ?? new MotionConfig();
            cfg.TeachingPoints = _teachingPoints.ToList();
            SaveConfigToFile(cfg);
            MessageBox.Show("Teaching Points saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnReloadPoints_Click(object sender, RoutedEventArgs e)
        {
            LoadTeachingPoints();
            Motion_OnLogMessage("[Teaching] Teaching points reloaded.");
        }
        #endregion

        #region Simulation & Alarms
        private void ChkTestProgramMode_Changed(object sender, RoutedEventArgs e)
        {
            MotionSequenceManager.Instance.IsTestProgramMode = chkTestProgramMode.IsChecked == true;
            Motion_OnLogMessage($"[Test Program] Mock Simulation Mode: {(MotionSequenceManager.Instance.IsTestProgramMode ? "ENABLED" : "DISABLED")}");
        }

        private void CbMockVisionResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbMockVisionResult?.SelectedItem is ComboBoxItem item)
            {
                MotionSequenceManager.Instance.MockVisionResult = item.Content?.ToString() ?? "OK";
            }
        }

        private void CbMockRobotCmd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbMockRobotCmd?.SelectedItem is ComboBoxItem item)
            {
                MotionSequenceManager.Instance.MockRobotCommand = item.Content?.ToString() ?? "Start";
            }
        }

        private void BtnClearAlarm_Click(object sender, RoutedEventArgs e)
        {
            var motion = MotionSequenceManager.Instance.Motion;
            if (motion != null)
            {
                motion.ClearAlarm(_currentAxis);
                txtAlarmCode.Text = "0x0000 (Cleared)";
                txtAlarmCode.Foreground = TileGreenBrush;
                Motion_OnLogMessage($"[Alarm] Alarm cleared on Axis {_currentAxis}");
            }
        }

        private void BtnClearHistory_Click(object sender, RoutedEventArgs e)
        {
            _alarmHistory.Clear();
        }
        #endregion

        #region Advanced Machine Settings
        private void LoadMotionConfigToUI(MotionConfig specificConfig = null)
        {
            try
            {
                var cfg = specificConfig ?? MotionSequenceManager.Instance.Motion?.Config;
                if (cfg == null && File.Exists(Common.MotionConfigFile))
                {
                    string json = File.ReadAllText(Common.MotionConfigFile);
                    cfg = Newtonsoft.Json.JsonConvert.DeserializeObject<MotionConfig>(json);
                }

                if (cfg != null)
                {
                    var axisCfg = cfg.Axes?.FirstOrDefault(a => a.AxisIndex == _currentAxis) ?? new AxisConfig();

                    txtPulsePerUnit.Text = axisCfg.PulsesPerUnit.ToString(CultureInfo.InvariantCulture);
                    txtMaxVel.Text = axisCfg.MaxVelocity.ToString(CultureInfo.InvariantCulture);
                    txtMaxAcc.Text = axisCfg.DefaultProfile.Acceleration.ToString(CultureInfo.InvariantCulture);
                    txtMaxDec.Text = axisCfg.DefaultProfile.Deceleration.ToString(CultureInfo.InvariantCulture);

                    chkEnableSoftLimits.IsChecked = axisCfg.EnableSoftwareLimits;
                    txtSoftLimitPos.Text = axisCfg.SoftwareLimitPositive.ToString(CultureInfo.InvariantCulture);
                    txtSoftLimitNeg.Text = axisCfg.SoftwareLimitNegative.ToString(CultureInfo.InvariantCulture);

                    cboHomingMode.SelectedIndex = axisCfg.Homing.HomeMethod >= 0 && axisCfg.Homing.HomeMethod <= 3 ? axisCfg.Homing.HomeMethod : 0;
                    txtHomeHighSpeed.Text = axisCfg.Homing.HighVelocity.ToString(CultureInfo.InvariantCulture);
                    txtHomeLowSpeed.Text = axisCfg.Homing.LowVelocity.ToString(CultureInfo.InvariantCulture);
                    txtHomeOffset.Text = axisCfg.Homing.OffsetPulses.ToString(CultureInfo.InvariantCulture);
                    

                    // IO bit mapping
                    if (cfg.IO != null)
                    {
                        txtIoBitCylFwd.Text = cfg.IO.SensorForwardDIBit.ToString();
                        txtIoBitCylBwd.Text = cfg.IO.SensorBackwardDIBit.ToString();
                        txtIoBitCylDO.Text = cfg.IO.CylinderDOBit.ToString();
                        txtIoBitVacDO.Text = cfg.IO.VacuumDOBit.ToString();
                        txtIoBitVacSensor.Text = cfg.IO.VacuumSensorDIBit.ToString();
                        txtIoBitSystemStop.Text = cfg.IO.SystemStopDIBit.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Motion_OnLogMessage($"[Config Error] Load failed: {ex.Message}");
            }
        }

        private void BtnSaveMotionConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cfg = MotionSequenceManager.Instance.Motion?.Config ?? new MotionConfig();
                var axisCfg = cfg.Axes?.FirstOrDefault(a => a.AxisIndex == _currentAxis);
                if (axisCfg == null)
                {
                    axisCfg = new AxisConfig { AxisIndex = _currentAxis, AxisName = $"Axis {_currentAxis}" };
                    cfg.Axes.Add(axisCfg);
                }

                double.TryParse(txtPulsePerUnit.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double ppu);
                axisCfg.PulsesPerUnit = ppu > 0 ? ppu : 1000.0;

                double.TryParse(txtMaxVel.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double maxVel);
                axisCfg.MaxVelocity = maxVel > 0 ? maxVel : 50000;

                double.TryParse(txtMaxAcc.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double maxAcc);
                axisCfg.DefaultProfile.Acceleration = maxAcc > 0 ? maxAcc : 500000;

                double.TryParse(txtMaxDec.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double maxDec);
                axisCfg.DefaultProfile.Deceleration = maxDec > 0 ? maxDec : 500000;

                axisCfg.EnableSoftwareLimits = chkEnableSoftLimits.IsChecked == true;
                double.TryParse(txtSoftLimitPos.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double softPos);
                axisCfg.SoftwareLimitPositive = softPos;
                double.TryParse(txtSoftLimitNeg.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double softNeg);
                axisCfg.SoftwareLimitNegative = softNeg;

                axisCfg.Homing.HomeMethod = (short)Math.Max(0, cboHomingMode.SelectedIndex);
                double.TryParse(txtHomeHighSpeed.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double hSpd);
                axisCfg.Homing.HighVelocity = hSpd > 0 ? hSpd : 5000;
                double.TryParse(txtHomeLowSpeed.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double lSpd);
                axisCfg.Homing.LowVelocity = lSpd > 0 ? lSpd : 1000;
                int.TryParse(txtHomeOffset.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int hOff);
                axisCfg.Homing.OffsetPulses = hOff;

                // IO bit mapping
                if (cfg.IO == null) cfg.IO = new IOConfig();
                int.TryParse(txtIoBitCylFwd.Text, out int cylFwdDi); cfg.IO.SensorForwardDIBit = cylFwdDi;
                int.TryParse(txtIoBitCylBwd.Text, out int cylBwdDi); cfg.IO.SensorBackwardDIBit = cylBwdDi;
                int.TryParse(txtIoBitCylDO.Text, out int cylDo); cfg.IO.CylinderDOBit = cylDo;
                int.TryParse(txtIoBitVacDO.Text, out int vacDo); cfg.IO.VacuumDOBit = vacDo;
                int.TryParse(txtIoBitVacSensor.Text, out int vacDi); cfg.IO.VacuumSensorDIBit = vacDi;
                int.TryParse(txtIoBitSystemStop.Text, out int stopDi); cfg.IO.SystemStopDIBit = stopDi;

                SaveConfigToFile(cfg);
                MessageBox.Show("Advanced Machine Configuration saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save Config Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnReloadMotionConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(Common.MotionConfigFile))
                {
                    string json = File.ReadAllText(Common.MotionConfigFile);
                    var cfg = Newtonsoft.Json.JsonConvert.DeserializeObject<MotionConfig>(json);
                    if (cfg != null)
                    {
                        LoadMotionConfigToUI(cfg);
                        LoadTeachingPoints(cfg);
                        Motion_OnLogMessage("[Config] Configuration reloaded successfully from file.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Reload Config Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveConfigToFile(MotionConfig config)
        {
            string dir = Path.GetDirectoryName(Common.MotionConfigFile);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Common.MotionConfigFile, json);
            Motion_OnLogMessage("[Config] Saved to motion_config.json");
        }
        #endregion

        #region Activity Logs
        private void Motion_OnLogMessage(string msg)
        {
            Common.Info(msg);
            Dispatcher.InvokeAsync(() =>
            {
                if (txtMotionLogs == null) return;

                string timeStampedMsg = msg.StartsWith("[") ? msg : $"[{DateTime.Now:HH:mm:ss}] {msg}";
                _logLines.Enqueue(timeStampedMsg);

                while (_logLines.Count > MaxLogLines)
                {
                    _logLines.Dequeue();
                }

                txtMotionLogs.Text = string.Join(Environment.NewLine, _logLines);
                txtMotionLogs.ScrollToEnd();
            });
        }

        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            _logLines.Clear();
            txtMotionLogs.Clear();
        }

        private void BtnCopyLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtMotionLogs.Text);
            }
            catch
            {
            }
        }

        private void UpdateMasterStatusUI()
        {
            var motion = MotionSequenceManager.Instance.Motion;
            if (motion != null)
            {
                txtMasterStatus.Text = motion.IsMasterOp ? "OP (6)" : $"State {motion.MasterStatus}";
            }
        }
        #endregion

        #region I/O Monitor Logic
        private void InitIoList()
        {
            if (DiItems.Count > 0) return;

            var cfg = MotionSequenceManager.Instance.Motion?.Config?.IO;

            // Khởi tạo 16 cổng DI (Digital Inputs)
            string[] diNames = new string[16]
            {
                "Cylinder Forward Sensor ",
                "Cylinder Backward Sensor ",
                "Vacuum Pressure Sensor ",
                "System Stop / Safety Sensor ",
                "General Digital Input 04",
                "General Digital Input 05",
                "General Digital Input 06",
                "General Digital Input 07",
                "General Digital Input 08",
                "General Digital Input 09",
                "General Digital Input 10",
                "General Digital Input 11",
                "General Digital Input 12",
                "General Digital Input 13",
                "General Digital Input 14",
                "General Digital Input 15"
            };

            for (short i = 0; i < 16; i++)
            {
                DiItems.Add(new IoPinDisplayItem { Pin = i, Name = diNames[i], IsOutput = false });
            }

           
            string[] doNames = new string[16]
            {
                "Cylinder Solenoid ",
                "Vacuum Solenoid ",
                "General Digital Output 02",
                "General Digital Output 03",
                "General Digital Output 04",
                "General Digital Output 05",
                "General Digital Output 06",
                "General Digital Output 07",
                "General Digital Output 08",
                "General Digital Output 09",
                "General Digital Output 10",
                "General Digital Output 11",
                "General Digital Output 12",
                "General Digital Output 13",
                "General Digital Output 14",
                "General Digital Output 15"
            };

            for (short i = 0; i < 16; i++)
            {
                DoItems.Add(new IoPinDisplayItem { Pin = i, Name = doNames[i], IsOutput = true });
            }
        }

        private void BtnToggleDO_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is IoPinDisplayItem item)
            {
                var pcieIo = IoJobCtrl.GetIOcardCtrl();
                if (pcieIo != null && pcieIo.IsInit)
                {
                    bool newState = !item.State;
                    if (item.Pin >= 1 && item.Pin <= pcieIo.OutputChannels)
                    {
                        pcieIo.SetPinOutput(item.Pin, newState);
                        item.State = newState;
                    }
                    return;
                }

                // Fallback sang motion
                var motion = MotionSequenceManager.Instance.Motion;
                if (motion != null)
                {
                    bool newState = !item.State;
                    motion.SetDigitalOutput(item.Pin, newState);
                    item.State = newState;
                }
            }
        }

        #endregion

        /// <summary>
        /// Lấy tọa độ hiện tại của trục servo gán trực tiếp vào dòng được bấm trong bảng Teaching Points
        /// </summary>
        private void BtnGetPosRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TeachingPoint pt)
            {
                var sts = MotionSequenceManager.Instance.Motion?.GetAxisState(_currentAxis);
                if (sts != null)
                {
                    pt.Position = Math.Round(sts.ActualPosition, 3);
                    pt.AxisIndex = _currentAxis;
                    dgTeachingPoints.Items.Refresh();
                    Motion_OnLogMessage($"[Teaching] Đã cập nhật tọa độ cho điểm '{pt.Name}': {pt.Position:F3} mm");
                }
            }
        }
    }

    public class IoPinDisplayItem : System.ComponentModel.INotifyPropertyChanged
    {
        public short Pin { get; set; }
        public string PinLabel => (IsOutput ? "DO " : "DI ") + Pin.ToString("D2");
        public string Name { get; set; }
        public bool IsOutput { get; set; }

        private bool _state;
        public bool State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged(nameof(State));
                    OnPropertyChanged(nameof(StateBrush));
                    OnPropertyChanged(nameof(StateText));
                }
            }
        }

        public Brush StateBrush => State
            ? (IsOutput ? new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)) : new SolidColorBrush(Color.FromRgb(0x10, 0x7C, 0x41)))
            : new SolidColorBrush(Color.FromRgb(0x3F, 0x3F, 0x46));

        public string StateText => State ? "HIGH (1)" : "LOW (0)";

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop));
    }

}
