using BeeLib.Math;
using BeevisionSolution.Controller;
using BeevisionSolution.Models;
using BeevisionSolution.Utils;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.Caliper;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using DocumentFormat.OpenXml.Vml.Office;

namespace BeevisionSolution.Views
{
    /// <summary>
    /// Interaction logic for OpCallWindow.xaml
    /// </summary>
    public partial class OpCallWindow : Window
    {
        private CogRecordDisplay dspMain;
        private FunctionJob _job;
        public bool IsNeedRetry = false;
        public double X;
        public double Y;
        public double Rotation;
        internal bool IsConfirmed { get; private set; }
        public static event EventHandler<FunctionJob> ResultUpdated;
        private DispatcherTimer _updateTimer;
        private bool _isCoordinatedMultiMarkMode;
        private ICogImage _pendingImage;

        // Thay đổi từ CogRectangleAffine sang CogPointMarker
        private CogPointMarker _interactivePoint = null;

        public OpCallWindow()
        {
            InitializeComponent();
            DataContext = this;
            dspMain = new CogRecordDisplay();
            dspMain.HandleCreated += dspMain_HandleCreated;
            hostParent.Child = dspMain;

            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(50);

            _updateTimer.Start();
        }

        public void SetJob(FunctionJob job)
        {
            SetJobContent(job, null);
        }

        internal void SetCoordinatedMultiMarkJob(FunctionJob job, ICogImage image)
        {
            _isCoordinatedMultiMarkMode = true;
            IsConfirmed = false;
            btnGrabImage.Visibility = Visibility.Collapsed;
            SetJobContent(job, image);
        }

        private void SetJobContent(FunctionJob job, ICogImage image)
        {
            _job = job;
            _pendingImage = image ?? job?.InputImage as ICogImage;
            TryDisplayPendingImage();
            if (txtJobName != null)
            {
                txtJobName.Text = job?.Name ?? "Unknown";
            }
        }

        private void TryDisplayPendingImage()
        {
            if (_pendingImage == null || dspMain == null || !dspMain.IsHandleCreated)
            {
                return;
            }

            SetDisplayContent(_pendingImage);
        }

        private void SetDisplayContent(ICogImage image)
        {
            try
            {
                if (image != null)
                {
                    Dispatcher.Invoke((Action)(() =>
                    {
                        dspMain.Image = image;
                        dspMain.Fit(true);
                    }));
                }
            }
            catch (Exception ex)
            {
                Common.Info(ex.Message);
            }
        }

        private void dspMain_HandleCreated(object sender, EventArgs e)
        {
            dspMain.AutoFit = true;
            dspMain.MouseMode = Cognex.VisionPro.Display.CogDisplayMouseModeConstants.Pointer;
            dspMain.HorizontalScrollBar = false;
            dspMain.VerticalScrollBar = false;
            dspMain.DrawingEnabled = true;
            dspMain.BackColor = System.Drawing.Color.Black;
            TryDisplayPendingImage();
            dspMain.Invalidate();
        }

        private void btnOpCallNG_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //callback result to main thread, return NG
            IsConfirmed = false;
            if (!_isCoordinatedMultiMarkMode)
            {
                this.DialogResult = false;
            }
            this.Close();
        }

        /// <summary>
        /// Retry
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGrabImage_Click(object sender, RoutedEventArgs e)
        {
            var camJob = JobController.GetCameraJob(_job.CamSettings.CameraId);
            if (null != camJob)
            {
                camJob.RunTool();
                if (camJob.RunStatus == CogToolResultConstants.Accept)
                {
                    ICogImage img = (ICogImage)camJob.OutputImage;
                    _job.InputImage = img;
                    SetDisplayContent(img);
                }
            }
            this.IsNeedRetry = true;
            this.DialogResult = false;
            this.Close();
        }

        private CogLineSegment _crossLineH = null;
        private CogLineSegment _crossLineV = null;
        private const string CROSS_H_NAME = "CrossH";
        private const string CROSS_V_NAME = "CrossV";

        private void btnManual_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                btnSetManual.IsEnabled = true;
                //string targetSpaceName = "@\\Checkerboard Calibration\\Fixture1";
                //{
                //    if (dspMain.Image != null)
                //    {
                //        try
                //        {
                //            dspMain.Image.SelectedSpaceName = targetSpaceName;
                //        }
                //        catch (Exception ex)
                //        {
                //            Common.Info("Cannot set space {0}: {1}. Using root space instead.", targetSpaceName, ex.Message);
                //            targetSpaceName = "#";
                //            dspMain.Image.SelectedSpaceName = "#";
                //        }
                //    }
                //}
                dspMain.InteractiveGraphics.Clear();
                dspMain.StaticGraphics.Clear();
                _interactivePoint = null;
                _crossLineH = null;
                _crossLineV = null;
                
                if (_job != null && dspMain.Image != null)
                {
                    string imageSpace = "#";
                    try
                    {
                        imageSpace = dspMain.Image.SelectedSpaceName;
                    }
                    catch
                    {
                        imageSpace = "#";
                    }
                    double centerX = 10;
                    double centerY = 10;
                    //centerX = dspMain.Image.Width / 2.0;
                    //centerY = dspMain.Image.Height / 2.0;
                    // Nếu đang dùng calibrated space, lấy tọa độ từ space transform
                    //if (imageSpace != "#" && imageSpace != "")
                    //{
                    //    try
                    //    {
                    //        
                    //        ICogTransform2D transform = dspMain.Image.GetTransform("#", imageSpace);

                    //       
                    //        double pixelCenterX = dspMain.Image.Width / 2.0;
                    //        double pixelCenterY = dspMain.Image.Height / 2.0;

                    //        transform.MapPoint(pixelCenterX, pixelCenterY, out centerX, out centerY);

                    //        Common.Info("Pixel center: ({0}, {1}) -> Calibrated center: ({2}, {3})",
                    //            pixelCenterX, pixelCenterY, centerX, centerY);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Common.Bug("Error getting calibrated coordinates: {0}", ex.Message);
                    //        // Fallback về pixel space
                    //        imageSpace = "#";
                    //        centerX = dspMain.Image.Width / 2.0;
                    //        centerY = dspMain.Image.Height / 2.0;
                    //    }
                    //}
                    //else
                    //{
                    //   
                    //    centerX = dspMain.Image.Width / 2.0;
                    //    centerY = dspMain.Image.Height / 2.0;
                    //}

                    _interactivePoint = new CogPointMarker();
                    _interactivePoint.SelectedSpaceName = imageSpace;
                    _interactivePoint.X = centerX;
                    _interactivePoint.Y = centerY;
                    _interactivePoint.Interactive = true;
                    _interactivePoint.Selected = true;
                    _interactivePoint.GraphicDOFEnable = CogPointMarkerDOFConstants.All;

                    // Subscribe to change event
                    _interactivePoint.Changed += InteractivePoint_Changed;

                    dspMain.InteractiveGraphics.Add(_interactivePoint, "ManualPoint", false);

                    // Tạo dấu cộng tùy chỉnh
                    UpdateCrossLines(centerX, centerY, imageSpace);

                    dspMain.MouseMode = CogDisplayMouseModeConstants.Pointer;
                    dspMain.Fit(true);
                    dspMain.Invalidate();

                    Common.Info("Point marker added at X , Y : ({0}, {1})", centerX, centerY);
                }
            }
            catch (Exception ex)
            {
                Common.Bug("Error in btnManual_Click: {0}", ex.Message);
                MessageBox.Show("Error enabling manual marking: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InteractivePoint_Changed(object sender, CogChangedEventArgs e)
        {
            if (_interactivePoint != null)
            {
                UpdateCrossLines(_interactivePoint.X, _interactivePoint.Y, _interactivePoint.SelectedSpaceName);
                dspMain.Invalidate();
            }
        }

        private void UpdateCrossLines(double x, double y, string spaceName)
        {
            int crossSize = 60;

            
            try { dspMain.StaticGraphics.Remove(CROSS_H_NAME); } catch { }
            try { dspMain.StaticGraphics.Remove(CROSS_V_NAME); } catch { }

            
            _crossLineH = new CogLineSegment();
            _crossLineH.SelectedSpaceName = spaceName;
            _crossLineH.SetStartEnd(x - crossSize, y, x + crossSize, y);
            _crossLineH.Color = CogColorConstants.Blue;
            _crossLineH.LineWidthInScreenPixels = 2;

            
            _crossLineV = new CogLineSegment();
            _crossLineV.SelectedSpaceName = spaceName;
            _crossLineV.SetStartEnd(x, y - crossSize, x, y + crossSize);
            _crossLineV.Color = CogColorConstants.Blue;
            _crossLineV.LineWidthInScreenPixels = 2;

            
            dspMain.StaticGraphics.Add(_crossLineH, CROSS_H_NAME);
            dspMain.StaticGraphics.Add(_crossLineV, CROSS_V_NAME);
        }



        protected override void OnClosed(EventArgs e)
        {
            if (_interactivePoint != null)
            {
                _interactivePoint.Changed -= InteractivePoint_Changed;
            }

            if (dspMain != null)
            {
                dspMain.InteractiveGraphics.Clear();
                dspMain.StaticGraphics.Clear();
            }
            base.OnClosed(e);
        }


        private void btnSetManual_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (_job != null && _interactivePoint != null)
                {
                    double pointX = _interactivePoint.X;
                    double pointY = _interactivePoint.Y;

                    string confirmMessage = $"Confirm coordinates:\nX: {pointX:F3}\nY: {pointY:F3}";

                    if (MessageBox.Show(confirmMessage, "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        SetResultsFromPoint(_interactivePoint);
                        IsConfirmed = true;
                        if (!_isCoordinatedMultiMarkMode)
                        {
                            this.DialogResult = true;
                        }
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Please mark a point first by clicking 'Mark Manually'",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Common.Bug("Error setting manual results: " + ex.Message);
                MessageBox.Show("Error setting manual results: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
        public void SetResultsFromPoint(CogPointMarker point)
        {
            if (point != null)
            {
                this.X = point.X;
                this.Y = point.Y;
                this.Rotation = 0; 

                if (!_isCoordinatedMultiMarkMode)
                {
                    var points = new List<double>();
                    points.Add(X);
                    points.Add(Y);
                    _job.ListDouble = points;

                    var plcCam = JobController.GetPlcCamByID(_job.PlcJobId);
                    JobController.SetOutPutData(_job, plcCam);
                    plcCam.IsOk = true;
                }
                // Logic kiểm tra OK/NG 
                //if (Math.Abs(X) < 0.065 && Math.Abs(Y) <= 0.065)
                //{
                //    plcCam.IsOk = true;
                //}
                //else
                //{
                //    plcCam.IsOk = false;
                //}

                Common.Info("Manual point set: X={0:F3}, Y={1:F3}", X, Y);
            }
        }

        public void SetResults(CogRectangleAffine _interactRegion, CogRectangleAffine _interactRegion1)
        {
            if (_interactRegion != null && _interactRegion1 != null)
            {
                this.DialogResult = true;
                var X1 = _interactRegion.CenterX;
                var Y1 = _interactRegion.CenterY;
                var Rotation1 = _interactRegion.Rotation;
                var X2 = _interactRegion1.CenterX;
                var Y2 = _interactRegion1.CenterY;
                var Rotation2 = _interactRegion1.Rotation;

                this.X = X2 - X1;
                this.Y = Y2 - Y1;
                this.Rotation = Rotation2 - Rotation1;
            }
            else
            {
                if (_interactRegion != null)
                {
                    this.X = _interactRegion.CenterX;
                    this.Y = _interactRegion.CenterY;
                    this.Rotation = _interactRegion.Rotation;
                }
                if (_interactRegion1 != null)
                {
                    this.X = _interactRegion1.CenterX;
                    this.Y = _interactRegion1.CenterY;
                    this.Rotation = _interactRegion1.Rotation;
                }
            }
            var points = new List<double>();
            points.Add(X);
            points.Add(Y);
            _job.ListDouble = points;
            var plcCam = JobController.GetPlcCamByID(_job.PlcJobId);
            JobController.SetOutPutData(_job, plcCam);
            if (X < 0.065 && Y <= 0.065)
            {
                plcCam.IsOk = true;
            }
            else
            {
                plcCam.IsOk = false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isCoordinatedMultiMarkMode)
            {
                TryDisplayPendingImage();
                return;
            }

            var camJob = JobController.GetCameraJob(_job.CamSettings.CameraId);
            if (null != camJob && camJob.OutputImage != null)
            {
                _pendingImage = camJob.OutputImage as ICogImage;
                TryDisplayPendingImage();
            }
        }

        // Cleanup khi window đóng
        //protected override void OnClosed(EventArgs e)
        //{
        //    if (dspMain != null)
        //    {
        //        dspMain.InteractiveGraphics.Clear();
        //        dspMain.StaticGraphics.Clear();
        //    }
        //    base.OnClosed(e);
        //}
    }
}
