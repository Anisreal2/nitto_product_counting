using BeevisionSolution.Models;
using BeevisionSolution.Utils;
using BeevisionSolution.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.Adapters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static BeevisionSolution.Utils.Common;

namespace BeevisionSolution.ViewComponents
{
    /// <summary>
    /// Interaction logic for LotInformation.xaml
    /// </summary>
    public partial class LotInformation : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string strPropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyName));
        }

        public bool IsChanged { get; set; } = false;
        public event EventHandler DataChanged;
        public event EventHandler OkClicked;
        public event EventHandler CancelClicked;
        public event EventHandler LoadClicked;
        public event EventHandler RefreshClicked;
        public event EventHandler AfterLoadEvent;

        public string previousSelection = string.Empty;
        private string _SelectedProfile;
        private bool firstInitFlag = true;
        private bool firstLoadedFlag = true;
        private string originalUserId;

        public string SelectedProfile
        {
            get { return _SelectedProfile; }
            set 
            { 
                _SelectedProfile = value; 
                Notify(); 
            }
        }

        public string ModelName
        {
            get
            {
                return cbbModels.SelectedItem?.ToString() ?? string.Empty;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                // Find the actual item that matches the string
                var matchedItem = cbbModels.Items
                    .Cast<object>()
                    .FirstOrDefault(i => i?.ToString() == value);

                if (matchedItem == null)
                {
                    return;
                }

                // Prevent triggering SelectionChanged
                cbbModels.SelectionChanged -= ComboBox_SelectionChanged;

                cbbModels.SelectedItem = matchedItem;

                cbbModels.SelectionChanged += ComboBox_SelectionChanged;

                Notify();
            }
        }

        private bool showFlag = false;
        public bool ShowFlag
        {
            get
            {
                return showFlag;
            }
            set
            {
                if(showFlag != value)
                {
                    showFlag = value;
                    Notify();

                    AfterLoadEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public LotInformation()
        {
            InitializeComponent();

            var mainWindow = (Application.Current.MainWindow as MainWindow2);
            firstInitFlag = mainWindow.firstInitFlag;
            firstLoadedFlag = mainWindow.firstLoadedFlag;

            Init();
            cbbModels.SelectionChanged += ComboBox_SelectionChanged;

            mainWindow.OnAllJobLoadedDone += OnJobLoadedDone;
        }

        private void OnJobLoadedDone(object sender)
        {
            var mainWindow = (Application.Current.MainWindow as MainWindow2);
            firstLoadedFlag = mainWindow.firstLoadedFlag;

            if (firstLoadedFlag)
            {
                SelectedProfile = Common.Settings.CurrentProfile;
                cbbModels.SelectedItem = _SelectedProfile;

                mainWindow.firstLoadedFlag = false;
            }
        }

        private void Init()
        {
            DataContext = this;
            cbbModels.Items.Clear();
            cbbModels.ItemsSource = GetAllProfiles();
            var mainWindow = (Application.Current.MainWindow as MainWindow2);
            if (firstInitFlag)
            {
                SelectedProfile = Common.Settings.CurrentProfile;
                mainWindow.firstInitFlag = false;
            }
            else
            {
                previousSelection = mainWindow.previousSelection;
                SelectedProfile = previousSelection;
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            string value = ((ComboBox)sender).SelectedItem.ToString() ?? string.Empty;
            if (firstLoadedFlag)
            {
                previousSelection = value;
            }
            if (value != null && !value.Equals(previousSelection))
            {
                IsChanged = true;
                previousSelection = value;

                // Notify listeners that data has changed
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool ValidateInput()
        {
            return true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            var mainWindow = (Application.Current.MainWindow as MainWindow2);
            mainWindow.previousSelection = previousSelection;

            IsChanged = false;

            OkClicked?.Invoke(this, EventArgs.Empty);
            btnLoad_Click(sender, e);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {


            if (IsChanged)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Bạn có những thay đổi chưa lưu. Bạn có muốn hủy thay đổi không?",
                    "Xác nhận thoát",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            var mainWindow = (Application.Current.MainWindow as MainWindow2);
            mainWindow.previousSelection = previousSelection;

            CancelClicked?.Invoke(this, EventArgs.Empty);
        }

        public void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            if (!SelectedProfile.Equals(Common.Settings.CurrentProfile) && !string.IsNullOrEmpty(SelectedProfile))
            {
                var mainWindow = (Application.Current.MainWindow as MainWindow2);

                // Reload and setup for logging
                StopAllLogger();
                Info("Changing profile to: {0}", SelectedProfile);
                Common.Settings.CurrentProfile = SelectedProfile;
                LoadClicked?.Invoke(this, EventArgs.Empty);
                Common.Settings.Save();
                mainWindow.Reload();
                StartAllLogger();
            }
        }

        private async void btnCopyProfile_Click(object sender, RoutedEventArgs e)
        {
            await DoCopyProfileAsync();
        }
        private async Task DoCopyProfileAsync()
        {
            try
            {
                var strProfile = txtProfileName.Text.Trim();

                if (string.IsNullOrEmpty(strProfile))
                    return;

                if (strProfile.Equals(Common.Settings.CurrentProfile))
                {
                    MessageBox.Show((string)TryFindResource("msgErrSameProfile"));
                    return;
                }

                if (Common.GetAllProfiles().Contains(strProfile))
                {
                    MessageBox.Show((string)TryFindResource("msgErrExistProfile"));
                    return;
                }

                int countBefore = Common.GetAllProfiles().Count;
                await Task.Run(() =>
                {
                    Common.CopyDir(Common.Settings.CurrentProfile, strProfile);
                });

                int timeout = 0;
                while (Common.GetAllProfiles().Count != countBefore + 1 && timeout++ < 2000)
                {
                    await Task.Delay(1);
                }
                Dispatcher.Invoke(() =>
                {
                    cbbModels.ItemsSource = Common.GetAllProfiles();
                });
                MessageBox.Show((string)TryFindResource("msgSuccessCopy"));
                txtProfileName.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show((string)TryFindResource("msgFailedCopy") + ex.Message);
            }
        }

        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(Common.Settings.CurrentProfile))
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    Common.CreateZipBackup();
                }));
            }
        }
    }   
}
