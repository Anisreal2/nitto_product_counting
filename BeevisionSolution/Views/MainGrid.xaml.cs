using BeevisionSolution.Controller;
using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using static BeevisionSolution.Utils.Common;
using Image = System.Drawing.Image;

namespace BeevisionSolution.Views
{
    public partial class MainGrid : UserControl, IDisposable
    {
        List<RecordDisplayWpf> lstDisplay = new List<RecordDisplayWpf>();
        private bool disposedValue;
        public bool IsShowToolButtons { get; set; } = true;
        public event Action<int, string, ICogImage> MeasureRequested;
        public MainGrid()
        {
            InitializeComponent();
            JobController.UpdateRetryInfo += JobController_UpdateRetryInfo;
            JobController.UpdateAlignTime += JobController_UpdateAlignTime;
            JobController.UpdateCycleTime += JobController_UpdateCycleTime;
        }
        public RecordDisplayWpf GetDisplayById(int displayId)
        {
            if (displayId < 0 || displayId >= lstDisplay.Count)
                return null;

            return lstDisplay[displayId];
        }
        private void JobController_UpdateRetryInfo(int displayId, int attempt, int maxRetries)
        {
            Dispatcher.Invoke(() =>
            {
                var display = GetDisplayById(displayId);
                if (display != null)
                {
                    display.SetRetryInfo(attempt, maxRetries);
                }
            });
        }

        private void JobController_UpdateAlignTime(int displayId, double alignTimeSeconds)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                RecordDisplayWpf display = GetDisplayById(displayId);
                if (display != null)
                {
                    display.SetAlignTime(alignTimeSeconds);
                }
            }));
        }

        private void JobController_UpdateCycleTime(IEnumerable<int> displayIds, double cycleTimeSeconds)
        {
            if (displayIds == null)
            {
                return;
            }

            List<int> displayIdList = new List<int>(displayIds);
            Dispatcher.BeginInvoke(new Action(() =>
            {
                for (int index = 0; index < displayIdList.Count; index++)
                {
                    RecordDisplayWpf display = GetDisplayById(displayIdList[index]);
                    if (display != null)
                    {
                        display.SetCycleTime(cycleTimeSeconds);
                    }
                }
            }));
        }

        public void SetSize(List<int> DisplayList, List<double> DisplaySizes)
        {
            if ((null == DisplayList) || (DisplayList.Count < 1))
                return;

            Clean();
            Dispatcher.Invoke(() =>
            {
                var count = 0;
                var lstJobs = JobController.GetAllJobs(false);
                var lstPlcCam = JobController.GetAllPlcCam();
                for (int idx = 0; idx < DisplayList.Count; idx++)
                {
                    var item = DisplayList[idx];
                    var grd = new Grid();
                    var row = new RowDefinition();
                    var f = 1.0;

                    row.Height = new System.Windows.GridLength(f, System.Windows.GridUnitType.Star);
                    grdMain.RowDefinitions.Add(row);

                    for (int i = 0; i < item; i++)
                    {
                        var dsp = new RecordDisplayWpf() { DisplayId = count++ };
                        dsp.IsShowGrabImageButton = true;
                        dsp.IsShowMeasureButton = true;
                        dsp.MeasureRequested += Display_MeasureRequested;
                        if (!IsShowToolButtons)
                        {
                            dsp.IsShowCrossLineButton = true;
                            dsp.IsShowSoloLiveButton = true;
                        }


                        grd.ColumnDefinitions.Add(new ColumnDefinition());
                        grd.Children.Add(dsp);
                        lstDisplay.Add(dsp);
                        Grid.SetColumn(dsp, i);
                    }
                    grdMain.Children.Add(grd);
                    Grid.SetRow(grd, idx);
                }
            });
        }

        private void Display_MeasureRequested(int displayId, string displayName, ICogImage image)
        {
            MeasureRequested?.Invoke(displayId, displayName, image);
        }
        public void RefreshAllCrossLines()
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var display in lstDisplay)
                {
                    display?.RefreshCrossLine();
                }
            });
        }
        public int Count => lstDisplay.Count;

        public ICogImage GetImage(int dspIndex)
        {
            var idx = dspIndex % lstDisplay.Count;
            return lstDisplay[idx].InputImage;
        }

        public void SetImage(ICogImage image, int dspIndex)
        {
            var idx = dspIndex % lstDisplay.Count;
            lstDisplay[idx].InputImage = image;
        }

        public void SetStaticGraphics(int dspIndex, List<CogPointMarker> markers)
        {
            var idx = dspIndex % lstDisplay.Count;
            lstDisplay[idx].SetStaticGraphics(markers);
        }

        public void SetStaticGraphics(int dspIndex, CogPointMarker marker)
        {
            var idx = dspIndex % lstDisplay.Count;
            lstDisplay[idx].SetStaticGraphics(marker);
        }

        public void SetStaticGraphics(int dspIndex, ICogGraphic graph)
        {
            var idx = dspIndex % lstDisplay.Count;
            lstDisplay[idx].SetStaticGraphics(graph);
        }

        public void SetWatermark(int dspIndex, string strWatermark)
        {
            var idx = dspIndex % lstDisplay.Count;
            lstDisplay[idx].ScreenWatermark = strWatermark;
        }

        public void SetFooterContent(int dspIndex, List<object> dspItems, double actualDist = double.NaN)
        {
            var idx = dspIndex % lstDisplay.Count;
            lstDisplay[idx].ActualDist = actualDist;
            lstDisplay[idx].SetFooterContent(dspItems);
        }

        public void SetFooterContentX(int dspIndex, List<object> dspItems, double actualDist = double.NaN)
        {
            var idx = dspIndex % lstDisplay.Count;
            lstDisplay[idx].ActualDist = actualDist;
            lstDisplay[idx].SetFooterContentX(dspItems);
        }

        public void SetDisplayContent(int dspIndex, ICogImage img, ICogRecord rcd)
        {
            var idx = dspIndex % lstDisplay.Count;
            lstDisplay[idx].SetDisplayContent(img, rcd);
            //lstDisplay[idx].InputImage = img;
            //lstDisplay[idx].Record = rcd;
        }

        public Image GetOverlayImage(int dspIndex)
        {
            var idx = dspIndex % lstDisplay.Count;
            return lstDisplay[idx].OverlayImage;
        }

        public void ClearStaticGraphics(int dspIndex)
        {
            var idx = dspIndex % lstDisplay.Count;
            lstDisplay[idx].ClearStaticGraphics();
        }

        public void ForceSetImage(ICogImage image, int dspIndex)
        {
            var idx = dspIndex % lstDisplay.Count;
            lstDisplay[idx].UpdateImageWithRecord = false;
            lstDisplay[idx].InputImage = image;
        }

        public void SetRecord(ICogRecord Record, int dspIndex)
        {
            var idx = dspIndex % lstDisplay.Count;
            lstDisplay[idx].Record = Record;
        }

        private void Clean()
        {
            if ((null != lstDisplay) && (lstDisplay.Count > 0))
            {
                Dispatcher.Invoke(() =>
                {
                    grdMain.Children.Clear();
                    grdMain.ColumnDefinitions.Clear();
                    grdMain.RowDefinitions.Clear();

                    for (int i = 0; i < lstDisplay.Count; i++)
                    {
                        lstDisplay[i].MeasureRequested -= Display_MeasureRequested;
                        lstDisplay[i].Dispose();
                        lstDisplay[i] = null;
                    }
                    lstDisplay.Clear();
                });
            }
        }

        public void ClearAll()
        {
            if ((null != lstDisplay) && (lstDisplay.Count > 0))
            {
                for (int i = 0; i < lstDisplay.Count; i++)
                    lstDisplay[i].Clear();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    JobController.UpdateRetryInfo -= JobController_UpdateRetryInfo;
                    JobController.UpdateAlignTime -= JobController_UpdateAlignTime;
                    JobController.UpdateCycleTime -= JobController_UpdateCycleTime;
                    MeasureRequested = null;
                    Clean();
                    lstDisplay = null;
                    grdMain = null;
                }

                disposedValue = true;
            }
        }

        ~MainGrid()
        {
            Dispose(disposing: false);
            lstDisplay = null;
            grdMain = null;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
