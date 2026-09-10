using BeeLib.Math;
using BeevisionSolution.Models;
using BeevisionSolution.Utils;
using BeevisionSolution.Views;
using Microsoft.DwayneNeed.Win32.Gdi32;
using netDxf.Tables;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace BeevisionSolution.Controller
{
    internal static class BeeAlgo
    {
        public static Pose DoAlign(Pose vPose, Pose rPose, AlignJob job, AlignMode alignMode = AlignMode.Off)
        {
            var calib = CalibManager.GetCalibrateById(job.CalibId);
            var trained = TrainedPointManager.GetTrainedPoint(job.Name, job.CalibId);

            var pose = calib.DoMath(trained, vPose, alignMode);
            if (pose != null)
            {
                pose.Th = Common.Compensation(pose.Th);
            }

            return pose;
        }

        public static Pose DoPreAlign(Pose vPose, Pose rPose, AlignJob job, AlignMode alignMode = AlignMode.Off)
        {
            var calib = CalibManager.GetCalibrateById(job.CalibId);
            var trained = TrainedPointManager.GetTrainedPoint(job.Name, job.CalibId);

            var pose = calib.DoMathPreAlign(trained, vPose, alignMode);
            if (pose != null)
            {
                pose.Th = Common.Compensation(pose.Th);
            }

            return pose;
        }

        public static bool TrainMaster(Pose vPose, Pose rPose, AlignJob job)
        {
            var calib = CalibManager.GetCalibrateById(job.CalibId);
            var trained = calib.Train(vPose, rPose, job.Name);
            return TrainedPointManager.AddTrainedPoint(trained);
        }
        public static bool TrainMasterCamMoving(Pose rbPickPose, Pose rbCurPose, Pose vPose, AlignJob job)
        {
            bool result = false;
            var calibMovingResult = CalibManager.GetCalibMovingResultById(job.CalibId);

            if (TrainTTM(calibMovingResult, rbCurPose, vPose, job.Name))
            {
                result = TrainTTR(rbPickPose, job.Name, job.CalibId);
            }
            return result;
        }
        public static bool CalHEM(List<Pose> rPoses, List<Pose> vPoses, uint CalibId, uint CameraId)
        {
            var calibResult = MathWorker.CalHEM(rPoses, vPoses, CalibId, CameraId);
            if (calibResult != null)
            {
                return CalibManager.AddCalibCamMoving(calibResult);
            }

            return false;
        }
        private static bool TrainTTM(CalibResult calibResult, Pose rPoses, Pose vPoses, string strName)
        {
            var result = MathWorker.TrainTTM(calibResult, rPoses, vPoses, strName);
            if (result != null)
            {
                return TrainedPointManager.AddTrainedMovingPoint(result);
            }

            return false;
        }

        public static bool TrainTTR(Pose rPoses, string strAlignJob, uint CalibId)
        {
            var calib = CalibManager.GetCalibMovingResultById(CalibId);
            TrainedMovingPoint p = TrainedPointManager.GetTrainedMovingPoint(strAlignJob, CalibId);
            var result = MathWorker.TrainTTR(rPoses, calib, p);
            return TrainedPointManager.AddTrainedMovingPoint(result);
        }

        public static Pose RunXTM(Pose rPoses, Pose vPoses, string strAlignJob, uint CalibId)
        {
            var trained = TrainedPointManager.GetTrainedMovingPoint(strAlignJob, CalibId);
            var calib = CalibManager.GetCalibMovingResultById(CalibId);

            var poseResult = MathWorker.RunXTM(rPoses, vPoses, trained, calib);
            if (poseResult != null)
            {
                poseResult.Th = Common.Compensation(poseResult.Th);
            }

            return poseResult;
        }

        public static bool TrainNPoints(Pose vMasterPose, string strName)
        {
            var result = MathWorker.TrainNPoints(vMasterPose, strName);
            Common.Info($"Data TrainNpoint : HomeFromTarget = {result.HomeFromTarget}");
            return TrainedPointManager.AddTrainedPoint(result);
        }

        public static Pose RunMathNPoints(TrainedPoint trainedPoint, Pose vPose, Pose offsetPose)
        {
            return MathWorker.RunMathNPoints(trainedPoint, vPose, offsetPose);
        }
        public static Pose Multimark(Pose VPoses1, Pose VPoses2, uint CalibId1, uint CalibId2, TrainedPoint trained1, TrainedPoint trained2)
        {
            var calib1 = CalibManager.GetCalibrateById(CalibId1);
            var calib2 = CalibManager.GetCalibrateById(CalibId2);
            //var trained1 = TrainedPointManager.GetTrainedPoint(strAlignJob1, CalibId1);
            //var trained2 = TrainedPointManager.GetTrainedPoint(strAlignJob2, CalibId2);
            return MathWorker.Multimark(VPoses1, VPoses2, calib1, calib2, trained1, trained2, "", "");
        }
        public static Pose Multimark_NOPartmark(Pose VPoses1, Pose VPoses2, uint CalibId1, uint CalibId2, TrainedPoint trained1, TrainedPoint trained2)
        {
            if (VPoses1 == null || VPoses2 == null)
            {
                Common.Bug("Multimark_NOPartmark: input pose is null - pose1: {0}, pose2: {1}", VPoses1 != null, VPoses2 != null);
                return null;
            }

            var calib1 = CalibManager.GetCalibrateById(CalibId1);
            var calib2 = CalibManager.GetCalibrateById(CalibId2);
            if (calib1 == null || calib2 == null)
            {
                Common.Bug("Multimark_NOPartmark: calibration is null - calib1: {0}, calib2: {1}", calib1 != null, calib2 != null);
                return null;
            }
            if (trained1 == null || trained2 == null)
            {
                Common.Bug("Multimark_NOPartmark: trained point is null - trained1: {0}, trained2: {1}", trained1 != null, trained2 != null);
                return null;
            }

            Pose p1, p2, p1train, p2train, P1Trainnew, P2Trainnew, p1runtime, p2runtime;
            // visiontrain image
            p1train = calib1.Home2DFromImage2D.Inverse().Compose(trained1.HomeFromTarget).ToPose();
            p2train = calib2.Home2DFromImage2D.Inverse().Compose(trained2.HomeFromTarget).ToPose();
            // convert to no partmark
            P1Trainnew = Return_goc_L(p1train, p2train, calib1, calib2);
            P2Trainnew = Return_goc_R(p1train, p2train, calib1, calib2);
            // runtime no partmark
            p1 = Return_goc_L(VPoses1, VPoses2, calib1, calib2);
            p2 = Return_goc_R(VPoses1, VPoses2, calib1, calib2);
            // convert to partmark runtime
            p1runtime = p1;
            p2runtime = p2;
            p1runtime.Th = p1train.Th + p1.Th - P1Trainnew.Th;
            p2runtime.Th = p2train.Th + p2.Th - P2Trainnew.Th;
            //var trained1 = TrainedPointManager.GetTrainedPoint(strAlignJob1, CalibId1);
            //var trained2 = TrainedPointManager.GetTrainedPoint(strAlignJob2, CalibId2);


            return MathWorker.Multimark(p1, p2, calib1, calib2, trained1, trained2, "", "");
        }
        private static Pose Return_goc_L(Pose L, Pose R, CalibrateResult calibL, CalibrateResult calibR)
        {
            // R trong L 
            LinearTransform ImgaeLfrRtarget = new LinearTransform();
            ImgaeLfrRtarget = calibL.Home2DFromImage2D.Inverse().Compose(calibR.Home2DFromImage2D.Compose(R));
            double goc_train = Math.Atan2(ImgaeLfrRtarget.ToPose().Y - L.Y, ImgaeLfrRtarget.ToPose().X - L.X);
            double angleDeg_goctrain = goc_train * 180 / Math.PI;
            // 

            var pose0 = new Pose(L.X, L.Y, angleDeg_goctrain);

            return pose0;
        }
        private static Pose Return_goc_R(Pose L, Pose R, CalibrateResult calibL, CalibrateResult calibR)
        {
            LinearTransform ImgaeLfrRtarget = new LinearTransform();
            ImgaeLfrRtarget = calibL.Home2DFromImage2D.Inverse().Compose(calibR.Home2DFromImage2D.Compose(R));
            double goc_train = Math.Atan2(ImgaeLfrRtarget.ToPose().Y - L.Y, ImgaeLfrRtarget.ToPose().X - L.X);
            double angleDeg_goctrain = goc_train * 180 / Math.PI;
            var pose0 = new Pose(R.X, R.Y, angleDeg_goctrain);

            return pose0;
        }
        public static bool TrainNPointsWithAngle(Pose vMasterPose, string strName)
        {
            try
            {
                if (vMasterPose == null)
                {
                    Common.Bug("TrainNPointsWithAngle: vMasterPose is null");
                    return false;
                }
                var result = MathWorker.TrainNPoints(vMasterPose, strName);

                TrainedPointManager.SaveVisionMasterPose(strName, vMasterPose);
                bool saveResult = TrainedPointManager.AddTrainedPoint(result);

                if (saveResult)
                {
                    Common.Info($"TrainNPointsWithAngle success: {strName}, VisionPose X={vMasterPose.X:F3}, Y={vMasterPose.Y:F3}, Th={vMasterPose.Th:F3}");
                }

                return saveResult;
            }
            catch (Exception ex)
            {
                Common.Bug($"Error in TrainNPointsWithAngle: {ex.Message}");
                return false;
            }
        }

        public static Pose RunMathNPointsNew(TrainedPoint trainedPoint, Pose vCurrentPose, Pose offsetPose, bool reverseCalculation = false)
        {
            try
            {
                if (trainedPoint == null)
                {
                    Common.Bug("RunMathNPointsNew: trainedPoint is null");
                    return null;
                }

                if (vCurrentPose == null)
                {
                    Common.Bug("RunMathNPointsNew: vCurrentPose is null");
                    return null;
                }

                Pose vMasterPose = TrainedPointManager.GetVisionMasterPose(trainedPoint.Name);

                if (vMasterPose == null)
                {
                    Common.Bug($"RunMathNPointsNew: Cannot find Vision Master Pose for {trainedPoint.Name}");
                    //return MathWorker.RunMathNPoints(trainedPoint, vCurrentPose, offsetPose);
                    return new Pose(0, 0, 0);
                }

                //  Trained master - Pose current
                //Pose diffPose = new Pose(
                //    vMasterPose.X - vCurrentPose.X,
                //    vMasterPose.Y - vCurrentPose.Y,
                //    vMasterPose.Th - vCurrentPose.Th
                //);
                Pose diffPose;
                if (reverseCalculation)
                {
                    diffPose = new Pose(
                        vCurrentPose.X - vMasterPose.X,
                        vCurrentPose.Y - vMasterPose.Y,
                        vCurrentPose.Th - vMasterPose.Th
                    );
                    Common.Info($"RunMathNPointsNew (REVERSE): Current({vCurrentPose.X:F3}, {vCurrentPose.Y:F3}, {vCurrentPose.Th:F3}) - Master({vMasterPose.X:F3}, {vMasterPose.Y:F3}, {vMasterPose.Th:F3})");
                }
                else
                {
                    diffPose = new Pose(
                        vMasterPose.X - vCurrentPose.X,
                        vMasterPose.Y - vCurrentPose.Y,
                        vMasterPose.Th - vCurrentPose.Th
                    );
                    Common.Info($"RunMathNPointsNew: Master({vMasterPose.X:F3}, {vMasterPose.Y:F3}, {vMasterPose.Th:F3}) - Current({vCurrentPose.X:F3}, {vCurrentPose.Y:F3}, {vCurrentPose.Th:F3})");
                }

                if (offsetPose != null)
                {
                    diffPose.X += offsetPose.X;
                    diffPose.Y += offsetPose.Y;
                    diffPose.Th += offsetPose.Th;
                }

                diffPose.Th = Common.Compensation(diffPose.Th);

                Common.Info($"RunMathNPointsNew: Result({diffPose.X:F3}, {diffPose.Y:F3}, {diffPose.Th:F3})");
                return diffPose;
            }
            catch (Exception ex)
            {
                Common.Bug($"Error in RunMathNPointsNew: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Tính khoảng cách thực (mm) giữa 2 điểm mark từ 2 camera khác nhau.
        /// Quy đổi cả 2 pose về cùng hệ tọa độ World thông qua calibration.
        /// </summary>
        public static double CalculateDistanceBetweenMarks(Pose vPose1, uint calibId1,Pose vPose2, uint calibId2)
        {
            var calib1 = CalibManager.GetCalibrateById(calibId1);
            var calib2 = CalibManager.GetCalibrateById(calibId2);

            if (calib1 == null || calib2 == null) return -1;

            // Chuyển từ pixel → World (mm) qua ma trận Hand-Eye
            var worldPose1 = calib1.Home2DFromImage2D.Compose(vPose1).ToPose();
            var worldPose2 = calib2.Home2DFromImage2D.Compose(vPose2).ToPose();

            double dx = worldPose2.X - worldPose1.X;
            double dy = worldPose2.Y - worldPose1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

    }
}
