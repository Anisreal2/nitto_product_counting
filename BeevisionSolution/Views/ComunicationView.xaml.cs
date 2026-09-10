using BeevisionSolution.Controller;
using BeevisionSolution.Models;
using BeevisionSolution.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BeevisionSolution.Views
{
    public class CommNodeModel
    {
        public int NodeId { get; set; }
        public string DeviceName { get; set; }
        public string Protocol { get; set; }
        public string Status { get; set; }
        public string Details { get; set; }
    }

    /// <summary>
    /// Interaction logic for ComunicationView.xaml
    /// </summary>
    public partial class ComunicationView : UserControl
    {
        public ObservableCollection<CommNodeModel> BusNodes { get; set; } = new ObservableCollection<CommNodeModel>();

        public ComunicationView()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadNodes();
        }

        private void LoadNodes()
        {
            BusNodes.Clear();
            BusNodes.Add(new CommNodeModel { NodeId = 1, DeviceName = "Inovance EtherCAT Master", Protocol = "EtherCAT", Status = "Active", Details = "Card 0 (IMC Controller)" });
            BusNodes.Add(new CommNodeModel { NodeId = 2, DeviceName = "Servo Drive Axis 0 (X)", Protocol = "CiA 402", Status = "Active", Details = "Node 1 - Profile Position / CSP" });
            BusNodes.Add(new CommNodeModel { NodeId = 3, DeviceName = "Servo Drive Axis 1 (Y)", Protocol = "CiA 402", Status = "Active", Details = "Node 2 - Profile Position / CSP" });
            BusNodes.Add(new CommNodeModel { NodeId = 4, DeviceName = "Servo Drive Axis 2 (Z)", Protocol = "CiA 402", Status = "Active", Details = "Node 3 - Profile Position / CSP" });
            BusNodes.Add(new CommNodeModel { NodeId = 5, DeviceName = "Digital I/O Module", Protocol = "Modbus RTU / IO", Status = "Online", Details = "COM Port Trigger & Strobe" });
            BusNodes.Add(new CommNodeModel { NodeId = 6, DeviceName = "BeeLight Controller", Protocol = "Serial RTU", Status = "Online", Details = "Channel 1-4 Strobe Controller" });

            dgrNodes.ItemsSource = BusNodes;
        }
    }
}

