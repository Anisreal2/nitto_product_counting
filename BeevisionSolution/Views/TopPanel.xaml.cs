
using BeevisionSolution.Controller;
using BeevisionSolution.Models;
using BeevisionSolution.Utils;
using BeevisionSolution.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static BeevisionSolution.Utils.Common;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace BeevisionSolution.Views
{
    /// <summary>
    /// Interaction logic for TopPanel.xaml
    /// </summary>
    public partial class TopPanel : UserControl, INotifyPropertyChanged
    {
        private readonly DispatcherTimer _dateTimeTimer;
        private int _plcTickCounter = 0;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string strPropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyName));
        }
        public TopPanel()
        {
            InitializeComponent();

            this.DataContext = Common.StatusManager;
            UpdateImageCountryFlag();
            txtAppName.Text = Common.Settings.AppName;
            JobController.UpdateData += JobController_UpdateData;
            lblModel.Text = Common.Settings.CurrentProfile;
            EditButton.Click += EditButton_Click;
            Loaded += TopPanel_Loaded;
            Unloaded += TopPanel_Unloaded;
            
            _dateTimeTimer = new DispatcherTimer();
            _dateTimeTimer.Interval = TimeSpan.FromSeconds(1);
            _dateTimeTimer.Tick += (s, e) =>
            {
                Common.StatusManager.CurrentDateTime =
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _plcTickCounter++;

                if (_plcTickCounter >= 2)
                {
                    _plcTickCounter = 0;
                    UpdatePlcStatus();
                }
            };
            _dateTimeTimer.Start();
        }

        private void TopPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow2;
            if (mainWindow == null)
            {
                return;
            }

            mainWindow.OnAllJobLoadedDone -= OnJobLoadedDone;
            mainWindow.OnAllJobLoadedDone += OnJobLoadedDone;
            UpdateModelName(Common.Settings.CurrentProfile);
        }

        private void TopPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow2;
            if (mainWindow != null)
            {
                mainWindow.OnAllJobLoadedDone -= OnJobLoadedDone;
            }
        }

        private void OnJobLoadedDone(object sender)
        {
            UpdateModelName(Common.Settings.CurrentProfile);
        }

        private void UpdateImageCountryFlag()
        {
            var intFlag = Common.Settings.CurrentLanguage;
            switch (intFlag)
            {
                case (int)Lang.en:
                    brdENLang.Opacity = 1.0;
                    brdVNLang.Opacity = 0.5;
                    break;
                case (int)Lang.vn:
                    brdVNLang.Opacity = 1.0;
                    brdENLang.Opacity = 0.5;
                    break;
                default:
                    brdENLang.Opacity = 1.0;
                    brdVNLang.Opacity = 0.5;
                    break;
            }
        }

        private void JobController_UpdateData(int displayId, string dataSend)
        {
            Dispatcher.Invoke(() =>
            {
                if (displayId >= 3)
                {
                    txtDataB.Text = dataSend;
                }
                else
                {

                    txtDataA.Text = dataSend;
                }
            });
        }
        private void UpdatePlcStatus()
        {
            var motion = Controller.MotionSequenceManager.Instance?.Motion;
            bool isOp = motion != null && motion.IsMasterOp;
            Common.StatusManager.PlcStatus = isOp ? PLCStatus.Online : PLCStatus.Offline;
            Common.StatusManager.PcSurvival = isOp;
        }

        //private void UpdatePlcStatus()
        //{
        //    if (_plc == null)
        //    {
        //        Common.StatusManager.PlcStatus = PLCStatus.Offline;
        //        return;
        //    }

        //    Common.StatusManager.PlcStatus = _plc.IsConnected
        //        ? PLCStatus.Online
        //        : PLCStatus.Offline;
        //}

        //private void UpdatePlcStatus()
        //{
        //    var mainWindow = Application.Current.MainWindow as MainWindow2;
        //    int connections = mainWindow?.ServerConnections ?? 0;

        //    Common.StatusManager.PlcStatus = connections > 0
        //        ? PLCStatus.Online
        //        : PLCStatus.Offline;
        //}

        private void SwitchLanguage(Lang lang)
        {
            if (Application.Current.MainWindow is MainWindow2 mainWindow)
            {
                mainWindow.SetLanguageDictionary(lang);
            }
        }

        private void brdVNLang_Click(object sender, RoutedEventArgs e)
        {
            SwitchLanguage(Lang.vn);
            Common.Settings.CurrentLanguage = (int)Lang.vn;
            Common.Settings.Save();
            Common.StatusManager.RefreshDependentTexts();
            UpdateImageCountryFlag();
            RefreshMainUI();
        }

        private void brdENLang_Click(object sender, RoutedEventArgs e)
        {
            SwitchLanguage(Lang.en);
            Common.Settings.CurrentLanguage = (int)Lang.en;
            Common.Settings.Save();
            Common.StatusManager.RefreshDependentTexts();
            UpdateImageCountryFlag();
            RefreshMainUI();
        }
        private void RefreshMainUI()
        {
            if (ImageView.CurrentInstance != null)
            {
                ImageView.CurrentInstance.Dispatcher.Invoke(() =>
                {
                    ImageView.CurrentInstance.RefreshTextButton(); 
                });
            }
        }
        public void UpdateModelName(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
            {
                return;
            }

            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => UpdateModelName(modelName)));
                return;
            }

            lblModel.Text = modelName;
            Info("TopPanel: Model name updated to '{0}'", modelName);
        }

        public void RefreshPlcReference()
        {
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            string modelName = Common.Settings.CurrentProfile;
            if (!string.IsNullOrEmpty(modelName))
            {
                Window parentWindow = Window.GetWindow(this);
                PopUpWindow.ShowDialog(parentWindow, this, ref modelName);
            }
        }

        private void BtnScreenshot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string rootFolder = Settings.RootFolder ?? Settings.SavingImageDirectory ?? BaseStorageDirectory;
                string screenshotFolder = Path.Combine(rootFolder, "Bee_Screenshot");
                if (!Directory.Exists(screenshotFolder))
                {
                    Directory.CreateDirectory(screenshotFolder);
                    Info("Created screenshot directory: {0}", screenshotFolder);
                }

                //screen Shots
                var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.CopyFromScreen (System.Drawing.Point.Empty, System.Drawing.Point.Empty ,bounds.Size);
                    }

                    //name file with timespan 
                    string fileName = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmssfff}.png";
                    string filePath = Path.Combine(screenshotFolder, fileName);
                    // save file 
                    bitmap.Save(filePath);
                    Info("Screenshot successfully saved at : {0}", filePath);
                    MessageBox.Show($"Screenshot saved:\n{filePath}", "Screenshot",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Bug("Error taking screenshot: {0}", ex.Message);
                MessageBox.Show($"Error taking screenshot:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
