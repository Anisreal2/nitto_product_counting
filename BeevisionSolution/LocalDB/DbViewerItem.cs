using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.LocalDB
{
    public class DbViewerItem
    {
        private int _countNG;
        private int _countOK;
        public string Date { get; set; }
        public int TotalNG
        {
            get { return _countNG; }
            set
            {
                _countNG = value;
                Total = _countOK + _countNG;
                RateOK = Total == 0 ? "0 %" : string.Format("{0} %", _countOK * 100 / Total);
                RateNG = Total == 0 ? "0 %" : string.Format("{0} %", _countNG * 100 / Total);
            }
        }
        public int TotalOK
        {
            get { return _countOK; }
            set
            {
                _countOK = value;
                Total = _countOK + _countNG;
                RateOK = Total == 0 ? "0 %" : string.Format("{0} %", _countOK * 100 / Total);
                RateNG = Total == 0 ? "0 %" : string.Format("{0} %", _countNG * 100 / Total);
            }
        }
        public int Total { get; set; }
        public string RateOK { get; set; }
        public string RateNG { get; set; }
        public HourData[] LstDataByHours { get; set; } = new HourData[24];
    }

    public class HourData
    {
        private int _countNG;
        private int _countOK;
        public int No { get; set; }//0->23 for 24h
        public string TimeShow { get; set; }
        public int TotalNG
        {
            get { return _countNG; }
            set
            {
                _countNG = value;
                Total = _countOK + _countNG;
                RateOK = Total == 0 ? "0 %" : string.Format("{0} %", _countOK * 100 / Total);
                RateNG = Total == 0 ? "0 %" : string.Format("{0} %", _countNG * 100 / Total);
            }
        }
        public int TotalOK
        {
            get { return _countOK; }
            set
            {
                _countOK = value;
                Total = _countOK + _countNG;
                RateOK = Total == 0 ? "0 %" : string.Format("{0} %", _countOK * 100 / Total);
                RateNG = Total == 0 ? "0 %" : string.Format("{0} %", _countNG * 100 / Total);
            }
        }
        public int Total { get; set; }
        public string RateOK { get; set; }
        public string RateNG { get; set; }

    }
}
