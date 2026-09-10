using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace BeevisionSolution.Utils
{
    /// <summary>
    /// Sets Windows local system time (requires sufficient privileges, typically Administrator).
    /// </summary>
    public static class WindowsSystemTimeHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetLocalTime(ref SYSTEMTIME lpSystemTime);

        /// <summary>
        /// Builds a <see cref="DateTime"/> from six PLC WORDs (year..second). Handles common spec edge cases (hour 24, minute/second 60).
        /// </summary>
        public static bool TryBuildLocalDateTimeFromPlcWords(ushort year, ushort month, ushort day, ushort hour, ushort minute, ushort second, out DateTime localTime)
        {
            localTime = default;
            if (year < 1970 || year > 2199) return false;
            if (month < 1 || month > 12) return false;
            if (day < 1 || day > 31) return false;

            int h = hour;
            if (h == 24) h = 0;
            if (h < 0 || h > 23) return false;

            int min = minute;
            if (min > 60) return false;
            if (min == 60) min = 0;

            int sec = second;
            if (sec > 60) return false;
            if (sec == 60) sec = 0;

            try
            {
                localTime = new DateTime(year, month, day, h, min, sec, DateTimeKind.Local);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        public static void SetWindowsLocalTimeOrThrow(DateTime localTime)
        {
            var st = new SYSTEMTIME
            {
                wYear = (ushort)localTime.Year,
                wMonth = (ushort)localTime.Month,
                wDay = (ushort)localTime.Day,
                wDayOfWeek = (ushort)localTime.DayOfWeek,
                wHour = (ushort)localTime.Hour,
                wMinute = (ushort)localTime.Minute,
                wSecond = (ushort)localTime.Second,
                wMilliseconds = (ushort)localTime.Millisecond
            };

            if (!SetLocalTime(ref st))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
