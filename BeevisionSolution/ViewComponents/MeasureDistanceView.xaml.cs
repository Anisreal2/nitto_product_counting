using BeevisionSolution.Models;
using BeevisionSolution.Utils;
using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.ImageFile;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BeevisionSolution.ViewComponents
{
    public partial class MeasureDistanceView : UserControl, IDisposable
    {
        private CogPointMarker _markerP1;
        private CogPointMarker _markerP2;
        private CogLineSegment _lineHorizontal;
        private CogLineSegment _lineVertical;
        private CogLineSegment _lineDiagonal;
        private CogGraphicLabel _labelHorizontal;
        private CogGraphicLabel _labelVertical;
        private CogGraphicLabel _labelDiagonal;
        private CogRecordDisplay cogDisplay;
        private MeasureDistanceConfig measureDistanceConfig;
        private int currentDisplayId = -1;
        private double savedScale = 1.0;
        private bool browseWhenLoaded;
        private bool cogDisplayReady;
        private bool hasPendingImageRequest;
        private ICogImage pendingImage;
        private string pendingSourceName;
        private int activeXLoadRetryCount;
        private bool disposed;

        public MeasureDistanceView()
        {
            InitializeComponent();

            measureDistanceConfig = MeasureDistanceConfig.Load();
            cogDisplay = new CogRecordDisplay();
            cogDisplay.HandleCreated += CogDisplay_HandleCreated;
            cogDisplay.HandleDestroyed += CogDisplay_HandleDestroyed;
            wfHost.Child = cogDisplay;

            InitializeCognexGraphics();
            cogDisplay.Changed += CogDisplay_Changed;
            Loaded += MeasureDistanceView_Loaded;
        }

        public void LoadDisplay(int displayId, ICogImage image, string displayName)
        {
            currentDisplayId = displayId;
            measureDistanceConfig = MeasureDistanceConfig.Load();
            savedScale = measureDistanceConfig.GetScale(displayId);

            txtDisplayId.Text = displayId.ToString(CultureInfo.InvariantCulture);
            txtScale.Text = savedScale.ToString("G", CultureInfo.CurrentCulture);

            if (image != null)
            {
                string sourceName = displayName;
                if (string.IsNullOrWhiteSpace(sourceName))
                {
                    sourceName = "Display " + displayId.ToString(CultureInfo.InvariantCulture);
                }

                QueueImage(image, sourceName);
                return;
            }

            QueueImage(null, null);
        }

        public bool ConfirmCanChangeDisplay()
        {
            if (!HasUnsavedScale())
            {
                return true;
            }

            MessageBoxResult result = MessageBox.Show(
                "The Measure Scale has not been saved. Do you want to save it now?",
                "Measure Distance",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                return SaveScale(false);
            }

            if (result == MessageBoxResult.No)
            {
                return true;
            }

            return false;
        }

        private void InitializeCognexGraphics()
        {
            _markerP1 = new CogPointMarker
            {
                Color = CogColorConstants.Magenta,
                SizeInScreenPixels = 20,
                GraphicType = CogPointMarkerGraphicTypeConstants.Crosshair,
                Interactive = true,
                GraphicDOFEnable = CogPointMarkerDOFConstants.All
            };
            _markerP1.Changed += Marker_Changed;

            _markerP2 = new CogPointMarker
            {
                Color = CogColorConstants.Green,
                SizeInScreenPixels = 20,
                GraphicType = CogPointMarkerGraphicTypeConstants.Crosshair,
                Interactive = true,
                GraphicDOFEnable = CogPointMarkerDOFConstants.All
            };
            _markerP2.Changed += Marker_Changed;

            _lineHorizontal = new CogLineSegment
            {
                Color = CogColorConstants.Cyan,
                LineStyle = CogGraphicLineStyleConstants.Dash
            };
            _lineVertical = new CogLineSegment
            {
                Color = CogColorConstants.Blue,
                LineStyle = CogGraphicLineStyleConstants.Dash
            };
            _lineDiagonal = new CogLineSegment
            {
                Color = CogColorConstants.Red,
                LineStyle = CogGraphicLineStyleConstants.Solid
            };

            _labelHorizontal = new CogGraphicLabel
            {
                Color = CogColorConstants.Cyan,
                Font = new Font("Consolas", 12, System.Drawing.FontStyle.Bold)
            };
            _labelVertical = new CogGraphicLabel
            {
                Color = CogColorConstants.Blue,
                Font = new Font("Consolas", 12, System.Drawing.FontStyle.Bold)
            };
            _labelDiagonal = new CogGraphicLabel
            {
                Color = CogColorConstants.Red,
                Font = new Font("Consolas", 12, System.Drawing.FontStyle.Bold)
            };
        }

        private void Marker_Changed(object sender, CogChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_markerP1 == null || _markerP2 == null || cogDisplay == null || cogDisplay.Image == null)
                {
                    return;
                }

                txtP1X.Text = string.Format(CultureInfo.CurrentCulture, "{0:F1}", _markerP1.X);
                txtP1Y.Text = string.Format(CultureInfo.CurrentCulture, "{0:F1}", _markerP1.Y);
                txtP2X.Text = string.Format(CultureInfo.CurrentCulture, "{0:F1}", _markerP2.X);
                txtP2Y.Text = string.Format(CultureInfo.CurrentCulture, "{0:F1}", _markerP2.Y);

                UpdateMeasurementVisualization();
                cogDisplay.Invalidate();
            }));
        }

        private sealed class MeasurementResult
        {
            public double P1X { get; set; }
            public double P1Y { get; set; }
            public double P2X { get; set; }
            public double P2Y { get; set; }
            public double RightX { get; set; }
            public double RightY { get; set; }
            public double HorizontalMm { get; set; }
            public double VerticalMm { get; set; }
            public double DiagonalMm { get; set; }
        }

        private MeasurementResult CalculateMeasurement()
        {
            double p1x = _markerP1.X;
            double p1y = _markerP1.Y;
            double p2x = _markerP2.X;
            double p2y = _markerP2.Y;
            double horizontalPx = Math.Abs(p2x - p1x);
            double verticalPx = Math.Abs(p2y - p1y);
            double diagonalPx = Math.Sqrt(
                ((p2x - p1x) * (p2x - p1x)) +
                ((p2y - p1y) * (p2y - p1y)));

            return new MeasurementResult
            {
                P1X = p1x,
                P1Y = p1y,
                P2X = p2x,
                P2Y = p2y,
                RightX = p2x,
                RightY = p1y,
                HorizontalMm = horizontalPx * savedScale,
                VerticalMm = verticalPx * savedScale,
                DiagonalMm = diagonalPx * savedScale
            };
        }

        private void RenderMeasurement(MeasurementResult result)
        {
            if (cogDisplay == null || cogDisplay.Image == null)
            {
                return;
            }

            RemoveStaticGraphic("LineH");
            RemoveStaticGraphic("LineV");
            RemoveStaticGraphic("LineD");
            RemoveStaticGraphic("LabelH");
            RemoveStaticGraphic("LabelV");
            RemoveStaticGraphic("LabelD");

            _lineHorizontal.StartX = result.P1X;
            _lineHorizontal.StartY = result.P1Y;
            _lineHorizontal.EndX = result.RightX;
            _lineHorizontal.EndY = result.RightY;

            _lineVertical.StartX = result.RightX;
            _lineVertical.StartY = result.RightY;
            _lineVertical.EndX = result.P2X;
            _lineVertical.EndY = result.P2Y;

            _lineDiagonal.StartX = result.P1X;
            _lineDiagonal.StartY = result.P1Y;
            _lineDiagonal.EndX = result.P2X;
            _lineDiagonal.EndY = result.P2Y;

            _labelHorizontal.Text = string.Format(CultureInfo.CurrentCulture, "H: {0:F3} MM", result.HorizontalMm);
            _labelHorizontal.X = (result.P1X + result.RightX) / 2;
            _labelHorizontal.Y = result.P1Y + 10;

            _labelVertical.Text = string.Format(CultureInfo.CurrentCulture, "V: {0:F3} MM", result.VerticalMm);
            _labelVertical.X = result.RightX + 8;
            _labelVertical.Y = (result.RightY + result.P2Y) / 2;

            _labelDiagonal.Text = string.Format(CultureInfo.CurrentCulture, "D: {0:F3} MM", result.DiagonalMm);
            _labelDiagonal.X = (result.P1X + result.P2X) / 2;
            _labelDiagonal.Y = (result.P1Y + result.P2Y) / 2;

            cogDisplay.StaticGraphics.Add(_lineHorizontal, "LineH");
            cogDisplay.StaticGraphics.Add(_lineVertical, "LineV");
            cogDisplay.StaticGraphics.Add(_lineDiagonal, "LineD");
            cogDisplay.StaticGraphics.Add(_labelHorizontal, "LabelH");
            cogDisplay.StaticGraphics.Add(_labelVertical, "LabelV");
            cogDisplay.StaticGraphics.Add(_labelDiagonal, "LabelD");

            txtHorizontal.Text = result.HorizontalMm.ToString("F3", CultureInfo.CurrentCulture);
            txtVertical.Text = result.VerticalMm.ToString("F3", CultureInfo.CurrentCulture);
            txtDistance.Text = result.DiagonalMm.ToString("F3", CultureInfo.CurrentCulture);
        }

        private void RemoveStaticGraphic(string groupName)
        {
            try
            {
                cogDisplay.StaticGraphics.Remove(groupName);
            }
            catch
            {
            }
        }

        private void UpdateMeasurementVisualization()
        {
            if (_markerP1 == null || _markerP2 == null || cogDisplay == null || cogDisplay.Image == null)
            {
                return;
            }

            MeasurementResult result = CalculateMeasurement();
            RenderMeasurement(result);
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            BrowseImage();
        }

        private void btnSaveScale_Click(object sender, RoutedEventArgs e)
        {
            SaveScale(true);
        }

        private bool SaveScale(bool showSuccessMessage)
        {
            double scaleMmPerPixel;
            if (!TryReadScale(out scaleMmPerPixel))
            {
                MessageBox.Show(
                    "Measure Scale must be a finite number greater than 0.",
                    "Measure Distance",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtScale.Focus();
                txtScale.SelectAll();
                return false;
            }

            if (currentDisplayId < 0)
            {
                MessageBox.Show(
                    "No display is selected.",
                    "Measure Distance",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            if (!measureDistanceConfig.SaveScale(currentDisplayId, scaleMmPerPixel))
            {
                MessageBox.Show(
                    "Cannot save Measure Scale. Check the application log and configuration folder permissions.",
                    "Measure Distance",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            savedScale = scaleMmPerPixel;
            txtScale.Text = savedScale.ToString("G", CultureInfo.CurrentCulture);
            UpdateMeasurementVisualization();

            if (showSuccessMessage)
            {
                MessageBox.Show(
                    "Measure Scale was saved for Display " + currentDisplayId.ToString(CultureInfo.InvariantCulture) + ".",
                    "Measure Distance",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            return true;
        }

        private bool HasUnsavedScale()
        {
            double currentScale;
            if (!TryReadScale(out currentScale))
            {
                return true;
            }

            return Math.Abs(currentScale - savedScale) > 0.000000000001;
        }

        private bool TryReadScale(out double scaleMmPerPixel)
        {
            string scaleText = txtScale.Text == null ? string.Empty : txtScale.Text.Trim();
            bool parsed = double.TryParse(
                scaleText,
                NumberStyles.Float,
                CultureInfo.CurrentCulture,
                out scaleMmPerPixel);

            if (!parsed)
            {
                parsed = double.TryParse(
                    scaleText,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out scaleMmPerPixel);
            }

            if (!parsed || double.IsNaN(scaleMmPerPixel) || double.IsInfinity(scaleMmPerPixel))
            {
                return false;
            }

            return scaleMmPerPixel > 0;
        }

        private void BrowseImage()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = FindResource("strBrowse") as string ?? "Select Image",
                Filter = "Image Files|*.bmp;*.png;*.jpg;*.jpeg;*.tiff;*.tif;*.gif|All Files|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                LoadImage(dialog.FileName);
            }
        }

        private void LoadImage(string filePath)
        {
            try
            {
                using (CogImageFileTool fileTool = new CogImageFileTool())
                {
                    fileTool.Operator.Open(filePath, CogImageFileModeConstants.Read);
                    fileTool.Run();
                    QueueImage(fileTool.OutputImage, filePath);
                }
            }
            catch (Exception ex)
            {
                string message = (FindResource("msgErrorOpenImage") as string ?? "Error opening image: ") + ex.Message;
                string title = FindResource("strError") as string ?? "Error";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QueueImage(ICogImage image, string sourceName)
        {
            pendingImage = image;
            pendingSourceName = sourceName;
            hasPendingImageRequest = true;
            activeXLoadRetryCount = 0;

            if (cogDisplayReady)
            {
                ApplyPendingImage();
            }
        }

        private void ApplyPendingImage()
        {
            if (disposed || !hasPendingImageRequest || !cogDisplayReady)
            {
                return;
            }

            if (cogDisplay == null || cogDisplay.IsDisposed || !cogDisplay.IsHandleCreated)
            {
                return;
            }

            try
            {
                if (pendingImage != null)
                {
                    SetImage(pendingImage, pendingSourceName);
                }
                else
                {
                    ClearImage();
                }
            }
            catch (System.Windows.Forms.AxHost.InvalidActiveXStateException ex)
            {
                activeXLoadRetryCount++;
                if (activeXLoadRetryCount <= 5)
                {
                    Dispatcher.BeginInvoke(
                        new Action(ApplyPendingImage),
                        DispatcherPriority.ApplicationIdle);
                    return;
                }

                hasPendingImageRequest = false;
                pendingImage = null;
                pendingSourceName = null;
                Common.Info("Measure Distance display is not ready: {0}", ex.Message);
                MessageBox.Show(
                    "The measurement display is not ready. Please close the window and try again.",
                    "Measure Distance",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            bool shouldBrowse = pendingImage == null;
            hasPendingImageRequest = false;
            pendingImage = null;
            pendingSourceName = null;
            activeXLoadRetryCount = 0;

            if (shouldBrowse)
            {
                RequestBrowse();
            }
        }

        private void CogDisplay_HandleCreated(object sender, EventArgs e)
        {
            cogDisplayReady = true;
            Dispatcher.BeginInvoke(
                new Action(ApplyPendingImage),
                DispatcherPriority.ContextIdle);
        }

        private void CogDisplay_HandleDestroyed(object sender, EventArgs e)
        {
            cogDisplayReady = false;
        }

        private void SetImage(ICogImage image, string sourceName)
        {
            cogDisplay.Image = image;
            cogDisplay.Fit(true);

            txtFilePath.Text = sourceName;
            txtFilePath.Foreground = System.Windows.Media.Brushes.White;
            txtGuide.Visibility = Visibility.Collapsed;

            InitializeMarkersOnImage();
        }

        private void ClearImage()
        {
            if (cogDisplay != null)
            {
                cogDisplay.InteractiveGraphics.Clear();
                cogDisplay.StaticGraphics.Clear();
                cogDisplay.Image = null;
            }

            txtFilePath.Text = FindResource("strNoImageSelected") as string ?? "No image selected";
            txtFilePath.Foreground = System.Windows.Media.Brushes.Gray;
            txtGuide.Visibility = Visibility.Visible;
            txtP1X.Text = "--";
            txtP1Y.Text = "--";
            txtP2X.Text = "--";
            txtP2Y.Text = "--";
            txtHorizontal.Text = "--";
            txtVertical.Text = "--";
            txtDistance.Text = "--";
        }

        private void InitializeMarkersOnImage()
        {
            cogDisplay.InteractiveGraphics.Clear();
            cogDisplay.StaticGraphics.Clear();

            ApplyPixelCoordinateSpaceToGraphics();

            double centerX = cogDisplay.Image.Width / 2.0;
            double centerY = cogDisplay.Image.Height / 2.0;
            double halfMarkerDistance = Math.Min(100.0, Math.Max(10.0, cogDisplay.Image.Width / 4.0));

            _markerP1.X = centerX - halfMarkerDistance;
            _markerP1.Y = centerY;
            _markerP2.X = centerX + halfMarkerDistance;
            _markerP2.Y = centerY;

            cogDisplay.InteractiveGraphics.Add(_markerP1, "P1", false);
            cogDisplay.InteractiveGraphics.Add(_markerP2, "P2", false);

            UpdateMeasurementVisualization();
            cogDisplay.Invalidate();
        }

        private void ApplyPixelCoordinateSpaceToGraphics()
        {
            const string pixelCoordinateSpaceName = "#";

            _markerP1.SelectedSpaceName = pixelCoordinateSpaceName;
            _markerP2.SelectedSpaceName = pixelCoordinateSpaceName;
            _lineHorizontal.SelectedSpaceName = pixelCoordinateSpaceName;
            _lineVertical.SelectedSpaceName = pixelCoordinateSpaceName;
            _lineDiagonal.SelectedSpaceName = pixelCoordinateSpaceName;
            _labelHorizontal.SelectedSpaceName = pixelCoordinateSpaceName;
            _labelVertical.SelectedSpaceName = pixelCoordinateSpaceName;
            _labelDiagonal.SelectedSpaceName = pixelCoordinateSpaceName;
        }

        private void RequestBrowse()
        {
            browseWhenLoaded = true;
            if (IsLoaded)
            {
                Dispatcher.BeginInvoke(new Action(OpenPendingBrowse));
            }
        }

        private void MeasureDistanceView_Loaded(object sender, RoutedEventArgs e)
        {
            OpenPendingBrowse();
        }

        private void OpenPendingBrowse()
        {
            if (!browseWhenLoaded)
            {
                return;
            }

            browseWhenLoaded = false;
            BrowseImage();
        }

        private void CogDisplay_Changed(object sender, CogChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cogDisplay != null)
                {
                    txtZoom.Text = string.Format(CultureInfo.CurrentCulture, "{0:F0}%", cogDisplay.Zoom * 100);
                }
            }));
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            Loaded -= MeasureDistanceView_Loaded;
            browseWhenLoaded = false;
            hasPendingImageRequest = false;
            pendingImage = null;
            pendingSourceName = null;

            if (_markerP1 != null)
            {
                _markerP1.Changed -= Marker_Changed;
            }

            if (_markerP2 != null)
            {
                _markerP2.Changed -= Marker_Changed;
            }

            if (cogDisplay != null)
            {
                cogDisplay.Changed -= CogDisplay_Changed;
                cogDisplay.HandleCreated -= CogDisplay_HandleCreated;
                cogDisplay.HandleDestroyed -= CogDisplay_HandleDestroyed;

                if (cogDisplayReady && cogDisplay.IsHandleCreated && !cogDisplay.IsDisposed)
                {
                    try
                    {
                        cogDisplay.InteractiveGraphics.Clear();
                        cogDisplay.StaticGraphics.Clear();
                        cogDisplay.Image = null;
                    }
                    catch (System.Windows.Forms.AxHost.InvalidActiveXStateException ex)
                    {
                        Common.Info("Measure Distance display was already inactive during cleanup: {0}", ex.Message);
                    }
                }

                cogDisplay.Dispose();
                cogDisplay = null;
            }

            if (wfHost != null)
            {
                wfHost.Child = null;
                wfHost.Dispose();
            }

        }
    }
}
