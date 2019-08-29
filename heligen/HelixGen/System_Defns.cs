// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HelixGen
{
    public class CSystem_Defns
    {
        // Update this string constant when a new version is released
        public static readonly string cstrPCR_Application_Version = "Assay Instrument [Prototype V1.00]";

        public static readonly string strDefaultSystemConfigurationPath = "..\\..\\Local_Configuration\\instrumentconfig.xml";
        public static readonly string strSavedSystemConfigurationPath = "..\\..\\ConfigFiles\\instrumentconfig.xml";
        public static readonly string strDefaultProtocolPath = "..\\..\\Protocols\\";
        public static readonly string strDefaultMeasurementLogPath = "..\\..\\Logs\\Measurement\\";
        public static readonly string strDefaultSystemLoggingPath = "..\\..\\Logs\\System\\";
        public static readonly string strSystemDebugLogFilename = "PCR_SystemDebugLog.log";
        public static readonly string strSystemDebugLogSubPath = "Debug\\";
        public static readonly string strUserOperationalLogFilename = "PCR_UserOperations.log";
        public static readonly string strUserOperationalLogSubPath = "Operational\\";
        public static readonly string strUserErrorLogFilename = "PCR_Errors.log";
        public static readonly string strUserErrorLogSubPath = "Error\\";

        public static readonly bool      cbONLY_ONE_LED_ON_MEASUREMENT = false;

        public static readonly int       ciMotorCommFailure = 100;
        public static readonly int       ciMotorCommInvalidResponseFormat = 101;
        public static readonly int       ciMotorLostStepEncoderMismatch = 102;

        public static readonly uint      cuiTotalTECs = 6;
        public static readonly uint      cuiTotalDevices = 1;
        public static readonly uint      cuiTotalAccelThermalBoards = 1;

        public static readonly uint      cuiPIDUpParamsId = 0;
        public static readonly uint      cuiPIDDownParamsId = 1;
        public static readonly string[]  cstrDeviceIdEntries = new string[] { "Device_1" }; // Must be in consecutive, ascending order
        public static readonly string[]  cstrTECEntries = new string[] { "TEC_1", "TEC_2", "TEC_3", "TEC_4", "TEC_5", "TEC_6" };
        // SWE; Hack to make it so that a single configuration TEC_1 sets all the TEC channels.
        //public static readonly string[] cstrTECEntries = new string[] { "TEC_1", "TEC_1", "TEC_1", "TEC_1", "TEC_1", "TEC_1" };


        // Safety parameters!!! Take care in changing these 
        public static readonly float     cfPCR_Safe_Heatsink_Temperature_Max_Sampling_Time_in_Seconds = 5.0F;
        public static readonly float     cfPCR_Safe_Heatsink_Temperature_Min_Sampling_Time_in_Seconds = 0.1F;

        public static readonly float     cfPCR_Safe_Heatsink_Temperature_in_Sampling_Time_in_Seconds = 1.0F;
        public static readonly float     cfPCR_Safe_Heatsink_Temperature_Max_Threshold_in_C = 50.0F;
        public static readonly float     cfPCR_Safe_TEC_Temperature_Max_Threshold_in_C = 110.0F;

        public static readonly float     cfPCR_Safe_Door_Safe_Open_Max_Temperature_Threshold_in_C = 50.0F;

        public static readonly char      ccDataLogDelimiter = ',';
        public static readonly float     cfPCR_Minimum_Measurement_Temperature_Sampling_Rate_In_Seconds = 0.1F;

        public static readonly uint      cuiPCR_Filter_Positions = 3;

        public static readonly float     cfLoad_Pressure_Tolerance = 1.5F;                // Load Pressure tolerance 
        public static readonly long      clLoad_Pressure_Target_Timeout_in_Ms = 15000;     // Load Pressure ramp timeout in milliseconds
        public static readonly float     cfPrime_Pressure_Tolerance = 1.5F;               // Prime Pressure tolerance
        public static readonly long      clPrime_Pressure_Target_Timeout_in_Ms = 15000;    // Prime Pressure ramp timeout in milliseconds
        public static readonly float     cfDigitize_Pressure_Tolerance = 1.5F;            // Digitize Pressure tolerance
        public static readonly long      clDigitize_Pressure_Target_Timeout_in_Ms = 15000; // Digitize Pressure ramp timeout in milliseconds

    } 

    public class CAssayRunParameters
    {
        public CAssayRunParameters(DateTime ts, string strProtocolFilePathName, string strDataResultsFilePathName)
        {
             m_StartProcessTime = ts;
             m_strProtocolFilePathName = strProtocolFilePathName;
             m_strDataResultsFilePathName = strDataResultsFilePathName;
        }
        public DateTime m_StartProcessTime;
        public string m_strProtocolFilePathName { get; set; }
        public string m_strDataResultsFilePathName { get; set; }
    };

    public class CPCRInstrumentSystemException : Exception
    {
        public string m_strSupplementalmsg { get; set; }
        public CPCRInstrumentSystemException(string strReason, string strSupplementalmsg = "") : base(strReason)
        {
            m_strSupplementalmsg = strSupplementalmsg;
        }
    };

    public class CThermalCriticalException : Exception
    {
        public string m_strSupplementalmsg { get; set; }
        public int m_iCriticalErrorCode { get; set; }
        public CThermalCriticalException(string strReason, int iCriticalErrorCode, string strSupplementalmsg = "") : base(strReason)
        {
            m_strSupplementalmsg = strReason;
            m_iCriticalErrorCode = 0;
        }
    };

    public class CMotionCriticalException : Exception
    {
        public string m_strSupplementalmsg { get; set; }
        public int m_iCriticalErrorCode { get; set; }
        public CMotionCriticalException(string strReason, int iCriticalErrorCode, string strSupplementalmsg = "")
            : base(strReason)
        {
            m_strSupplementalmsg = strReason;
            m_iCriticalErrorCode = iCriticalErrorCode;
        }
    };

    public class CBadParamException : Exception 
    { 
        public CBadParamException(string strReason) : base(strReason)
        {

        }
    };
}
