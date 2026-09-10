using BeevisionSolution.Models;
using BeevisionSolution.Utils;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using static BeevisionSolution.Utils.Common;
using static BeevisionSolution.Utils.Constant;

namespace BeevisionSolution.ViewComponents
{
    /// <summary>
    /// Interaction logic for ToolBlockEditorView.xaml
    /// </summary>
    public partial class ToolBlockEditorView : UserControl, IDisposable
    {
        private CogToolBlockEditV2 _cogToolBlockEditor;
        private bool disposed = false;
        public bool Available { get; private set; } = false;
        private CogToolBlock _tb;
        private Object _InputImage = null;
        private Object _outputImage = null;
        private BaseJob _job;
        private bool needSaving = true;
        private bool forceClose = false;
        public delegate void OnHitRunButton(Object obj);
        public OnHitRunButton OnHitRunButtonEvent;

        public Object InputImage
        {
            get => _InputImage;
            internal set
            {
                _InputImage = value;
                if (Available && (null != _InputImage))
                {
                    if (_cogToolBlockEditor.Subject.Inputs.Contains(strInputImageKey))
                        _cogToolBlockEditor.Subject.Inputs[strInputImageKey].Value = _InputImage;
                }
            }
        }

        public Object OutputImage
        {
            get => _outputImage;
        }

        public ToolBlockEditorView()
        {
            InitializeComponent();
        }

        public ToolBlockEditorView(BaseJob job)
        {
            InitializeComponent();

            if ((null != job) && (job.Initialized))
            {
                _job = job;
                _cogToolBlockEditor = new CogToolBlockEditV2();
                mToolBlockEditorHost.Child = _cogToolBlockEditor;
                _cogToolBlockEditor.HandleCreated += _cogToolBlockEditor_HandleCreated;
            }
        }

        public async Task SetJobAsync(BaseJob job, object inputImage)
        {
            _job = job;
            _InputImage = inputImage;

            if ((null != job) && (job.Initialized))
            {
                if (_cogToolBlockEditor == null)
                {
                    _cogToolBlockEditor = new CogToolBlockEditV2();
                    mToolBlockEditorHost.Child = _cogToolBlockEditor;
                    _cogToolBlockEditor.HandleCreated += _cogToolBlockEditor_HandleCreated;
                    // Sự kiện HandleCreated sẽ tự động nạp ToolBlock
                }
                else
                {
                    // Tái sử dụng control hiện tại, chỉ thay đổi data để UI không bị đơ
                    Mouse.OverrideCursor = Cursors.Wait;
                    await Task.Delay(10); // Ép UI cập nhật con trỏ chuột

                    if (_cogToolBlockEditor.Subject != null)
                    {
                        _cogToolBlockEditor.Subject.Ran -= Subject_Ran;
                        _cogToolBlockEditor.Subject = null;
                    }

                    if (_tb != null) _tb.Dispose();
                    
                    // Chạy DeepCopy dưới background để không treo giao diện
                    var copiedTb = await Task.Run(() => (CogToolBlock)CogSerializer.DeepCopyObject((CogToolBlock)_job.ToolBlock));

                    _cogToolBlockEditor.Subject = _tb = copiedTb;

                    if (_cogToolBlockEditor.Subject.Inputs.Contains(strInputImageKey))
                        _cogToolBlockEditor.Subject.Inputs[strInputImageKey].Value = _InputImage;

                    _cogToolBlockEditor.Subject.Ran += Subject_Ran;

                    Mouse.OverrideCursor = null;
                }
            }
        }

        private async void _cogToolBlockEditor_HandleCreated(object sender, EventArgs e)
        {
            try
            {
                Available = true;
                
                Mouse.OverrideCursor = Cursors.Wait;
                await Task.Delay(10);

                var copiedTb = await Task.Run(() => (CogToolBlock)CogSerializer.DeepCopyObject((CogToolBlock)_job.ToolBlock));
                
                _cogToolBlockEditor.Subject = _tb = copiedTb;
                
                if (_cogToolBlockEditor.Subject.Inputs.Contains(strInputImageKey))
                    _cogToolBlockEditor.Subject.Inputs[strInputImageKey].Value = _InputImage;

                _cogToolBlockEditor.BackColor = System.Drawing.SystemColors.Control;
                _cogToolBlockEditor.ForeColor = System.Drawing.Color.Black;

                _cogToolBlockEditor.Subject.Ran += Subject_Ran;
                
                Mouse.OverrideCursor = null;
            }
            catch(Exception ex)
            {
                Mouse.OverrideCursor = null;
                Common.Bug(ex.Message);
            }
        }

        private void Subject_Ran(object sender, EventArgs e)
        {
            if (_cogToolBlockEditor.Subject.Outputs.Contains(strOutputImageKey))
            {
                _outputImage = _cogToolBlockEditor.Subject.Outputs[strOutputImageKey].Value;
                OnHitRunButtonEvent?.Invoke(_outputImage);
            }

        }

        private async void btnReload_Click(object sender, RoutedEventArgs e)
        {
            if (null != _job)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                await Task.Delay(10);
                
                if (_cogToolBlockEditor.Subject != null)
                {
                    _cogToolBlockEditor.Subject.Ran -= Subject_Ran;
                    _cogToolBlockEditor.Subject = null;
                }

                if (_tb != null) _tb.Dispose();
                
                var copiedTb = await Task.Run(() => (CogToolBlock)CogSerializer.DeepCopyObject((CogToolBlock)_job.ToolBlock));
                
                _cogToolBlockEditor.Subject = _tb = copiedTb;
                if (_cogToolBlockEditor.Subject.Inputs.Contains(strInputImageKey))
                    _cogToolBlockEditor.Subject.Inputs[strInputImageKey].Value = _InputImage;
                
                _cogToolBlockEditor.Subject.Ran += Subject_Ran;
                    
                Mouse.OverrideCursor = null;
            }
            needSaving = true;
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (Apply())
            {
                if (_job.Save())
                    SuccessSaving();
                else
                    FailedSaving();
            }
            else
            {
                FailedApplying();
            }
            needSaving = false;
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (!Apply()) FailedApplying();
            needSaving = false;
        }

        private bool Apply()
        {
            bool ret = false;
            if (null != _job)
            {
                try
                {
                    var obj = CogSerializer.DeepCopyObject(_cogToolBlockEditor.Subject);
                    var old = _job.ToolBlock;
                    if (null != old)
                    {
                        var tb = old as CogToolBlock;
                        tb.Dispose();
                    }
                    old = null;
                    _job.ToolBlock = obj;
                    ret = true;
                }
                catch { }
            }

            return ret;
        }

        private void FailedSaving()
        {
            MessageBox.Show("Failed on saving ToolBlock, Please Reload and try again", "Saving ToolBlock");
        }

        private void FailedApplying()
        {
            MessageBox.Show("Failed on applying change to ToolBlock, Please Reload and try again", "Applying Change");
        }

        private void SuccessSaving()
        {
            MessageBox.Show(String.Format("Done Saving {0}.", _job.Name), "Saving ToolBlock");
        }

        private void btnLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            _cogToolBlockEditor.Subject = _tb = (CogToolBlock)LoadToolBlock(_job.JobFile);
            if (_cogToolBlockEditor.Subject.Inputs.Contains(strInputImageKey))
                _cogToolBlockEditor.Subject.Inputs[strInputImageKey].Value = _InputImage;
            needSaving = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (null != _cogToolBlockEditor)
                    {
                        _tb?.Dispose();
                        _cogToolBlockEditor.Dispose();
                        while (_cogToolBlockEditor.Disposing) Thread.Sleep(1);
                    }
                    _tb = null;
                    _job = null;
                    _InputImage = null;
                    _cogToolBlockEditor = null;
                }
                disposed = true;
            }
        }

        ~ToolBlockEditorView()
        {
            Dispose(disposing: false);
            MemoryCleanup();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
            MemoryCleanup();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            //OnHitRunButtonEvent?.Invoke();
        }
    }
}
