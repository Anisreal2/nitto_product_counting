using BeevisionSolution.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Models
{
    [Serializable]
    public class LayoutSetting
    {
        public int TotalItem { get; set; }
        public int ItemsPerLine { get; set; } // Số mục trên mỗi line
        public DirectionType DirectionType { get; set; } // Hướng đi 
                                                         // Tính số hàng và cột tự động
        public bool IsLoadPreviousData { get; set; } = false;
        [field:NonSerialized]
        public bool IsColumnBased =>
            DirectionType.ToString().StartsWith("Column");
        [field: NonSerialized]
        public int RowCount
        {
            get
            {
                if (ItemsPerLine <= 0) return 1;
                return IsColumnBased ? ItemsPerLine : (int)Math.Ceiling((double)TotalItem / ItemsPerLine);
            }
        }

        [field: NonSerialized]
        public int ColumnCount
        {
            get
            {
                if (ItemsPerLine <= 0) return 1;
                return IsColumnBased ? (int)Math.Ceiling((double)TotalItem / ItemsPerLine) : ItemsPerLine;
            }
        }

    }
    
}
