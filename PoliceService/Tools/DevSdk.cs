using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PoliceService.Tools
{
    public enum DevType
    {
        DevCard,
        DevLocator,
        DevReader
    }

    public enum MsgType
    {
        MsgLocate,      // 定位信号
        MsgCut,         // 剪断信号
        MsgHeartBeat,   // 心跳信号
        MsgCall,        // 呼叫信号
        MsgShake,       // 震动信号
        MsgLeave,       // 离开信号
        MsgOffAlarm,    // 报警灯关闭信号
        MsgCheckPower,  // 电量检测信号
        MsgHeartRate,   // 心率信号
        MsgExtra,       // 补发信号
        MsgNoPower,     // 没电信号
        MsgReset,       // 重置信号
        MsgBoundary,    // 跨界信号

        /** 以下为SDK追加消息 **/
        // 一般情况下，可以通过其它任何消息中DevMsg::isLowpower的值来判断是否低电，这个值是原始数据中携带的
        // 也可以使用此消息来单独处理低电，它是SDK经过统计处理后追加的消息，isLowpower为真是表示低电，否则表示正常
        // 此消息会跟随着MsgLocate消息出现
        MsgPower,

        // 当checkOffline设置为1时有效。如果在offlineTime时间内没有收到此设备的信号，则发出离线信号
        MsgOffline,

        MsgLast
    }
    public struct DevMsg
    {
        public MsgType type;
        public DevType device;         // 硬件类型
        public Int32 cardID;         // 标签/腕带ID,当DevType为DevCard时有意义
        public Int32 readerID;       // 阅读器ID，总是有意义
        public Int32 locatorID;      // 大于0时有意义，表示定位器ID
        public Int32 readerRSSI;     // 阅读器信号强度，由于硬件版本的差异，此值要么全有意义，要么全没意义(总是0)
        public Int32 locatorRssiX;   // 当MsgType为[MsgLocate, MsgExtra]时，此值有意义，表示定位器的信号强度
        public Int32 locatorRssiY;   // 定位器的信号强度
        public char isCut;          // 如果此值为1，表示硬件被破坏。因为有专门的MsgCut消息，可以忽视此值
        public char isShake;        // 如果此值为1，表示硬件处于震动状态。仅当MsgType为MsgShake是有意义，1表示震动，0表示静止
        public char isLowpower;     // 硬件是否低电。硬件为标签时有意义，因为有追加的MsgPower消息，可以在MsgType为MsgPower时使用此值
        public Int32 heartRate;      // 心率，当MsgType为[MsgHeartBeat, MsgHeartRate]之一时有意义
        public Int32 power;          // 电量，当MsgType为[MsgHeartBeat, MsgReset, MsgCheckPower]之一时有意义
        public Int64 time;           // 消息时间戳
        public int version;        // 硬件版本号，DevType为DevCard时有意义
    }


    public class DevSDK
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe public delegate   void  MsgCallback(DevMsg msg, void* ctx);
        [DllImport("ccrfidDevSDK.dll", CallingConvention = CallingConvention.Cdecl)]
        unsafe public extern static void ccrfidDevSdkStartWork(MsgCallback fun, void* ctx);

        [DllImport("ccrfidDevSDK.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static void ccrfidDevSdkStopWork();

    }
}
