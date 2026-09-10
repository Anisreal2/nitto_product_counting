using BeeLib.Math;
using BeevisionSolution.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using static BeevisionSolution.Utils.Common;

namespace BeevisionSolution.Controller
{
    class TrainedPointManager
    {
        private static List<TrainedPoint> lstTrainedPoints = null;
        private static List<TrainedMovingPoint> lstTrainedMovingPoints = null;
        const uint CstTrainedNPointID = 10000;

        public static void Cleanup()
        {
            if (null != lstTrainedPoints) lstTrainedPoints.Clear();
            if (null != _visionMasterPoses) _visionMasterPoses.Clear();
        }

        public static List<TrainedPoint> GetAllTrainedPoints(bool forceReload = false)
        {
            if (forceReload || (null == lstTrainedPoints) || (lstTrainedPoints.Count < 1))
                lstTrainedPoints = GetObjectFromFile<List<TrainedPoint>>(TrainedPointsConfigFile);

            return lstTrainedPoints;
        }
        public static List<TrainedMovingPoint> GetAllTrainedMovingPoints(bool forceReload = false)
        {
            if (forceReload || (null == lstTrainedMovingPoints) || (lstTrainedMovingPoints.Count < 1))
                lstTrainedMovingPoints = GetObjectFromFile<List<TrainedMovingPoint>>(TrainedPointsTTMConfigFile);

            return lstTrainedMovingPoints;
        }

        public static TrainedPoint GetTrainedPoint(String strName, UInt32 CalibId) => GetAllTrainedPoints().FirstOrDefault(pnt => (pnt.Name.Equals(strName) && pnt.CalibId.Equals(CalibId)));
        public static TrainedMovingPoint GetTrainedMovingPoint(string strName, UInt32 calibID) => GetAllTrainedMovingPoints().FirstOrDefault(pnt => (pnt.Name.Equals(strName) && pnt.CalibId.Equals(calibID)));
        public static TrainedPoint GetTrainedNPoint(string strName) => GetAllTrainedPoints().FirstOrDefault(pnt => (pnt.Name.Equals(strName) && pnt.CalibId.Equals(CstTrainedNPointID)));
        public static bool AddTrainedPoint(TrainedPoint trained)
        {
            var lst = GetAllTrainedPoints();
            var existed = false;

            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i].Name.Equals(trained.Name) && lst[i].CalibId.Equals(trained.CalibId))
                {
                    lst[i] = trained;
                    existed = true;
                    break;
                }
            }
            if (!existed) lst.Add(trained);

            return SaveToFile();
        }
        public static bool AddTrainedMovingPoint(TrainedMovingPoint trained)
        {
            var lst = GetAllTrainedMovingPoints();
            var existed = false;

            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i].Name.Equals(trained.Name) && lst[i].CalibId.Equals(trained.CalibId))
                {
                    lst[i] = trained;
                    existed = true;
                    break;
                }
            }
            if (!existed) lst.Add(trained);

            return SaveTTMToFile();
        }
        private static Dictionary<string,Pose> _visionMasterPoses = new Dictionary<string,Pose>();
        public static void LoadVisionMasterPoses()
        {
            try
            {
                if (File.Exists(Common.VisionMasterPosesConfigFile))
                {
                    _visionMasterPoses = GetObjectFromFile<Dictionary<string, Pose>>(Common.VisionMasterPosesConfigFile)
                        ?? new Dictionary<string, Pose>();
                }
                else
                {
                    _visionMasterPoses = new Dictionary<string, Pose>();
                }
            }
            catch (Exception ex)
            {
                Bug($"Error loading Vision Master Poses: {ex.Message}");
                _visionMasterPoses = new Dictionary<string, Pose>();
            }
        }
        public static bool SaveVisionMasterPose(string strName, Pose vMasterPose)
        {
            try
            {
                if (vMasterPose == null)
                {
                    Bug("SaveVisionMasterPose: vMasterPose is null");
                    return false;
                }

                if (_visionMasterPoses == null)
                {
                    LoadVisionMasterPoses();
                }
                _visionMasterPoses[strName] = vMasterPose;

                bool saveResult = SaveObjectToFile(_visionMasterPoses, Common.VisionMasterPosesConfigFile);

                if (saveResult)
                {
                    Info($"Saved Vision Master Pose for {strName}: X={vMasterPose.X:F3}, Y={vMasterPose.Y:F3}, Th={vMasterPose.Th:F3}");
                }

                return saveResult;
            }
            catch (Exception ex)
            {
                Bug($"Error saving Vision Master Pose: {ex.Message}");
                return false;
            }
        }
        public static Pose GetVisionMasterPose(string strName)
        {
            try
            {
                    LoadVisionMasterPoses();

                if (_visionMasterPoses.ContainsKey(strName))
                {
                    return _visionMasterPoses[strName];
                }
                return null;
            }
            catch (Exception ex)
            {
                Bug($"Error getting Vision Master Pose: {ex.Message}");
                return null;
            }
        }



        public static bool SaveToFile() => SaveObjectToFile(GetAllTrainedPoints(), TrainedPointsConfigFile);
        public static bool SaveTTMToFile() => SaveObjectToFile(GetAllTrainedMovingPoints(), TrainedPointsTTMConfigFile);
    }
}
