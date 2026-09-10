using BeeLib.Math;
using BeevisionSolution.Controller;
using BeevisionSolution.Models;
using BeevisionSolution.Utils;
using Cognex.DataMan.SDK;
using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.Exceptions;
using Cognex.VisionPro.Implementation;
using Cognex.VisionPro.ToolBlock;
using Microsoft.DwayneNeed.Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using static BeevisionSolution.Utils.Common;
using static BeevisionSolution.Utils.Constant;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Image = System.Drawing.Image;

namespace BeevisionSolution.Views
{
    public partial class RecordDisplayWpf : UserControl, IDisposable, INotifyPropertyChanged
    {
        private CogRecordDisplay dspMain;
        private FileSystemWatcher watcher;
        private DataManSystem dataManConn;
        private int _DisplayId;
        private string _ScreenWatermark;
        private int _Attempt = 0;
        private int _MaxRetries = 0;
        private double _alignTimeSeconds = double.NaN;
        private double _cycleTimeSeconds = double.NaN;
        private bool _isShowCrossLine;
        private bool disposed;
        private bool _isShowCrossLineButton = true;
        private readonly object _graphicsLock = new object();
        /// <summary>Giới hạn số segment vạch thước trên crosshair (tránh lag khi live).</summary>
        private const int CrosshairRulerMaxGraphics = 220;
        /// <summary>Cứ mỗi N bước lưới (theo stepPx) là một vạch major — đếm từ tâm ảnh để hai bên đối xứng.</summary>
        private const int CrosshairRulerMajorEvery = 5;
        /// <summary>Số segment CrossRuler_* đã add lần vẽ thành công trước (chỉ Remove đúng từng đó — tránh CogDisplayUnknownGroupNameException).</summary>
        private int _lastCrossRulerSegmentCount;
        /// <summary>Không xử lý Changed khi đang tự cập nhật StaticGraphics (tránh đệ quy).</summary>
        private bool _suppressCrossLineChanged;

        /// <summary>
        /// Tránh vòng lặp: cập nhật StaticGraphics → <see cref="DspMain_Changed"/> → vẽ lại.
        /// Bỏ qua vẽ khi layout/transform/chưa thay đổi so với lần vẽ thành công .
        /// </summary>
        private int? _lastCrossLineLayoutStamp;

        private void InvalidateCrossLineLayoutStamp() => _lastCrossLineLayoutStamp = null;

        /// <summary>Trộn bit double thành int; toàn bộ phép toán unchecked.</summary>
        private static int MixHashLong(long bits)
        {
            unchecked
            {
                uint hi = (uint)(bits >> 32);
                uint lo = (uint)bits;
                return (int)(hi * 397u ^ lo);
            }
        }

        /// <summary>Làm tròn trước khi hash để tránh jitter nhỏ từ MapPoint/transform làm stamp đổi liên tục.</summary>
        private static long DoubleToStableLongBits(double v)
        {
            if (double.IsNaN(v) || double.IsInfinity(v))
                return 0L;
            double q = Math.Round(v, 3);
            return BitConverter.DoubleToInt64Bits(q);
        }

        private int ComputeCrossLineLayoutStamp(ICogImage image)
        {
            var transform = image.PixelFromRootTransform;
            transform.MapPoint(0, 0, out var p0x, out var p0y);
            transform.MapPoint(image.Width, image.Height, out var p1x, out var p1y);
            unchecked
            {
                int h = 17;
                h = h * 31 + image.Width;
                h = h * 31 + image.Height;
                h = h * 31 + MixHashLong(DoubleToStableLongBits(p0x));
                h = h * 31 + MixHashLong(DoubleToStableLongBits(p0y));
                h = h * 31 + MixHashLong(DoubleToStableLongBits(p1x));
                h = h * 31 + MixHashLong(DoubleToStableLongBits(p1y));
                h = h * 31 + (Settings?.ShowCenterLine == true ? 1 : 0);
                h = h * 31 + (_isShowCrossLine ? 1 : 0);
                h = h * 31 + MixHashLong(DoubleToStableLongBits(GetCrosshairMeasureScaleMmPerPixel()));
                return h;
            }
        }

        /// <summary>True khi không cần vẽ lại (đã có stamp trùng lần vẽ thành công trước).</summary>
        private bool IsCrossLineLayoutUnchangedSinceLastDraw()
        {
            if (dspMain == null || dspMain.Image == null)
                return false;
            var image = dspMain.Image;
            if (image.Width <= 0 || image.Height <= 0)
                return false;
            int stamp = ComputeCrossLineLayoutStamp(image);
            return _lastCrossLineLayoutStamp.HasValue && _lastCrossLineLayoutStamp.Value == stamp;
        }

        private bool _soloLiveActive;
        private SoloLiveTriggerState _soloLiveState;
        private bool _showSoloLiveButton = true;
        private bool _showGrabImageButton;
        private bool _showMeasureButton;
        private bool _grabImageInProgress;

        public event Action<int, string, ICogImage> MeasureRequested;

        public bool IsShowSoloLiveButton
        {
            get => _showSoloLiveButton;
            set
            {
                _showSoloLiveButton = value;
                Dispatcher.Invoke(() =>
                {
                    if (btnSoloLive != null)
                        btnSoloLive.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                });
            }
        }

        public bool IsShowMeasureButton
        {
            get => _showMeasureButton;
            set
            {
                _showMeasureButton = value;
                Dispatcher.Invoke(() =>
                {
                    if (btnMeasureDistance != null)
                    {
                        btnMeasureDistance.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                    }
                });
            }
        }

        public bool IsShowGrabImageButton
        {
            get => _showGrabImageButton;
            set
            {
                _showGrabImageButton = value;
                Dispatcher.Invoke(() =>
                {
                    if (btnGrabImage != null)
                    {
                        btnGrabImage.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                    }
                });
            }
        }

        public bool Available { get; private set; } = false;
        public bool UpdateImageWithRecord { get; internal set; } = true;
        private ICogImage _InputImage = null;
        public string ScreenWatermark
        {
            get => _ScreenWatermark; set
            {
                _ScreenWatermark = value;
                Notify();
            }
        }

        Brush headerForeground = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0xFF));
        Brush okForeground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
        Brush ngForeground = new SolidColorBrush(Color.FromRgb(238, 75, 43));
        public bool IsShowCrossLine
        {
            get => _isShowCrossLine;
            set
            {
                _isShowCrossLine = value;
                if (_isShowCrossLine)
                {
                    DrawCrossLine();
                }
                else
                {
                    DrawCrossLine(true);//remove existed crossline
                }

            }
        }


        public bool IsShowCrossLineButton
        {
            get => _isShowCrossLineButton;
            set
            {
                _isShowCrossLineButton = value;
                if (_isShowCrossLineButton)
                {
                    btnCrossLine.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    btnCrossLine.Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }

        public int Attempt
        {
            get => _Attempt;
            set
            {
                _Attempt = value;
                Notify();
                Notify(nameof(RetryInfo)); 
            }
        }

        private double _ActualDist = double.NaN;
        public double ActualDist
        {
            get => _ActualDist;
            set
            {
                _ActualDist = value;
                Notify();
                Dispatcher.Invoke(() =>
                {
                    //if (txtActualDist != null)
                    //    txtActualDist.Visibility = double.IsNaN(_ActualDist) ? Visibility.Collapsed : Visibility.Visible;
                });
                // update crossline graphics if needed
                UpdateCrossLine();
            }
        }

        public int MaxRetries
        {
            get => _MaxRetries;
            set
            {
                _MaxRetries = value;
                Notify();
                Notify(nameof(RetryInfo)); 
            }
        }

        public string RetryInfo
        {
            get
            {
                if (_MaxRetries > 0)
                {
                    return $"Retry: {_Attempt}/{_MaxRetries}";
                }
                return string.Empty;
            }
        }

        public string TimingInfo
        {
            get
            {
                bool hasAlignTime = !double.IsNaN(_alignTimeSeconds);
                bool hasCycleTime = !double.IsNaN(_cycleTimeSeconds);

                if (hasAlignTime && hasCycleTime)
                {
                    return string.Format(
                        CultureInfo.InvariantCulture,
                        "Align: {0:F3} s  |  Cycle: {1:F3} s",
                        _alignTimeSeconds,
                        _cycleTimeSeconds);
                }

                if (hasAlignTime)
                {
                    return string.Format(
                        CultureInfo.InvariantCulture,
                        "Align: {0:F3} s",
                        _alignTimeSeconds);
                }

                if (hasCycleTime)
                {
                    return string.Format(
                        CultureInfo.InvariantCulture,
                        "Cycle: {0:F3} s",
                        _cycleTimeSeconds);
                }

                return string.Empty;
            }
        }

        public void SetAlignTime(double alignTimeSeconds)
        {
            if (alignTimeSeconds >= 0 &&
                !double.IsNaN(alignTimeSeconds) &&
                !double.IsInfinity(alignTimeSeconds))
            {
                _alignTimeSeconds = alignTimeSeconds;
            }
            else
            {
                _alignTimeSeconds = double.NaN;
            }

            Notify(nameof(TimingInfo));
        }

        public void SetCycleTime(double cycleTimeSeconds)
        {
            if (cycleTimeSeconds >= 0 &&
                !double.IsNaN(cycleTimeSeconds) &&
                !double.IsInfinity(cycleTimeSeconds))
            {
                _cycleTimeSeconds = cycleTimeSeconds;
            }
            else
            {
                _cycleTimeSeconds = double.NaN;
            }

            Notify(nameof(TimingInfo));
        }



        public int DisplayId
        {
            get => _DisplayId;
            internal set
            {
                _DisplayId = value;
                Notify();
            }
        }
        public void SetImage(ICogImage image, int displayId)
        {
            if (Available)
            {
                Dispatcher.Invoke(() =>
                {
                    _InputImage = image;
                    DisplayId = displayId;
                    dspMain.Image = _InputImage;
                    dspMain.Record = new CogRecord() { Content = _InputImage };
                    dspMain.BackColor = System.Drawing.Color.Black;
                    dspMain.Invalidate();

                    if (IsShowCrossLine)
                    {
                        DrawCrossLine();
                    }
                });
            }
        }

        public RecordDisplayWpf()
        {
            InitializeComponent();
            ScreenWatermark = "Beevision Solution";
            DisplayId = 0;
            Attempt = 0;
            MaxRetries = 0;
            ActualDist = double.NaN;
            DataContext = this;
            dspMain = new CogRecordDisplay();
            dspMain.HandleCreated += dspMain_HandleCreated;
            hostParent.Child = dspMain;
        }

        public Image OverlayImage => dspMain.CreateContentBitmap(Cognex.VisionPro.Display.CogDisplayContentBitmapConstants.Display);

        public void SetDisplayContentX(ICogImage image, ICogRecord record)
        {
            _InputImage = image;

            record.Content = _InputImage;
            dspMain.Image = _InputImage;
            dspMain.Record = record;
            UpdateCrossLine();
        }

        public void Clear()
        {
            Dispatcher.Invoke(() =>
            {
                _InputImage = null;
                txtFooter.Text = string.Empty;
                txtMark?.Inlines.Clear();
                ActualDist = double.NaN;
                SetAlignTime(double.NaN);
                SetCycleTime(double.NaN);
                if (Available) dspMain.Image = null;
            });
        }
        //set watermark text
        public void SetWatermark(string strWatermark)
        {
            ScreenWatermark = strWatermark;
        }

        public void SetRetryInfo(int attempt, int maxRetries)
        {
            Dispatcher.Invoke(() =>
            {
                Attempt = attempt;
                MaxRetries = maxRetries;
            });
        }
        public void SetFooterContentX(List<object> dspItems)
        {
            txtMark?.Inlines.Clear();
            txtCore.Text = "0.000";

            if ((null != dspItems) && (dspItems.Count > 0))
            {
                var lst = new List<Inline>();
                var count = 0;
                var isOverallNg = false;

                foreach (var item in dspItems)
                {
                    // pose for mark
                    if (item is Pose pose)
                    {
                        SetMarkPose(pose);
                        continue;
                    }
                    // check if double and not NaN for score
                    if (item is double score && !double.IsNaN(score))
                    {
                        double scorePercent = score * 100.0;
                        txtCore.Text = scorePercent.ToString("0.000", CultureInfo.InvariantCulture);
                        continue;
                    }
                    if (item is bool b && b == false)
                    {
                        isOverallNg = true;
                    }
                    if (count > 0)
                        lst.Add(MajorSeparator);

                    lst.Add(GetInline(item));
                    count++;
                }

                if (isOverallNg)
                {
                    // NG should not keep previous pose/score on screen.
                    txtMark?.Inlines.Clear();
                    txtCore.Text = "0.000";
                }

                txtFooter.Inlines.Clear();
                txtFooter.Inlines.AddRange(lst);
            }
        }
        private void SetMarkPose(Pose p)
        {
            txtMark.Inlines.Clear();

            //txtMark.Inlines.Add(new Run((string)TryFindResource("strCoordinates")));
            txtMark.Inlines.Add(new Run(
                $"{StdFormat(p.X)}, {StdFormat(p.Y)}, {StdFormat(p.Th)}")
            {
                Foreground = okForeground
            });
        }
        private ICollection<Inline> GetInlines(Pose p)
        {
            return new List<Inline> {
                new Run("X: ") { Foreground = headerForeground }, new Run(StdFormat(p.X)), MinorSeparator,
                new Run("Y: ") { Foreground = headerForeground }, new Run(StdFormat(p.Y)), MinorSeparator,
                new Run("A: ") { Foreground = headerForeground }, new Run(StdFormat(p.Th))
            };
        }

        private Inline GetInline(object obj)
        {
            if (obj is string s)
                return new Run(s);

            if (obj is bool b)
            {
                if (b == false)
                {
                    return new Run("NG") { Foreground = ngForeground };
                }
                if (b == true)
                {
                    return new Run("OK") { Foreground = okForeground };
                }
            }

            if (obj is ValueType v)
                return new Run(StdFormat(v));

            return new Run("NA.");
        }

        private void dspMain_HandleCreated(object sender, EventArgs e)
        {
            dspMain.AutoFit = true;
            dspMain.MouseMode = Cognex.VisionPro.Display.CogDisplayMouseModeConstants.Pan;
            dspMain.HorizontalScrollBar = false;
            dspMain.VerticalScrollBar = false;
            try
            {
                dspMain.BackColor = System.Drawing.Color.Black;
                dspMain.Invalidate();
            }
            catch (Exception ex)
            {
                Common.Bug($"Failed to set BackColor: {ex.Message}");
            }
            Available = true;
            dspMain.Changed += DspMain_Changed;
        }

        private void DspMain_Changed(object sender, CogChangedEventArgs e)
        {
            if (_suppressCrossLineChanged) return;
            if (!Settings.ShowCenterLine && !_isShowCrossLine) return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_suppressCrossLineChanged) return;
                if (!Available || dspMain == null) return;
                if (!Settings.ShowCenterLine && !_isShowCrossLine) return;
                if (dspMain.Image == null) return;

                // Không queue vẽ lại khi layout/transform đã trùng — tránh gọi DrawCrossLineSegmentsCore liên tục từ Changed
                if (IsCrossLineLayoutUnchangedSinceLastDraw())
                    return;

                DrawCrossLineSegmentsCore();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        public void SetDisplayContent(ICogImage image, ICogRecord record)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (Available)
                {
                    _InputImage = image;

                    record.Content = _InputImage;
                    dspMain.Image = _InputImage;
                    dspMain.Record = record;
                    dspMain.BackColor = System.Drawing.Color.Black;
                    dspMain.Invalidate();

                    UpdateCrossLine();
                    if (IsShowCrossLine)
                    {
                        lock(_graphicsLock)
                        {
                            DrawCrossLine();
                        }
                    }

                }
            }));
        }

        public void SetFooterContent(List<object> dspItems)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                SetFooterContentX(dspItems);
            }));
        }
        public void StartLiveDisplay(ICogAcqFifo fifo, bool continuous)
        {
            if (dspMain != null)
            {
                try
                {
                    dspMain.StartLiveDisplay(fifo, continuous);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (Settings.ShowCenterLine || _isShowCrossLine)
                            RefreshCrossLine();
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
                catch (Exception ex)
                {
                    Common.Bug($"StartLiveDisplay error: {ex.Message}");
                }
            }
        }

        public void StopLiveDisplay()
        {
            if (dspMain != null)
            {
                try
                {
                    dspMain.StopLiveDisplay();
                }
                catch (Exception ex)
                {
                    Common.Bug($"StopLiveDisplay error: {ex.Message}");
                }
            }

            if (_soloLiveActive)
            {
                _soloLiveActive = false;
                var state = _soloLiveState;
                _soloLiveState = null;
                RestoreSoloLiveFifoTrigger(state);
            }

            UpdateSoloLiveVisual(false);
        }

        private void BtnSoloLive_Click(object sender, RoutedEventArgs e)
        {
            if (_soloLiveActive)
                StopLiveDisplay();
            else
                TryStartSoloLiveForThisDisplay();
        }

        private async void BtnGrabImage_Click(object sender, RoutedEventArgs e)
        {
            if (_grabImageInProgress)
            {
                return;
            }

            CameraJob cameraJob = JobController.GetCameraJobByDisplayId(DisplayId);
            if (cameraJob == null)
            {
                MessageBox.Show(
                    "No unique camera job is configured for Display " + DisplayId + ".",
                    "Grab Image",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (_soloLiveActive)
            {
                MessageBox.Show(
                    "Stop Live mode before grabbing a single image.",
                    "Grab Image",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            List<PlcCam> plcCameraJobs = JobController.GetAllPlcCam();
            if (plcCameraJobs != null && plcCameraJobs.Any(job => job != null && job.IsProcessing))
            {
                MessageBox.Show(
                    "A PLC vision cycle is running. Wait until the cycle is complete.",
                    "Grab Image",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!cameraJob.Initialized)
            {
                MessageBox.Show(
                    "The camera job is not initialized: " + cameraJob.Name,
                    "Grab Image",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!cameraJob.Available)
            {
                MessageBox.Show(
                    "The camera is busy: " + cameraJob.Name,
                    "Grab Image",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            _grabImageInProgress = true;
            btnGrabImage.IsEnabled = false;

            try
            {
                bool grabSucceeded = await cameraJob.RunToolAsync();
                ICogImage grabbedImage = cameraJob.OutputImage as ICogImage;

                if (!grabSucceeded ||
                    cameraJob.RunStatus != CogToolResultConstants.Accept ||
                    grabbedImage == null)
                {
                    Common.Bug(
                        "Manual grab failed. DisplayId={0}, CameraJob={1}, Status={2}",
                        DisplayId,
                        cameraJob.Name,
                        cameraJob.RunStatus);

                    MessageBox.Show(
                        "Cannot grab an image from camera job: " + cameraJob.Name,
                        "Grab Image",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                if (!disposed)
                {
                    ClearResultDataAfterManualGrab();
                    SetImage(grabbedImage, DisplayId);
                }
            }
            catch (Exception ex)
            {
                Common.Bug(
                    "Manual grab exception. DisplayId={0}, CameraJob={1}, Error={2}",
                    DisplayId,
                    cameraJob.Name,
                    ex.Message);

                MessageBox.Show(
                    "Grab Image failed: " + ex.Message,
                    "Grab Image",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                _grabImageInProgress = false;
                if (!disposed && btnGrabImage != null)
                {
                    btnGrabImage.IsEnabled = true;
                }
            }
        }

        private void ClearResultDataAfterManualGrab()
        {
            txtMark.Inlines.Clear();
            txtFooter.Inlines.Clear();
            txtCore.Text = "--";
            txtTactTime.Text = string.Empty;

            ActualDist = double.NaN;
            Attempt = 0;
            MaxRetries = 0;
            SetAlignTime(double.NaN);
            SetCycleTime(double.NaN);
        }

        private void BtnMeasureDistance_Click(object sender, RoutedEventArgs e)
        {
            ICogImage currentImage = null;
            if (dspMain != null)
            {
                currentImage = dspMain.Image;
            }

            if (currentImage == null)
            {
                currentImage = _InputImage;
            }

            MeasureRequested?.Invoke(DisplayId, ScreenWatermark, currentImage);
        }

        /// <summary>Màu icon: xám khi tắt, đỏ khi đang live solo.</summary>
        private void UpdateSoloLiveVisual(bool liveActive)
        {
            if (iconSoloLive == null) return;
            iconSoloLive.Foreground = liveActive
                ? new SolidColorBrush(Color.FromRgb(0xFF, 0x44, 0x44))
                : new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
        }

        private void TryStartSoloLiveForThisDisplay()
        {
            if (!Available || dspMain == null)
            {
                UpdateSoloLiveVisual(false);
                return;
            }

            var camJob = JobController.GetCameraJobByDisplayId(DisplayId);
            if (camJob == null || !camJob.Initialized)
            {
                MessageBox.Show(
                    (string)TryFindResource("msgSoloLiveNoJob") ?? "No camera job for this display.",
                    (string)TryFindResource("strLive") ?? "Live",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                UpdateSoloLiveVisual(false);
                return;
            }

            var toolBlock = camJob.ToolBlock as CogToolBlock;
            CogAcqFifoTool acqTool = null;
            try
            {
                acqTool = toolBlock?.Tools.OfType<CogAcqFifoTool>().FirstOrDefault();
            }
            catch (Exception ex)
            {
                Common.Bug($"Solo live acq tool read: {ex.Message}");
            }

            if (acqTool?.Operator == null)
            {
                MessageBox.Show(
                    (string)TryFindResource("msgSoloLiveNoJob") ?? "No acquire FIFO.",
                    (string)TryFindResource("strLive") ?? "Live",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                UpdateSoloLiveVisual(false);
                return;
            }

            var fifo = acqTool.Operator;
            object prevModel = null;
            var switched = false;
            try
            {
                const bool shouldSwitchToFreeRun = true;
                if (shouldSwitchToFreeRun)
                {
                    TryGetTriggerModelFifo(fifo, out prevModel);
                    switched = TrySetTriggerModelByNamesFifo(fifo, "FreeRun", "Freerun", "Continuous");
                    if (!switched)
                        Common.Bug($"Solo live: cannot set FreeRun for display {DisplayId}");
                    else
                        Thread.Sleep(80);
                }

                StartLiveDisplay(fifo, true);
                _soloLiveState = new SoloLiveTriggerState
                {
                    AcqTool = acqTool,
                    PreviousTriggerModel = prevModel,
                    SwitchedToFreeRun = switched
                };
                _soloLiveActive = true;
                UpdateSoloLiveVisual(true);
            }
            catch (Exception ex)
            {
                Common.Bug($"Solo live start display {DisplayId}: {ex.Message}");
                _soloLiveState = null;
                _soloLiveActive = false;
                UpdateSoloLiveVisual(false);
                MessageBox.Show(ex.Message, (string)TryFindResource("strLive") ?? "Live", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestoreSoloLiveFifoTrigger(SoloLiveTriggerState state)
        {
            if (state?.AcqTool?.Operator == null || !state.SwitchedToFreeRun)
                return;

            try
            {
                var restored = false;
                if (state.PreviousTriggerModel != null)
                    restored = TrySetTriggerModelFifo(state.AcqTool.Operator, state.PreviousTriggerModel);

                if (!restored)
                    restored = TrySetTriggerModelByNamesFifo(state.AcqTool.Operator, "Manual", "OneShot", "Software");

                if (!restored)
                    Common.Bug($"Solo live: cannot restore trigger for display {DisplayId}.");
            }
            catch (Exception ex)
            {
                Common.Bug($"Solo live restore trigger display {DisplayId}: {ex.Message}");
            }
        }

        private sealed class SoloLiveTriggerState
        {
            public CogAcqFifoTool AcqTool { get; set; }
            public object PreviousTriggerModel { get; set; }
            public bool SwitchedToFreeRun { get; set; }
        }

        private static bool TryGetTriggerModelFifo(ICogAcqFifo fifo, out object triggerModel)
        {
            triggerModel = null;
            if (fifo == null) return false;
            var triggerParams = fifo.GetType().GetProperty("OwnedTriggerParams")?.GetValue(fifo);
            if (triggerParams == null) return false;
            var modelProp = triggerParams.GetType().GetProperty("TriggerModel");
            if (modelProp == null) return false;
            triggerModel = modelProp.GetValue(triggerParams);
            return triggerModel != null;
        }

        private static bool TrySetTriggerModelFifo(ICogAcqFifo fifo, object triggerModel)
        {
            if (fifo == null || triggerModel == null) return false;
            var triggerParams = fifo.GetType().GetProperty("OwnedTriggerParams")?.GetValue(fifo);
            if (triggerParams == null) return false;
            var modelProp = triggerParams.GetType().GetProperty("TriggerModel");
            if (modelProp == null || !modelProp.PropertyType.IsEnum) return false;

            try
            {
                object valueToSet = triggerModel;
                if (triggerModel.GetType() != modelProp.PropertyType)
                    valueToSet = Enum.Parse(modelProp.PropertyType, triggerModel.ToString(), ignoreCase: true);
                modelProp.SetValue(triggerParams, valueToSet);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TrySetTriggerModelByNamesFifo(ICogAcqFifo fifo, params string[] enumNames)
        {
            if (fifo == null) return false;
            var triggerParams = fifo.GetType().GetProperty("OwnedTriggerParams")?.GetValue(fifo);
            if (triggerParams == null) return false;
            var modelProp = triggerParams.GetType().GetProperty("TriggerModel");
            if (modelProp == null || !modelProp.PropertyType.IsEnum) return false;

            foreach (var enumName in enumNames)
            {
                try
                {
                    var enumValue = Enum.Parse(modelProp.PropertyType, enumName, ignoreCase: true);
                    modelProp.SetValue(triggerParams, enumValue);
                    return true;
                }
                catch
                {
                    // next
                }
            }
            return false;
        }

        public ICogImage InputImage
        {
            get => _InputImage;
            internal set
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    if (Available)
                    {
                        _InputImage = value;
                        if (!UpdateImageWithRecord)
                        {
                            dspMain.Record = new CogRecord() { Content = _InputImage };
                            dspMain.Image = _InputImage;
                            UpdateImageWithRecord = true;
                            dspMain.BackColor = System.Drawing.Color.Black;
                            dspMain.Invalidate();
                            UpdateCrossLine();
                        }
                    }
                }));
            }
        }

        public ICogRecord Record
        {
            get => dspMain.Record;

            internal set
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    if ((null != _InputImage) && Available)
                    {
                        value.Content = _InputImage;
                        dspMain.Record = value;
                        dspMain.Image = _InputImage;
                        dspMain.BackColor = System.Drawing.Color.Black;
                        dspMain.Invalidate();
                        UpdateCrossLine();
                    }
                }));
            }
        }

        public void SetStaticGraphics(List<CogPointMarker> markers)
        {
            if (Available && (null != markers) && (markers.Count > 0) && (null != _InputImage))
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    foreach (var m in markers)
                    {
                        m.Color = Settings.CrosshairColor;
                        m.SizeInScreenPixels = Settings.GraphicSize;
                        m.SelectedSpaceName = _InputImage.SelectedSpaceName;
                        dspMain.StaticGraphics.Add(m, "cross");
                    }
                }));
            }
        }

        public void SetStaticGraphics(CogPointMarker marker)
        {
            if (Available && (null != marker) && (null != _InputImage))
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    marker.Color = Settings.CrosshairColor;
                    marker.SizeInScreenPixels = Settings.GraphicSize;
                    marker.SelectedSpaceName = _InputImage.SelectedSpaceName;
                    dspMain.StaticGraphics.Add(marker, "cross");
                }));
            }
        }

        public void SetStaticGraphics(ICogGraphic graph)
        {
            if (Available && (null != graph) && (null != _InputImage))
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    dspMain.StaticGraphics.Add(graph, "graph");
                }));
            }
        }

        public void ClearStaticGraphics()
        {
            if (Available)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    dspMain.StaticGraphics.Clear();
                    _lastCrossRulerSegmentCount = 0;
                }));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    MeasureRequested = null;
                    Dispatcher?.Invoke(CleanGUI);
                }
                dspMain = null;
                hostParent = null;
                disposed = true;
            }
        }

        void CleanGUI()
        {
            try
            {
                if (dspMain != null)
                    dspMain.Changed -= DspMain_Changed;
            }
            catch { }
            dspMain?.Dispose();
            hostParent?.Dispose();
        }

        ~RecordDisplayWpf()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify([CallerMemberName] string strPropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyName));
        }
        private void UpdateCrossLine()
        {
            if (!Available) return;

            Dispatcher.Invoke(() =>
            {
                // Live view chỉ cập nhật dspMain.Image; _InputImage có thể null — vẫn cần vẽ lại cross.
                if (_InputImage == null && dspMain.Image == null)
                    return;

                dspMain.StaticGraphics.Clear();
                InvalidateCrossLineLayoutStamp();
                _lastCrossRulerSegmentCount = 0;

                if (Settings.ShowCenterLine || _isShowCrossLine)
                {
                    DrawCrossLine();
                }
            });
        }
        public void RefreshCrossLine()
        {
            UpdateCrossLine();
        }

        private static double GetCrosshairMeasureScaleMmPerPixel()
        {
            try
            {
                if (Settings != null && Settings.CrossLineScale > 0)
                    return Settings.CrossLineScale;
            }
            catch { }

            return 1.0;
        }

        private static double SelectNiceMmStep(double approxMm)
        {
            if (approxMm <= 0 || double.IsNaN(approxMm) || double.IsInfinity(approxMm))
                return 1.0;

            double[] candidates = { 0.1, 0.2, 0.5, 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000 };
            foreach (var c in candidates)
            {
                if (c >= approxMm * 0.95)
                    return c;
            }

            return Math.Max(approxMm, 1.0);
        }

        private static double ComputeCrosshairRulerStepPx(double imageWidth, double imageHeight, double mmPerPixel)
        {
            double halfMaxPx = Math.Max(imageWidth, imageHeight) * 0.5;
            const int targetTicksPerHalfAxis = 18;
            const double minStepPx = 6.0;
            double maxStepPx = Math.Max(minStepPx, Math.Min(imageWidth, imageHeight));

            if (mmPerPixel > 0 && halfMaxPx > 0)
            {
                double halfMaxMm = halfMaxPx * mmPerPixel;
                double approxStepMm = halfMaxMm / targetTicksPerHalfAxis;
                double stepMm = SelectNiceMmStep(Math.Max(approxStepMm, 0.05));
                double stepPx = stepMm / mmPerPixel;
                return Math.Max(minStepPx, Math.Min(stepPx, maxStepPx));
            }

            double fallback = Math.Max(minStepPx, halfMaxPx / targetTicksPerHalfAxis);
            return Math.Max(minStepPx, Math.Min(fallback, maxStepPx));
        }

        private void RemoveCrosshairRulerGraphics()
        {
            if (dspMain == null) return;
            int n = _lastCrossRulerSegmentCount;
            if (n <= 0) return;
            for (int i = 0; i < n; i++)
            {
                try { dspMain.StaticGraphics.Remove($"CrossRuler_{i}"); } catch { }
            }
            _lastCrossRulerSegmentCount = 0;
        }

        private void TeardownOrphanCrosshairRulerSlots()
        {
            if (dspMain == null) return;
            for (int i = 0; i < CrosshairRulerMaxGraphics; i++)
            {
                try { dspMain.StaticGraphics.Remove($"CrossRuler_{i}"); }
                catch (CogDisplayUnknownGroupNameException) { }
                catch { /* best-effort cleanup */ }
            }
            _lastCrossRulerSegmentCount = 0;
        }

        /// <summary>
        /// Thêm hai đoạn CrossLineH/V và vạch thước (theo <see cref="AppSettings.CrossLineScale"/> mm/pixel).
        /// Dùng sau mỗi frame live qua <see cref="DspMain_Changed"/>.
        /// </summary>
        private void DrawCrossLineSegmentsCore()
        {
            if (dspMain == null || dspMain.Image == null) return;

            Cognex.VisionPro.ICogImage image = dspMain.Image;
            if (image.Width <= 0 || image.Height <= 0)
                return;

            int layoutStamp = ComputeCrossLineLayoutStamp(image);
            if (_lastCrossLineLayoutStamp.HasValue && _lastCrossLineLayoutStamp.Value == layoutStamp)
                return;

            CogColorConstants drawGridColor = CogColorConstants.Blue;
            int drawLineWidth = 1;
            if (drawLineWidth == 0)
                drawLineWidth = 1;

            _suppressCrossLineChanged = true;
            bool drewOk = false;
            try
            {
                RemoveCrosshairRulerGraphics();

                CogLineSegment lineH = new CogLineSegment();
                lineH.LineStyle = CogGraphicLineStyleConstants.Solid;
                double sx = 0.0;
                double sy = image.Height / 2;
                double ex = image.Width;
                double ey = image.Height / 2;

                var transform = dspMain.Image.PixelFromRootTransform;
                double outx, outy, outEX, outEY;
                transform.MapPoint(sx, sy, out outx, out outy);
                transform.MapPoint(ex, ey, out outEX, out outEY);

                lineH.StartX = outx;
                lineH.StartY = outy;
                lineH.EndX = outEX;
                lineH.EndY = outEY;

                lineH.Color = drawGridColor;
                lineH.LineWidthInScreenPixels = drawLineWidth;

                lineH.SelectedSpaceName = "#";
                dspMain.StaticGraphics.Add(lineH, "CrossLineH");

                CogLineSegment lineV = new CogLineSegment();
                lineV.LineStyle = CogGraphicLineStyleConstants.Solid;
                double vx1 = image.Width / 2;
                double vy1 = 0.0;
                double vx2 = image.Width / 2;
                double vy2 = image.Height;
                double vOutX1, vOutY1, vOutX2, vOutY2;
                transform.MapPoint(vx1, vy1, out vOutX1, out vOutY1);
                transform.MapPoint(vx2, vy2, out vOutX2, out vOutY2);
                lineV.StartX = vOutX1;
                lineV.StartY = vOutY1;
                lineV.EndX = vOutX2;
                lineV.EndY = vOutY2;
                lineV.Color = drawGridColor;
                lineV.LineWidthInScreenPixels = drawLineWidth;
                lineV.SelectedSpaceName = "#";
                dspMain.StaticGraphics.Add(lineV, "CrossLineV");

                double scaleMmPerPx = GetCrosshairMeasureScaleMmPerPixel();
                double stepPx = ComputeCrosshairRulerStepPx(image.Width, image.Height, scaleMmPerPx);
                double cx = image.Width * 0.5;
                double cy = image.Height * 0.5;
                double minorLen = Math.Max(4.0, Math.Min(image.Height, image.Width) * 0.012);
                double majorLen = minorLen * 1.75;
                int rulerSlot = 0;

                void addRulerTick(double p1x, double p1y, double p2x, double p2y, CogColorConstants tickColor, int tickWidth)
                {
                    if (rulerSlot >= CrosshairRulerMaxGraphics) return;
                    transform.MapPoint(p1x, p1y, out var ox1, out var oy1);
                    transform.MapPoint(p2x, p2y, out var ox2, out var oy2);
                    var seg = new CogLineSegment
                    {
                        LineStyle = CogGraphicLineStyleConstants.Solid,
                        StartX = ox1,
                        StartY = oy1,
                        EndX = ox2,
                        EndY = oy2,
                        Color = tickColor,
                        LineWidthInScreenPixels = tickWidth,
                        SelectedSpaceName = "#"
                    };
                    dspMain.StaticGraphics.Add(seg, $"CrossRuler_{rulerSlot}");
                    rulerSlot++;
                }

                // Vạch ngang: duyệt từ tâm ra hai bên (đối xứng hoàn toàn).
                for (int k = 1; cx + k * stepPx <= image.Width || cx - k * stepPx >= 0; k++)
                {
                    if (rulerSlot >= CrosshairRulerMaxGraphics) break;
                    bool major = k % CrosshairRulerMajorEvery == 0;
                    double len = major ? majorLen : minorLen;
                    int tw = major ? Math.Max(drawLineWidth, 2) : drawLineWidth;

                    double xr = cx + k * stepPx;
                    if (xr <= image.Width)
                        addRulerTick(xr, cy - len, xr, cy + len, drawGridColor, tw);

                    double xl = cx - k * stepPx;
                    if (xl >= 0)
                        addRulerTick(xl, cy - len, xl, cy + len, drawGridColor, tw);
                }

                // Vạch dọc: duyệt từ tâm ra hai bên (đối xứng hoàn toàn).
                for (int k = 1; cy + k * stepPx <= image.Height || cy - k * stepPx >= 0; k++)
                {
                    if (rulerSlot >= CrosshairRulerMaxGraphics) break;
                    bool major = k % CrosshairRulerMajorEvery == 0;
                    double len = major ? majorLen : minorLen;
                    int tw = major ? Math.Max(drawLineWidth, 2) : drawLineWidth;

                    double yd = cy + k * stepPx;
                    if (yd <= image.Height)
                        addRulerTick(cx - len, yd, cx + len, yd, drawGridColor, tw);

                    double yu = cy - k * stepPx;
                    if (yu >= 0)
                        addRulerTick(cx - len, yu, cx + len, yu, drawGridColor, tw);
                }

                _lastCrossRulerSegmentCount = rulerSlot;
                drewOk = true;
            }
            catch (Exception ex)
            {
                TeardownOrphanCrosshairRulerSlots();
                InvalidateCrossLineLayoutStamp();
                Common.Bug($"DrawCrossLine: {ex.Message}");
            }
            finally
            {
                _suppressCrossLineChanged = false;
            }

            if (drewOk)
                _lastCrossLineLayoutStamp = layoutStamp;
        }

        public void DrawCrossLine(bool isRemove = false)
        {
            if (dspMain == null)
                return;

            if (isRemove)
            {
                _suppressCrossLineChanged = true;
                try
                {
                    try
                    {
                        dspMain.StaticGraphics.Clear();
                    }
                    catch { }
                }
                finally
                {
                    _suppressCrossLineChanged = false;
                    _isShowCrossLine = false;
                    InvalidateCrossLineLayoutStamp();
                    _lastCrossRulerSegmentCount = 0;
                }
                return;
            }

            if (dspMain.Image != null)
                DrawCrossLineSegmentsCore();
        }

        

        private void btnCrossLine_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsShowCrossLine = !IsShowCrossLine;
        }

        private void btnOpCallNG_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //grvOpcall.Visibility = System.Windows.Visibility.Hidden;
            //callback result to main thread, return NG
        }

        private void btnManual_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //btnSetManual.IsEnabled = true;
            //enable region for manual marking
        }

        private void btnSetManual_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //grvOpcall.Visibility = System.Windows.Visibility.Hidden;
            //callback result to main thread, return pose from manual
        }

        private void btnTrigger_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        }
    }
}
