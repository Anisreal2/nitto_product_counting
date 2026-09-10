using BeeLib.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using static BeevisionSolution.Utils.Common;

namespace BeevisionSolution.Controller
{
    class CalibManager
    {
        private static List<CalibrateResult> lstCalibResults = new List<CalibrateResult> { };
        private static List<CalibResult> lstCalibCamMoving = new List<CalibResult> { };

        public static void Cleanup()
        {
            if (null != lstCalibResults) lstCalibResults.Clear();
        }

        public static List<CalibrateResult> GetAllCalibs(bool forceReload = false)
        {
            if (forceReload || (null == lstCalibResults) || (lstCalibResults.Count < 1))
                lstCalibResults = GetObjectFromFile<List<CalibrateResult>>(CalibsConfigFile, JsonPrivate);

            return lstCalibResults;
        }
        public static List<CalibResult> GetAllCalibCamMoving(bool forceReload = false)
        {
            if (forceReload || (null == lstCalibCamMoving) || (lstCalibCamMoving.Count < 1))
                lstCalibCamMoving = GetObjectFromFile<List<CalibResult>>(CalibsCamMovingConfigFile, JsonPrivate);
            return lstCalibCamMoving;
        }
        public static CalibrateResult GetCalibrateById(UInt32 CalibId)
        {
            if (CalibId == 0) return null;
            return GetAllCalibs().FirstOrDefault(calib => calib.CalibId.Equals(CalibId));
        }
        public static CalibResult GetCalibMovingResultById(uint calibID)
        {
            return GetAllCalibCamMoving().FirstOrDefault(calib => calib.CalibId == calibID);
        }
        public static bool AddCalib(CalibrateResult calib)
        {
            var lst = GetAllCalibs();
            var existed = false;

            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i].CalibId.Equals(calib.CalibId))
                {
                    lst[i] = calib;
                    existed = true;
                    break;
                }
            }
            if (!existed) lst.Add(calib);

            return SaveToFile();
        }
        public static bool AddCalibCamMoving(CalibResult calib)
        {
            var lst = GetAllCalibCamMoving();
            var existed = false;
            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i].CalibId.Equals(calib.CalibId))
                {
                    lst[i] = calib;
                    existed = true;
                    break;
                }
            }
            if (!existed) lst.Add(calib);

            return SaveCamMovingToFile();
        }
        public static bool RemoveCalibById(UInt32 CalibId)
        {
            var lst = GetAllCalibs();
            var calib = lst.FirstOrDefault(c => c.CalibId.Equals(CalibId));
            if (null != calib)
            {
                lst.Remove(calib);
                return SaveToFile();
            }
            return false;
        }

        public static AlignMode GetAlignMode(string modeStr)
        {
            if (string.IsNullOrWhiteSpace(modeStr))
                return AlignMode.Off;

            if (int.TryParse(modeStr, out int modeInt))
            {
                return (AlignMode)modeInt;
            }

            if (Enum.TryParse<AlignMode>(modeStr, true, out AlignMode result))
            {
                return result;
            }

            return AlignMode.Off;
        }

        public static bool SaveToFile() => SaveObjectToFile(GetAllCalibs(), CalibsConfigFile);
        public static bool SaveCamMovingToFile() => SaveObjectToFile(GetAllCalibCamMoving(), CalibsCamMovingConfigFile);
    }
}
