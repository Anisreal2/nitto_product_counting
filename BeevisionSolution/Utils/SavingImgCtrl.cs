using BeevisionSolution.Models;
using Cognex.VisionPro;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace BeevisionSolution.Utils
{
    internal class SavingImgCtrl
    {
        private static readonly ConcurrentQueue<SaveImageModel> _qSaveImage = new ConcurrentQueue<SaveImageModel>();
        private static volatile bool _isProcessing = true;
        private static readonly object _initLock = new object();
        private static SemaphoreSlim _emptySlots;
        private static SemaphoreSlim _itemsReady;
        private static bool _workersStarted;
        private static int _capacity;
        private static DateTime _lastQueueSaturationLogUtc = DateTime.MinValue;

        public static void Init()
        {
            EnsureWorkersStarted();
        }

        private static int GetClampedCapacity()
        {
            int cap = 80;
            try
            {
                if (Common.Settings != null && Common.Settings.SaveImageQueueMaxCapacity > 0)
                    cap = Common.Settings.SaveImageQueueMaxCapacity;
            }
            catch
            {
                /* use default */
            }

            if (cap < 10) cap = 10;
            if (cap > 500) cap = 500;
            return cap;
        }

        private static void EnsureWorkersStarted()
        {
            if (_workersStarted)
                return;

            lock (_initLock)
            {
                if (_workersStarted)
                    return;

                _capacity = GetClampedCapacity();
                _emptySlots = new SemaphoreSlim(_capacity, _capacity);
                _itemsReady = new SemaphoreSlim(0, _capacity);

                int numOfThread = 1;
                if (Common.Settings != null && Common.Settings.NoOfSavingThread > 1)
                    numOfThread = Common.Settings.NoOfSavingThread;

                for (int i = 0; i < numOfThread; i++)
                {
                    var thSave = new Thread(SaveImageWorker)
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.AboveNormal
                    };
                    thSave.Start();
                }

                _workersStarted = true;
                Common.Info("SavingImgCtrl started: {0} saver thread(s), queue capacity {1}.", numOfThread, _capacity);
            }
        }

        private static void SaveImageWorker()
        {
            while (_isProcessing)
            {
                try
                {
                    if (!_itemsReady.Wait(250))
                        continue;

                    if (!_qSaveImage.TryDequeue(out var model) || model == null)
                        continue;

                    try
                    {
                        if (model.IsRaw)
                        {
                            Common.SavePlainImage(model.FilePath, model.Grade, model.FileName, model.RawImage);
                        }
                        else if (model.IsBarcode)
                        {
                            Common.SaveOverlayImageBarcode(model.FilePath, model.Grade, model.FileName, model.GraphicImg);
                        }
                        else
                        {
                            Common.SaveOverlayImage(model.FilePath, model.Grade, model.FileName, model.GraphicImg);
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.Bug("SaveImageWorker save failed: {0}", ex.Message);
                        Common.Bug(ex.StackTrace);
                    }
                    finally
                    {
                        DisposeSaveModel(model);
                        try
                        {
                            _emptySlots?.Release();
                        }
                        catch
                        {
                            /* ignore */
                        }
                    }
                }
                catch (Exception ex)
                {
                    Common.Bug("SaveImageWorker Exception: {0}", ex.Message);
                    Common.Bug(ex.StackTrace);
                }
            }
        }

        /// <summary>Release GDI overlays we own; do not IDisposable.Dispose <see cref="SaveImageModel.RawImage"/> here — Cognex/UI may still share that reference.</summary>
        private static void DisposeSaveModel(SaveImageModel model)
        {
            if (model == null)
                return;

            try
            {
                model.GraphicImg?.Dispose();
                model.GraphicImg = null;
            }
            catch
            {
                /* best-effort */
            }

            model.RawImage = null;
        }

        public static void AddImage(SaveImageModel img)
        {
            if (img == null)
                return;

            EnsureWorkersStarted();

            try
            {
                if (_emptySlots != null && _emptySlots.CurrentCount == 0)
                {
                    var utc = DateTime.UtcNow;
                    if ((utc - _lastQueueSaturationLogUtc).TotalSeconds >= 3)
                    {
                        Common.Info(
                            "Save-image queue saturated (capacity {0}); blocking producers until saver catches up.",
                            _capacity);
                        _lastQueueSaturationLogUtc = utc;
                    }
                }
            }
            catch
            {
                /* non-fatal */
            }

            _emptySlots.Wait();
            _qSaveImage.Enqueue(img);
            _itemsReady.Release();
        }

        public static void AddRawImage(string filePath, string fileName, bool isOK, ICogImage rawImage)
        {
            var img = new SaveImageModel
            {
                IsRaw = true,
                RawImage = rawImage,
                FilePath = filePath,
                FileName = fileName,
                Grade = isOK ? Grade.OK : Grade.NG
            };
            AddImage(img);
        }
    }
}
