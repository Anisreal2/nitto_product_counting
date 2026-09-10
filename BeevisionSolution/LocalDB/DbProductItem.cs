using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.LocalDB
{
    public class DbProductItem
    {
        public string OperatorId { get; set; }
        public string ProductName { get; set; }
        public string ItemCode { get; set; }
        public string LotCode { get; set; }
        public double S11Value { get; set; }
        public double S12Value { get; set; }
        public double S21Value { get; set; }
        public double S22Value { get; set; }
        public double SDD11Value { get; set; }
        public double SDD22Value { get;set; }
        public DateTime CreatedTime { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
