using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BeeLightModule.Models
{
    public enum ThreeDPC_ControllerProtocol
    {
        Modbus,
        Custom,
    };

    public class HZControllerApi
    {
        const string LTSControler_DLL = "HZController.dll";

        [DllImport(LTSControler_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ThreeDPC_LTSController_SetProtocolType(ThreeDPC_ControllerProtocol type);

        [DllImport(LTSControler_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ThreeDPC_LTSController_SetTimeOut(long nTime);

        [DllImport(LTSControler_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ThreeDPC_LTSController_GetTimeOut();

        [DllImport(LTSControler_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ThreeDPC_LTSController_SetTimeWait(long nTime);

        [DllImport(LTSControler_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ThreeDPC_LTSController_GetTimeWait();

        /// <summary>
        /// 初始化串口链接
        /// </summary>
        /// <param name="SerialPortName">串口号</param>
        /// <param name="nBaudRate">波特率</param>
        /// <returns>0 成功，-1 失败</returns>
        [DllImport(LTSControler_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ThreeDPC_LTSController_InitSerialPort(String SerialPortName, int nBaudRate);

        /// <summary>
        /// 释放串口连接
        /// </summary>
        /// <returns>0 成功，-1 失败</returns>
        [DllImport(LTSControler_DLL)]
        public static extern int ThreeDPC_LTSController_ReleaseSerialPort();

        /// <summary>
        /// 网口通讯初始化
        /// </summary>
        /// <param name="IP">启动服务IP地址</param>
        /// <param name="nPort">启动服务端口号</param>
        /// <returns>0 成功，-1 失败</returns>
        [DllImport(LTSControler_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ThreeDPC_CustomLTSController_InitNetwork(String IP, int nPort);

        /// <summary>
        /// 释放网口连接
        /// </summary>
        /// <returns>0 成功，-1 失败</returns>
        [DllImport(LTSControler_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ThreeDPC_CustomLTSController_ReleaseNetwork();

        /// <summary>
        /// 设置单通道状态
        /// </summary>
        /// <param name="channelIdx">通道号</param>
        /// <param name="nState">状态（0x00 关闭，0x01 打开）</param>
        /// <returns>0 成功，-1 失败</returns>
        [DllImport(LTSControler_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ThreeDPC_CustomLTSController_SetChannelState(int channelIdx, int nState);

        /// <summary>
        /// 设置单通道亮度
        /// </summary>
        /// <param name="channelIdx">通道号</param>
        /// <param name="vlaue">亮度值（范围0-255）</param>
        /// <returns>0 成功，-1 失败</returns>
        [DllImport(LTSControler_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ThreeDPC_CustomLTSController_SetIntensity(int channelIdx, int vlaue);

        /// <summary>
        /// 读取单通道亮度
        /// </summary>
        /// <param name="channelIdx">通道号</param>
        /// <returns>0 成功，-1 失败</returns>
        [DllImport(LTSControler_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ThreeDPC_CustomLTSController_ReadIntensity(int channelIdx);

        /// <summary>
        ///  读取控制器工作模式
        /// </summary>
        /// <returns>0x00-常亮，0x01-频闪，0x02-外部触发，0x03-内部触发，0x04-软件触发,-1 失败</returns>
        [DllImport(LTSControler_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ThreeDPC_CustomLTSController_ReadModE();

        /// <summary>
        /// 设置控制器工作模式
        /// </summary>
        /// <param name="nModE">0x00-常亮，0x01-频闪，0x02-外部触发，0x03-内部触发，0x04-软件触发</param>
        /// <returns>0 成功，-1 失败</returns>
        [DllImport(LTSControler_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ThreeDPC_CustomLTSController_SetModE(int nModE);
    }
}
