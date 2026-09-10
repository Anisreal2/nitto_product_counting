using BeevisionSolution.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static BeevisionSolution.Utils.Common;

namespace BeevisionSolution.Views
{
    /// <summary>
    /// Interaction logic for PopUpWindow.xaml
    /// </summary>
    public partial class PopUpWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string strPropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyName));
        }

        public TopPanel Panel { get; set; }
        public string ModelName { get; private set; }
        public new bool DialogResult { get; private set; }
        private readonly MainWindow2 _mainWindow;
        private bool _isPlcModelChange;

        
        public PopUpWindow(TopPanel panel, string currentProductCode, string userId = "", string modelName = "", string trialNo = "", string lotNumber = "")
        {
            InitializeComponent();
            _mainWindow = Application.Current.MainWindow as MainWindow2;

            Panel = panel;
            editControl.ModelName = modelName;
            editControl.OkClicked += EditControl_OkClicked;
            editControl.CancelClicked += EditControl_CancelClicked;
            editControl.DataChanged += EditControl_DataChanged;
            editControl.LoadClicked += EditControl_LoadClicked;

            if (_mainWindow != null)
            {
                _mainWindow.OnAllJobLoadedDone += OnJobLoadedDone;
                _mainWindow.OnChangeModelPlcTrigger += OnPlcTriggerChangeDone;
            }

            this.MouseLeftButtonDown += (s, e) => this.DragMove();
            Closed += PopUpWindow_Closed;
        }

        private void PopUpWindow_Closed(object sender, EventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.OnAllJobLoadedDone -= OnJobLoadedDone;
                _mainWindow.OnChangeModelPlcTrigger -= OnPlcTriggerChangeDone;
            }
        }

        private async Task OnPlcTriggerChangeDone(string modelName)
        {
            try
            {
                if (string.IsNullOrEmpty(modelName))
                {
                    Bug("OnPlcTriggerChangeDone: modelName is null or empty");
                    return;
                }

                var allProfiles = Common.GetAllProfiles();
                if (!allProfiles.Contains(modelName))
                {
                    Bug("OnPlcTriggerChangeDone: Model '{0}' not found", modelName);
                    return;
                }

                if (Application.Current?.Dispatcher == null)
                {
                    Bug("OnPlcTriggerChangeDone: Application.Current or Dispatcher is null");
                    return;
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        if (_mainWindow == null)
                        {
                            Bug("OnPlcTriggerChangeDone: MainWindow is null");
                            return;
                        }

                        if (editControl == null)
                        {
                            Bug("OnPlcTriggerChangeDone: editControl is null in Dispatcher");
                            return;
                        }

                        editControl.SelectedProfile = modelName;
                        editControl.ModelName = modelName;
                        ModelName = modelName;
                        editControl.IsChanged = false;

                        if (modelName.Equals(Common.Settings.CurrentProfile))
                        {
                            Panel?.UpdateModelName(Common.Settings.CurrentProfile);
                            DialogResult = true;
                            Close();
                            return;
                        }

                        _isPlcModelChange = true;
                        editControl.btnLoad_Click(editControl, new RoutedEventArgs());
                    }
                    catch (Exception ex)
                    {
                        Bug("OnPlcTriggerChangeDone Exception in Dispatcher: {0}\nStackTrace: {1}", ex.Message, ex.StackTrace);
                    }
                });
            }
            catch (Exception ex)
            {
                Bug("OnPlcTriggerChangeDone Exception: {0}\nStackTrace: {1}", ex.Message, ex.StackTrace);
            }
        }

        private void OnJobLoadedDone(object sender)
        {
            ModelName = Common.Settings.CurrentProfile;
            Panel?.UpdateModelName(ModelName);

            if (_mainWindow != null)
            {
                _mainWindow.previousSelection = ModelName;
            }

            if (_isPlcModelChange)
            {
                DialogResult = true;
                Close();
                return;
            }

            WindowState = WindowState.Normal;
        }

        private void EditControl_LoadClicked(object sender, EventArgs e)
        {
            ModelName = editControl.ModelName;
            this.WindowState = WindowState.Minimized;
        }

        private void EditControl_DataChanged(object sender, EventArgs e)
        {
            
        }

        private void EditControl_CancelClicked(object sender, EventArgs e)
        {
            ModelName = editControl.ModelName;

            DialogResult = false;
            Close();
        }

        private void EditControl_OkClicked(object sender, EventArgs e)
        {
            ModelName = editControl.ModelName;
            //ProductCode = editControl.ProductCode;
            //UserId = editControl.UserId;
            //TrialNo = editControl.TrialNo;
            //LotNumber = editControl.LotNumber;

            DialogResult = true;
            Close();
        }
        public static bool ShowDialog(Window owner, TopPanel topPanel, ref string modelName)
        {
            var dialog = new PopUpWindow(topPanel, modelName);
            dialog.Owner = owner;
            dialog.ShowDialog();

            if (dialog.DialogResult)
            {

                modelName = dialog.ModelName;
                return true;
            }

            return false;
        }     

    }
}
