using BeeLib.Math;
using BeevisionSolution.Controller;
using BeevisionSolution.Jobs;
using BeevisionSolution.LocalDB;
using BeevisionSolution.Models;
using BeevisionSolution.Utils;
using BeevisionSolution.ViewComponents;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;
using static BeevisionSolution.Utils.Common;

namespace BeevisionSolution.Views
{
    /// <summary>
    /// Interaction logic for ImageView.xaml
    /// </summary>
    public partial class ImageView : UserControl, INotifyPropertyChanged
    {
        // Fields từ LiveGrid
        private bool disposed = false;
        private int CurrentCamera = -1;
        public static ImageView CurrentInstance { get; set; }
        private bool _isLiveViewRunning = false;
        private static readonly TimeSpan AdminSessionDuration = TimeSpan.FromMinutes(60);
        private DispatcherTimer _autoLockTimer;
        private DateTime _adminSessionExpiresAt;
        private Dictionary<int, CogAcqFifoTool> _liveFifoTools = new Dictionary<int, CogAcqFifoTool>();
        private readonly object _liveSync = new object();
        private int _liveToggleInProgress = 0;

        private OptionWindow optionWindow;
        private ToolSettingView toolSettingWindow;
        private AccountManagerView accountManagerView;
        private ComunicationView communicationView;
        private ModelsManagerView modelsManagerView;
        private MeasureDistanceView measureDistanceView;
        private Window measureDistanceWindow;

        private MainGrid mainGridView;
        private UserControl currentView;

        #region Commands
        public ICommand DistanceInspectCommand { get; }
        public ICommand ResetCountCommand { get; }
        public ICommand AutoRunCommand { get; }
        public ICommand StopAutoCommand { get; }
        public ICommand AcquireCommand { get; }
        public ICommand LiveCommand { get; }
        public ICommand ManualRunCommand { get; }
        public ICommand OpenOptionCommand { get; }
        public ICommand OpenToolBlockCommand { get; }
        public ICommand PLCInterfaceCommand { get; }
        public ICommand LogCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand OpenImageViewCommand { get; }
        public ICommand AccountManagerCommand { get; }
        public ICommand OpenCommunicationCommand { get; }
        public ICommand OpenModelsManagerCommand { get; }
        public ICommand OpenLightManagerCommand { get; }
        public ICommand OpenlogManagerCommand { get; }
        public ICommand OpenMotionControlCommand { get; }
        #endregion
        public ImageView()
        {
            InitializeComponent();
            CurrentInstance = this;
            _autoLockTimer = new DispatcherTimer();
            _autoLockTimer.Interval = TimeSpan.FromSeconds(1);
            _autoLockTimer.Tick += AutoLockTimer_Tick;
            OpenOptionCommand = new Utils.RelayCommand(ExecuteOpenOption);
            OpenToolBlockCommand = new Utils.RelayCommand(ExecuteOpenToolBlock);
            AutoRunCommand = new Utils.RelayCommand(
                ExecuteAutoRun,
                () => Common.StatusManager.ApplicationStatus != ApplicationStatus.AutoRunning
            );
            StopAutoCommand = new Utils.RelayCommand(
                ExecuteStopAuto,
                () => Common.StatusManager.ApplicationStatus == ApplicationStatus.AutoRunning
            );
            ManualRunCommand = new Utils.RelayCommand(ExecuteManualRun);
            LiveCommand = new Utils.RelayCommand(ExecuteLive);
            ExitCommand = new Utils.RelayCommand(ExcuteExit);
            ResetCountCommand = new Utils.RelayCommand(ExecuteResetCount);
            OpenImageViewCommand = new Utils.RelayCommand(ExcuteOpenMainView);
            AccountManagerCommand = new Utils.RelayCommand(ExcuteAccountManager);
            OpenCommunicationCommand = new Utils.RelayCommand(ExecuteOpenCommunication);
            OpenModelsManagerCommand = new Utils.RelayCommand(ExecuteOpenModelsManager);
            OpenLightManagerCommand = new Utils.RelayCommand(ExecuteOpenLightManager);
            OpenlogManagerCommand = new Utils.RelayCommand(ExecuteOpenLogManager);
            OpenMotionControlCommand = new Utils.RelayCommand(ExecuteOpenMotionControl);
            mainGridView = dspGrid;
            mainGridView.MeasureRequested += MainGridView_MeasureRequested;

            communicationView = new ComunicationView();
            modelsManagerView = new ModelsManagerView();

            Init();
        }

        private void MainGridView_MeasureRequested(int displayId, string displayName, ICogImage image)
        {
            if (measureDistanceView != null && !measureDistanceView.ConfirmCanChangeDisplay())
            {
                return;
            }

            if (measureDistanceWindow == null)
            {
                measureDistanceView = new MeasureDistanceView();
                measureDistanceWindow = new Window
                {
                    Title = "Measure Distance",
                    Width = 1100,
                    Height = 760,
                    MinWidth = 850,
                    MinHeight = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Content = measureDistanceView,
                    Owner = Window.GetWindow(this)
                };
                measureDistanceWindow.Closing += MeasureDistanceWindow_Closing;
                measureDistanceWindow.Closed += MeasureDistanceWindow_Closed;
            }

            if (!measureDistanceWindow.IsVisible)
            {
                measureDistanceWindow.Show();
            }
            else
            {
                if (measureDistanceWindow.WindowState == WindowState.Minimized)
                {
                    measureDistanceWindow.WindowState = WindowState.Normal;
                }

                measureDistanceWindow.Activate();
            }

            measureDistanceView.LoadDisplay(displayId, image, displayName);
        }

        private void MeasureDistanceWindow_Closing(object sender, CancelEventArgs e)
        {
            if (measureDistanceView != null && !measureDistanceView.ConfirmCanChangeDisplay())
            {
                e.Cancel = true;
            }
        }

        private void MeasureDistanceWindow_Closed(object sender, EventArgs e)
        {
            if (measureDistanceWindow != null)
            {
                measureDistanceWindow.Closing -= MeasureDistanceWindow_Closing;
                measureDistanceWindow.Closed -= MeasureDistanceWindow_Closed;
                measureDistanceWindow.Content = null;
            }

            if (measureDistanceView != null)
            {
                measureDistanceView.Dispose();
            }

            measureDistanceView = null;
            measureDistanceWindow = null;
        }

        private void CloseMeasureDistanceWindow()
        {
            if (measureDistanceWindow == null)
            {
                if (measureDistanceView != null)
                {
                    measureDistanceView.Dispose();
                    measureDistanceView = null;
                }
                return;
            }

            measureDistanceWindow.Closing -= MeasureDistanceWindow_Closing;
            measureDistanceWindow.Close();
        }

        private OptionWindow GetOptionWindow()
        {
            if (optionWindow == null)
            {
                optionWindow = new OptionWindow();
            }
            return optionWindow;
        }
        private ToolSettingView GetToolSettingWindow()
        {
            if (toolSettingWindow == null)
            {
                toolSettingWindow = new ToolSettingView();
            }
            return toolSettingWindow;
        }
        private AccountManagerView GetAccountManagerView()
        {
            if (accountManagerView == null)
            {
                accountManagerView = new AccountManagerView();
            }
            return accountManagerView;
        }
        private ComunicationView GetCommunicationView()
        {
            if (communicationView == null)
            {
                communicationView = new ComunicationView();
            }
            return communicationView;
        }
        private ModelsManagerView GetModelsManagerView()
        {
            if (modelsManagerView == null)
            {
                modelsManagerView = new ModelsManagerView();
            }
            return modelsManagerView;
        }
        private MotionControlView motionControlView;
        private MotionControlView GetMotionControlView()
        {
            if (motionControlView == null)
                motionControlView = new MotionControlView();
            return motionControlView;
        }
        private void ExecuteOpenMotionControl()
        {
            StopAllCameraLive();
            ShowView(GetMotionControlView());
        }
        private void DisposeWindow(UserControl window)
        {
            if (window == null) return;

            try
            {
                // Dispose If implement IDisposable
                if (window is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                // Clear reference
                if (window == optionWindow) optionWindow = null;
                else if (window == toolSettingWindow) toolSettingWindow = null;
                else if (window == accountManagerView) accountManagerView = null;
                else if (window == communicationView) communicationView = null;
                else if (window == modelsManagerView) modelsManagerView = null;
            }
            catch (Exception ex)
            {
                Common.Bug($"Error disposing window: {ex.Message}");
            }
        }
        private void ExcuteAccountManager()
        {
            ShowView(GetAccountManagerView());
        }

        private void ExcuteOpenMainView()
        {
            ShowView(mainGridView);
            btnLiveView.IsEnabled = true;
            
        }

        private void ExecuteResetCount()
        {
            // check app is running
            if(Common.StatusManager.ApplicationStatus == ApplicationStatus.AutoRunning)
            {
                MessageBox.Show((string)TryFindResource("msgAppIsRunning"));
                return;
            }
            var result = MessageBox.Show((string)TryFindResource("msgResetQuantity")
                ,
                (string)TryFindResource("strReset"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            // reset count
            Common.ResultModel = new ResultModel();
            UpdateQuantity();
        }

        private void ExecuteOpenCommunication()
        {
            StopAllCameraLive();
            ShowView(GetCommunicationView());
        }

        private void ExecuteOpenModelsManager()
        {
            StopAllCameraLive();
            if (Common.CurrentOperationMode == OperationMode.Engineer || Common.CurrentOperationMode == OperationMode.Master)
            {
                ShowView(modelsManagerView);

            }
        }
        private void ExecuteOpenOption()
        {
            StopAllCameraLive();
            ShowView(GetOptionWindow());
            optionWindow.LoadSettings();
        }

        private void ExecuteOpenToolBlock()
        {
            StopAllCameraLive();
            // check role again to show tool setting
            if (Common.CurrentOperationMode == OperationMode.Master)
            {
                ShowView(GetToolSettingWindow());
            }
        }

        private void ShowView(UserControl view)
        {
            mainContentArea.Content = view;
            currentView = view;
            btnLiveView.IsEnabled = false;
        }



        private void ExecuteLive()
        {
            // Guard against fast re-click / re-entrancy which can break VisionPro live state.
            if (Interlocked.Exchange(ref _liveToggleInProgress, 1) == 1)
                return;

            var prevStatus = Common.StatusManager.ApplicationStatus;
            try
            {
                var shouldStart = !_isLiveViewRunning;

                if (shouldStart)
                {
                    Common.StatusManager.ApplicationStatus = ApplicationStatus.CameraLive;
                    var started = StartAllCameraLive();
                    _isLiveViewRunning = started;
                    if (!started)
                    {
                        // Restore previous status if live failed to start
                        Common.StatusManager.ApplicationStatus = prevStatus;
                    }
                }
                else
                {
                    StopAllCameraLive();
                    _isLiveViewRunning = false;
                    Common.StatusManager.ApplicationStatus = Common.IsAutoMode
                        ? ApplicationStatus.AutoRunning
                        : ApplicationStatus.Manual;
                }
            }
            catch (Exception ex)
            {
                _isLiveViewRunning = false;
                Common.StatusManager.ApplicationStatus = prevStatus;
                Common.Bug($"ExecuteLive crash-guard: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                Interlocked.Exchange(ref _liveToggleInProgress, 0);
            }
        }

        
        private void StopAllCameraLive()
        {
            List<int> displayIds;
            Dictionary<int, LiveTriggerState> triggerStates;
            lock (_liveSync)
            {
                displayIds = _liveFifoTools.Keys.ToList();
                _liveFifoTools.Clear();
                triggerStates = new Dictionary<int, LiveTriggerState>(_liveTriggerStates);
                _liveTriggerStates.Clear();
            }

            foreach (var displayId in displayIds)
            {
                var display = dspGrid.GetDisplayById(displayId);
                if (display == null) continue;

                try
                {
                    display.StopLiveDisplay();
                }
                catch (Exception ex)
                {
                    Common.Bug($"Lỗi StopLiveDisplay display {displayId}: {ex.Message}");
                }

                // Restore trigger mode after live stop for cameras that were switched.
                if (triggerStates.TryGetValue(displayId, out var state) && state?.AcqTool?.Operator != null && state.SwitchedToFreeRun)
                {
                    try
                    {
                        var restored = false;
                        if (state.PreviousTriggerModel != null)
                        {
                            restored = TrySetTriggerModel(state.AcqTool.Operator, state.PreviousTriggerModel);
                        }

                        if (!restored)
                        {
                            restored = TrySetTriggerModelByNames(state.AcqTool.Operator, "Manual", "OneShot", "Software");
                        }

                        if (!restored)
                        {
                            Common.Bug($"Cannot restore trigger mode for display {displayId}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.Bug($"Restore trigger fail display {displayId}: {ex.Message}");
                    }
                }
            }
            for (int i = 0; i < dspGrid.Count; i++)
            {
                try
                {
                    dspGrid.GetDisplayById(i)?.StopLiveDisplay();
                }
                catch (Exception ex)
                {
                    Common.Bug($"Lỗi StopLiveDisplay (sweep) display {i}: {ex.Message}");
                }
            }
        }

        private class LiveTriggerState
        {
            public CogAcqFifoTool AcqTool { get; set; }
            public object PreviousTriggerModel { get; set; }   // enum Cognex runtime
            public bool SwitchedToFreeRun { get; set; }
        }

        private readonly Dictionary<int, LiveTriggerState> _liveTriggerStates = new Dictionary<int, LiveTriggerState>();
        private readonly HashSet<uint> _camsNeedFreeRunForLive = new HashSet<uint>
        {

        };
        private static bool TryGetTriggerModel(ICogAcqFifo fifo, out object triggerModel)
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
        private static bool TrySetTriggerModel(ICogAcqFifo fifo, object triggerModel)
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
                {
                    valueToSet = Enum.Parse(modelProp.PropertyType, triggerModel.ToString(), ignoreCase: true);
                }
                modelProp.SetValue(triggerParams, valueToSet);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private static bool TrySetTriggerModelByNames(ICogAcqFifo fifo, params string[] enumNames)
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
                    // Try next enum name candidate.
                }
            }
            return false;
        }

        private bool StartAllCameraLive()
        {
            try
            {
                var cameraJobs = JobController.GetJobs<CameraJob>(false);
                if (cameraJobs == null || cameraJobs.Count == 0)
                {
                    MessageBox.Show((string)TryFindResource("msgErrorLoadJob"), (string)TryFindResource("strLive"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Ensure any existing live streams are stopped first to avoid VisionPro state collisions.
                StopAllCameraLive();

                var startedDisplays = new List<int>();

                foreach (var camJob in cameraJobs)
                {
                    CogToolBlock toolBlock = null;
                    if (camJob == null)
                        continue;
                    toolBlock = camJob.ToolBlock as CogToolBlock;
                    if (toolBlock == null)
                        continue;

                    CogAcqFifoTool acqTool = null;
                    try
                    {
                        acqTool = toolBlock.Tools.OfType<CogAcqFifoTool>().FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        Common.Bug($"StartAllCameraLive: cannot read acq tool for {camJob?.Name}: {ex.Message}");
                    }

                    if (acqTool?.Operator == null)
                        continue;

                    var displayId = (int)camJob.DisplayId;
                    var display = dspGrid.GetDisplayById(displayId);
                    if (display == null)
                        continue;

                    var fifo = acqTool.Operator;
                    object prevModel = null;
                    bool switched = false;
                    try
                    {
                        // If list is empty, apply FreeRun for all cameras during live.
                        var shouldSwitchToFreeRun = _camsNeedFreeRunForLive.Count == 0
                            || _camsNeedFreeRunForLive.Contains((uint)camJob.CamSettings.CameraId);
                        if (shouldSwitchToFreeRun)
                        {
                            TryGetTriggerModel(fifo, out prevModel);
                            // đổi sang FreeRun trước khi live
                            switched = TrySetTriggerModelByNames(fifo, "FreeRun", "Freerun", "Continuous");
                            if (!switched)
                            {
                                Common.Bug($"Cannot set FreeRun for cam {camJob.Name} (Display {displayId})");
                            }
                            else
                            {
                                // tùy camera, có thể cần nghỉ nhẹ
                                Thread.Sleep(80);
                            }
                        }
                        display.StartLiveDisplay(fifo, true);
                        startedDisplays.Add(displayId);
                        lock (_liveSync)
                        {
                            _liveTriggerStates[displayId] = new LiveTriggerState
                            {
                                AcqTool = acqTool,
                                PreviousTriggerModel = prevModel,
                                SwitchedToFreeRun = switched
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.Bug($"Error StartLiveDisplay camera {camJob.Name} (ID {displayId}): {ex.Message}");
                    }
                }

                lock (_liveSync)
                {
                    foreach (var id in startedDisplays.Distinct())
                    {
                        // store a placeholder to allow StopAllCameraLive to stop by displayId
                        // (we don't rely on the tool instance later, only the keys)
                        _liveFifoTools[id] = null;
                    }
                }

                if (startedDisplays.Count == 0)
                {
                    MessageBox.Show((string)TryFindResource("msgLiveFail"), (string)TryFindResource("strLive"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Common.Bug($"StartAllCameraLive crash-guard: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }
        private void ExecuteOpenLightManager()
        {
            try
            {
                string appDirectory = Common.Settings.OptDirectory;
                string optControllerPath = Path.Combine(appDirectory, "OPTController.exe");
                if (!File.Exists(optControllerPath))
                {
                    string parentDirectory = Directory.GetParent(appDirectory).FullName;
                    if (parentDirectory != null)
                    {
                        optControllerPath = Path.Combine(parentDirectory, "OPTController.exe");
                    }
                }
                if (File.Exists(optControllerPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = optControllerPath,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(optControllerPath)
                    });
                    Common.Info($"Launched OPTController.exe from: {optControllerPath}");
                }
                else
                {
                    MessageBox.Show("OPTController.exe not found.", "Light Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error launching OPTController.exe:\n{ex.Message}", "Light Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void ExecuteOpenLogManager()
        {
            try
            { 
                string LogDirectory = Common.Settings.SavingImageDirectory;
                if (Directory.Exists(LogDirectory))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = LogDirectory,
                        UseShellExecute = true
                    });
                    Common.Info($"Opened Log directory: {LogDirectory}");
                }
                else
                {
                    try
                    {
                        Directory.CreateDirectory(LogDirectory);
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = LogDirectory,
                            UseShellExecute = true
                        });
                        Common.Info($"Created and opened Log directory: {LogDirectory}");
                    }
                    catch (Exception createEx)
                    {
                        MessageBox.Show(
                            $"Log directory not found and cannot be created:\n{LogDirectory}\n\nError: {createEx.Message}",
                            "Log Manager",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        Common.Bug($"Cannot create log directory: {LogDirectory}\n{createEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
            $"Error opening Log directory:\n{ex.Message}",
            "Log Manager",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );
                Common.Bug($"Error opening Log directory: {ex.Message}\n{ex.StackTrace}");
            }
        }
        private void ExcuteExit()
        {
            Application.Current.MainWindow.Close();
        }
        private void ExecuteManualRun()
        {
            Common.StatusManager.ApplicationStatus = ApplicationStatus.Manual;
            Common.Settings.SystemStatus = (int)ApplicationStatus.Manual;
            Common.Settings.Save();
        }

        private void ExecuteStopAuto()
        {
            Common.StatusManager.ApplicationStatus = ApplicationStatus.Manual;
            Common.IsAutoMode = false;
            CommandManager.InvalidateRequerySuggested();
            Common.Settings.Save();
        }

        private void ExecuteAutoRun()
        {
            Common.StatusManager.ApplicationStatus = ApplicationStatus.AutoRunning;
            Common.IsAutoMode = true;
            _isLiveViewRunning = false;
            CommandManager.InvalidateRequerySuggested();
            Common.Settings.Save();
        }



        public void CollapsedAllLiveDisplay()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                mainContentArea.Visibility = Visibility.Collapsed;
            });
        }

        public void VisibleAllLiveDisplay()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                mainContentArea.Visibility = Visibility.Visible;
            });
        }
        private void Init()
        {
            DataContext = this;

            var mainWindow = (Application.Current.MainWindow as MainWindow2);
            mainWindow.OnAllJobLoadedDone += OnJobLoadedDone;
            mainWindow.OnJobToolProcessing += OnJobToolResponse;
            mainWindow.OnProductRetrieveHandleEvent += OnJobDone;
            JobController.OnClearDisplayForJob += ClearDisplayForJob;
            JobController.InspectionPreviewUpdated += ShowInspectionPreview;
            if(Common.Settings != null)
            {
                Common.StatusManager.ApplicationStatus = (ApplicationStatus)Common.Settings.SystemStatus;
                Common.IsAutoMode = Common.StatusManager.ApplicationStatus == ApplicationStatus.AutoRunning;
            }
            Common.OperationModeChanged += OperationModeChanged_ipl;
            Common.LoadResultSettings();
            if(Common.ResultModel != null)
            {
                UpdateQuantity();
            }
            SavingImgCtrl.Init();  
            var displayList = Settings.DisplayList;
            var displaySize = Settings.DisplaySizes;
            dspGrid.SetSize(displayList, displaySize);

        }
        

        private void OnJobLoadedDone(object sender)
        {
            LoadJobs(false);
        }

        private void OnJobToolResponse(FunctionJob job, string msg)
        {
        }

        /// <summary>Clears previous result on main view when a new job cycle starts (avoids showing old OK when new capture is NG).</summary>
        private void ClearDisplayForJob(IEnumerable<int> displayIds)
        {
            if (displayIds == null) return;
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    foreach (var displayId in displayIds)
                    {
                        dspGrid.SetFooterContent(displayId, new List<object> { "" });
                        ClearAlignResult(displayId);
                    }
                }
                catch (Exception ex)
                {
                    Bug("ClearDisplayForJob Exception: {0}", ex.Message);
                }
            });
        }

        private void OnJobDone(List<object> lstResults, int displayID, string fileName, bool isOK, double actualDist = double.NaN)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (lstResults != null && lstResults.Count > 0)
                    {
                        dspGrid.SetFooterContent(displayID, lstResults, actualDist);
                        SetAlignResult(lstResults, displayID);
                    }
                    else
                    {
                        dspGrid.SetFooterContent(displayID, new List<object> { "" }, actualDist);
                    }
                    // set count
                    setResultCount(displayID, isOK);
                    UpdateQuantity();
                }
                catch (Exception ex)
                {
                    Bug("OnJobDone Exception: {0}", ex.Message);

                    Bug(ex.StackTrace);
                }

            });
        }

        private void setResultCount(int displayID, bool isOK)
        {
            StageResult stageResult;

            // stage
            if (displayID >= 2)
                stageResult = Common.ResultModel.StageB;
            else
                stageResult = Common.ResultModel.StageA;

            stageResult.TotalQuantity++;

            if (isOK)
                stageResult.OKQuantity++;
            else
                stageResult.NGQuantity++;
        }

        private void SetAlignResult(List<object> lstResults, int displayId)
        {
            var pose = lstResults.OfType<Pose>().FirstOrDefault();

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (pose != null)
                {
                    if (displayId >= 2)
                    {
                        //txtXB.Text = pose.X.ToString("F3");
                        //txtYB.Text = pose.Y.ToString("F3");
                        //txtThetaB.Text = pose.Th.ToString("F3");
                    }
                    else
                    {
                        //txtXA.Text = pose.X.ToString("F3");
                        //txtYA.Text = pose.Y.ToString("F3");
                        //txtThetaA.Text = pose.Th.ToString("F3");
                    }
                }
                else
                {
                    ClearAlignResult(displayId);
                }
            });
        }
        private void ClearAlignResult(int displayId)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (displayId >= 2)
                {
                    //txtXB.Text = "-";
                    //txtYB.Text = "-";
                    //txtThetaB.Text = "-";
                }
                else
                {
                    //txtXA.Text = "-";
                    //txtYA.Text = "-";
                    //txtThetaA.Text = "-";
                }
            });
        }
        private void UpdateQuantity()   
        {
            var stageA = Common.ResultModel.StageA;
            var stageB = Common.ResultModel.StageB;
            // Stage A
            //txtTotalA.Text = stageA.TotalQuantity.ToString();
            //txtOKA.Text = stageA.OKQuantity.ToString();
            //txtNGA.Text = stageA.NGQuantity.ToString();

            // Stage B
            //txtTotalB.Text = stageB.TotalQuantity.ToString();
            //txtOKB.Text = stageB.OKQuantity.ToString();
            //txtNGB.Text = stageB.NGQuantity.ToString();
        }

        private async void LoadJobs(bool force = false)
        {
            //jobsLoaded = 0;
            var lst = JobController.GetAllJobs(false);



            if ((null == lst) || (lst.Count < 1))
                return;
            foreach (var job in lst)
            {
                if (job is CameraJob cJob)
                {
                    // Prevent duplicated subscriptions if LoadJobs is called multiple times.
                    cJob.OnToolBlockRan -= OnCamJobRan;
                    cJob.OnToolBlockRan += OnCamJobRan;
                }
                else if (job is AlignJob aJob)
                {
                    aJob.OnToolBlockRan -= OnAlignJobRan;
                    aJob.OnToolBlockRan += OnAlignJobRan;

                }
                else if (job is IspJob iJob)
                {
                    iJob.OnToolBlockRan -= OnIspJobRan;
                    iJob.OnToolBlockRan += OnIspJobRan;
                }
                else if(job is WatcherJob wJob)
                {
                    wJob.OnToolBlockRan -= OnWatcherJobRan;
                    wJob.OnToolBlockRan += OnWatcherJobRan;
                }
            }

            var c0 = Settings.Watermarks == null ? 0 : Settings.Watermarks.Count;
            var ndsp = Settings.DisplayList.Sum();

            for (int idx = 0; idx < c0; idx++)
            {
                if (idx >= ndsp) break;

                var txt = Settings.Watermarks[idx];
                dspGrid.SetWatermark(idx, txt);
            }


        }
        private void OnCamJobRan(Object sendor, object img, object CamId)
        {
            var job = sendor as CameraJob;
            if (job != null && img != null)
            {
                var dspId = (int)job.DisplayId;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        dspGrid.SetImage((ICogImage)img, dspId);
                    }
                    catch (Exception ex)
                    {
                        Common.Bug($"OnCamJobRan UI update error (dsp {dspId}): {ex.Message}");
                    }
                }), DispatcherPriority.Render);
            }
        }

        private void ShowInspectionPreview(int displayId, ICogImage image, string watermark)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dspGrid.ForceSetImage(image, displayId);
                    if (!string.IsNullOrEmpty(watermark))
                    {
                        dspGrid.SetWatermark(displayId, watermark);
                    }
                }
                catch (Exception ex)
                {
                    Common.Bug("ShowInspectionPreview UI update error (display {0}): {1}", displayId, ex.Message);
                }
            }), DispatcherPriority.Render);
        }

        private void OnAlignJobRan(Object sendor, object Result, object Results)
        {
            var job = (FunctionJob)sendor;
            var CameraId = (UInt32)job.CamSettings.CameraId;
            var dspId = (int)job.DisplayId;
            var record = (ICogRecord)job.Record;
            var img = (ICogImage)job.OutputImage;

            if (img != null && record != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        dspGrid.SetDisplayContent(dspId, img, record);
                    }
                    catch (Exception ex)
                    {
                        Common.Bug($"OnAlignJobRan UI update error (dsp {dspId}): {ex.Message}");
                    }
                }), DispatcherPriority.Render);
            }

        }

        private void OnIspJobRan(Object sendor, object Result, object Results)
        {
            var job = (IspJob)sendor;
            var dspId = (int)job.DisplayId;
            var record = (ICogRecord)job.Record;
            var img = (ICogImage)job.OutputImage;

            if (img != null && record != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        dspGrid.SetDisplayContent(dspId, img, record);
                    }
                    catch (Exception ex)
                    {
                        Common.Bug($"OnIspJobRan UI update error (dsp {dspId}): {ex.Message}");
                    }
                }), DispatcherPriority.Render);
            }

        }

        private void OnWatcherJobRan(Object sendor, object Result, object Results)
        {
            var job = (FunctionJob)sendor;
            var dspId = (int)job.DisplayId;
            var record = (ICogRecord)job.Record;
            var img = (ICogImage)job.OutputImage;

            if (img != null && record != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        dspGrid.SetDisplayContent(dspId, img, record);
                    }
                    catch (Exception ex)
                    {
                        Common.Bug($"OnWatcherJobRan UI update error (dsp {dspId}): {ex.Message}");
                    }
                }), DispatcherPriority.Render);
            }

        }
        private void OperationModeChanged_ipl(OperationMode Mode)
        {

            Dispatcher.Invoke((Action)(() =>
            {
                switch (Mode)
                {
                    case OperationMode.Master:
                        lblStatus.Content = "Master";
                        btnUserAccount.Content = (string)TryFindResource("strSwitchUser");
                        UpdateUsernameDisplay();
                        ShowToolDisplay(true);
                        StartAdminSessionCountdown();
                        break;

                    case OperationMode.Engineer:
                        StopAdminSessionCountdown();
                        lblStatus.Content = "Engineer";
                        btnUserAccount.Content = (string)TryFindResource("strSwitchUser");
                        UpdateUsernameDisplay();
                        ShowToolDisplay(true);
                        break;

                    case OperationMode.Operator:
                    default:
                        StopAdminSessionCountdown();
                        lblStatus.Content = "Operator";
                        btnUserAccount.Content = (string)TryFindResource("strSwitchUser");
                        UpdateUsernameDisplay();
                        ShowToolDisplay(false);
                        break;
                }
            }));
        }
        private async Task ShowLoginDialogToSwitchUser()
        {
            var metroWindow = (Application.Current.MainWindow as MainWindow2);
            var loginDialog = new ViewComponents.LoginDialog(metroWindow);

            await metroWindow.ShowMetroDialogAsync(loginDialog);
            await loginDialog.WaitUntilUnloadedAsync();

            if (loginDialog.LoggedInAccount != null)
            {
                var account = loginDialog.LoggedInAccount;
                Common.Info($"User switched to {account.Username} ({account.Role})");

                Dispatcher.Invoke(() =>
                {
                    lblStatus.Content = account.Role.ToString();
                    UpdateUsernameDisplay();
                    RefreshUIByRole();
                    CheckAndRedirectToMainView();

                    if (account.Role == UserRole.Master)
                    {
                        StartAdminSessionCountdown();
                    }
                    else
                    {
                        StopAdminSessionCountdown();
                    }
                });
            }
        }
        private void CheckAndRedirectToMainView()
        {
            bool restricted = currentView == toolSettingWindow ||
                              currentView == accountManagerView ||
                              currentView == modelsManagerView;

            if (restricted)
            {
                ShowView(mainGridView);
                btnLiveView.IsEnabled = true;
                Common.Info("Redirected to main view due to insufficient permissions");
            }
        }
        // Dispose all when  dispose ImageView
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                try
                {
                    var mainWindow = (Application.Current.MainWindow as MainWindow2);
                    if (mainWindow != null)
                    {
                        mainWindow.OnAllJobLoadedDone -= OnJobLoadedDone;
                        mainWindow.OnJobToolProcessing -= OnJobToolResponse;
                        mainWindow.OnProductRetrieveHandleEvent -= OnJobDone;
                    }
                    JobController.InspectionPreviewUpdated -= ShowInspectionPreview;
                }
                catch { /* best-effort detach */ }

                try { Common.OperationModeChanged -= OperationModeChanged_ipl; } catch { /* best-effort detach */ }
                if (_autoLockTimer != null)
                {
                    _autoLockTimer.Stop();
                    _autoLockTimer.Tick -= AutoLockTimer_Tick;
                    _autoLockTimer = null;
                }
                JobController.OnClearDisplayForJob -= ClearDisplayForJob;
                StopAllCameraLive();

                DisposeWindow(optionWindow);
                DisposeWindow(toolSettingWindow);
                DisposeWindow(accountManagerView);
                DisposeWindow(communicationView);
                DisposeWindow(modelsManagerView);
                if (mainGridView != null)
                {
                    mainGridView.MeasureRequested -= MainGridView_MeasureRequested;
                }
                CloseMeasureDistanceWindow();
 
                disposed = true;
            }
        }
        private void RefreshUIByRole()
        {
            Dispatcher.Invoke(() =>
            {
                var role = Common.CurrentUser?.Role ?? UserRole.Operator;

                bool isMaster = role == UserRole.Master;
                bool isEngineer = role == UserRole.Engineer;
                btnOpenTool.Visibility = isMaster ? Visibility.Visible : Visibility.Collapsed;
                btnAccountManager.Visibility = isMaster ? Visibility.Visible : Visibility.Collapsed;

                //btnSetup.Visibility = isMaster || isEngineer ? Visibility.Visible : Visibility.Collapsed;
                //btnCalibration.Visibility = isMaster || isEngineer ? Visibility.Visible : Visibility.Collapsed;
                btnModelsManager.Visibility = isMaster || isEngineer ? Visibility.Visible : Visibility.Collapsed;
                btnMotionControl.Visibility = Visibility.Visible;

                lblStatus.Content = role.ToString();
                UpdateUsernameDisplay();
            });
        }
        public void ShowToolDisplay(bool v)
        {
            RefreshUIByRole();
        }


       

        private void StartAdminSessionCountdown()
        {
            if (Common.CurrentUser == null ||
                Common.CurrentUser.Role != UserRole.Master ||
                Common.CurrentOperationMode != OperationMode.Master)
            {
                StopAdminSessionCountdown();
                return;
            }

            _adminSessionExpiresAt = DateTime.Now.Add(AdminSessionDuration);
            TimeCountDown = AdminSessionDuration;
            IsAdminSessionVisible = true;

            _autoLockTimer.Stop();
            _autoLockTimer.Start();
        }

        private void StopAdminSessionCountdown()
        {
            if (_autoLockTimer != null)
            {
                _autoLockTimer.Stop();
            }

            TimeCountDown = TimeSpan.Zero;
            IsAdminSessionVisible = false;
        }

        private void AutoLockTimer_Tick(object sender, EventArgs e)
        {
            if (Common.CurrentUser == null ||
                Common.CurrentUser.Role != UserRole.Master ||
                Common.CurrentOperationMode != OperationMode.Master)
            {
                StopAdminSessionCountdown();
                return;
            }

            TimeSpan remainingTime = _adminSessionExpiresAt.Subtract(DateTime.Now);
            if (remainingTime <= TimeSpan.Zero)
            {
                LogoutAdminToOperator();
                return;
            }

            int remainingSeconds = (int)Math.Ceiling(remainingTime.TotalSeconds);
            TimeCountDown = TimeSpan.FromSeconds(remainingSeconds);
        }

        private void LogoutAdminToOperator()
        {
            StopAdminSessionCountdown();

            Account operatorAccount = null;
            try
            {
                List<Account> accounts = AccountManager.GetAllAccounts();
                if (accounts != null)
                {
                    operatorAccount = accounts.FirstOrDefault(account =>
                        account != null &&
                        account.Role == UserRole.Operator &&
                        account.IsActive);
                }
            }
            catch (Exception ex)
            {
                Common.Bug("Cannot load Operator account during automatic logout: {0}", ex.Message);
            }

            Common.CurrentUser = operatorAccount;
            Common.CurrentOperationMode = OperationMode.Operator;
            RefreshUIByRole();
            CheckAndRedirectToMainView();

            if (operatorAccount != null)
            {
                Common.Info("Admin session expired. Switched automatically to Operator account: {0}", operatorAccount.Username);
            }
            else
            {
                Common.Bug("Admin session expired, but no active Operator account was found. Operator permissions were applied without a user account.");
            }
        }
        private TimeSpan _tsp;
        private bool _isAdminSessionVisible;

        public bool IsAdminSessionVisible
        {
            get => _isAdminSessionVisible;
            private set
            {
                if (_isAdminSessionVisible == value)
                {
                    return;
                }

                _isAdminSessionVisible = value;
                Notify();
            }
        }

        public TimeSpan TimeCountDown
        {
            get => _tsp;
            set
            {
                _tsp = value;
                Notify();
                Notify(nameof(AdminSessionCountdownText));
            }
        }

        public string AdminSessionCountdownText
        {
            get
            {
                int totalMinutes = (int)Math.Floor(TimeCountDown.TotalMinutes);
                return string.Format("{0:00}:{1:00}", totalMinutes, TimeCountDown.Seconds);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string strPropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyName));
        }
        public void UpdateUsernameDisplay()
        {
            Dispatcher.Invoke(() =>
            {
                if (Common.CurrentUser != null)
                {
                    lblUsername.Content = Common.CurrentUser.Username;
                }
                else
                {
                    lblUsername.Content = "-";
                }
            });
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var metroWindow = (Application.Current.MainWindow as MainWindow2);
            metroWindow?.SetLanguageDictionary((Lang)Settings.CurrentLanguage);
            RefreshTextButton();
        }
        public void RefreshTextButton()
        {
           
            if (Common.CurrentUser != null)
            {
                lblStatus.Content = Common.CurrentUser.Role.ToString();
                btnUserAccount.Content = (string)TryFindResource("strSwitchUser");
                UpdateUsernameDisplay();
            }
            else
            {
                lblStatus.Content = Common.CurrentOperationMode.ToString();
                btnUserAccount.Content = (string)TryFindResource("strLogin");
                lblUsername.Content = "-";
            }
        }
        private async void btnUserAccount_Click(object sender, RoutedEventArgs e)
        {
            CollapsedAllLiveDisplay();
            await ShowLoginDialogToSwitchUser();
            VisibleAllLiveDisplay();
        }
    }
}
