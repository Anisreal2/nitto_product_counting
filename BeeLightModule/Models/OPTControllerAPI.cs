using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace BeeLightModule.Models
{
    public class OPTControllerAPI
    {
        public const int OPT_SUCCEED = 0;                               //Operation succeed
        public const int OPT_ERR_INVALIDHANDLE = 3001001;                 //Invalid handle
        public const int OPT_ERR_UNKNOWN = 3001002;                  //Error unknown 
        public const int OPT_ERR_INITSERIAL_FAILED = 3001003;             //Failed to initialize a serial port
        public const int OPT_ERR_RELEASESERIALPORT_FAILED = 3001004;      //Failed to release a serial port
        public const int OPT_ERR_SERIALPORT_UNOPENED = 3001005;       //Attempt to access an unopened serial port
        public const int OPT_ERR_CREATEETHECON_FAILED = 3001006;       //Failed to create an Ethernet connection
        public const int OPT_ERR_DESTROYETHECON_FAILED = 3001007;        //Failed to destroy an Ethernet connection
        public const int OPT_ERR_SN_NOTFOUND = 3001008;        //SN is not found
        public const int OPT_ERR_TURNONCH_FAILED = 3001009;        //Failed to turn on the specified channel(s)
        public const int OPT_ERR_TURNOFFCH_FAILED = 3001019;        //Failed to turn off the specified channel(s)
        public const int OPT_ERR_SET_INTENSITY_FAILED = 3001011;        //Failed to set the intensity for the specified channel(s)
        public const int OPT_ERR_READ_INTENSITY_FAILED = 3001012;       //Failed to read the intensity for the specified channel(s)	
        public const int OPT_ERR_SET_TRIGGERWIDTH_FAILED = 3001013;       //Failed to set trigger pulse width	
        public const int OPT_ERR_READ_TRIGGERWIDTH_FAILED = 3001014;      //Failed to read trigger pulse width
        public const int OPT_ERR_READ_HBTRIGGERWIDTH_FAILED = 3001015;     //Failed to read high brightness trigger pulse width
        public const int OPT_ERR_SET_HBTRIGGERWIDTH_FAILED = 3001016;     //Failed to set high brightness trigger pulse width
        public const int OPT_ERR_READ_SN_FAILED = 3001017;          //Failed to read serial number
        public const int OPT_ERR_READ_IPCONFIG_FAILED = 3001018;       //Failed to read IP address
        public const int OPT_ERR_CHINDEX_OUTRANGE = 3001019;        //Index(es) out of the range
        public const int OPT_ERR_WRITE_FAILED = 3001020;             //Failed to write data
        public const int OPT_ERR_PARAM_OUTRANGE = 3001021;             //Parameter(s) out of the range 
        public const int OPT_ERR_READ_MAC_FAILED = 3001022;             //failed to read MAC
        public const int OPT_ERR_SET_MAXCURRENT_FAILED = 3001023;       //failed to set max current
        public const int OPT_ERR_READ_MAXCURRENT_FAILED = 3001024; //failed to read max current
        public const int OPT_ERR_SET_TRIGGERACTIVATION_FAILED = 3001025;  //failed to set trigger activation
        public const int OPT_ERR_READ_TRIGGERACTIVATION_FAILED = 3001026; //failed to read trigger activation
        public const int OPT_ERR_SET_WORKMODE_FAILED = 3001027;   //failed to set work mode
        public const int OPT_ERR_READ_WORKMODE_FAILED = 3001028;  //failed to read work mode
        public const int OPT_ERR_SET_BAUDRATE_FAILED = 3001029;  //failed to set baud rate
        public const int OPT_ERR_SET_CHANNELAMOUNT_FAILED = 3001030;  //failed to set channel amount
        public const int OPT_ERR_SET_DETECTEDMINLOAD_FAILED = 3001031;  //failed to set detected min load
        public const int OPT_ERR_READ_OUTERTRIGGERFREQUENCYUPPERBOUND_FAILED = 3001032;   //failed to read outer trigger frequency upper bound
        public const int OPT_ERR_SET_AUTOSTROBEFREQUENCY_FAILED = 3001033; //failed to set auto-strobe frequency
        public const int OPT_ERR_READ_AUTOSTROBEFREQUENCY_FAILED = 3001034;   //failed to read auto-strobe frequency
        public const int OPT_ERR_SET_DHCP_FAILED = 3001035;     //failed to set DHCP
        public const int OPT_ERR_SET_LOADMODE_FAILED = 3001036;   //failed to set load mode
        public const int OPT_ERR_READ_PROPERTY_FAILED = 3001037; //failed to read property
        public const int OPT_ERR_CONNECTION_RESET_FAILED = 3001038;   //failed to reset connection
        public const int OPT_ERR_SET_HEARTBEAT_FAILED = 3001039;  //failed to set ethe connection heartbeat
        public const int OPT_ERR_GETCONTROLLERLIST_FAILED = 3001040;    //Failed to get controler(s) list           
        public const int OPT_ERR_SOFTWARETRIGGER_FAILED = 3001041;    //Failed to software trigger                
        public const int OPT_ERR_GET_CHANNELSTATE_FAILED = 3001042;   //Failed to get channelstate          
        public const int OPT_ERR_SET_KEEPALIVEPARAMETERS_FAILED = 3001043;    //Failed to set keepalvie parameters          
        public const int OPT_ERR_ENABLE_KEEPALIVE_FAILED = 3001044;   //Failed to enable/disable keepalive
        public const int OPT_ERR_READSTEPCOUNT_FAILED = 3001045;  //Failed to read step count           
        public const int OPT_ERR_SETTRIGGERMODE_FAILED = 3001046;   //Failed to set trigger mode    
        public const int OPT_ERR_READTRIGGERMODE_FAILED = 3001047;   //Failed to read trigger mode      
        public const int OPT_ERR_SETCURRENTSTEPINDEX_FAILED = 3001048;   //Failed to set current step index          
        public const int OPT_ERR_READCURRENTSTEPINDEX_FAILED = 3001049;    //Failed to read current step index          
        public const int OPT_ERR_RESETSEQ_FAILED = 3001050;  //Failed to reset SEQ
        public const int OPT_ERR_SETTRIGGERDELAY_FAILED = 3001051;     //Failed to set trigger delay
        public const int OPT_ERR_GET_TRIGGERDELAY_FAILED = 3001052;    //Failed to get trigger delay
        public const int OPT_ERR_SETMULTITRIGGERDELAY_FAILED = 3001053;    //Failed to set multiple channels trigger delay
        public const int OPT_ERR_SETSEQTABLEDATA_FAILED = 3001054;    //Failed to set SEQ table data
        public const int OPT_ERR_READSEQTABLEDATA_FAILED = 3001055;     //Failed to Read SEQ table data
        public const int OPT_ERR_READ_CHANNELS_FAILED = 3001056;    //Failed to read controller//s channel
        public const int OPT_ERR_READ_KEEPALIVE_STATE_FAILED = 3001057;     //Failed to read the state of keepalive
        public const int OPT_ERR_READ_KEEPALIVE_CONTINUOUS_TIME_FAILED = 3001058;    //Failed to read the continuous time of keepalive
        public const int OPT_ERR_READ_DELIVERY_TIMES_FAILED = 3001059;    //Failed to read the delivery times of prop packet
        public const int OPT_ERR_READ_INTERVAL_TIME_FAILED = 3001060;     //Failed to read the interval time of prop packet
        public const int OPT_ERR_READ_OUTPUTBOARD_VISION_FAILED = 3001061;     //Failed to read the vision of output board
        public const int OPT_ERR_READ_DETECT_MODE_FAILED = 3001062;    //Failed to read detect mode of load
        public const int OPT_ERR_SET_BOOT_STATE_MODE_FAILED = 3001063;     //Failed to set mode of boot State
        public const int OPT_ERR_READ_MODEL_BOOT_MODE_FAILED = 3001064;    //Failed to read the specified channel boot state
        public const int OPT_ERR_SET_OUTERTRIGGERFREQUENCYUPPERBOUND_FAILED = 3001065;   //failed to set outer trigger frequency upper bound
        public const int OPT_ERR_SET_IPCONFIG_FAILED = 3001066;   //Failed to set IP configuration of the controller
        public const int OPT_ERR_SET_VOLTAGE_FAILEDAs = 3001067;   //Failed to set voltage value
        public const int OPT_ERR_READ_VOLTAGE_FAILEDAs = 3001068;   //Failed to read voltage value
        public const int OPT_ERR_SET_TIMEUNIT_FAILED = 3001069; //Failed to set time unit
        public const int OPT_ERR_READ_TIMEUNIT_FAILED = 3001070;   //Failed to read time unit
        public const int OPT_ERR_FILEEXT = 3001071;     //File suffix name is wrong
        public const int OPT_ERR_FILEPATH_EMPTY = 3001072;     //File path is empty
        public const int OPT_ERR_FILE_MAGIC_NUM = 3001073;   //magic number is wrong
        public const int OPT_ERR_FILE_CHECKSUM = 3001074;   //Checksum is wrong
        public const int OPT_ERR_SEQDATA_EQUAL = 3001075;    //Current SEQ table data is different from load file data
        public const int OPT_ERR_SET_HB_TIMEUNIT_FAILED = 3001076;     //Failed to set highlight time unit
        public const int OPT_ERR_READ_HB_TIMEUNIT_FAILED = 3001077;    //Failed to read highlight time unit
        public const int OPT_ERR_SET_TRIGGERDELAY_TIMEUNIT_FAILED = 3001078;     //Failed to set trigger delay time unit
        public const int OPT_ERR_READ_TRIGGERDELAY_TIMEUNIT_FAILED = 3001079;    //Failed to read trigger delay time unit
        public const int OPT_ERR_SET_PERCENT_FAILED = 3001080;     //Failed to set percent of brightening current
        public const int OPT_ERR_READ_PERCENT_FAILED = 3001081;    //Failed to read percent of brightening current
        public const int OPT_ERR_SET_HB_LIMIT_STATE_FAILED = 3001082;  //Failed to set high light trigger output duty limit switch state
        public const int OPT_ERR_READ_HB_LIMIT_STATE_FAILED = 3001083;  //Failed to read high light trigger output duty limit switch state
        public const int OPT_ERR_SET_HB_TRIGGER_OUTPUT_DUTY_RATIO_FAILED = 3001084;   //Failed to set high light trigger output duty limit ratio
        public const int OPT_ERR_READ_HB_TRIGGER_OUTPUT_DUTY_RATIO_FAILED = 3001085;     //Failed to read high light trigger output duty limit ratio
        public const int OPT_ERR_SET_DIFF_PRESURE_LIMIT_STATE_FAILED = 3001086;   //Failed to set differential pressure limit function switch status
        public const int OPT_ERR_READ_DIFF_PRESURE_LIMIT_STATE_FAILED = 3001087;   //Failed to read differential pressure limit function switch status

        //Declare the 
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_InitSerialPort(string comName, out IntPtr controllerHandle);

        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_ReleaseSerialPort(IntPtr controllerHandle);


        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_CreateEthernetConnectionByIP(string serverIPAddress, out IntPtr controllerHandle);


        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_CreateEthernetConnectionBySN(string serialNumber, out IntPtr controllerHandle);
        // End Function

        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_DestroyEthernetConnection(IntPtr controllerHandle);
        // End Function

        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_TurnOnChannel(IntPtr controllerHandle, int channelIndex);
        // End Function

        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_TurnOnMultiChannel(IntPtr controllerHandle, int[] channelIndexArray, int length);
        // End Function

        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_TurnOffChannel(IntPtr controllerHandle, int channelIndex);
        // End Function

        [DllImport("OPTController.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        public static extern long OPTController_TurnOffMultiChannel(IntPtr controllerHandle, int[] channelIndexArray, int length);
        // End Function

        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_SetIntensity(IntPtr controllerHandle, int channelIndex, int intensity);
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetMultiIntensity(IntPtr controllerHandle, ByRef intensityArray As IntensityItem, ByVal arrayLength)
        // End Function

        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_ReadIntensity(IntPtr controllerHandle, int channelIndex, out int intensity);
        // End Function

        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_SetTriggerWidth(IntPtr controllerHandle, int channelIndex, int triggerWidth);
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetMultiTriggerWidth(IntPtr controllerHandle, ByRef triggerWidthArray As TriggerWidthItem, ByVal arrayLength)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadTriggerWidth(IntPtr controllerHandle, ByVal channelIndex, ByRef triggerWidth)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetHBTriggerWidth(IntPtr controllerHandle, ByVal channelIndex, ByVal HBTriggerWidth)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetMultiHBTriggerWidth(IntPtr controllerHandle, ByRef HBtriggerWidthArray As HBTriggerWidthItem, ByVal arrayLength)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadHBTriggerWidth(IntPtr controllerHandle, ByVal channelIndex, ByRef HBTriggerWidth)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_EnableResponse(IntPtr controllerHandle, ByVal nisResponse)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_EnableCheckSum(IntPtr controllerHandle, ByVal nenable)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_EnablePowerOffBackup(IntPtr controllerHandle, ByVal nisSave)
        // End Function
        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_ReadSN(IntPtr controllerHandle, out string SN);
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadProperties(IntPtr controllerHandle, ByVal properties, ByVal value As StringBuilder)
        // End Function

        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_ReadIPConfig(IntPtr controllerHandle, out string IP, out string subnetMask, out string defaultGateway);
        // End Function

        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_ReadIPConfigBySN(string SN, out string IP, out string subnetMask, out string defaultGateway);
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetMaxCurrent(IntPtr controllerHandle, ByVal channelIndex, ByVal current)
        // End Function
        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadMaxCurrent(IntPtr controllerHandle, ByVal channelIndex, ByVal mode, ByRef value)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetMultiMaxCurrent(IntPtr controllerHandle, ByRef maxCurrentArray As MaxCurrentItem, ByVal arrayLength)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadMAC(IntPtr controllerHandle, ByVal MAC As StringBuilder)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetTriggerActivation(IntPtr controllerHandle, ByVal channelIndex, ByVal triggerActivation)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadTriggerActivation(IntPtr controllerHandle, ByVal channelIndex, ByRef triggerActivation)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetWorkMode(IntPtr controllerHandle, ByVal workMode)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadWorkMode(IntPtr controllerHandle, ByRef workMode)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetOuterTriggerFrequencyUpperBound(IntPtr controllerHandle, ByVal channelIndex, ByVal maxFrequency)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadOuterTriggerFrequencyUpperBound(IntPtr controllerHandle, ByVal channelIndex, ByRef maxFrequency)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_AutoDetectLoadOnce(IntPtr controllerHandle)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetAutoStrobeFrequency(IntPtr controllerHandle, ByVal channelIndex, ByVal frequency)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadAutoStrobeFrequency(IntPtr controllerHandle, ByVal channelIndex, ByRef frequency)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_EnableDHCP(IntPtr controllerHandle, ByVal nbDHCP)
        // End Function


        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetLoadMode(IntPtr controllerHandle, ByVal channelIndex, ByVal loadMode)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_GetVersion(ByVal version As StringBuilder)
        // End Function


        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ConnectionResetBySN(ByVal serialNumber As StringBuilder)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ConnectionResetByIP(ByVal serverIPAddress As StringBuilder)
        // End Function

        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_IsConnect(IntPtr controllerHandle);
        // End Function


        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetEthernetConnectionHeartBeat(IntPtr controllerHandle, ByVal timeout As UInteger)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_GetControllerListOnEthernet(ByVal snList As StringBuilder)
        // End Function
        // //Like this: Dim devs As New StringBuilder(0,1024)     OPTController_GetControllerListOnEthernet(devs).

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_GetChannelState(IntPtr controllerHandle, ByVal channelIdx, ByRef state)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetKeepaliveParameter(IntPtr controllerHandle, ByVal keepalive_time, ByVal keepalive_intvl, ByVal keepalive_probes)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_EnableKeepalive(IntPtr controllerHandle, ByVal nenable)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SoftwareTrigger(IntPtr controllerHandle, ByVal channelIdx, ByVal time)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_MultiSoftwareTrigger(IntPtr controllerHandle, ByRef softwareTriggerArray As SoftwareTriggerItem, ByVal length)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadStepCount(IntPtr controllerHandle, ByVal moduleIndex, ByRef count)
        // End Function
        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_SetTriggerMode(IntPtr controllerHandle, int moduleIndex, int mode);
        // End Function
        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_ReadTriggerMode(IntPtr controllerHandle, int moduleIndex, out int mode);
        // End Function
        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetCurrentStepIndex(IntPtr controllerHandle, ByVal moduleIndex, ByVal curStepIndex)
        // End Function
        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadCurrentStepIndex(IntPtr controllerHandle, ByVal moduleIndex, ByRef curStepIndex)
        // End Function
        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ResetSEQ(IntPtr controllerHandle, ByVal moduleIndex)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetSeqTable(IntPtr controllerHandle, ByVal moduleIndex, ByVal seqCount, ByRef triggerSource, ByRef intensity, ByRef pulseWidth)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadSeqTable(IntPtr controllerHandle, ByVal moduleIndex, ByRef seqCount, ByRef triggerSource, ByRef intensity, ByRef pulseWidth)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SaveSeqFile(ByVal seqCount, ByRef triggerSource, ByRef intensity, ByRef pulseWidth, ByVal filePath As StringBuilder)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_LoadSeqFile(ByVal filePath As StringBuilder, ByRef seqCount, ByRef triggerSource, ByRef intensity, ByRef pulseWidth)
        // End Function


        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_CompareSeqTable(IntPtr controllerHandle, ByVal moduleIndex, ByVal filePath As StringBuilder)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SaveSeqFileToCSV(ByVal seqCount, ByRef triggerSource, ByRef intensity, ByRef pulseWidth, ByVal filePath As StringBuilder)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_LoadSeqFileFromCSV(ByVal filePath As StringBuilder, ByRef seqCount, ByRef triggerSource, ByRef intensity, ByRef pulseWidth)
        // End Function


        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_CompareSeqTableFromCSV(IntPtr controllerHandle, ByVal moduleIndex, ByVal filePath As StringBuilder)
        // End Function

        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_SetTriggerDelay(IntPtr controllerHandle, int channelIndex, int triggerDelay);
        // End Function

        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_GetTriggerDelay(IntPtr controllerHandle, int channelIndex, out int triggerDelay);
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetMultiTriggerDelay(IntPtr controllerHandle, ByRef triggerDelayArray As TriggerDelayItem, ByVal length)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_GetControllerChannels(IntPtr controllerHandle, ByRef channels)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadKeepaliveSwitchState(IntPtr controllerHandle, ByRef state)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadContinuousKeepaliveTime(IntPtr controllerHandle, ByRef time)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadPacketDeliveryTimes(IntPtr controllerHandle, ByRef times)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadIntervalTimeOfPropPacket(IntPtr controllerHandle, ByRef time)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern long OPTController_ReadOutputBoardVision(IntPtr controllerHandle, ByVal vision As StringBuilder)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadLoadDetectMode(IntPtr controllerHandle, ByVal channelIndex, ByRef mode)
        // End Function


        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetBootState(IntPtr controllerHandle, ByVal channelIndex, ByVal mode)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadModelBootState(IntPtr controllerHandle, ByVal channelIndex, ByRef state)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetOutputVoltage(ByVal controllerHandle, ByVal channelIndex, ByVal voltage)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadOutputVoltage(ByVal controllerHandle, ByVal channelIndex, ByRef voltage)
        // End Function


        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_SetTimeUnit(IntPtr controllerHandle, int channelIndex, int timeUnit);
        // End Function

        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_ReadTimeUnit(IntPtr controllerHandle, int channelIndex, out int timeUnit);
        // End Function



        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetHBTriggerUnit(IntPtr controllerHandle, ByVal channelIndex, ByVal timeUnit)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadHBTriggerUnit(IntPtr controllerHandle, ByVal channelIndex, ByRef timeUnit)
        // End Function

        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_SetTriggerDelayUnit(IntPtr controllerHandle, int channelIndex, int timeUnit);
        // End Function

        [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long OPTController_ReadTriggerDelayUnit(IntPtr controllerHandle, int channelIndex, out int timeUnit);
        // End Function


        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetPercentOfBrighteningCurrent(IntPtr controllerHandle, ByVal channelIndex, ByVal percentage)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadPercentOfBrighteningCurrent(IntPtr controllerHandle, ByVal channelIndex, ByRef percentage)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetHBTriggerOutputDutyLimitSwitchState(IntPtr controllerHandle, ByVal channelIndex, ByVal state)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadHBTriggerOutputDutyLimitSwitchState(IntPtr controllerHandle, ByVal channelIndex, ByRef state)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetHBTriggerOutputDutyRatio(IntPtr controllerHandle, ByVal channelIndex, ByVal ratio)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadHBTriggerOutputDutyRatio(IntPtr controllerHandle, ByVal channelIndex, ByRef ratio)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_SetDiffPresureLimitSwitchState(IntPtr controllerHandle, ByVal channelIndex, ByVal state)
        // End Function

        // [DllImport("OPTController.dll", CallingConvention = CallingConvention.Cdecl)]
        // public static extern long OPTController_ReadDiffPresureLimitSwitchState(IntPtr controllerHandle, ByVal channelIndex, ByRef state)
        // End Function

    }
    //public class IntensityItem
    //{
    //    int channelIndex;        //The channel index value of controller
    //    int intensity;             //The intensity for the corresponding channel index 
    //}

    //public class TriggerWidthItem
    //{
    //    int channelIndex;       //The channel index value of controller
    //    int triggerWidth;            //The trigger width for the corresponding channel index
    //}
    //public class HBTriggerWidthItem
    //{
    //    int channelIndex;            //The channel index value of controller
    //    int HBTriggerWidth;        //The high brightness trigger width for the corresponding channel index 
    //}

    //public class SoftwareTriggerItem
    //{
    //    int channelIndex;           //The channel index value of controller
    //    int softwareTriggerTime;     //The software trigger time for the corresponding channel index 
    //}


    //public class TriggerDelayItem
    //{
    //    int channelIndex;        //The channel index value of controller
    //    int triggerDelayTime;       //The trigger delay for the corresponding channel index 

    //}

    //public class MaxCurrentItem
    //{
    //    int channelIndex;            //The channel index value of controller
    //    int maxCurrent;             //The maximum current for the corresponding channel index 
    //}

}
