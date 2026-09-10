using BeevisionSolution.Models;
using BeevisionSolution.Utils;
using BeevisionSolution.Views;
using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Discovery;
using Cognex.DataMan.SDK.Utils;
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.Implementation.Internal;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.DwayneNeed.Shapes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace BeevisionSolution.Controller
{
    public class DatamanController: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Func<Image, Task> OnBarcodeImageReceive;
        public Action<string, string> OnCogImageReceive;
        public Dictionary<DataManSystem, ResultCollector> ListDataman;
        public Dictionary<ResultCollector, DataManSystem> CollectorToSystem;
        public Dictionary<string, bool> ListReconnect;
        public static string SheetId { get; set; }

        private SynchronizationContext _syncContext = null;
        private EthSystemDiscoverer _ethSystemDiscoverer = null;
        private SerSystemDiscoverer _serSystemDiscoverer = null;
        private object _currentResultInfoSyncLock = new object();
        private bool _closing = false;
        private bool _autoconnect = false;
        private bool _isStop = true;
        private readonly object _listAddItemLock = new object();

        private List<ReaderContentModel> _readers;
        public List<ReaderContentModel> Readers
        {
            get
            {
                return _readers;
            }
            set
            {
                if(_readers != value )
                {
                    _readers = value;
                    OnPropertyChanged(nameof(Readers));
                }
            }
        }

        private List<object> _connectionList;
        public List<object> ConnectionList
        {
            get
            {
                return _connectionList;
            }
            set
            {
                if( _connectionList != value )
                {
                    _connectionList = value;
                    OnPropertyChanged(nameof(ConnectionList));
                }
            }
        }

        public DatamanController()
        {
            Init();
        }

        public void Init()
        {
            _syncContext = new SynchronizationContext();

            _ethSystemDiscoverer = new EthSystemDiscoverer();
            _serSystemDiscoverer = new SerSystemDiscoverer();

            // Subscribe to the system discoved event.
            _ethSystemDiscoverer.SystemDiscovered += new EthSystemDiscoverer.SystemDiscoveredHandler(OnEthSystemDiscovered);
            _serSystemDiscoverer.SystemDiscovered += new SerSystemDiscoverer.SystemDiscoveredHandler(OnSerSystemDiscovered);

            // Ask the discoverers to start discovering systems.
            _ethSystemDiscoverer.Discover();
            _serSystemDiscoverer.Discover();

            // Binding
            ListDataman = new Dictionary<DataManSystem, ResultCollector>();
            CollectorToSystem = new Dictionary<ResultCollector, DataManSystem>();
            Readers = new List<ReaderContentModel>();
            //ListConnection = new List<DataManSystem>();
            //ListResultCollector = new List<ResultCollector>();
            ConnectionList = new List<object>();

            OnCogImageReceive += JobController.OnCogImageReceive;
            JobController.OnSheetIdReceive += OnSheetIdReceive;
        }

        #region Device Discovery Events

        private void OnEthSystemDiscovered(EthSystemDiscoverer.SystemInfo systemInfo)
        {
            _syncContext.Post(
                new SendOrPostCallback(
                    delegate
                    {
                        ConnectionList.Add(systemInfo);
                        Common.Info($"Device: {systemInfo.Name} is discovered by ethernet-based");

                        DataManSystem system = null;
                        ISystemConnector connector = null;
                        ResultCollector results = null;
                        ConnectionProcessing(system, connector, results);
                    }),
                    null);
        }

        private void OnSerSystemDiscovered(SerSystemDiscoverer.SystemInfo systemInfo)
        {
            _syncContext.Post(
                new SendOrPostCallback(
                    delegate
                    {
                        ConnectionList.Add(systemInfo);
                        Common.Info($"Device: {systemInfo.Name} is discovered by serial ports");

                        DataManSystem system = null;
                        ISystemConnector connector = null;
                        ResultCollector results = null;
                        ConnectionProcessing(system, connector, results);
                    }),
                    null);
        }

        #endregion
        #region Device Events

        private void OnBinaryDataTransferProgress(object sender, BinaryDataTransferProgressEventArgs args)
        {
            //Common.Info("OnBinaryDataTransferProgress", string.Format("{0}: {1}% of {2} bytes (Type={3}, Id={4})", args.Direction == TransferDirection.Incoming ? "Receiving" : "Sending", args.TotalDataSize > 0 ? (int)(100 * (args.BytesTransferred / (double)args.TotalDataSize)) : -1, args.TotalDataSize, args.ResultType.ToString(), args.ResponseId));
        }

        private void OffProtocolByteReceived(object sender, OffProtocolByteReceivedEventArgs args)
        {
            //Common.Info("OffProtocolByteReceived", string.Format("{0}", (char)args.Byte));
        }

        private void AutomaticResponseArrived(object sender, AutomaticResponseArrivedEventArgs args)
        {
            //Common.Info("AutomaticResponseArrived", string.Format("Type={0}, Id={1}, Data={2} bytes", args.DataType.ToString(), args.ResponseId, args.Data != null ? args.Data.Length : 0));
        }

        private void OnLiveImageArrived(IAsyncResult result)
        {
            //try
            //{
            //    Image image = _system.EndGetLiveImage(result);

            //    _syncContext.Post(
            //        delegate
            //        {
            //            Size image_size = Gui.FitImageInControl(image.Size, picResultImage.Size);
            //            Image fitted_image = Gui.ResizeImageToBitmap(image, image_size);
            //            picResultImage.Image = fitted_image;
            //            picResultImage.Invalidate();

            //            _system.BeginGetLiveImage(
            //                    Cognex.DataMan.SDK.ImageFormat.jpeg,
            //                    ImageSize.Sixteenth,
            //                    ImageQuality.Medium,
            //                    OnLiveImageArrived,
            //                    null);
            //        },
            //    null);
            //}
            //catch
            //{
            //}
        }

        private void OnSystemConnected(object sender, EventArgs args)
        {
            _syncContext.Post(
                delegate
                {
                    var value = sender as DataManSystem;
                    Common.Info("System connected");
                    if (value.Connector is EthSystemConnector ethConn)
                    {
                        foreach (var reader in Readers)
                        {
                            if (reader.DeviceIp.Equals(ethConn.Address))
                            {
                                Common.Info($"{reader.DeviceName}: System connected");
                            }
                        }
                    }
                    else if (value.Connector is SerSystemConnector serConn)
                    {
                        foreach (var reader in Readers)
                        {
                            if (reader.DevicePort.Equals(ConvertCharacter(serConn.PortName)))
                            {
                                Common.Info($"{reader.DeviceName}: System connected");
                            }
                        }
                    }
                },
                null);
        }

        private void OnSystemDisconnected(object sender, EventArgs args)
        {
            _syncContext.Post(
                async delegate
                {
                    var value = sender as DataManSystem;
                    string key = GetSystemKey(value);

                    if (value.Connector is EthSystemConnector ethConn)
                    {
                        foreach (var reader in Readers)
                        {
                            if (reader.DeviceIp.Equals(ethConn.Address))
                            {
                                Common.Info($"{reader.DeviceName}: System disconnected");
                            }
                        }
                    }
                    else if (value.Connector is SerSystemConnector serConn)
                    {
                        foreach (var reader in Readers)
                        {
                            if (reader.DevicePort.Equals(ConvertCharacter(serConn.PortName)))
                            {
                                Common.Info($"{reader.DeviceName}: System disconnected");
                            }
                        }
                    }

                    if (!ListReconnect.ContainsKey(key))
                    {
                        ListReconnect[key] = true;
                        await RetryReconnect(value, key);
                    }
                },
                null);
        }

        private void OnKeepAliveResponseMissed(object sender, EventArgs args)
        {
            _syncContext.Post(
                async delegate
                {
                    var value = sender as DataManSystem;
                    string key = GetSystemKey(value);

                    if (value.Connector is EthSystemConnector ethConn)
                    {
                        foreach(var reader in Readers)
                        {
                            if (reader.DeviceIp.Equals(ethConn.Address))
                            {
                                Common.Info($"{reader.DeviceName}: Keep-alive response missed");
                            }
                        }
                    }

                    if (!ListReconnect.ContainsKey(key))
                    {
                        ListReconnect[key] = true;
                        await RetryReconnect(value, key);
                    }
                },
                null);
        }

        private void OnSystemWentOnline(object sender, EventArgs args)
        {
            _syncContext.Post(
                delegate
                {
                    var value = sender as DataManSystem;
                    if(value.Connector is EthSystemConnector ethConn)
                    {
                        var currentReader = Readers.FirstOrDefault(x => x.DeviceIp.Equals(ethConn.Address));
                        Common.Info($"{currentReader.DeviceName}: System went online");
                    }
                    else if (value.Connector is SerSystemConnector serConn)
                    {
                        var currentReader = Readers.FirstOrDefault(x => x.DevicePort.Equals(ConvertCharacter(serConn.PortName)));
                        Common.Info($"{currentReader.DeviceName}: System went online");
                    } 
                },
                null);
        }

        private void OnSystemWentOffline(object sender, EventArgs args)
        {
            _syncContext.Post(
                delegate
                {
                    var value = sender as DataManSystem;
                    if (value.Connector is EthSystemConnector ethConn)
                    {
                        var currentReader = Readers.FirstOrDefault(x => x.DeviceIp.Equals(ethConn.Address));
                        Common.Info($"{currentReader.DeviceName}: System went offline");
                    }
                    else if (value.Connector is SerSystemConnector serConn)
                    {
                        var currentReader = Readers.FirstOrDefault(x => x.DevicePort.Equals(ConvertCharacter(serConn.PortName)));
                        Common.Info($"{currentReader.DeviceName}: System went offline");
                    }
                },
                null);
        }

        #endregion

        public void ConnectionProcessing(DataManSystem system, ISystemConnector connector, ResultCollector results)
        {
            try
            {


                for(int i = 0; i <= ConnectionList.Count - 1; i++)
                {
                    var items = ConnectionList[i];
                    var system_info = items;
                    StringBuilder serial = new StringBuilder();
                    StringBuilder port = new StringBuilder();

                    if (system_info is EthSystemDiscoverer.SystemInfo)
                    {
                        EthSystemDiscoverer.SystemInfo eth_system_info = system_info as EthSystemDiscoverer.SystemInfo;
                        EthSystemConnector conn = new EthSystemConnector(eth_system_info.IPAddress, eth_system_info.Port);

                        // mac, name, port, serial, ip
                        bool isExist = Readers.Any(x => x.DeviceId == eth_system_info.MacAddress);
                        bool isDupplicate = Readers.Any(x => x.DeviceName == eth_system_info.Name);
                        if (!isExist && !isDupplicate)
                        {
                            IsActive isEnableLog = IsActive.Inactive;
                            IsActive isActiveDisplay = IsActive.Inactive;
                            bool isActiveDevice = false;

                            Readers.Add(new ReaderContentModel(
                                eth_system_info.MacAddress,
                                eth_system_info.Name,
                                eth_system_info.Type,
                                eth_system_info.SerialNumber,
                                eth_system_info.Port,
                                eth_system_info.IPAddress,
                                eth_system_info.SubnetMask,
                                eth_system_info.DefaultGateway,
                                string.Empty,
                                StatusMode.OK,
                                isActiveDisplay,
                                IsCompare.Deny,
                                isEnableLog,
                                isActiveDevice));
                        }
                        else
                        {
                            ReaderContentModel current = Readers.Where(x => x.DeviceId == eth_system_info.MacAddress).FirstOrDefault();
                            current.Status = StatusMode.OK;
                        }

                        conn.UserName = "admin";
                        conn.Password = "";

                        connector = conn;
                    }
                    else if (system_info is SerSystemDiscoverer.SystemInfo)
                    {
                        SerSystemDiscoverer.SystemInfo ser_system_info = system_info as SerSystemDiscoverer.SystemInfo;
                        SerSystemConnector conn = new SerSystemConnector(ser_system_info.PortName, ser_system_info.Baudrate);

                        foreach (char c in ser_system_info.SerialNumber)
                        {
                            if (char.IsLetter(c))
                            {
                                int value = char.ToUpper(c) - 'A' + 1;
                                serial.Append(value);
                            }
                            else if (char.IsDigit(c))
                            {
                                serial.Append(c);
                            }
                        }

                        foreach (char c in ser_system_info.PortName)
                        {
                            if (char.IsLetter(c))
                            {
                                int value = char.ToUpper(c) - 'A' + 1;
                                port.Append(value);
                            }
                            else if (char.IsDigit(c))
                            {
                                port.Append(c);
                            }
                        }

                        var serialResult = Convert.ToInt64(serial.ToString());
                        var portResult = Convert.ToInt32(port.ToString());

                        // mac, name, port, serial, ip
                        bool isExist = Readers.Any(x => x.DeviceId == serialResult);
                        bool isDupplicate = Readers.Any(x => x.DeviceName == ser_system_info.Name);
                        if (!isExist && !isDupplicate)
                        {
                            IsActive isEnableLog = IsActive.Inactive;
                            IsActive isActiveDisplay = IsActive.Inactive;
                            bool isActiveDevice = false;

                            Readers.Add(new ReaderContentModel(
                                serialResult,
                                ser_system_info.Name,
                                ser_system_info.Type,
                                ser_system_info.SerialNumber,
                                portResult,
                                null,
                                null,
                                null,
                                string.Empty,
                                StatusMode.OK,
                                isActiveDisplay,
                                IsCompare.Deny,
                                isEnableLog,
                                isActiveDevice));
                        }
                        else
                        {
                            ReaderContentModel current = Readers.Where(x => x.DeviceId == serialResult).FirstOrDefault();
                            current.Status = StatusMode.OK;
                        }

                        connector = conn;
                    }

                    system = new DataManSystem(connector);
                    system.DefaultTimeout = 5000;

                    // Subscribe to events that are signalled when the system is connected / disconnected.
                    system.SystemConnected += new SystemConnectedHandler(OnSystemConnected);
                    system.SystemDisconnected += new SystemDisconnectedHandler(OnSystemDisconnected);
                    system.SystemWentOnline += new SystemWentOnlineHandler(OnSystemWentOnline);
                    system.SystemWentOffline += new SystemWentOfflineHandler(OnSystemWentOffline);
                    system.KeepAliveResponseMissed += new KeepAliveResponseMissedHandler(OnKeepAliveResponseMissed);
                    system.BinaryDataTransferProgress += new BinaryDataTransferProgressHandler(OnBinaryDataTransferProgress);
                    system.OffProtocolByteReceived += new OffProtocolByteReceivedHandler(OffProtocolByteReceived);
                    system.AutomaticResponseArrived += new AutomaticResponseArrivedHandler(AutomaticResponseArrived);

                    // Subscribe to events that are signalled when the device sends auto-responses.
                    ResultTypes requested_result_types = ResultTypes.ReadXml | ResultTypes.Image | ResultTypes.ImageGraphics;
                    results = new ResultCollector(system, requested_result_types);
                    //_results.ComplexResultCompleted += new ComplexResultCompletedEventHandler((s, ev) => Results_ComplexResultCompleted(s, ev, _btnContext));
                    results.ComplexResultCompleted += Results_ComplexResultCompleted;
                    //_results.SimpleResultDropped += Results_SimpleResultDropped;
                    system.SetKeepAliveOptions(true, 3000, 1000);

                    system.Connect();

                    try
                    {
                        system.SetResultTypes(requested_result_types);
                    }
                    catch(Exception e)
                    {
                        Common.Info(e.ToString());
                    }

                    ListDataman[system] = results;
                    CollectorToSystem[results] = system;

                    //remove discovered connection when successfully connect to device
                    ConnectionList.Remove(items);
                }
                _autoconnect = true;

                Readers = new List<ReaderContentModel>(Readers.OrderBy(i => i.DeviceId));
            }
            catch (Exception ex)
            {
                CleanupConnection(system);
                Common.Bug("Discorver Error: " + ex.ToString());
            }
        }

        private void Results_ComplexResultCompleted(object sender, ComplexResult e)
        {
            if (sender is ResultCollector collector && CollectorToSystem.TryGetValue(collector, out var system))
            {
                _syncContext.Post(
                    delegate 
                    { 
                        Common.Info("Dataman trigger received"); 
                        ShowResult(system, e); 
                    }, 
                    null);
            }
        }

        private string GetReadStringFromResultXml(DataManSystem system, string resultXml, string node)
        {
            try
            {
                XmlDocument doc = new XmlDocument();

                doc.LoadXml(resultXml);

                XmlNode getNode = doc.SelectSingleNode("result/general/" + node);

                if (getNode != null && system != null && system.State == ConnectionState.Connected)
                {
                    if (node.Equals("full_string"))
                    {
                        XmlAttribute encoding = getNode.Attributes["encoding"];
                        if (encoding != null && encoding.InnerText == "base64")
                        {
                            if (!string.IsNullOrEmpty(getNode.InnerText))
                            {
                                byte[] code = Convert.FromBase64String(getNode.InnerText);
                                return system.Encoding.GetString(code, 0, code.Length);
                            }
                            else
                            {
                                return "";
                            }
                        }
                    }

                    return getNode.InnerText;
                }
            }
            catch(Exception ex)
            {
                Common.Bug("XML Parse Error: " + ex.Message);
            }

            return "";
        }

        public void CleanupConnection(DataManSystem sys)
        {
            if (null != sys)
            {
                sys.SystemConnected -= OnSystemConnected;
                sys.SystemDisconnected -= OnSystemDisconnected;
                sys.SystemWentOnline -= OnSystemWentOnline;
                sys.SystemWentOffline -= OnSystemWentOffline;
                sys.KeepAliveResponseMissed -= OnKeepAliveResponseMissed;
                sys.BinaryDataTransferProgress -= OnBinaryDataTransferProgress;
                sys.OffProtocolByteReceived -= OffProtocolByteReceived;
                sys.AutomaticResponseArrived -= AutomaticResponseArrived;
                sys.Disconnect();
                sys.Dispose();
            }

            sys = null;
        }

        public void Dispose()
        {
            // clear dataman connection
            foreach(var dtm in ListDataman)
            {
                dtm.Value.Dispose();
                CleanupConnection(dtm.Key);
            }

            ListDataman.Clear();

            foreach (var kvp in CollectorToSystem)
            {
                if (!ListDataman.ContainsKey(kvp.Value))
                {
                    kvp.Key.Dispose();
                    CleanupConnection(kvp.Value);
                }
            }
            CollectorToSystem.Clear();

            Readers.Clear();

            _ethSystemDiscoverer?.Dispose();
            _serSystemDiscoverer?.Dispose();

            OnCogImageReceive = null;
            OnBarcodeImageReceive = null;
        }

        private void ShowResult(DataManSystem system, ComplexResult complexResult)
        {
            List<Image> images = new List<Image>();
            Image tmpImage = null;
            List<string> image_graphics = new List<string>();
            string read_result = null;
            string read_status = null;
            string read_source = null;
            int read_trigger_index = -1;
            int read_trigger_time = -1;
            int read_decode_time = -1;
            string read_generator = null;
            string read_setup = null;
            string module_size = null;
            string symbology = null;
            int result_id = -1;
            string result_dt = null;
            string image_name = null;

            ResultTypes collected_results = ResultTypes.None;

            lock (_currentResultInfoSyncLock)
            {
                try
                {
                    foreach (var simple_result in complexResult.SimpleResults)
                    {
                        collected_results |= simple_result.Id.Type;

                        switch (simple_result.Id.Type)
                        {
                            case ResultTypes.Image:
                                Image image = ImageArrivedEventArgs.GetImageFromImageBytes(simple_result.Data);
                                if (image != null)
                                {
                                    tmpImage = (Image)image.Clone();
                                }
                                images.Add(image);
                                break;
                            case ResultTypes.ImageGraphics:
                                image_graphics.Add(simple_result.GetDataAsString());
                                break;
                            case ResultTypes.ReadXml:
                                read_result = !string.IsNullOrEmpty(GetReadStringFromResultXml(system, simple_result.GetDataAsString(), "full_string").Trim()) ? GetReadStringFromResultXml(system, simple_result.GetDataAsString().Trim(), "full_string") : "NG";
                                read_status = GetReadStringFromResultXml(system, simple_result.GetDataAsString(), "status");
                                read_source = GetReadStringFromResultXml(system, simple_result.GetDataAsString(), "result_source");
                                read_trigger_index = !string.IsNullOrEmpty(GetReadStringFromResultXml(system, simple_result.GetDataAsString(), "trigger_index")) ? Convert.ToInt32(GetReadStringFromResultXml(system, simple_result.GetDataAsString(), "trigger_index")) : 99999;
                                read_trigger_time = Convert.ToInt32(GetReadStringFromResultXml(system, simple_result.GetDataAsString(), "trigger_time"));
                                read_decode_time = !string.IsNullOrEmpty(GetReadStringFromResultXml(system, simple_result.GetDataAsString(), "decode_time")) ? Convert.ToInt32(GetReadStringFromResultXml(system, simple_result.GetDataAsString(), "decode_time")) : 99999;
                                read_generator = GetReadStringFromResultXml(system, simple_result.GetDataAsString(), "generator");
                                read_setup = !string.IsNullOrEmpty(GetReadStringFromResultXml(system, simple_result.GetDataAsString(), "read_setup")) ? GetReadStringFromResultXml(system, simple_result.GetDataAsString(), "read_setup") : "0";
                                module_size = !string.IsNullOrEmpty(GetReadStringFromResultXml(system, simple_result.GetDataAsString(), "module_size")) ? GetReadStringFromResultXml(system, simple_result.GetDataAsString(), "module_size") : "0.00";
                                symbology = !string.IsNullOrEmpty(GetReadStringFromResultXml(system, simple_result.GetDataAsString(), "symbology")) ? GetReadStringFromResultXml(system, simple_result.GetDataAsString(), "symbology") : "NO RESULT";
                                result_dt = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt");
                                result_id = simple_result.Id.Id;
                                break;
                            case ResultTypes.ReadString:
                                read_result = simple_result.GetDataAsString();
                                result_id = simple_result.Id.Id;
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Common.Bug("Result Parsed Error: " + ex.ToString());
                }
            }

            lock (_listAddItemLock)
            {
                try
                {
                    // reader save result
                    foreach (var reader in Readers)
                    {
                        if (reader.DeviceName == read_source)
                        {
                            var lotCode = JobController.SplitByIndex(SheetId, 4, 11);
                            reader.Result = read_result;
                            image_name = DateTime.Now.ToString($"{lotCode}_ddMMyyyy_HHmmsstt");

                            if (images.Count > 0)
                            {
                                Image imageResult = Gui.ResizeImageToBitmap(images[0], images[0].Size);
                                // image result graph
                                //if (image_graphics.Count > 0)
                                //{
                                //    using (Graphics g = Graphics.FromImage(imageResult))
                                //    {
                                //        foreach (var graphics in image_graphics)
                                //        {
                                //            ResultGraphics rg = GraphicsResultParser.Parse(graphics, new System.Drawing.Rectangle(0, 0, imageResult.Width, imageResult.Height));
                                //            ResultGraphicsRenderer.PaintResults(g, rg);
                                //        }
                                //    }
                                //}

                                // saving image by enqueue
                                SaveImageModel img = new SaveImageModel();
                                img.IsRaw = false;
                                img.GraphicImg = imageResult;
                                img.FileName = image_name;
                                img.FilePath = Common.GetLoggingFolderBarcode(reader.DeviceName, lotCode);
                                img.Grade = reader.Result.Equals("NG") ? Grade.NG : Grade.OK;
                                img.IsBarcode = true;
                                SavingImgCtrl.AddImage(img);

                                // convert to icogimage
                                string path = Path.Combine(img.FilePath, "Raw", img.Grade == Grade.OK ? "OK" : "NG", img.FileName + ".jpg");
                                //var iCogImage = ConvertToCogImage(imageResult, path);
                                //if(iCogImage != null)
                                //{
                                //    Common.Info($"Convert ICogImage: Push image from barcode {path} successfull");
                                //}
                                //else
                                //{
                                //    Common.Info($"Convert ICogImage: Failed to convert image from {path}");
                                //}

                                //push image to queue of ip dictionary
                                OnCogImageReceive?.Invoke(reader.DeviceIpString, path);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Common.Bug("Read Result Error: {0}\nStack Trace: {1}", ex.Message, ex.StackTrace);
                }
            }

        }

        private async Task RetryReconnect(DataManSystem oldSystem, string key)
        {
            Common.Info($"Starting reconnect loop for {key}");

            while (true)
            {
                try
                {
                    // Cleanup old connection
                    if (ListDataman.ContainsKey(oldSystem))
                    {
                        var collector = ListDataman[oldSystem];
                        collector.ComplexResultCompleted -= Results_ComplexResultCompleted;
                        CollectorToSystem.Remove(collector);
                        ListDataman.Remove(oldSystem);
                    }
                    oldSystem.Disconnect();

                    // Find the reader info by IP or Port
                    var reader = Readers.FirstOrDefault(r => r.DeviceIp.ToString() == key || r.DevicePort.ToString() == key);
                    if (reader == null)
                    {
                        Common.Info($"Reader not found for {key}, stop reconnect.");
                        break;
                    }

                    // Try reconnect
                    ISystemConnector connector;
                    if (!string.IsNullOrEmpty(reader.DeviceIp.ToString()))
                    {
                        connector = new EthSystemConnector(reader.DeviceIp, reader.DevicePort)
                        {
                            UserName = "admin",
                            Password = ""
                        };
                    }
                    else
                    {
                        connector = new SerSystemConnector(reader.DevicePort.ToString(), 115200);
                    }

                    var system = new DataManSystem(connector)
                    {
                        DefaultTimeout = 5000
                    };

                    // re-subscribe events
                    system.SystemConnected += OnSystemConnected;
                    system.SystemDisconnected += OnSystemDisconnected;
                    system.SystemWentOnline += OnSystemWentOnline;
                    system.SystemWentOffline += OnSystemWentOffline;
                    system.KeepAliveResponseMissed += OnKeepAliveResponseMissed;
                    system.BinaryDataTransferProgress += OnBinaryDataTransferProgress;
                    system.OffProtocolByteReceived += OffProtocolByteReceived;
                    system.AutomaticResponseArrived += AutomaticResponseArrived;

                    var results = new ResultCollector(system, ResultTypes.ReadXml | ResultTypes.Image | ResultTypes.ImageGraphics);
                    results.ComplexResultCompleted += Results_ComplexResultCompleted;

                    system.SetKeepAliveOptions(true, 3000, 1000);

                    system.Connect();
                    system.SetResultTypes(ResultTypes.ReadXml | ResultTypes.Image | ResultTypes.ImageGraphics);

                    ListDataman[system] = results;
                    CollectorToSystem[results] = system;

                    Common.Info($"Reconnected successfully: {key}");
                    ListReconnect.Remove(key);
                    return;
                }
                catch (Exception ex)
                {
                    Common.Bug($"Reconnect failed for {key}: {ex.Message}");
                }

                await Task.Delay(2000); 
            }
        }

        private ICogImage ConvertToCogImage(Image img, string path)
        {
            if(img == null)
            {
                return null;
            }

            for (int numTries = 0; numTries < 200; numTries++)
            {
                try
                {
                    var fileTool = new CogImageFileTool();
                    fileTool.Operator.Open(path, CogImageFileModeConstants.Read);
                    fileTool.Run();
                    return fileTool.OutputImage;
                }
                catch (IOException ex)
                {
                    Common.Info("ConvertToCogImage IO : Wait for file ready");
                }
                catch (Exception ex)
                {
                    Common.Info("ConvertToCogImage: Wait for file ready");
                }

                Thread.Sleep(50);
            }

            Common.Info("ConvertToCogImage timeout waiting for file: {0}", path);
            return null;
        }

        private string GetSystemKey(DataManSystem sys)
        {
            if (sys.Connector is EthSystemConnector eth)
            {
                return eth.Address.ToString();
            }
                
            if (sys.Connector is SerSystemConnector ser)
            {
                return ConvertCharacter(ser.PortName).ToString();
            }
                
            return null;
        }

        private int ConvertCharacter(string convertString)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in convertString)
            {
                if (char.IsLetter(c))
                {
                    int value = char.ToUpper(c) - 'A' + 1;
                    stringBuilder.Append(value);
                }
                else if (char.IsDigit(c))
                {
                    stringBuilder.Append(c);
                }
            }

            return Convert.ToInt32(stringBuilder.ToString());
        }

        public static void OnSheetIdReceive(string sheetId)
        {
            if (!string.IsNullOrEmpty(sheetId))
            {
                SheetId = sheetId;
            }
            else
            {
                SheetId = "";
            }

            Common.Info($"Dataman SheetID Receive: {SheetId}");
        }
    }
}
