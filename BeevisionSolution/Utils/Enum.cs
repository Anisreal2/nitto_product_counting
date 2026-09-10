using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Utils
{
    public enum DirectionType
    {
        // Row-wise snake
        Row_LeftToRight_OddRow,   // Hàng lẻ L→R, hàng chẵn R→L (truyền thống)
        Row_LeftToRight_AllRow,   // Hàng lẻ L→R, hàng chẵn L→R (không đảo)
        Row_RightToLeft_OddRow,   // Hàng lẻ R→L, hàng chẵn L→R (truyền thống)
        Row_RightToLeft_AllRow,   // Hàng lẻ R→L, hàng chẵn R→L (không đảo)

        // Column-wise snake
        Column_TopToBottom_OddCol,    // Cột lẻ Top→Bottom, cột chẵn Bottom→Top (truyền thống)
        Column_TopToBottom_AllCol,    // Cột lẻ Top→Bottom, cột chẵn Top→Bottom (không đảo)
        Column_BottomToTop_OddCol,    // Cột lẻ Bottom→Top, cột chẵn Top→Bottom (truyền thống)
        Column_BottomToTop_AllCol,     // Cột lẻ Bottom→Top, cột chẵn Bottom→Top (không đảo) 

        Column_ZigZag_RightToLeft_TopDownOdd,     // 11 10 1 / 12 9 2 / ...
        Column_ZigZag_RightToLeft_BottomUpOdd // ngược lại: bắt đầu dưới-trái lên
    }
    public enum InspectionResult
    {
        None,
        OK,
        NG
    }
    public enum UserRole
    {
        Operator,
        Engineer,
        Master = 999,
    }
    /// <summary>
    /// Application status enumeration
    /// </summary>
    public enum ApplicationStatus
    {
        Manual,
        AutoRunning,
        RunningAuto,
        CameraLive,
        Stopped
    }
    public enum Lang
    {
        en,
        vn
    }

    /// <summary>
    /// PLC connection status
    /// </summary>
    public enum PLCStatus
    {
        Online,
        Offline
    }

    /// <summary>
    /// Camera identifier
    /// </summary>
    public enum CameraId
    {
        CamA1,
        CamA2,
        CamB1,
        CamB2
    }

    /// <summary>
    /// Brush size for mask editor
    /// </summary>
    public enum BrushSize
    {
        Small,
        Medium,
        Large
    }

    /// <summary>
    /// Brush color for mask editor
    /// </summary>
    public enum BrushColor
    {
        White,
        Red,
        Black
    }
}
