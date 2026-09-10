using BeevisionSolution.Models;
using BeevisionSolution.Utils;
using BeevisionSolution.Views;
using log4net.Filter;
using Microsoft.Xaml.Behaviors.Media;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BeevisionSolution.Controller
{
    public class SerialPortController: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event EventHandler<string> DataReceived;
        public bool GlobalLoggingEnabled { get; set; } = false;

        private ConcurrentDictionary<string, SerialPort> listPort;
        private ConcurrentDictionary<string, object> portsLocks;
        private ConcurrentDictionary<string, object> dataLocks;
        private ConcurrentDictionary<string, bool> loggingStates;
        public SerialPortController()
        {
            listPort = new ConcurrentDictionary<string, SerialPort>();
            portsLocks = new ConcurrentDictionary<string, object>();
            dataLocks = new ConcurrentDictionary<string, object>();
            loggingStates = new ConcurrentDictionary<string, bool>();
        }

        public bool Connect(string portName, int baudRate = 9600, int timeout = 500)
        {
            if (listPort.ContainsKey(portName))
            {
                DataReceived?.Invoke(this, $"Port {portName} already in used");
                return true;
            }

            var portLock = portsLocks.GetOrAdd(portName, new object());

            lock (portLock)
            {
                try
                {
                    // init
                    var serialPort = new SerialPort(portName, baudRate)
                    {
                        Parity = Parity.None,
                        DataBits = 8,
                        StopBits = StopBits.One,
                        Handshake = Handshake.None,
                        ReadTimeout = timeout,
                        WriteTimeout = timeout,
                        NewLine = "\r\n",
                        DtrEnable = true,
                        RtsEnable = true
                    };
                    
                    serialPort.DataReceived += SerialPort_DataReceived;

                    serialPort.Open();

                    if (serialPort.IsOpen)
                    {
                        loggingStates[portName] = true;
                        GlobalLoggingEnabled = loggingStates[portName];

                        //Common.Info($"Successfully connected to {portName} at {baudRate} baud");
                        DataReceived?.Invoke(this, $"Successfully connected to {portName} at {baudRate} baud");

                        DataReceived?.Invoke(this, "Configuring....");
                        //Config here when init
                        if (serialPort.BytesToRead > 0)
                        {
                            string garbageData = serialPort.ReadExisting();
                            DataReceived?.Invoke(this, $"Cleared startup data: {garbageData.Trim()}");
                        }
                        serialPort.DiscardInBuffer();
                        serialPort.DiscardOutBuffer();

                        //End Configuring
                        DataReceived?.Invoke(this, "Finish configured");
                        listPort[portName] = serialPort;


                        // test function
                        //bool communicationTest = TestBasicCommunication(serialPort, portName);
                        //if (communicationTest)
                        //{
                        //    // Add to list only if communication works
                        //    listPort[portName] = serialPort;
                        //    DataReceived?.Invoke(this, $"✅ Successfully connected to {portName}");
                        //    DataReceived?.Invoke(this, "Device configured and ready");
                        //    return true;
                        //}
                        //else
                        //{
                        //    DataReceived?.Invoke(this, "⚠️ Connected but device not responding properly");
                        //    // Still add to list but warn user
                        //    listPort[portName] = serialPort;
                        //    return true;
                        //}
                        return true;
                    }
                    else
                    {
                        //Common.Info($"Failed to open connection to {portName}");
                        DataReceived?.Invoke(this, $"Failed to open connection to {portName}");
                        return false;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    //Common.Bug($"Access denied to {portName}: {ex.Message}");
                    DataReceived?.Invoke(this, $"Access denied to {portName}: {ex.Message}");
                    return false;
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    //Common.Bug($"Invalid port name {portName}: {ex.Message}");
                    DataReceived?.Invoke(this, $"Invalid port name {portName}: {ex.Message}");
                    return false;
                }
                catch (ArgumentException ex)
                {
                    //Common.Bug($"Invalid argument for {portName}: {ex.Message}");
                    DataReceived?.Invoke(this, $"Invalid argument for {portName}: {ex.Message}");
                    return false;
                }
                catch (InvalidOperationException ex)
                {
                    //Common.Bug($"Port {portName} is already open: {ex.Message}");
                    DataReceived?.Invoke(this, $"Port {portName} is already open: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    //Common.Bug($"Unexpected error connecting to {portName}: {ex.Message}");
                    DataReceived?.Invoke(this, $"Unexpected error connecting to {portName}: {ex.Message}");
                    return false;
                }
            }
        }

        public bool IsLoggingEnabled(string portName)
        {
            return loggingStates.TryGetValue(portName, out bool isEnabled) && isEnabled == true;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            SerialPort port = (SerialPort)sender;
            if (port == null || string.IsNullOrEmpty(port.PortName))
            {
                DataReceived?.Invoke(this, "Error: Invalid port in DataReceived event");
                return;
            }

            if (!IsLoggingEnabled(port.PortName))
            {
                return;
            }

            var dataLock = dataLocks.GetOrAdd(port.PortName, new object());

            lock (dataLock)
            {
                try
                {
                    if (!port.IsOpen)
                    {
                        DataReceived?.Invoke(this, $"{port.PortName}: Port closed during read");
                        return;
                    }

                    if (port.BytesToRead > 0)
                    {
                        string data = port.ReadExisting();
                        if (!string.IsNullOrEmpty(data))
                        {
                            string[] lines = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string line in lines)
                            {
                                string trimmedLine = line.Trim();
                                if (!string.IsNullOrEmpty(trimmedLine))
                                {
                                    DataReceived?.Invoke(this, $"{port.PortName}'s Received Data: {trimmedLine}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Common.Info($"Error reading data: {ex.Message}");
                    DataReceived?.Invoke(this, $"Error reading data: {ex.Message}");
                }
            }
        }

        public bool SendCommand(string portName, string command)
        {
            if (listPort.TryGetValue(portName, out SerialPort port))
            {
                if (port == null || !port.IsOpen)
                {
                    DataReceived?.Invoke(this, "Cannot send command - Arduino not connected");
                    return false;
                }

                var portLock = portsLocks.GetOrAdd(portName, new object());

                lock (portLock)
                {
                    try
                    {
                        port.WriteLine(command);
                        DataReceived?.Invoke(this, $"{portName}'s Command Sent: {command}");
                        return true;
                    }
                    catch (TimeoutException ex)
                    {
                        DataReceived?.Invoke(this, $"Send timeout: {ex.Message}");
                        return false;
                    }
                    catch (InvalidOperationException ex)
                    {
                        DataReceived?.Invoke(this, $"Port not available: {ex.Message}");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        DataReceived?.Invoke(this, $"Send error: {ex.Message}");
                        return false;
                    }
                }
            }
            else
            {
                DataReceived?.Invoke(this, "Cannot send command - Arduino not connected");
                return false;
            }
        }

        // FSR Command Syntax
        #region FSR Command Syntax
        public bool ATConnection(string portName)
        {
            return SendCommand(portName, "AT");
        }

        public bool ATVersion(string portName)
        {
            return SendCommand(portName, "AT+VERSION");
        }

        public bool ATGetBaudRate(string portName)
        {
            return SendCommand(portName, "AT+BAUD?");
        }

        public bool ATSetBaudRate(string portName, int baudRate = 9600)
        {
            if (baudRate != 9600)
            {
                return SendCommand(portName, $"AT+BAUD={baudRate}");
            }
            else
            {
                return SendCommand(portName, $"AT+BAUD=DEFAULT");
            }
        }

        public bool ATGetParity(string portName)
        {
            return SendCommand(portName, "AT+PARITY?");
        }

        public bool ATSetParity(string portName, string parity = "DEFAULT")
        {
            return SendCommand(portName, $"AT+PARITY={parity}");            
        }

        public bool ATGetSampleConfig(string portName)
        {
            return SendCommand(portName, "AT+SAMPLE?");
        }

        public bool ATSetSampleConfig(string portName, int time = 5)
        {
            //time is in millisecond
            if(time != 5 && time <= 5000 && time > 5)
            {
                return SendCommand(portName, $"AT+SAMPLE={time}");
            }
            else
            {
                return SendCommand(portName, "AT+SAMPLE=DEFAULT");
            }
        }

        public bool ATGetCycle(string portName)
        {
            return SendCommand(portName, "AT+CYCLE?");
        }

        public bool ATSetCycle(string portName, int time = 100)
        {
            // time is in millisecond
            if (time != 100 && time <= 5000 && time > 100)
            {
                return SendCommand(portName, $"AT+CYCLE={time}");
            }
            else
            {
                return SendCommand(portName, "AT+CYCLE=DEFAULT");
            }
        }

        public bool ATGetKalmanEst(string portName)
        {
            return SendCommand(portName, "AT+KALMAN_ESTIMATE?");
        }

        public bool ATSetKalmanEst(string portName, int value = 0)
        {
            if (value != 0 && value <= 1000 && value > 0)
            {
                return SendCommand(portName, $"AT+KALMAN_ESTIMATE={value}");
            }
            else
            {
                return SendCommand(portName, "AT+KALMAN_ESTIMATE=DEFAULT");
            }
        }

        public bool ATGetKalmanError(string portName)
        {
            return SendCommand(portName, "AT+KALMAN_ERROR?");
        }

        public bool ATSetKalmanError(string portName, int value = 1)
        {
            if (value != 1 && value <= 1000 && value > 1)
            {
                return SendCommand(portName, $"AT+KALMAN_ERROR={value}");
            }
            else
            {
                return SendCommand(portName, "AT+KALMAN_ERROR=DEFAULT");
            }
        }

        public bool ATGetKalmanQ(string portName)
        {
            return SendCommand(portName, "AT+KALMAN_Q?");
        }

        public bool ATSetKalmanQ(string portName, double value = 0.01)
        {
            if (value != 0.01 && value <= 99.9 && value > 0.01)
            {
                return SendCommand(portName, $"AT+KALMAN_Q={value}");
            }
            else
            {
                return SendCommand(portName, "AT+KALMAN_Q=DEFAULT");
            }
        }

        public bool ATGetKalmanR(string portName)
        {
            return SendCommand(portName, "AT+KALMAN_R?");
        }

        public bool ATSetKalmanR(string portName, int value = 10)
        {
            if (value != 10 && value <= 1000 && value > 10)
            {
                return SendCommand(portName, $"AT+KALMAN_R={value}");
            }
            else
            {
                return SendCommand(portName, "AT+KALMAN_R=DEFAULT");
            }
        }

        public bool ATGetMaxSize(string portName)
        {
            return SendCommand(portName, "AT+MAX_SIZE?");
        }

        public bool ATSetMaxSize(string portName, int size = 1)
        {
            if (size != 1 && size <= 100 && size > 1)
            {
                return SendCommand(portName, $"AT+MAX_SIZE={size}");
            }
            else
            {
                return SendCommand(portName, "AT+MAX_SIZE=DEFAULT");
            }
        }

        public bool ATGetMode(string portName)
        {
            return SendCommand(portName, "AT+MODE?");
        }

        public bool ATSetMode(string portName, int mode = 0)
        {
            // data type
            // 0 = raw
            // 1 = kalman
            // 2 = avg
            if (mode != 0 && mode <= 2 && mode > 0)
            {
                return SendCommand(portName, $"AT+MODE={mode}");
            }
            else
            {
                return SendCommand(portName, "AT+MODE=DEFAULT");
            }
        }

        public bool ATGetUpdate(string portName)
        {
            return SendCommand(portName, "AT+UPDATE?");
        }

        public bool ATSetUpdate(string portName, int type = 0)
        {
            // mode
            // 0 = manual
            // 1 = auto
            if (type != 0 && type <= 1 && type > 0)
            {
                return SendCommand(portName, $"AT+UPDATE={type}");
            }
            else
            {
                return SendCommand(portName, "AT+UPDATE=DEFAULT");
            }
        }

        public bool ATData(string portName)
        {
            return SendCommand(portName, "AT+DATA");
        }

        public bool ATSetLevelA(string portName, int value = 1)
        {
            if (value != 500 && value <= 1000 && value >= 1)
            {
                return SendCommand(portName, $"AT+LEVEL_A={value}");
            }
            else
            {
                return SendCommand(portName, $"AT+LEVEL_A=DEFAULT");
            }
        }

        public bool ATGetLevelAConfig(string portName)
        {
            return SendCommand(portName, "AT+LEVEL_A?");
        }

        public bool ATSetLevelB(string portName, int value = 1)
        {
            if (value != 500 && value <= 1000 && value >= 1)
            {
                return SendCommand(portName, $"AT+LEVEL_B={value}");
            }
            else
            {
                return SendCommand(portName, $"AT+LEVEL_B=DEFAULT");
            }
        }

        public bool ATGetLevelBConfig(string portName)
        {
            return SendCommand(portName, "AT+LEVEL_B?");
        }

        public bool ATResetFactory(string portName)
        {
            return SendCommand(portName, "AT+RESET_FACTORY");
        }

        public bool ATHelp(string portName)
        {
            return SendCommand(portName, "AT+HELP");
        }
        #endregion

        public void Disconnect(string portName)
        {
            if (!listPort.Any())
            {
                DataReceived?.Invoke(this, $"Currently not connected to any ports");
                return;
            }

            if (listPort.TryRemove(portName, out SerialPort port))
            {
                var portLock = portsLocks.GetOrAdd(portName, new object());

                lock (portLock)
                {
                    try
                    {
                        if (port != null)
                        {
                            if (port.IsOpen)
                            {
                                port.DataReceived -= SerialPort_DataReceived;
                                port.Close();
                            }

                            port.Dispose();
                            DataReceived?.Invoke(this, $"Port {portName} cleanup");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Common.Bug($"Error disconnecting: {ex.Message}");
                        DataReceived?.Invoke(this, $"Error disconnecting: {ex.Message}");
                    }
                }
            }
            else
            {
                DataReceived?.Invoke(this, $"Port {portName} is currently not connected");
            }
        }

        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        public bool IsPortConnected(string portName)
        {
            return listPort.TryGetValue(portName, out SerialPort port) && port?.IsOpen == true;
        }

        public void EnableLogging(string portName)
        {
            loggingStates[portName] = true;
            GlobalLoggingEnabled = loggingStates[portName];
        }

        public void DisableLogging(string portName)
        {
            loggingStates[portName] = false;
            GlobalLoggingEnabled = loggingStates[portName];
        }
        private bool TestBasicCommunication(SerialPort port, string portName)
        {
            try
            {
                DataReceived?.Invoke(this, "Testing device communication...");

                port.DiscardInBuffer();
                port.WriteLine("AT+DATA");


                DateTime startTime = DateTime.Now;
                while ((DateTime.Now - startTime).TotalMilliseconds < 2000) // 2 second timeout
                {
                    if (port.BytesToRead > 0)
                    {
                        string response = port.ReadExisting().Trim();
                        if (!string.IsNullOrEmpty(response))
                        {
                            DataReceived?.Invoke(this, $"Device responded: {response}");
                            return true;
                        }
                    }
                    Thread.Sleep(50);
                }

                DataReceived?.Invoke(this, "No response from device to test command");
                return false;
            }
            catch (Exception ex)
            {
                DataReceived?.Invoke(this, $"Communication test error: {ex.Message}");
                return false;
            }
        }
    }
}
