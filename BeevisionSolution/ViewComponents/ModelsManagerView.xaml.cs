using BeevisionSolution.Models;
using BeevisionSolution.Utils;
using BeevisionSolution.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static BeevisionSolution.Utils.Common;

namespace BeevisionSolution.ViewComponents
{
    /// <summary>
    /// Interaction logic for ModelsManagerView.xaml
    /// </summary>
    public partial class ModelsManagerView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string strPropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyName));
        }

        private ObservableCollection<ModelItem> _models;
        public ObservableCollection<ModelItem> Models
        {
            get { return _models; }
            set
            {
                _models = value;
                Notify();
            }
        }

        private ModelItem _selectedModel;
        public ModelItem SelectedModel
        {
            get { return _selectedModel; }
            set
            {
                _selectedModel = value;
                Notify();
            }
        }
        private bool _hasCopiedModel;
        public bool HasCopiedModel
        {
            get { return _hasCopiedModel; }
            set
            {
                _hasCopiedModel = value;
                Notify();
            }
        }

        private string _copiedModelName;
        private int? _copiedRowIndex;

        public ModelsManagerView()
        {
            InitializeComponent();
            DataContext = this;
            Models = new ObservableCollection<ModelItem>();
            Init();
            
            var mainWindow = (Application.Current.MainWindow as MainWindow2);
            if (mainWindow != null)
            {
                mainWindow.OnAllJobLoadedDone += OnJobLoadedDone;
            }
        }

        private void OnJobLoadedDone(object sender)
        {
            RefreshModels();
        }

        private void Init()
        {
            RefreshModels();
            LoadSourceModels();
        }

        private void RefreshModels()
        {
            Dispatcher.Invoke(() =>
            {
                Models.Clear();
                var profiles = GetAllProfiles();

                var profileMap = new Dictionary<int, string>();
                var usedProfiles = new HashSet<string>();

                foreach (var profile in profiles)
                {
                    if (profile.Contains("."))
                    {
                        var parts = profile.Split(new[] { '.' }, 2);
                        if (parts.Length > 1 && int.TryParse(parts[0].Trim(), out int modelNum))
                        {
                            // Nếu số thứ tự hợp lệ (1-100) và chưa có profile nào ở vị trí đó
                            if (modelNum >= 1 && modelNum <= 100 && !profileMap.ContainsKey(modelNum))
                            {
                                profileMap[modelNum] = profile;
                                usedProfiles.Add(profile);
                            }
                        }
                    }
                }

                int nextPosition = 1;
                foreach (var profile in profiles)
                {
                    if (!usedProfiles.Contains(profile))
                    {
                        // Tìm vị trí trống đầu tiên
                        while (nextPosition <= 100 && profileMap.ContainsKey(nextPosition))
                        {
                            nextPosition++;
                        }
                        if (nextPosition <= 100)
                        {
                            profileMap[nextPosition] = profile;
                            usedProfiles.Add(profile);
                            nextPosition++;
                        }
                    }
                }

                for (int i = 0; i < 100; i++)
                {
                    int modelNumber = i + 1;
                    string modelName = string.Empty;

                    if (profileMap.ContainsKey(modelNumber))
                    {
                        modelName = profileMap[modelNumber];
                    }

                    Models.Add(new ModelItem
                    {
                        ModelNumber = modelNumber,
                        ModelName = modelName
                    });
                }
            });
        }

        private void LoadSourceModels()
        {
            Dispatcher.Invoke(() =>
            {
                cbbSourceModel.Items.Clear();
                var profiles = GetAllProfiles();
                foreach (var profile in profiles)
                {
                    cbbSourceModel.Items.Add(profile);
                }
                if (cbbSourceModel.Items.Count > 0)
                {
                    cbbSourceModel.SelectedIndex = 0;
                }
            });
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshModels();
            LoadSourceModels();
        }

        private async void btnCopyModel_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedModel == null)
            {
                MessageBox.Show("Please select a model to copy from the table.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _copiedModelName = SelectedModel.ModelName;
            _copiedRowIndex = SelectedModel.ModelNumber - 1;
            
            MessageBox.Show($"Model '{_copiedModelName}' copied. Select a target position and press Ctrl+V or double-click to paste.", 
                "Model Copied", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void btnExecuteCopy_Click(object sender, RoutedEventArgs e)
        {
            var sourceModel = cbbSourceModel.SelectedItem?.ToString();
            var targetModelName = txtTargetModelName.Text.Trim();

            if (string.IsNullOrEmpty(sourceModel))
            {
                MessageBox.Show("Please select a source model.", "No Source Model",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(targetModelName))
            {
                MessageBox.Show("Please enter a target model name.", "No Target Name",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (targetModelName.Equals(sourceModel))
            {
                var msg = TryFindResource("msgErrSameProfile");
                MessageBox.Show(msg != null ? msg.ToString() : "Source and target cannot be the same.",
                    "Invalid Operation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string baseModelName = sourceModel;
            if (sourceModel.Contains("."))
            {
                var parts = sourceModel.Split(new[] { '.' }, 2);
                if (parts.Length > 1 && int.TryParse(parts[0].Trim(), out _))
                {
                    baseModelName = parts[1].Trim();
                }
            }

            int modelNumber = GetAllProfiles().Count + 1;
            if (targetModelName.Contains("."))
            {
                var parts = targetModelName.Split(new[] { '.' }, 2);
                if (parts.Length > 1 && int.TryParse(parts[0].Trim(), out int parsedNumber))
                {
                    modelNumber = parsedNumber;
                    baseModelName = parts[1].Trim();
                }
            }

            string finalTargetName = $"{modelNumber}. {baseModelName}";

            if (GetAllProfiles().Contains(finalTargetName))
            {
                var msg = TryFindResource("msgErrExistProfile");
                MessageBox.Show(msg != null ? msg.ToString() : "Model already exists.",
                    "Model Exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int countBefore = GetAllProfiles().Count;
                await Task.Run(() =>
                {
                    CopyDir(sourceModel, finalTargetName);
                });

                int timeout = 0;
                while (GetAllProfiles().Count != countBefore + 1 && timeout++ < 2000)
                {
                    await Task.Delay(1);
                }

                Dispatcher.Invoke(() =>
                {
                    RefreshModels();
                    LoadSourceModels();
                    txtTargetModelName.Text = string.Empty;
                });

                var successMsg = TryFindResource("msgSuccessCopy");
                MessageBox.Show(successMsg != null ? successMsg.ToString() : "Model copied successfully.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                var failedMsg = TryFindResource("msgFailedCopy");
                var errorMsg = failedMsg != null ? failedMsg.ToString() + ex.Message : "Failed to copy model: " + ex.Message;
                MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(Settings.CurrentProfile))
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    CreateZipBackup();
                    MessageBox.Show("Backup process started. Check the Backups folder when completed.", 
                        "Backup Started", MessageBoxButton.OK, MessageBoxImage.Information);
                }));
            }
            else
            {
                MessageBox.Show("No current profile selected for backup.", 
                    "No Profile", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void dgModels_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_copiedRowIndex.HasValue && SelectedModel != null)
            {
                PasteModel(_copiedRowIndex.Value, SelectedModel.ModelNumber - 1);
            }
        }

        private void dgModels_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.C && e.Key != Key.V)
            {
                return;
            }
            bool isCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (!isCtrlPressed)
            {
                return;
            }
            var selectedItem = dgModels.SelectedItem as ModelItem;

            if (selectedItem == null)
            {
                return;
            }
            if (e.Key == Key.C)
            {
                if (!string.IsNullOrEmpty(selectedItem.ModelName))
                {
                    _copiedModelName = selectedItem.ModelName;
                    _copiedRowIndex = selectedItem.ModelNumber - 1;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.V)
            {
                if (_copiedRowIndex.HasValue)
                {
                    PasteModel(_copiedRowIndex.Value, selectedItem.ModelNumber - 1);
                    e.Handled = true;
                }
            }
        }
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            var selectedItem = dgModels.SelectedItem as ModelItem;

            menuItemCopy.IsEnabled = selectedItem != null && !string.IsNullOrEmpty(selectedItem.ModelName);
            menuItemPaste.IsEnabled = _copiedRowIndex.HasValue && selectedItem != null;
        }

        private void MenuItem_Copy_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = dgModels.SelectedItem as ModelItem;

            if (selectedItem == null)
            {
                MessageBox.Show("Please select a model to copy.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrEmpty(selectedItem.ModelName))
            {
                MessageBox.Show("Selected model has no name to copy.", "Empty Model",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _copiedModelName = selectedItem.ModelName;
            _copiedRowIndex = selectedItem.ModelNumber - 1;

            MessageBox.Show($"Model '{_copiedModelName}' copied. Select a target position and right-click to paste.",
                "Model Copied", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItem_Paste_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = dgModels.SelectedItem as ModelItem;

            if (selectedItem == null)
            {
                MessageBox.Show("Please select a target position to paste.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!_copiedRowIndex.HasValue)
            {
                MessageBox.Show("No model copied. Please copy a model first.", "Nothing to Paste",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PasteModel(_copiedRowIndex.Value, selectedItem.ModelNumber - 1);
        }

        private void RowCopyButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var modelItem = button.CommandParameter as ModelItem;
            if (modelItem == null) return;

            if (string.IsNullOrEmpty(modelItem.ModelName))
            {
                MessageBox.Show("This model has no name to copy.", "Empty Model",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _copiedModelName = modelItem.ModelName;
            _copiedRowIndex = modelItem.ModelNumber - 1;

            MessageBox.Show($"Model '{_copiedModelName}' copied. Click Paste button on target row to paste.",
                "Model Copied", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RowPasteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var targetModelItem = button.CommandParameter as ModelItem;
            if (targetModelItem == null) return;

            if (!_copiedRowIndex.HasValue)
            {
                MessageBox.Show("No model copied. Please copy a model first.", "Nothing to Paste",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PasteModel(_copiedRowIndex.Value, targetModelItem.ModelNumber - 1);
        }

        private async void PasteModel(int sourceIndex, int targetIndex)
        {
            if (sourceIndex == targetIndex)
            {
                MessageBox.Show("Cannot copy model to itself.", "Invalid Operation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (sourceIndex < 0 || sourceIndex >= Models.Count)
            {
                MessageBox.Show("Invalid source model.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sourceModelItem = Models[sourceIndex];
            if (string.IsNullOrEmpty(sourceModelItem.ModelName))
            {
                MessageBox.Show("Source model has no name.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sourceModel = sourceModelItem.ModelName; 

            string baseModelName = sourceModel;
            if (sourceModel.Contains("."))
            {
                var parts = sourceModel.Split(new[] { '.' }, 2);
                if (parts.Length > 1 && int.TryParse(parts[0].Trim(), out _))
                {
                    baseModelName = parts[1].Trim();
                }
            }

            var targetModelName = $"{targetIndex + 1}. {baseModelName}";

            
            var existingProfiles = GetAllProfiles();
            var existingModelAtPosition = existingProfiles.FirstOrDefault(p =>
            {
                if (p.Contains("."))
                {
                    var parts = p.Split(new[] { '.' }, 2);
                    if (parts.Length > 1 && int.TryParse(parts[0].Trim(), out int num))
                    {
                        return num == targetIndex + 1;
                    }
                }
                return false;
            });

            
            if (!string.IsNullOrEmpty(existingModelAtPosition) && existingModelAtPosition != sourceModel)
            {
                try
                {
                    
                    var oldModelPath = Path.Combine(BaseProfilesFolder, existingModelAtPosition);
                    var newOldModelName = $"{targetIndex + 1}. {baseModelName}_old_{DateTime.Now:yyyyMMddHHmmss}";
                    var newOldModelPath = Path.Combine(BaseProfilesFolder, newOldModelName);
                    
                    if (Directory.Exists(oldModelPath))
                    {
                        Directory.Move(oldModelPath, newOldModelPath);
                    }
                }
                catch (Exception ex)
                {
                    targetModelName = $"{targetIndex + 1}. {baseModelName}_{DateTime.Now:yyyyMMddHHmmss}";
                }
            }
            else if (existingProfiles.Contains(targetModelName))
            {
                targetModelName = $"{targetIndex + 1}. {baseModelName}_{DateTime.Now:yyyyMMddHHmmss}";
            }

            try
            {
                int countBefore = GetAllProfiles().Count;
                await Task.Run(() =>
                {
                    CopyDir(sourceModel, targetModelName);
                });

                int timeout = 0;
                while (GetAllProfiles().Count != countBefore + 1 && timeout++ < 2000)
                {
                    await Task.Delay(1);
                }

                Dispatcher.Invoke(() =>
                {
                    RefreshModels();
                    LoadSourceModels();
                });

                MessageBox.Show($"Model '{sourceModel}' copied to '{targetModelName}'.",
                    "Model Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy model: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class ModelItem : INotifyPropertyChanged
    {
        private int _modelNumber;
        private string _modelName;

        public int ModelNumber
        {
            get { return _modelNumber; }
            set
            {
                _modelNumber = value;
                OnPropertyChanged();
            }
        }

        public string ModelName
        {
            get { return _modelName; }
            set
            {
                _modelName = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
