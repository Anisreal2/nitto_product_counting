using BeevisionSolution.Models;
using BeevisionSolution.Utils;
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Microsoft.Extensions.FileSystemGlobbing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static BeevisionSolution.Utils.Constant;

namespace BeevisionSolution.Jobs
{
    public class WatcherJob : FunctionJob
    {
        [JsonIgnore]
        private FileSystemWatcher watcher;
        [JsonIgnore]
        private CogImageFileTool imageFileTool;
        [JsonIgnore]
        private bool _disposed;
        [JsonIgnore]
        public bool IsReadyToGetNextImage = true;
        [JsonIgnore]
        public bool IsWatcherImageReady = false;
        public event Action<Object, ICogImage> OnTrigger = null;
        public string WatcherFolder { get; set; }
        public string ImageType { get; set; }
        public string WatcherName {  get; set; }

        public WatcherJob(String strName, String strJobFile)
        {
            this.VisionType = JobType.TypeInspection;
            this.Name = strName;
            this.JobFile = strJobFile;
        }

        public WatcherJob() : this(strDefaultAlignName, strDefaultJobFile)
        {
        }

        public void StartMonitorFolder()
        {
            if (!string.IsNullOrEmpty(WatcherFolder) && Directory.Exists(WatcherFolder))
            {
                watcher = new FileSystemWatcher();
                watcher.Path = WatcherFolder;
                watcher.NotifyFilter = NotifyFilters.CreationTime
                                    | NotifyFilters.FileName
                                    | NotifyFilters.LastAccess
                                    | NotifyFilters.LastWrite;
                watcher.Created += OnImageCreated;
                watcher.Filter = string.Format("*.{0}", ImageType);
                watcher.IncludeSubdirectories = true;
                watcher.EnableRaisingEvents = true;
                imageFileTool = new CogImageFileTool();
                Common.Info("Start monirtoring folder: {0}", WatcherFolder);
            }
        }

        private void OnImageCreated(object sender, FileSystemEventArgs e)
        {
            string value = e.FullPath;
            //Info("OnImageCreated found: {0}", value);
            //if (tmpImgPath == value)
            //{
            //    Info("{0}, image was already in process", Name);
            //    return;
            //}
            //tmpImgPath = value;
            FileStream stream = WaitForFile(e.FullPath, FileMode.Open, FileAccess.Read);

            if (stream != null)
            {
                stream.Close();
                //convert bmp to cogimage and set to input image
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                try
                {

                    if (!string.IsNullOrEmpty(e.FullPath))
                    {
                        imageFileTool.Operator.Open(e.FullPath, CogImageFileModeConstants.Read);
                        imageFileTool.Run();
                        if (imageFileTool.RunStatus.Result == CogToolResultConstants.Accept)
                        {
                            this.InputImage = imageFileTool.OutputImage;
                            IsReadyToGetNextImage = false;
                            IsWatcherImageReady = true;
                            Common.Info("New Image , send 1 to PLC");
                        }
                        else
                        {
                            Common.Info("Image File Tool run failed: " + imageFileTool.RunStatus.Message);
                        }
                    }
                    else
                    {
                        Common.Bug("Watcher Image Create: Path not found");
                    }

                    //if (fileNameWithoutExtension.Contains("NG"))
                    //{
                    //    //Dispatcher.Invoke(new Action(() =>
                    //    //{
                    //    //    tblResult.Text = "NG";
                    //    //    tblResult.Foreground = Brushes.Red;
                    //    //    //FitImageToView();
                    //    //}));
                    //}
                    //else if (fileNameWithoutExtension.Contains("OK"))
                    //{
                    //    //Dispatcher.Invoke(new Action(() =>
                    //    //{
                    //    //    tblResult.Text = "OK";
                    //    //    tblResult.Foreground = Brushes.Blue;
                    //    //    //FitImageToView();
                    //    //}));
                    //}
                }
                catch (Exception ex)
                {
                    Common.Bug("Watcher exception: {0}", ex.Message);
                }
            }
        }

        private FileStream WaitForFile(string fullPath, FileMode mode, FileAccess access)
        {
            for (int numTries = 0; numTries < 200; numTries++)
            {
                if (!IsReadyToGetNextImage)
                {
                    Thread.Sleep(50);
                }

                FileStream fs = null;
                try
                {
                    fs = new FileStream(fullPath, mode, access);
                    Common.Info("{0} Image ready", Name);

                    return fs;
                }
                catch (IOException ex)
                {
                    Common.Bug("WaitForImage exception: {0}", ex.Message);
                    if (fs != null)
                    {
                        fs.Dispose();
                    }
                    Thread.Sleep(50);
                }
            }
            Common.Info("{0} Image null, time out", Name);
            return null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                // managed resources
                if (watcher != null)
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Created -= OnImageCreated;
                    watcher.Dispose();
                    watcher = null;
                }

                if (imageFileTool != null)
                {
                    imageFileTool.Dispose();
                    imageFileTool = null;
                }

                OnTrigger = null;
            }


        }


        ~WatcherJob()
        {
            Dispose(true);
        }
    }
}
