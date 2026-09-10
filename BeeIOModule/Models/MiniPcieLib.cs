using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace BeeIOModule.Models
{
    class MiniPcieLib
    {
        #region
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetPcieData(int CardID, int address, ref int ret);


        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetPcieData(int CardID, int address, int data);
        #endregion

        #region 板卡基础类函数
        /// <summary>
        /// 获取板卡数量
        /// </summary>
        /// <param name="cardNum">板卡数量</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_GetCardNum(ref int cardNum);

        /// <summary>
        /// 初始化板卡
        /// </summary>
        /// <param name="cardId">卡号</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_InitMiniPcie(int cardId);

        /// <summary>
        /// 复位板卡
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_ResetCard(int cardid);

        /// <summary>
        /// 获取板卡版本信息
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="Version">版本号</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetVersion(int cardid,ref int Version);

        /// <summary>
        /// 从文件加载参数
        /// </summary>
        /// <param name="filename">文件路径，需绝对路径</param>
        /// <returns>异常参数信息： -abcde  a：（卡号+1） bc：通道号 ed：错误类型 4：锁存滤波 5：锁存距离 6：触发距离 7：脉宽 8：偏移 9：绑定   </returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_LoadParamFromFile(string filename);


        /// <summary>
        /// 设置当前板卡为GPIO模式
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_SetOpModeGPIO(int cardid);



        /// <summary>
        /// 设置当前板卡为混合模式
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_SetOpModeBlend(int cardid);


        /// <summary>
        /// 设置当前模式为触发模式
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_SetOpModePulse(int cardid);

        /// <summary>
        /// 获取当前模式
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="OpMode">模式：0-GPIO 1-触发 3-混合模式</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetOpMode(int cardid, ref int OpMode);

        /// <summary>
        /// 设置gpio模式下输出状态
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="GPIOOutput">GPIO下输出状态，一个bit对应一个通道，如果是blend，该函数控制端子台的OUT09~OUT16 一个bit对应一个通道</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_SetGPIOOutput(int cardid, int GPIOOutput);

        /// <summary>
        /// 获取GPIO模式下的输出状态
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="GPIOOutput">GPIO下的输出状态，一个bit对应一个通道，如果是blend，该函数控制端子台的OUT09~OUT16 一个bit对应一个通道</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetGPIOOutput(int cardid, ref int GPIOOutput);

        /// <summary>
        /// 获取输入状态
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="GPIOInput">输入状态，一个bit对应一个通道，如果是blend，该函数读取端子台的IN09~IN12 一个bit对应一个通道</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetGPIOInput(int cardid, ref int GPIOInput);

        #endregion


        #region 编码器类函数
        /// <summary>
        /// 获取E2I12O16编码器数据
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="EncChn">编码器通道，暂时只有0有效</param>
        /// <param name="EncData">编码器数据</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetEncoderData(int cardid, int EncChn, ref long EncData);

        /// <summary>
        /// 开启虚拟编码器，编码器速度为3125000p/s
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="EncChn">编码器通道，暂时只有0有效</param>
        /// <param name="Enable">开启和关闭</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_EnableVisualEncoder(int cardid, int EncChn, int Enable);

        /// <summary>
        /// 设置当前的编码器数据
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="EncChn">编码器通道，暂时只有0有效</param>
        /// <param name="EncData">设置值</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_SetEncoderData(int cardid, int EncChn, long EncData);


        /// <summary>
        /// 获取编码器速度（预留，目前不可用）
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="EncNo">编码器通道</param>
        /// <param name="EncVel">编码器速度，单位脉冲/s</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetEncVel(int cardid, int EncNo, ref int EncVel);

        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetEncBindingSts(int cardid, int LtcChn, ref int EncChn);

        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_SetEncBindingSts(int cardid, int LtcChn, int EncChn);
        #endregion


        #region 保护类函数 
        /// <summary>
        /// 设置锁存的最小距离
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="SafeDistance">安全距离</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_SetLTCSafeDistance(int cardid, uint SafeDistance);

        /// <summary>
        /// 获取锁存的最小距离
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="SafeDistance">获取到的安全距离</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetLTCSafeDistance(int cardid, ref uint SafeDistance);

        /// <summary>
        /// 设置锁存滤波时间
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="FilterTime">滤波时间，单位10ns</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_SetLTCFilterTime(int cardid, uint FilterTime);

        /// <summary>
        /// 获取到的锁存滤波时间
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="FilterTime">滤波时间，单位10ns</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetLTCFilterTime(int cardid, ref uint FilterTime);


        /// <summary>
        /// 设置触发输出的最小间隔
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="SafeDistance">最小触发间隔</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_SetTrigSafeDistance(int cardid, uint SafeDistance);

        /// <summary>
        /// 获取当前设置的触发最小间隔
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="SafeDistance">获取到的触发间隔</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetTrigSafeDistance(int cardid, ref uint SafeDistance);
        #endregion


        #region 触发类函数
        /// <summary>
        /// 获取触发通道的触发次数
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="TrigNo">触发通道号</param>
        /// <param name="Count">获取到的触发次数</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetTrigCnt(int cardid, int TrigNo, ref int Count);

        /// <summary>
        /// 获取踢料缓存区的点位数量
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="TrigNo">触发通道号</param>
        /// <param name="Count">获取到的缓存数量</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetClassifyFifoCnt(int cardid, int TrigNo, ref int Count);

        /// <summary>
        /// 获取相机缓存区的点位数量
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="TrigNo">触发通道号</param>
        /// <param name="Count">获取到的缓存数量</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetCameraFifoCnt(int cardid, int TrigNo, ref int Count);

        /// <summary>
        /// 设置相机偏移值
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="TrigNo">触发通道号</param>
        /// <param name="Offset">相机偏移距离</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_SetCameraOffset(int cardid, int TrigNo, ulong Offset);

        /// <summary>
        /// 获取相机偏移值
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="TrigNo">触发通道号</param>
        /// <param name="Offset">获取到的偏移值</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetCameraOffset(int cardid, int TrigNo, ref ulong Offset);

        /// <summary>
        /// 设置触发通道类型以及绑定关系
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="TrigNo">要设定的触发通道号</param>
        /// <param name="LTCNo">绑定的锁存通道</param>
        /// <param name="Type">输出类型：0-相机 1-踢料</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_TrigBindingLTC(int cardid, int TrigNo, int LTCNo, int Type);

        /// <summary>
        /// 设置触发输出脉宽
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="TrigNo">要设定的触发通道号</param>
        /// <param name="Width">设定的脉宽，单位10ns</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_SetTrigPulseWidth(int cardid, int TrigNo, uint Width);

        /// <summary>
        /// 获取触发输出脉宽
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="TrigNo">要读取的通道号</param>
        /// <param name="Width">触发脉宽，单位10ns</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetTrigPulseWidth(int cardid, int TrigNo, ref uint Width);

        /// <summary>
        /// 设置踢料通道的踢料触发坐标，自动放入队列最底部
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="TrigNo">触发通道号</param>
        /// <param name="ClassifyPosition">踢料坐标</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_SetClassifyPosition(int cardid, int TrigNo, long ClassifyPosition);


        /// <summary>
        /// 获取当前触发瞬间位置
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="TrigNo">通道号</param>
        /// <param name="LatestTrigPos">获取到的位置</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetLatestTrigPos(int cardid, int TrigNo, ref long LatestTrigPos);

        /// <summary>
        /// 重置触发输出计数
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="TrigNo">触发通道号</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_ResetTrigCnt(int cardid, int TrigNo);


        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_ResetClassifyFifo(int cardid, int TrigNo);

        /// <summary>
        /// 获取触发输出的绑定关系
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="TrigNo">触发通道号</param>
        /// <param name="LTCNo">绑定的锁存通道</param>
        /// <param name="Type">当前输出类型</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetBindingSts(int cardid, int TrigNo, ref int LTCNo, ref int Type);


        /// <summary>
        /// 设置触发输出为LineMode，并且设置interval
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="LineTriggerEnableMask">触发通道使能码，bit0对应通道0，bit1对应通道1，以此类推</param>
        /// <param name="TrigStartPosArrayAdd">触发起点数组，长度固定16长度，每个索引对应通道的间隔值</param>
        /// <param name="TrigIntervalArrayAdd">触发间隔数组，长度固定16长度，每个索引对应通道的间隔值</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_SetLineTirgParam(int cardid, uint LineTriggerEnableMask, ref long TrigStartPosArrayAdd, ref uint TrigIntervalArrayAdd);


        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetLineTirgParam(int cardid, ref uint LineTriggerEnableMask, ref long TrigStartPosArrayAdd, ref uint TrigIntervalArrayAdd);


        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_SetLineTirgParamSingle(int cardid, uint LineTriggerChn, int Enable, long StartPos, uint TrigInterval);


        #endregion


        #region 锁存类函数
        /// <summary>
        /// 读取锁存缓存区中的点位数量
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="LTCNo">要读取的锁存通道</param>
        /// <param name="FifoCnt">获取到的锁存通道号</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetLTCFifoCnt(int cardid, int LTCNo, ref uint FifoCnt);

        /// <summary>
        /// 获取锁存缓存区中的最顶端数据，获取后自动清除该数据
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="LTCNo">锁存通道</param>
        /// <param name="LTCData">获取到的最顶层数据</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetLTCFifoData(int cardid, int LTCNo, ref long LTCData);

        /// <summary>
        /// 获取锁存瞬间的最新数据
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="LTCNo">锁存通道号</param>
        /// <param name="LTCData">获取到的最新数据</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetLTCFifoDataSt(int cardid, int LTCNo, ref long LTCData);




        /// <summary>
        /// 清除锁存通道高速计数值
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="LTCNo">锁存通道号</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E2I12O16_ClearLTCCounter(int cardid, int LTCNo);


        /// <summary>
        /// 获取锁存通道高速计数值
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="LTCNo">锁存通道号</param>
        /// <param name="CounterData">高速计数值</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetLTCCounterData(int cardid, int LTCNo, ref uint CounterData);

        /// <summary>
        /// 获取当前锁存高速计数器的计数值
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="LTCNo">锁存通道号</param>
        /// <param name="LTCCounter">获取到的高速计数器数据</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetLTCCounter(int cardid, int LTCNo, ref uint LTCCounter);

        /// <summary>
        /// 重置锁存高速计数器的数据
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="LTCNo">锁存通道号</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_ResetLTCCounter(int cardid, int LTCNo);

        /// <summary>
        /// 设置编码器计数方式
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="EncChn">编码器通道号</param>
        /// <param name="EncDir">编码器计数方式：0-双向 1-单正向</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_SetEncCountMode(int cardid, int EncChn, int EncDir);

        /// <summary>
        /// 获取编码器计数方式
        /// </summary>
        /// <param name="cardid">卡号</param>
        /// <param name="EncChn">编码器通道号</param>
        /// <param name="EncDir">编码器计数方式：0-双向 1-单正向</param>
        /// <returns></returns>
        [DllImport("MiniPcieLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MP_E2I12O16_GetEncCountMode(int cardid, int EncChn, ref int EncDir);
        #endregion


    }
}

