using BeevisionSolution.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualBasic.Devices;
using System.Runtime.CompilerServices;

namespace BeevisionSolution.ViewComponents
{
    /// <summary>
    /// Interaction logic for PcHealthy.xaml
    /// </summary>
    public partial class PcHealthy : UserControl, IDisposable
    {
        //PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        PerformanceCounter cpuCounter = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");

        PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        //PerformanceCounter ramTotMB = new PerformanceCounter("Memory", "Total MBytes");

        DriveInfo[] drives = DriveInfo.GetDrives();

        Timer _timer = new Timer();

        //ulong _totalMemory = 0;

        public PCHealthyModel UsageModel = new PCHealthyModel();
        public PcHealthy()
        {
            InitializeComponent();
            this.DataContext = UsageModel;

            //_totalMemory = GetTotalRam();

            _timer.Interval = 1000;
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            //UsageModel.AppVersion = $"{version}";//debug
            UsageModel.AppVersion = "1.0.1";
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }
        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            Dispose();
        }
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CounterUpdate();
        }

        private void CounterUpdate()
        {
            UsageModel.CpuUsage = (int)cpuCounter.NextValue();

            UsageModel.RamUsage = 100 - (int)(_cInfo.AvailablePhysicalMemory * 100 / _cInfo.TotalPhysicalMemory);

            foreach (DriveInfo drive in drives)
            {
                if (drive.Name == "C:\\" && drive.IsReady)
                {
                    UsageModel.HardDriveC = 100 - (int)(100 * drive.TotalFreeSpace / drive.TotalSize);
                }
                else if (drive.Name == "D:\\" && drive.IsReady)
                {
                    UsageModel.HardDriveD = 100 - (int)(100 * drive.TotalFreeSpace / drive.TotalSize);
                }
            }


        }
        Microsoft.VisualBasic.Devices.ComputerInfo _cInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Total Ram in MBytes</returns>
        //private long GetTotalRam()
        //{
        //    _totalMemory = _cInfo.TotalPhysicalMemory;
        //}

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }

    public class PCHealthyModel : PropertyChangedAbstract
    {
        private int _cpu, _ramUsage, _hardDriveC, _hardDriveD;
        private string _appVersion;
        public string AppVersion
        {
            get { return _appVersion; }
            set
            {
                _appVersion = value;
                Notify();
            }
        }
        public int CpuUsage
        {
            get { return _cpu; }
            set
            {
                _cpu = value;
                Notify();
            }
        }
        public int RamUsage
        {
            get { return _ramUsage; }
            set
            {
                _ramUsage = value;
                Notify();
            }
        }
        public int HardDriveC
        {
            get { return _hardDriveC; }
            set
            {
                _hardDriveC = value;
                Notify();
            }
        }
        public int HardDriveD
        {
            get { return _hardDriveD; }
            set
            {
                _hardDriveD = value;
                Notify();
            }
        }
    }
}
