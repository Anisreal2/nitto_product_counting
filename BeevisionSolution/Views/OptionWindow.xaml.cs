using BeevisionSolution.Utils;
using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows;
using BeevisionSolution.Controller;
using BeevisionSolution.Models;
using System.Collections.Generic;
using System.Globalization;

namespace BeevisionSolution.Views
{
    public partial class OptionWindow : UserControl, INotifyPropertyChanged
    {
        private readonly List<MultiMarkLengthCheckSettingRow> _lengthCheckSettingRows =
            new List<MultiMarkLengthCheckSettingRow>();

        public OptionWindow()
        {
            InitializeComponent();
            Loaded += OptionWindow_Loaded;
            LoadSettings();
            LoadMultiMarkLengthCheckSettings();
            LoadDriveInfo();
        }

        // Tự động tải lại cấu hình mỗi khi màn hình Option được mở lên
        private void OptionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            LoadMultiMarkLengthCheckSettings();
            LoadDriveInfo();
        }

        private string _totalSpace { get; set; }
        public string TotalSpace
        {
            get { return _totalSpace; }
            set
            {
                _totalSpace = value;
                Notify();
            }
        }
        private string _freeSpace { get; set; }
        public string FreeSpace
        {
            get { return _freeSpace; }
            set
            {
                _freeSpace = value;
                Notify();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string strPropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyName));
        }

        public void LoadSettings()
        {
            var s = Common.Settings;

            tbMotionLimitX.Text = s.MotionLimitX.ToString();
            tbMotionLimitY.Text = s.MotionLimitY.ToString();
            tbMotionLimitTheta.Text = s.MotionLimitTheta.ToString();

            tbAutoBackupTimeHours.Text = s.AutoBackupTimeHours.ToString();
            tbSizeLimit.Text = s.SizeLimit.ToString();

            chkAutoModelChange.IsChecked = s.AutoModelChange;
            chkShowCenterLine.IsChecked = s.ShowCenterLine;

            chkSaveScreenshotImage.IsChecked = s.SaveScreenshotImage;
            chkSaveOkRaw.IsChecked = s.SaveImageOK;
            chkSaveNgRaw.IsChecked = s.SaveImageNG;
            SelectSavingImageFormat(s.SavingImageFormat);
            tbImageFactorOK.Text = s.ImageFactorOK.ToString();
            tbImageFactorNG.Text = s.ImageFactorNG.ToString();
            tbOverlayLineWidth.Text = s.OverlayLineWidth.ToString();

            chkModifyScoreLimit.IsChecked = s.ModifyScoreLimit;
            txtScoreLimit.Text = s.ScoreLimit.ToString();

            txtInspectDelay.Text = s.InspectDelay.ToString("F2");

            chkUseRetryMode.IsChecked = s.UseRetryMode;
            tbRetryX.Text = s.RetryX.ToString();
            tbRetryY.Text = s.RetryY.ToString();
            tbRetryTheta.Text = s.RetryTheta.ToString();
            tbMaxRetries.Text = s.MaxRetries.ToString();

            tbCrossLineScale.Text = s.CrossLineScale.ToString();

            chkAutoCleanUp.IsChecked = s.AutoCleanUp;
            txtDriveSpaceThreshold.Text = s.DriveSpaceThreshold.ToString();
            tbDaysInHistory.Text = s.DaysInHistory.ToString();

            chkEnableAutoZipBackup.IsChecked = s.EnableAutoZipBackup;
            tbZipBackupIntervalDays.Text = s.ZipBackupIntervalDays.ToString();
            txtSavingImageDirectory.Text = string.IsNullOrEmpty(s.SavingImageDirectory) ? "(Use the default folder.)" : s.SavingImageDirectory;
            txtRootFolder.Text = string.IsNullOrEmpty(s.RootFolder) ? "(Use the default folder.)" : s.RootFolder;
            chkUseOpCallForMultiMark.IsChecked = s.UseOpCallForMultiMark;

            chkIsLightControl.IsChecked = s.IsLightControl;
            UpdateLightControlButtonsVisibility();
        }

        private void LoadDriveInfo()
        {
            try
            {
                string root = Path.GetPathRoot(Common.Settings.LoggingDirectory);
                DriveInfo d = new DriveInfo(root);
                txtTotalSpace.Text = $"{d.TotalSize / (1024 * 1024 * 1024)} GB";
                txtFreeSpace.Text = $"{d.AvailableFreeSpace / (1024 * 1024 * 1024)} GB";
            }
            catch
            {
                txtTotalSpace.Text = "Unknown";
                txtFreeSpace.Text = "Unknown";
            }
        }

        private void SaveSettings()
        {
            var s = Common.Settings;

            s.MotionLimitX = ParseDouble(tbMotionLimitX.Text);
            s.MotionLimitY = ParseDouble(tbMotionLimitY.Text);
            s.MotionLimitTheta = ParseDouble(tbMotionLimitTheta.Text);

            s.AutoBackupTimeHours = ParseInt(tbAutoBackupTimeHours.Text);
            s.SizeLimit = ParseInt(tbSizeLimit.Text);

            s.AutoModelChange = chkAutoModelChange.IsChecked == true;
            s.ShowCenterLine = chkShowCenterLine.IsChecked == true;

            s.SaveScreenshotImage = chkSaveScreenshotImage.IsChecked == true;
            s.SaveImageOK = chkSaveOkRaw.IsChecked == true;
            s.SaveImageNG = chkSaveNgRaw.IsChecked == true;
            s.SavingImageFormat = GetSelectedSavingImageFormat();
            s.ImageFactorOK = ParseFloat(tbImageFactorOK.Text);
            s.ImageFactorNG = ParseFloat(tbImageFactorNG.Text);
            s.OverlayLineWidth = Math.Max(1, Math.Min(20, ParseInt(tbOverlayLineWidth.Text)));

            s.ModifyScoreLimit = chkModifyScoreLimit.IsChecked == true;
            s.ScoreLimit = ParseDouble(txtScoreLimit.Text);

            s.InspectDelay = ParseDouble(txtInspectDelay.Text);

            s.UseRetryMode = chkUseRetryMode.IsChecked == true;
            s.RetryX = ParseDouble(tbRetryX.Text);
            s.RetryY = ParseDouble(tbRetryY.Text);
            s.RetryTheta = ParseDouble(tbRetryTheta.Text);
            s.MaxRetries = ParseInt(tbMaxRetries.Text);

            s.DriveSpaceThreshold = ParseDouble(txtDriveSpaceThreshold.Text);
            s.DaysInHistory = ParseInt(tbDaysInHistory.Text);

            s.AutoCleanUp = chkAutoCleanUp.IsChecked == true;
            s.EnableAutoZipBackup = chkEnableAutoZipBackup.IsChecked == true;
            s.ZipBackupIntervalDays = ParseInt(tbZipBackupIntervalDays.Text);
            s.UseOpCallForMultiMark = chkUseOpCallForMultiMark.IsChecked == true;

            s.IsLightControl = chkIsLightControl.IsChecked == true;

            s.CrossLineScale = ParseDouble(tbCrossLineScale.Text);
            string savingDir = txtSavingImageDirectory.Text;
            if (savingDir == "Use the default folder." || string.IsNullOrWhiteSpace(savingDir))
            {
                s.SavingImageDirectory = null;
            }
            else
            {
                s.SavingImageDirectory = savingDir;
            }
            string rootDir = txtRootFolder.Text;
            if (rootDir == "Use the default folder." || string.IsNullOrWhiteSpace(rootDir))
            {
                s.RootFolder = null;
            }
            else
            {
                s.RootFolder = rootDir;
            }

            s.Save();
        }

        private void tbDaysInHistory_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
                return;
            }
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
                if (int.TryParse(newText, out int value))
                {
                    if (value > 99)
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
        }

        private void tbDaysInHistory_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && !string.IsNullOrEmpty(textBox.Text))
            {
                if (int.TryParse(textBox.Text, out int value))
                {
                    if (value > 99)
                    {
                        textBox.Text = "99";
                        textBox.CaretIndex = textBox.Text.Length;
                    }
                    else if (value < 0)
                    {
                        textBox.Text = "0";
                        textBox.CaretIndex = textBox.Text.Length;
                    }
                }
            }
        }

        private void btnBrowseLogDirectory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dialog.Description = "Choose a folder to save images and logs.";
                    dialog.ShowNewFolderButton = true;

                    // Set initial directory nếu đã có
                    if (!string.IsNullOrEmpty(txtSavingImageDirectory.Text) &&
                        txtSavingImageDirectory.Text != "(Use the default folder.)" &&
                        System.IO.Directory.Exists(txtSavingImageDirectory.Text))
                    {
                        dialog.SelectedPath = txtSavingImageDirectory.Text;
                    }
                    else if (!string.IsNullOrEmpty(Common.Settings.LoggingDirectory) &&
                             System.IO.Directory.Exists(Common.Settings.LoggingDirectory))
                    {
                        dialog.SelectedPath = System.IO.Path.GetDirectoryName(Common.Settings.LoggingDirectory);
                    }

                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        txtSavingImageDirectory.Text = dialog.SelectedPath;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error when selecting folder: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnBrowseRootFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dialog.Description = "Choose a root folder for screenshots and other files.";
                    dialog.ShowNewFolderButton = true;

                    if (!string.IsNullOrEmpty(txtRootFolder.Text) &&
                        txtRootFolder.Text != "(Use the default folder.)" &&
                        System.IO.Directory.Exists(txtRootFolder.Text))
                    {
                        dialog.SelectedPath = txtRootFolder.Text;
                    }
                    else if (!string.IsNullOrEmpty(Common.Settings.RootFolder) &&
                             System.IO.Directory.Exists(Common.Settings.RootFolder))
                    {
                        dialog.SelectedPath = Common.Settings.RootFolder;
                    }

                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        txtRootFolder.Text = dialog.SelectedPath;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error when selecting folder: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static double ParseDouble(string txt)
        {
            double.TryParse(txt, out double v);
            return v;
        }

        private static int ParseInt(string txt)
        {
            int.TryParse(txt, out int v);
            return v;
        }

        private static float ParseFloat(string txt)
        {
            float.TryParse(txt, out float v);
            return v;
        }

        private void SelectSavingImageFormat(ImageFormat fmt)
        {
            string targetTag = (fmt?.ToString() ?? "Png").ToLowerInvariant();
            foreach (var item in cbSavingImageFormat.Items)
            {
                if (item is ComboBoxItem cbItem)
                {
                    string tag = (cbItem.Tag?.ToString() ?? string.Empty).ToLowerInvariant();
                    if (tag == targetTag)
                    {
                        cbSavingImageFormat.SelectedItem = cbItem;
                        return;
                    }
                }
            }

            cbSavingImageFormat.SelectedIndex = 0;
        }

        private ImageFormat GetSelectedSavingImageFormat()
        {
            if (cbSavingImageFormat.SelectedItem is ComboBoxItem cbItem)
            {
                switch (cbItem.Tag?.ToString())
                {
                    case "Jpeg": return ImageFormat.Jpeg;
                    case "Bmp": return ImageFormat.Bmp;
                    case "Tiff": return ImageFormat.Tiff;
                    default: return ImageFormat.Png;
                }
            }

            return ImageFormat.Png;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveMultiMarkLengthCheckSettings();
                SaveSettings();
                JobController.ReloadSettings();
                Common.InitAutoZipBackup();
                MessageBox.Show((string)TryFindResource("msgSaveSuccessful"), (string)TryFindResource("optTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
                var imageView = ImageView.CurrentInstance;
                imageView?.dspGrid.RefreshAllCrossLines();
            }
            catch (Exception ex)
            {
                MessageBox.Show((string)TryFindResource("msgSaveFailed") + ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMultiMarkLengthCheckSettings()
        {
            int previouslySelectedJobId = -1;
            if (cmbLengthCheckPlcCam.SelectedItem is MultiMarkLengthCheckSettingRow previousRow && previousRow.PlcCamera != null)
            {
                previouslySelectedJobId = previousRow.PlcCamera.PlcJobId;
            }

            _lengthCheckSettingRows.Clear();

            List<PlcCam> plcCameras = JobController.GetAllPlcCam();
            if (plcCameras != null)
            {
                for (int index = 0; index < plcCameras.Count; index++)
                {
                    PlcCam plcCamera = plcCameras[index];
                    if (plcCamera == null || !plcCamera.IsMultiMarks)
                    {
                        continue;
                    }

                    _lengthCheckSettingRows.Add(new MultiMarkLengthCheckSettingRow
                    {
                        PlcCamera = plcCamera,
                        DisplayName = BuildLengthCheckDisplayName(plcCamera),
                        IsEnabled = plcCamera.EnableLengthCheck,
                        ExpectedLengthText = plcCamera.ExpectedLengthMillimeters.ToString("0.###", CultureInfo.InvariantCulture),
                        ToleranceText = plcCamera.LengthToleranceMillimeters.ToString("0.###", CultureInfo.InvariantCulture),
                        OffsetText = plcCamera.LengthOffsetMillimeters.ToString("0.###", CultureInfo.InvariantCulture)
                    });
                }
            }

            cmbLengthCheckPlcCam.ItemsSource = null;
            cmbLengthCheckPlcCam.ItemsSource = _lengthCheckSettingRows;

            if (_lengthCheckSettingRows.Count > 0)
            {
                int selectIndex = 0;
                if (previouslySelectedJobId >= 0)
                {
                    for (int i = 0; i < _lengthCheckSettingRows.Count; i++)
                    {
                        if (_lengthCheckSettingRows[i].PlcCamera != null &&
                            _lengthCheckSettingRows[i].PlcCamera.PlcJobId == previouslySelectedJobId)
                        {
                            selectIndex = i;
                            break;
                        }
                    }
                }

                cmbLengthCheckPlcCam.SelectedIndex = selectIndex;
                txtNoMultiMarkPlcCam.Visibility = Visibility.Collapsed;
                spLengthCheckDetails.Visibility = Visibility.Visible;
            }
            else
            {
                txtNoMultiMarkPlcCam.Visibility = Visibility.Visible;
                spLengthCheckDetails.Visibility = Visibility.Collapsed;
            }
        }

        private static string BuildLengthCheckDisplayName(PlcCam plcCamera)
        {
            string jobName = plcCamera.JobName;
            if (string.IsNullOrWhiteSpace(jobName))
            {
                jobName = "Unnamed Job";
            }

            string activeState = string.Empty;
            if (!plcCamera.IsActive)
            {
                activeState = " [Inactive]";
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "ID {0} - {1}{2}",
                plcCamera.PlcJobId,
                jobName,
                activeState);
        }

        private void cmbLengthCheckPlcCam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MultiMarkLengthCheckSettingRow selectedSetting =
                cmbLengthCheckPlcCam.SelectedItem as MultiMarkLengthCheckSettingRow;
            spLengthCheckDetails.DataContext = selectedSetting;
        }

        private void SaveMultiMarkLengthCheckSettings()
        {
            var parsedSettings = new List<ParsedMultiMarkLengthCheckSetting>();

            for (int index = 0; index < _lengthCheckSettingRows.Count; index++)
            {
                MultiMarkLengthCheckSettingRow settingRow = _lengthCheckSettingRows[index];

                double expectedLength;
                if (!TryParseFiniteDouble(settingRow.ExpectedLengthText, out expectedLength))
                {
                    throw new InvalidOperationException(
                        settingRow.DisplayName + ": Expected Length must be a valid number.");
                }

                double tolerance;
                if (!TryParseFiniteDouble(settingRow.ToleranceText, out tolerance) || tolerance < 0)
                {
                    throw new InvalidOperationException(
                        settingRow.DisplayName + ": Tolerance must be a number greater than or equal to 0.");
                }

                double offset;
                if (!TryParseFiniteDouble(settingRow.OffsetText, out offset))
                {
                    throw new InvalidOperationException(
                        settingRow.DisplayName + ": Length Offset must be a valid number.");
                }

                if (settingRow.IsEnabled && expectedLength <= 0)
                {
                    throw new InvalidOperationException(
                        settingRow.DisplayName + ": Expected Length must be greater than 0 when Length Check is enabled.");
                }

                parsedSettings.Add(new ParsedMultiMarkLengthCheckSetting
                {
                    SettingRow = settingRow,
                    ExpectedLengthMillimeters = expectedLength,
                    ToleranceMillimeters = tolerance,
                    OffsetMillimeters = offset
                });
            }

            var snapshots = new List<MultiMarkLengthCheckSettingSnapshot>();
            for (int index = 0; index < parsedSettings.Count; index++)
            {
                ParsedMultiMarkLengthCheckSetting parsedSetting = parsedSettings[index];
                PlcCam plcCamera = parsedSetting.SettingRow.PlcCamera;

                snapshots.Add(new MultiMarkLengthCheckSettingSnapshot
                {
                    PlcCamera = plcCamera,
                    IsEnabled = plcCamera.EnableLengthCheck,
                    ExpectedLengthMillimeters = plcCamera.ExpectedLengthMillimeters,
                    ToleranceMillimeters = plcCamera.LengthToleranceMillimeters,
                    OffsetMillimeters = plcCamera.LengthOffsetMillimeters
                });

                plcCamera.EnableLengthCheck = parsedSetting.SettingRow.IsEnabled;
                plcCamera.ExpectedLengthMillimeters = parsedSetting.ExpectedLengthMillimeters;
                plcCamera.LengthToleranceMillimeters = parsedSetting.ToleranceMillimeters;
                plcCamera.LengthOffsetMillimeters = parsedSetting.OffsetMillimeters;
            }

            List<PlcCam> plcCameras = JobController.GetAllPlcCam();
            if (plcCameras == null)
            {
                throw new InvalidOperationException(
                    "Cannot load PlcCam configuration. The existing configuration file was not changed.");
            }

            bool saveSucceeded = Common.SaveObjectToFile(plcCameras, Common.PlcCamerasConfigFile);
            if (saveSucceeded)
            {
                return;
            }

            for (int index = 0; index < snapshots.Count; index++)
            {
                MultiMarkLengthCheckSettingSnapshot snapshot = snapshots[index];
                snapshot.PlcCamera.EnableLengthCheck = snapshot.IsEnabled;
                snapshot.PlcCamera.ExpectedLengthMillimeters = snapshot.ExpectedLengthMillimeters;
                snapshot.PlcCamera.LengthToleranceMillimeters = snapshot.ToleranceMillimeters;
                snapshot.PlcCamera.LengthOffsetMillimeters = snapshot.OffsetMillimeters;
            }

            throw new IOException("Cannot save MultiMark Length Check settings.");
        }

        private static bool TryParseFiniteDouble(string text, out double value)
        {
            bool parsed = double.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.CurrentCulture,
                out value);

            if (!parsed)
            {
                parsed = double.TryParse(
                    text,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out value);
            }

            if (!parsed || double.IsNaN(value) || double.IsInfinity(value))
            {
                value = 0;
                return false;
            }

            return true;
        }

        private void tbMaxRetries_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
                return;
            }
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
                if (int.TryParse(newText, out int value))
                {
                    if (value > 10)
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
        }

        private void tbMaxRetries_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && !string.IsNullOrEmpty(textBox.Text))
            {
                if (int.TryParse(textBox.Text, out int value))
                {
                    if (value > 10)
                    {
                        textBox.Text = "10";
                        textBox.CaretIndex = textBox.Text.Length;
                    }
                    else if (value < 0)
                    {
                        textBox.Text = "0";
                        textBox.CaretIndex = textBox.Text.Length;
                    }
                }
            }
        }

        private void UpdateLightControlButtonsVisibility()
        {
            bool isLightControl = chkIsLightControl?.IsChecked == true;
            // When disabling IsLightControl, show buttons to manually control all lights.
            spAllLightButtons.Visibility = isLightControl ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }

        private void chkIsLightControl_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // Apply immediately so job execution reflects the setting.
            bool enabled = chkIsLightControl.IsChecked == true;
            Common.Settings.IsLightControl = enabled;
            JobController.Settings.IsLightControl = enabled;
            UpdateLightControlButtonsVisibility();
        }

        private void btnOnAllLight_Click(object sender, RoutedEventArgs e)
        {
            JobController.TurnOnAllLights();
        }

        private void btnOffAllLight_Click(object sender, RoutedEventArgs e)
        {
            JobController.TurnOffAllLights();
        }
    }

    internal sealed class MultiMarkLengthCheckSettingRow : INotifyPropertyChanged
    {
        private PlcCam _plcCamera;
        private string _displayName;
        private bool _isEnabled;
        private string _expectedLengthText;
        private string _toleranceText;
        private string _offsetText;

        public PlcCam PlcCamera
        {
            get => _plcCamera;
            set { _plcCamera = value; OnPropertyChanged(); }
        }

        public string DisplayName
        {
            get => _displayName;
            set { _displayName = value; OnPropertyChanged(); }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(); }
        }

        public string ExpectedLengthText
        {
            get => _expectedLengthText;
            set { _expectedLengthText = value; OnPropertyChanged(); }
        }

        public string ToleranceText
        {
            get => _toleranceText;
            set { _toleranceText = value; OnPropertyChanged(); }
        }

        public string OffsetText
        {
            get => _offsetText;
            set { _offsetText = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal sealed class ParsedMultiMarkLengthCheckSetting
    {
        public MultiMarkLengthCheckSettingRow SettingRow { get; set; }
        public double ExpectedLengthMillimeters { get; set; }
        public double ToleranceMillimeters { get; set; }
        public double OffsetMillimeters { get; set; }
    }

    internal sealed class MultiMarkLengthCheckSettingSnapshot
    {
        public PlcCam PlcCamera { get; set; }
        public bool IsEnabled { get; set; }
        public double ExpectedLengthMillimeters { get; set; }
        public double ToleranceMillimeters { get; set; }
        public double OffsetMillimeters { get; set; }
    }
}
