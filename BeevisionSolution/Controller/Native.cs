using BeeLib.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Controller
{
    public static class Native
    {
        //[SuppressUnmanagedCodeSecurityAttribute()]
        //[DllImport("BeeLibrary.dll", EntryPoint = "HandEye")]
        //public static extern IntPtr HandEye(CppPose[] srcPoses, CppPose[] dstPoses, out int nRows);

        //[SuppressUnmanagedCodeSecurityAttribute()]
        //[MethodImpl(MethodImplOptions.InternalCall)]
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("BeeLibrary.dll", EntryPoint = "CheckLicense")]
        public static extern int CheckLicense();
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("bcore.dll", EntryPoint = "HandEye")]
        public static extern IntPtr HandEye(CppPose[] srcPoses, CppPose[] dstPoses, out int nRows);

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("bcore.dll", EntryPoint = "HandEyeNoIntrinsic")]
        public static extern IntPtr HandEyeNoIntrinsic(CppPose[] srcPoses, CppPose[] dstPoses, out int nRows);

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("bcore.dll", EntryPoint = "HandEye_Old")]
        public static extern IntPtr HandEye_Old(CppPose[] srcPoses, CppPose[] dstPoses, out int nRows);

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("bcore.dll", EntryPoint = "Perspective2D")]
        public static extern IntPtr Perspective2D(CppPoint[] src, CppPoint[] dst, out int nRows);

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("bcore.dll", EntryPoint = "Perspective2D")]
        public static extern IntPtr Perspective(CppPoint[] src, CppPoint[] dst, out int nRows);

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("bcore.dll", EntryPoint = "Perspective3D")]
        public static extern IntPtr Perspective3D(CppPoint[] src, CppPoint[] dst, out int nRows);
        /*
        HANDEYE_API int CheckLicense(void);
	    HANDEYE_API void* CheckLibLicense(void);
	    HANDEYE_API void* CheckLibLicenseX(int& dwSize);
         */
    }
}
