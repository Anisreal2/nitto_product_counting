using BeevisionSolution.Controller;
using BeevisionSolution.Jobs;
using BeevisionSolution.Models;
using BeevisionSolution.ViewComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static BeevisionSolution.Utils.Constant;
using System.Threading.Tasks;

namespace BeevisionSolution.Views
{
    /// <summary>
    /// Interaction logic for ToolSettingView.xaml
    /// </summary>
    public partial class ToolSettingView : UserControl, IDisposable
    {
        ToolBlockEditorView wCamera = new ToolBlockEditorView();
        ToolBlockEditorView wHeTbl = new ToolBlockEditorView();

        public ToolSettingView()
        {
            InitializeComponent();
            wCamera.OnHitRunButtonEvent += OnGrabImage;
            
            var mainWindow = (Application.Current.MainWindow as MainWindow2);
            if (mainWindow != null)
                mainWindow.OnAllJobLoadedDone += OnJobLoadedDone;
            Init();
        }

        private void OnJobLoadedDone(object sender)
        {
            Init();
        }

        private void Init()
        {
            var lst = JobController.GetAllJobs(false);
            if (lst.Count <= 0)
            {
                return;
            }

            var lstToolJobs = new List<BaseJob>();
            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i] is AlignJob || lst[i] is IspJob || lst[i] is WatcherJob)
                {
                    lstToolJobs.Add(lst[i]);
                }
            }

            cbxJobs.ItemsSource = lstToolJobs;
            if ((null != lstToolJobs) && (lstToolJobs.Count > 0))
            {
                //cbxJobs.SelectedIndex = 0;
                Dispatcher.BeginInvoke(new System.Action(async () =>
                {
                    await LoadJobForSelectedItem();
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private void OnGrabImage(object image)
        {
            if (wHeTbl != null)
            {
                wHeTbl.InputImage = image;
            }
        }
        private async Task LoadJobForSelectedItem()
        {
            if (cbxJobs.Items.Count < 1 || cbxJobs.SelectedIndex < 0)
                return;

            var job = (BaseJob)cbxJobs.SelectedItem;
            if (job == null)
                return;

            Mouse.OverrideCursor = Cursors.Wait;

            if (!job.Initialized)
                job.Init();

            var img = (Object)null;
            var camJob = JobController.GetCameraJob(job.CamSettings.CameraId);

            if (null != camJob)
            {
                if ((null == camJob.OutputImage) && (null == camJob.LastValidImage))
                {
                    await camJob.RunToolAsync();
                    img = camJob.OutputImage;
                }
                else if (null != camJob.LastValidImage)
                {
                    img = camJob.LastValidImage;
                }
                else
                {
                    img = camJob.OutputImage;
                }
            }

            await wCamera.SetJobAsync(camJob, null);

            if (!panelLeft.Children.Contains(wCamera))
            {
                panelLeft.Children.Clear();
                panelLeft.Children.Add(wCamera);
            }
            
            await wHeTbl.SetJobAsync(job, img);

            if (!panelRight.Children.Contains(wHeTbl))
            {
                panelRight.Children.Clear();
                panelRight.Children.Add(wHeTbl);
            }

            Mouse.OverrideCursor = null;
        }

        private async void cbxJobs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadJobForSelectedItem();
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            var job = (FunctionJob)cbxJobs.SelectedItem;
            OpCallWindow w = new OpCallWindow();
            w.SetJob(job);
            w.ShowDialog();
        }

        private void btnSaveOffset_Click(object sender, RoutedEventArgs e)
        {
        }

        public void Dispose()
        {
            var mainWindow = (Application.Current.MainWindow as MainWindow2);
            if (mainWindow != null)
            {
                mainWindow.OnAllJobLoadedDone -= OnJobLoadedDone;
            }

            if (wCamera != null)
            {
                wCamera.OnHitRunButtonEvent -= OnGrabImage;
                wCamera.Dispose();
                wCamera = null;
            }

            if (wHeTbl != null)
            {
                wHeTbl.Dispose();
                wHeTbl = null;
            }
        }
    }
}
