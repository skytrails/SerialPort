using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System_Defns;
using XmlDocument_Support_Utilities;

namespace System_Configuration_File_Reader
{
    public class CSystem_Configuration_File_Reader
    {
        public class CSystem_Instrument_Configuration
        {
            public string m_strSerialNumber { get; set; }
            public string m_strProductNumber { get; set; }
            public string m_strProductName { get; set; }
        }

        public class CSystem_Misc_Configuration
        {
            public string                         m_strSystemLogPath { get; set; }
            public string                         m_strSystemProtocolPath { get; set; }
            public string                         m_strMeasurementLogPath { get; set; }
            public float                          m_fProtocolCycleStep_MinTemperature { get; set; }
            public float                          m_fProtocolCycleStep_MaxTemperature { get; set; }
            public float                          m_fProtocolNonCycleStep_MinTemperature { get; set; }
            public uint                           m_uiProtocolNonCycleStep_MinTemp_MaxHoldTimeinSeconds { get; set; }
            public float                          m_fProtocolNonCycleStep_MaxTemperature { get; set; }
            public float                          m_fThermalRampSamplePeriodInSeconds { get; set; }
            public float                          m_fThermalRampTimeoutInSeconds { get; set; }
        }

        public class CSystem_Motor_Controller_HW_Configuration
        {
            public string m_strControllerName { get; set; }
            public string m_strPort { get; set; }
            // TBD
        }

        public class CSystem_Thermal_Controller_HW_Configuration
        {
            public string m_strControllerName { get; set; }
            public string m_strModel { get; set; }
            public string m_strPort { get; set; }
            // TBD
        }

        public class CSystem_Motor_HW_Channel_Configuration
        {
            public string                                m_strControllerName { get; set; }
            public uint                                  m_uiMotorChannel { get; set; }
            public uint                                  m_uiMotorHomeSpeed { get; set; }
            public uint                                  m_uiMotorStartSpeed { get; set; }
            public uint                                  m_uiMotorMaxSpeed { get; set; }
            public uint                                  m_uiMotorAccel { get; set; }
            public uint                                  m_uiMotorDecel { get; set; }
            public uint                                  m_uiMotorMoveCurrent { get; set; } 
            public uint                                  m_uiMotorHoldCurrent { get; set; }
            public uint                                  m_uiMotorJerk { get; set; }
            public uint                                  m_uiMotorResolution { get; set; }
            public uint                                  m_uiMotorProfileMode { get; set; }
            public uint                                  m_uiMotorHomeTimeout { get; set; }
            public uint                                  m_uiPosition_Move_Timeout { get; set; }
            public uint                                  m_uiMotorDirection { get; set; }
            public uint                                  m_uiMotorMaxNumLostSteps { get; set; }
            public bool                                  m_bEncoderEnabled { get; set; }
            public uint                                  m_uiEncoderMonitorTimer_ms { get; set; }
            public uint                                  m_uiEncoderMonitorPulseChangeThreshold { get; set; }
            public uint                                  m_uiEncoderMonitorErrorCountThreshold { get; set; }
            public uint                                  m_uiEncoderDirectionPolarity { get; set; }
            public int                                   m_iEncoderStartOffset { get; set; }
            public float                                 m_fEncoderScalingFactor { get; set; }
        }

        public class CTEC_From_Temperature_PID_Element
        {
            public CTEC_From_Temperature_PID_Element()
            {
                m_TemperatureRange = new CTemperatureRange();
                m_ToTemperatures = new List<CTEC_HW_Ramping_Configuration>();
            }
            public CTEC_From_Temperature_PID_Element(CTEC_From_Temperature_PID_Element obj)
            {
                m_TemperatureRange = new CTemperatureRange(obj.m_TemperatureRange);
                m_ToTemperatures = new List<CTEC_HW_Ramping_Configuration>();
                for (uint ui = 0; ui < obj.m_ToTemperatures.Count; ui++)
                {
                    m_ToTemperatures.Add(new CTEC_HW_Ramping_Configuration(obj.m_ToTemperatures.ToArray()[ui]));
                }
            }
            public CTemperatureRange                          m_TemperatureRange;
            public List<CTEC_HW_Ramping_Configuration>        m_ToTemperatures;
        }

        public class CTEC_HW_PID_Configuration
        {
            public float m_fRabbitGain;
            public float m_fRabbitGain2;
            public float m_fRabbitGainOffset;
            public float m_fRabbitDerivGain;
            public float m_fPGain;
            public float m_fDGain;
            public float m_fIGain;
        }

        public class CTEC_HW_Ramping_Configuration
        {
            public CTEC_HW_Ramping_Configuration() 
            { 
                m_TemperatureRange = new CTemperatureRange(); 
                m_PID_Settings = new CTEC_HW_PID_Configuration[2];
                for (int i = 0; i < m_PID_Settings.Length; i++)
                {
                    m_PID_Settings[i] = new CTEC_HW_PID_Configuration();
                }
            }
            public CTEC_HW_Ramping_Configuration(CTEC_HW_Ramping_Configuration obj)
            {
                m_TemperatureRange = new CTemperatureRange(obj.m_TemperatureRange);
                m_PID_Settings = new CTEC_HW_PID_Configuration[2];
                for (int i = 0; i < m_PID_Settings.Length; i++)
                {
                    m_PID_Settings[i] = new CTEC_HW_PID_Configuration();
                }
                for (int i = 0; i < m_PID_Settings.Length; i++)
                {
                    m_PID_Settings[i].m_fDGain = obj.m_PID_Settings[i].m_fDGain;
                    m_PID_Settings[i].m_fIGain = obj.m_PID_Settings[i].m_fIGain;
                    m_PID_Settings[i].m_fPGain = obj.m_PID_Settings[i].m_fPGain;
                    m_PID_Settings[i].m_fRabbitDerivGain = obj.m_PID_Settings[i].m_fRabbitDerivGain;
                    m_PID_Settings[i].m_fRabbitGain = obj.m_PID_Settings[i].m_fRabbitGain;
                    m_PID_Settings[i].m_fRabbitGain2 = obj.m_PID_Settings[i].m_fRabbitGain2;
                    m_PID_Settings[i].m_fRabbitGainOffset = obj.m_PID_Settings[i].m_fRabbitGainOffset;
                }
                m_fDeadBand = obj.m_fDeadBand;
                m_fOvershootOffset = obj.m_fOvershootOffset;
                m_uiOvershootDuration = obj.m_uiOvershootDuration;
                m_fSetpointOffset = obj.m_fSetpointOffset;
                m_fPBand = obj.m_fPBand;
                m_fLowClamp = obj.m_fLowClamp;
                m_fHighClamp = obj.m_fHighClamp;
            }

            public CTemperatureRange           m_TemperatureRange;
            public CTEC_HW_PID_Configuration[] m_PID_Settings;
            public float                       m_fDeadBand;
            public float                       m_fOvershootOffset;
            public uint                        m_uiOvershootDuration;
            public float                       m_fSetpointOffset;
            public float                       m_fPBand;
            public float                       m_fLowClamp;
            public float                       m_fHighClamp;
        }

        public class CTemperatureRange
        {
            public CTemperatureRange() {}
            public CTemperatureRange(CTemperatureRange obj)
            {
                m_fLowTemperature = obj.m_fLowTemperature;
                m_fHighTemperature = obj.m_fHighTemperature;
            }
            public float m_fLowTemperature;
            public float m_fHighTemperature;
        }

        public class CTEC_Channel_Configuration
        {
            public CTEC_Channel_Configuration()
            {
                m_Step_PID_RampUp_Range_List = new List<CTEC_From_Temperature_PID_Element>();
                m_Step_PID_RampDown_Range_List = new List<CTEC_From_Temperature_PID_Element>();
            }

            public CTEC_Channel_Configuration(CTEC_Channel_Configuration obj)
            {
                m_strControllerName = obj.m_strControllerName;
                m_uiTECAddress = obj.m_uiTECAddress;
                m_fThermA_Coeff = obj.m_fThermA_Coeff;
                m_fThermB_Coeff = obj.m_fThermB_Coeff;
                m_fThermC_Coeff = obj.m_fThermC_Coeff;
                m_ControlPIDSampleTimeInSeconds = obj.m_ControlPIDSampleTimeInSeconds;

                m_Step_PID_RampUp_Range_List = new List<CTEC_From_Temperature_PID_Element>();
                for (uint ui = 0; ui < m_Step_PID_RampUp_Range_List.Count; ui++)
                {
                    m_Step_PID_RampUp_Range_List.Add(new CTEC_From_Temperature_PID_Element(m_Step_PID_RampUp_Range_List.ToArray()[ui]));
                }
                m_Step_PID_RampDown_Range_List = new List<CTEC_From_Temperature_PID_Element>();
                for (uint ui = 0; ui < m_Step_PID_RampDown_Range_List.Count; ui++)
                {
                    m_Step_PID_RampDown_Range_List.Add(new CTEC_From_Temperature_PID_Element(m_Step_PID_RampDown_Range_List.ToArray()[ui]));
                }
                m_Step_PID_RampUp_Default = new CTEC_HW_Ramping_Configuration(obj.m_Step_PID_RampUp_Default);
                m_Step_PID_RampDown_Default = new CTEC_HW_Ramping_Configuration(obj.m_Step_PID_RampDown_Default);
                m_fErrorTermBand = obj.m_fErrorTermBand;
                m_fErrorTermCount = obj.m_fErrorTermCount;
                m_fSteadyStatePowerLimit = obj.m_fSteadyStatePowerLimit;
                m_fSteadyStatePowerLimitCount = obj.m_fSteadyStatePowerLimitCount;
            }

            public string                                                   m_strControllerName { get; set; }
            public uint                                                     m_uiTECAddress { get; set; }
            public float                                                    m_ControlPIDSampleTimeInSeconds { get; set; }
            public List<CTEC_From_Temperature_PID_Element>                  m_Step_PID_RampUp_Range_List;
            public CTEC_HW_Ramping_Configuration                            m_Step_PID_RampUp_Default;
            public List<CTEC_From_Temperature_PID_Element>                  m_Step_PID_RampDown_Range_List;
            public CTEC_HW_Ramping_Configuration                            m_Step_PID_RampDown_Default;
            public float                                                    m_fErrorTermBand { get; set; }
            public float                                                    m_fErrorTermCount { get; set; }
            public float                                                    m_fSteadyStatePowerLimit { get; set; }
            public float                                                    m_fSteadyStatePowerLimitCount { get; set; }
            public float                                                    m_fThermA_Coeff { get; set; }
            public float                                                    m_fThermB_Coeff { get; set; }
            public float                                                    m_fThermC_Coeff { get; set; }
        }

        //public class 
        public class CPCR_Phidget_Input
        {
            public uint m_uiDI_Index;
        }

        //public class 
        public class CPCR_Phidget_Output
        {
            public uint m_uiDO_Index;
        }

        public class CPCR_Accel_Motor_Digital_Input
        {
            public string m_strMotorController;
            public uint m_uiDI_Channel;
            public uint m_uiDI_On_Level;
        }

        //public class 
        public class CPCR_Accel_Motor_Digital_Output
        {
            public CPCR_Accel_Motor_Digital_Output() { m_uiDutyCycle = 60; } 
            public string m_strMotorController;
            public uint m_uiDO_Channel;
            public uint m_uiDutyCycle;
        }

        //public class 
        public class CPCR_NI_USB_Analog_Input
        {
            public string m_strNIControllerChannel;
            public uint   m_uiAI_Channel;
            public float  m_fMaxReadback;
            public float  m_fMinReadback;
        }

        //public class 
        public class CPCR_NI_USB_Analog_Output
        {
            public string m_strNIControllerChannel;
            public uint m_uiAO_Channel;
            public float m_fMaxSetpoint;
            public float m_fMinSetpoint;
        }

        //public class 
        public class CPCR_NI_Digital_Output
        {
            public string m_strNIControllerChannel;
            public uint m_uiDO_Channel;
        }

        //public class 
        // Fan and heatsink monitoring and control
        public class CPCR_Fan_Control_Band
        {
            public int CompareTo(CPCR_Fan_Control_Band value)
            {
                return this.m_fLowerTemperature.CompareTo(value.m_fLowerTemperature);
            }
            public float m_fUpperTemperature;   // Band Upper temperature in Centigrade.
            public float m_fLowerTemperature;   // Band Lower temperature in Centigrade.
            public float m_fDeadband;           // Required temperature excursion (in C) required to enter this band.
            public uint m_uiFanDutyCycle;      // Fan duty cycle used while in this band
        }

        public class CPCR_Fan_and_HeatSink_Configuration
        {
            public string m_strControllerName { get; set; }
            public uint m_uiTECAddress { get; set; }
            public CPCR_Fan_Control_Band[] m_Bands;
            public float m_fSampleTimeInSeconds;
            public float m_fThermA_Coeff { get; set; }
            public float m_fThermB_Coeff { get; set; }
            public float m_fThermC_Coeff { get; set; }
        }

        public class CPCR_Thermal_Block_Fan_Control_Thermistor_Configuration
        {
            public string m_strControllerName { get; set; }
            public uint  m_uiTECAddress { get; set; }
            public uint  m_uiHeaterChannel { get; set; }
            public float m_fThermA_Coeff { get; set; }
            public float m_fThermB_Coeff { get; set; }
            public float m_fThermC_Coeff { get; set; }
        }

        // public class
        public class CPumpIOControls
        {
            public CPumpIOControls() { m_Control = new CPCR_Accel_Motor_Digital_Output(); }
            public CPCR_Accel_Motor_Digital_Output m_Control;
        }


        // public class
        public class CPneumaticIOControls
        {
            public CPneumaticIOControls() { m_UpstreamPressureSensor = new CPCR_NI_USB_Analog_Input(); m_InletValveControl = new CPCR_Accel_Motor_Digital_Output(); m_OutletValveControl = new CPCR_Accel_Motor_Digital_Output(); m_VentValveControl = new CPCR_Accel_Motor_Digital_Output(); }
            public CPCR_NI_USB_Analog_Input        m_UpstreamPressureSensor;
            public CPCR_Accel_Motor_Digital_Output m_InletValveControl;
            public CPCR_Accel_Motor_Digital_Output m_OutletValveControl;
            public CPCR_Accel_Motor_Digital_Output m_VentValveControl;
        }

        // public class
        public class CRegulatorIOControls
        {
            public CRegulatorIOControls() { m_RegulatorPressureSetpoint = new CPCR_NI_USB_Analog_Output(); m_RegulatorPressureReadback = new CPCR_NI_USB_Analog_Input(); }
            public CPCR_NI_USB_Analog_Output       m_RegulatorPressureSetpoint;
            public CPCR_NI_USB_Analog_Input        m_RegulatorPressureReadback;
        }

        // public class
        public class COpticalEmitterConfiguration
        {
            public COpticalEmitterConfiguration() { m_EmitterAmplitudeControl = new CPCR_NI_USB_Analog_Output(); m_EmitterOnOffControl = new CPCR_NI_Digital_Output(); m_FanControl = new CPCR_Accel_Motor_Digital_Output(); }
            public CPCR_NI_USB_Analog_Output       m_EmitterAmplitudeControl;
            public int                             m_iDefaultSetpoint;
            public CPCR_NI_Digital_Output          m_EmitterOnOffControl;
            public CPCR_Accel_Motor_Digital_Output m_FanControl;
        }

        //public class 
        public class CMotor_Home_Input
        {
            public string m_strControllerName { get; set; }
            public uint m_uiMotorChannel { get; set; }
        }

        public class CCamera_Information
        {
            public string m_strManualSaveFolder;
            public string m_strInitialSettingsFilename;
        }

        public class COptical_Filter_Motor_HW_Channel_Calibration
        {
            public ulong m_ulFilter_1_Position_Y_Offset_from_Home_In_MotorUSteps; 
            public ulong m_ulFilter_2_Position_Y_Offset_from_Home_In_MotorUSteps; 
            public ulong m_ulFilter_3_Position_Y_Offset_from_Home_In_MotorUSteps;
            public ulong m_ulBlock_Emitter_Position_Y_Offset_from_Home_In_MotorUSteps;
        }

        public class CCamera_Motor_HW_Channel_Calibration
        {
            public ulong m_ulChip_1_Array_1_2_Position_X_Offset_from_Home_In_MotorUSteps;
            public ulong m_ulChip_1_Array_3_4_Position_X_Offset_from_Home_In_MotorUSteps;
            public ulong m_ulChip_1_Array_5_6_Position_X_Offset_from_Home_In_MotorUSteps;
            public ulong m_ulChip_1_Array_7_8_Position_X_Offset_from_Home_In_MotorUSteps; 
        }

        public class CPneumatic_Regulator_Calibration
        {
            public float m_fSetpointOffsetinPSI;
        }

        public class CSystemExhaustFans
        {
            public CSystemExhaustFans()
            {
                m_LeftFanControl = new CPCR_Accel_Motor_Digital_Output();
                m_RightFanControl = new CPCR_Accel_Motor_Digital_Output();
            }
            public CPCR_Accel_Motor_Digital_Output m_LeftFanControl;
            public CPCR_Accel_Motor_Digital_Output m_RightFanControl;
        }

        public class CSystem_Configuration_Obj
        {
            public CSystem_Configuration_Obj()
            {
                m_InstrumentConfiguration                                = new CSystem_Instrument_Configuration();
                m_MiscellaneousConfiguration                             = new CSystem_Misc_Configuration();
                m_MotorControllerConfigurations                          = new Dictionary<string, CSystem_Motor_Controller_HW_Configuration>(StringComparer.InvariantCultureIgnoreCase);
                m_ThermalControllerConfigurations                        = new Dictionary<string, CSystem_Thermal_Controller_HW_Configuration>(StringComparer.InvariantCultureIgnoreCase);
                m_TEC_Channel_Configurations                             = new Dictionary<string, CTEC_Channel_Configuration>(StringComparer.InvariantCultureIgnoreCase);
                m_OpticalFilter_Y_Motor_ChannelConfiguration             = new CSystem_Motor_HW_Channel_Configuration();
                m_Camera_X_Motor_ChannelConfiguration                    = new CSystem_Motor_HW_Channel_Configuration();
                m_PCR_Fan_and_HeatSink_Configuration                     = new CPCR_Fan_and_HeatSink_Configuration();
                m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration = new CPCR_Thermal_Block_Fan_Control_Thermistor_Configuration();
                m_SystemExhaustFans                                      = new CSystemExhaustFans();
                m_OpticalFilter_Y_Motor_ChannelCalibration               = new COptical_Filter_Motor_HW_Channel_Calibration();
                m_Camera_X_Motor_ChannelCalibration                      = new CCamera_Motor_HW_Channel_Calibration();
                m_Pneumatic_Regulator_Calibration                        = new CPneumatic_Regulator_Calibration();
                m_iIdlePressureSetpointinPSI                             = 0; 
                m_PumpIOControls                                         = new CPumpIOControls();
                m_RegulatorIOControls                                    = new CRegulatorIOControls();
                m_PneumaticIOControls                                    = new CPneumaticIOControls();
                m_OpticalEmitterConfiguration                            = new COpticalEmitterConfiguration();
                m_Camera_Information                                     = new CCamera_Information();
            }

            public CSystem_Instrument_Configuration                                  m_InstrumentConfiguration;
            public CSystem_Misc_Configuration                                        m_MiscellaneousConfiguration;
            public Dictionary<string, CSystem_Motor_Controller_HW_Configuration>     m_MotorControllerConfigurations;
            public Dictionary<string, CSystem_Thermal_Controller_HW_Configuration>   m_ThermalControllerConfigurations;
            public Dictionary<string, CTEC_Channel_Configuration>                    m_TEC_Channel_Configurations;
            public CSystem_Motor_HW_Channel_Configuration                            m_OpticalFilter_Y_Motor_ChannelConfiguration;
            public CSystem_Motor_HW_Channel_Configuration                            m_Camera_X_Motor_ChannelConfiguration;
            public CPCR_Fan_and_HeatSink_Configuration                               m_PCR_Fan_and_HeatSink_Configuration;
            public CPCR_Thermal_Block_Fan_Control_Thermistor_Configuration           m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration;
            public CSystemExhaustFans                                                m_SystemExhaustFans;
            public COpticalEmitterConfiguration                                      m_OpticalEmitterConfiguration;
            public COptical_Filter_Motor_HW_Channel_Calibration                      m_OpticalFilter_Y_Motor_ChannelCalibration;
            public CCamera_Motor_HW_Channel_Calibration                              m_Camera_X_Motor_ChannelCalibration;
            public CPneumatic_Regulator_Calibration                                  m_Pneumatic_Regulator_Calibration;
            public CCamera_Information                                               m_Camera_Information;
            // Pneumatic configuration settings
            public int                                                               m_iIdlePressureSetpointinPSI;
            public CPumpIOControls                                                   m_PumpIOControls;
            public CRegulatorIOControls                                              m_RegulatorIOControls;
            public CPneumaticIOControls                                              m_PneumaticIOControls;
        }

        private void ParseInstrumentSettings(Positional_XmlElement instrsettings_node, CSystem_Configuration_Obj PCRConfigurationObj)
        {
            bool bRequiredSerialNumberFound = false;
            bool bRequiredProductNumberFound = false;
            bool bRequiredProductNameFound = false;

            foreach (Positional_XmlElement instrument_setting_node in instrsettings_node)
            {
                if (instrument_setting_node.NodeType == XmlNodeType.Element)
                {
                    if ("SERIALNO" == instrument_setting_node.Name.ToUpper())
                    {
                        if (!bRequiredSerialNumberFound)
                        {
                            PCRConfigurationObj.m_InstrumentConfiguration.m_strSerialNumber = instrument_setting_node.InnerText;
                            bRequiredSerialNumberFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument settings section has duplicate <" + instrument_setting_node.Name.ToUpper() + "> entry.");
                        }
                    }
                    else if ("PRODUCTNUMBER" == instrument_setting_node.Name.ToUpper())
                    {
                        if (!bRequiredProductNumberFound)
                        {
                            PCRConfigurationObj.m_InstrumentConfiguration.m_strProductNumber = instrument_setting_node.InnerText;
                            bRequiredProductNumberFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument settings section has duplicate <" + instrument_setting_node.Name.ToUpper() + "> entry.");
                        }
                    }
                    else if ("PRODUCTNAME" == instrument_setting_node.Name.ToUpper())
                    {
                        if (!bRequiredProductNameFound)
                        {
                            PCRConfigurationObj.m_InstrumentConfiguration.m_strProductName = instrument_setting_node.InnerText;
                            bRequiredProductNameFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument settings section has duplicate <" + instrument_setting_node.Name.ToUpper() + "> entry.");
                        }
                    }
                    else
                    {
                        // Parse error, first node after 'Measurements' must be 'Measurement'
                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument settings section contains unexpected <" + instrument_setting_node.Name.ToUpper() + "> tag.");
                    }
                }
            }
            // Verify required fields present
            if (!(bRequiredSerialNumberFound && bRequiredProductNumberFound && bRequiredProductNameFound))
            {
                throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Missing " + (((!bRequiredSerialNumberFound) ? "<SerialNo>" : (!bRequiredProductNumberFound) ? "<ProductNumber>" : "<ProductName>")) + " required following <InstrumentSettings> tag.");
            }
        }

        private void ParseMiscSettings(Positional_XmlElement miscsettings_node, CSystem_Configuration_Obj PCRConfigurationObj)
        {
            bool bOptionalSystemLogPathFound                                     = false;
            bool bOptionalSystemProtocolPathFound                                = false;
            bool bOptionalMeasurementLogPathFound                                = false;
            bool bRequiredProtocolCycleStep_MinTemperatureFound                  = false;
            bool bRequiredProtocolCycleStep_MaxTemperatureFound                  = false;
            bool bRequiredProtocolNonCycleStep_MinTemperatureFound               = false;
            bool bRequiredProtocolNonCycleStep_MinTemp_MaxHoldTimeinSecondsFound = false;
            bool bRequiredProtocolNonCycleStep_MaxTemperatureFound               = false;
            bool bRequiredThermalSamplePeriodFound                               = false;
            bool bRequiredThermalRampTimeoutFound                                = false;

            foreach (Positional_XmlElement misc_setting_node in miscsettings_node)
            {
                if (misc_setting_node.NodeType == XmlNodeType.Element)
                {
                    if ("SYSTEMLOGPATH" == misc_setting_node.Name.ToUpper())
                    {
                        if (!bOptionalSystemLogPathFound)
                        {
                            PCRConfigurationObj.m_MiscellaneousConfiguration.m_strSystemLogPath = Path.GetFullPath(misc_setting_node.InnerText);
                            bOptionalSystemLogPathFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                        }
                    }
                    if ("SYSTEMPROTOCOLPATH" == misc_setting_node.Name.ToUpper())
                    {
                        if (!bOptionalSystemProtocolPathFound)
                        {
                            PCRConfigurationObj.m_MiscellaneousConfiguration.m_strSystemProtocolPath = Path.GetFullPath(misc_setting_node.InnerText);
                            bOptionalSystemProtocolPathFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                        }
                    }
                    else if ("MEASUREMENTLOGPATH" == misc_setting_node.Name.ToUpper())
                    {
                        if (!bOptionalMeasurementLogPathFound)
                        {
                            PCRConfigurationObj.m_MiscellaneousConfiguration.m_strMeasurementLogPath = Path.GetFullPath(misc_setting_node.InnerText);
                            bOptionalMeasurementLogPathFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                        }
                    }
                    else if ("PROTOCOLCYCLESTEP_MINTEMPERATURE" == misc_setting_node.Name.ToUpper())
                    {
                        if (!bRequiredProtocolCycleStep_MinTemperatureFound)
                        {
                            PCRConfigurationObj.m_MiscellaneousConfiguration.m_fProtocolCycleStep_MinTemperature = Convert.ToSingle(misc_setting_node.InnerText);
                            bRequiredProtocolCycleStep_MinTemperatureFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                        }
                    }
                    else if ("PROTOCOLCYCLESTEP_MAXTEMPERATURE" == misc_setting_node.Name.ToUpper())
                    {
                        if (!bRequiredProtocolCycleStep_MaxTemperatureFound)
                        {
                            PCRConfigurationObj.m_MiscellaneousConfiguration.m_fProtocolCycleStep_MaxTemperature = Convert.ToSingle(misc_setting_node.InnerText);
                            bRequiredProtocolCycleStep_MaxTemperatureFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                        }
                    }
                    else if ("PROTOCOLNONCYCLESTEP_MINTEMPERATURE" == misc_setting_node.Name.ToUpper())
                    {
                        if (!bRequiredProtocolNonCycleStep_MinTemperatureFound)
                        {
                            PCRConfigurationObj.m_MiscellaneousConfiguration.m_fProtocolNonCycleStep_MinTemperature = Convert.ToSingle(misc_setting_node.InnerText);
                            bRequiredProtocolNonCycleStep_MinTemperatureFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                        }
                    }
                    else if ("PROTOCOLNONCYCLESTEP_MINTEMP_MAXHOLDTIMEINSECONDS" == misc_setting_node.Name.ToUpper())
                    {
                        if (!bRequiredProtocolNonCycleStep_MinTemp_MaxHoldTimeinSecondsFound)
                        {
                            PCRConfigurationObj.m_MiscellaneousConfiguration.m_uiProtocolNonCycleStep_MinTemp_MaxHoldTimeinSeconds = Convert.ToUInt32(misc_setting_node.InnerText);
                            bRequiredProtocolNonCycleStep_MinTemp_MaxHoldTimeinSecondsFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                        }
                    }
                    else if ("PROTOCOLNONCYCLESTEP_MAXTEMPERATURE" == misc_setting_node.Name.ToUpper())
                    {
                        if (!bRequiredProtocolNonCycleStep_MaxTemperatureFound)
                        {
                            PCRConfigurationObj.m_MiscellaneousConfiguration.m_fProtocolNonCycleStep_MaxTemperature = Convert.ToSingle(misc_setting_node.InnerText);
                            bRequiredProtocolNonCycleStep_MaxTemperatureFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                        }
                    }
                    else if ("THERMALRAMPSAMPLEPERIODINSECONDS" == misc_setting_node.Name.ToUpper())
                    {
                        if (!bRequiredThermalSamplePeriodFound)
                        {
                            PCRConfigurationObj.m_MiscellaneousConfiguration.m_fThermalRampSamplePeriodInSeconds = Convert.ToSingle(misc_setting_node.InnerText);
                            bRequiredThermalSamplePeriodFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                        }
                    }
                    else if ("THERMALRAMPTIMEOUTINSECONDS" == misc_setting_node.Name.ToUpper())
                    {
                        if (!bRequiredThermalRampTimeoutFound)
                        {
                            PCRConfigurationObj.m_MiscellaneousConfiguration.m_fThermalRampTimeoutInSeconds = Convert.ToSingle(misc_setting_node.InnerText);
                            bRequiredThermalRampTimeoutFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                        }
                    }
                    else
                    {
                        // Parse error, first node after 'Measurements' must be 'Measurement'
                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section contains unexpected <" + misc_setting_node.Name.ToUpper() + "> tag.");
                    }
                }
            }
            // Verify required fields present (none presently defined)
            if (!(bRequiredThermalSamplePeriodFound && 
                  bRequiredProtocolCycleStep_MinTemperatureFound &&
                  bRequiredProtocolCycleStep_MaxTemperatureFound &&
                  bRequiredProtocolNonCycleStep_MinTemperatureFound &&
                  bRequiredProtocolNonCycleStep_MinTemp_MaxHoldTimeinSecondsFound &&
                  bRequiredProtocolNonCycleStep_MaxTemperatureFound &&
                  bRequiredThermalRampTimeoutFound))
            {
                string strMissingElement = (!bRequiredThermalSamplePeriodFound) ? "<ThermalSamplePeriodInSeconds>" :
                                           (!bRequiredProtocolCycleStep_MinTemperatureFound) ? "<ProtocolCycleStep_MinTemperature>" :
                                           (!bRequiredProtocolCycleStep_MaxTemperatureFound) ? "<ProtocolCycleStep_MaxTemperature>" :
                                           (!bRequiredProtocolNonCycleStep_MinTemperatureFound) ? "<ProtocolNonCycleStep_MinTemperature>" :
                                           (!bRequiredProtocolNonCycleStep_MinTemp_MaxHoldTimeinSecondsFound) ? "<ProtocolNonCycleStep_MinTemp_MaxHoldTimeinSeconds>" :
                                           (!bRequiredProtocolNonCycleStep_MaxTemperatureFound) ? "<ProtocolNonCycleStep_MaxTemperature>" : "<ThermalRampTimeoutInSeconds>";
                throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Missing " + strMissingElement + " required following <MiscSettings> tag.");
            }
            if (!bOptionalSystemLogPathFound)
            {
                PCRConfigurationObj.m_MiscellaneousConfiguration.m_strSystemLogPath = Path.GetFullPath(CSystem_Defns.strDefaultSystemLoggingPath);
            }
            if (!bOptionalSystemProtocolPathFound)
            {
                PCRConfigurationObj.m_MiscellaneousConfiguration.m_strSystemProtocolPath = Path.GetFullPath(CSystem_Defns.strDefaultProtocolPath);
            }
            if (!bOptionalMeasurementLogPathFound)
            {
                PCRConfigurationObj.m_MiscellaneousConfiguration.m_strMeasurementLogPath = Path.GetFullPath(CSystem_Defns.strDefaultMeasurementLogPath);
            }
        }

        private void ParseHWSettings(Positional_XmlElement HWSettings_node, CSystem_Configuration_Obj PCRConfigurationObj)
        {
            bool bRequiredMotorControllersSectionFound = false;
            bool bRequiredThermalControllersSectionFound = false;
            bool bRequiredOpticalFilter_Y_MotorSectionFound = false;
            bool bRequiredCamera_X_MotorSectionFound = false;
            bool bRequiredPCR_Fan_Heatsink_SectionFound = false;
            bool bRequiredPCR_Thermal_Block_Fan_Control_Thermistor_SectionFound = false;
            bool bRequiredTECControllerChannelsSectionFound = false;
            bool bRequiredPneumatic_SettingsSectionFound = false;
            bool bRequiredPneumatic_IO_HandlingSectionFound = false;
            bool bRequiredEmitter_IO_Handling_SectionFound = false;
            bool bRequiredCamera_Handling_SectionFound = false;

            foreach (Positional_XmlElement hw_setting_section_node in HWSettings_node)
            {
                if (hw_setting_section_node.NodeType == XmlNodeType.Element)
                {
                    if ("MOTORCONTROLLERS" == hw_setting_section_node.Name.ToUpper())
                    {
                        if (bRequiredMotorControllersSectionFound)
                        {
                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  Duplicate <MotorControllers> entry.");
                        }
                        else
                        {
                            bRequiredMotorControllersSectionFound = true; // Required definition
                        }
                        //bool bRequiredMotorControllerSectionFound = false;
                        foreach (Positional_XmlElement motorcontrollers_subnode in hw_setting_section_node)
                        {
                            if (motorcontrollers_subnode.NodeType == XmlNodeType.Element)
                            {
                                if ("MOTORCONTROLLER" == motorcontrollers_subnode.Name.ToUpper())
                                {
                                    //bRequiredMotorControllerSectionFound = true; // Required definition
                                    bool bRequiredNameFound = false;
                                    bool bRequiredPortFound = false;
                                    CSystem_Motor_Controller_HW_Configuration MotorControllerHWConfigurationObj = new CSystem_Motor_Controller_HW_Configuration();
                                    foreach (Positional_XmlElement motorcontroller_subnode in motorcontrollers_subnode)
                                    {
                                        if (motorcontroller_subnode.NodeType == XmlNodeType.Element)
                                        {
                                            string tag_name = motorcontroller_subnode.Name.ToUpper();
                                            if ("PORT" == tag_name)
                                            {
                                                if (bRequiredPortFound)
                                                {
                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <MotorController> section has duplicate <Port> entry.");
                                                }
                                                else
                                                {
                                                    bRequiredPortFound = true; // Required definition
                                                }
                                                MotorControllerHWConfigurationObj.m_strPort = motorcontroller_subnode.InnerText;
                                            }
                                            else if ("NAME" == tag_name)
                                            {
                                                if (bRequiredNameFound)
                                                {
                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <MotorController> section has duplicate <Name> entry.");
                                                }
                                                else
                                                {
                                                    bRequiredNameFound = true; // Required definition
                                                }
                                                MotorControllerHWConfigurationObj.m_strControllerName = motorcontroller_subnode.InnerText;
                                            }
                                            else
                                            {
                                                // Unexpected tag defined - parse error
                                                throw new CPCRInstrumentSystemException("Configuration file parse error.  <Port> definition expected following <FilterMotor> tag, instead found <" + tag_name + ">.");
                                            }
                                        }
                                    }
                                    // Verify required fields present
                                    if (!(bRequiredPortFound && bRequiredNameFound))
                                    {
                                        string strMissingElement = !(bRequiredPortFound) ? "<Port>" : "<Name>";
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <MotorController> tag.");
                                    }
                                    bRequiredMotorControllersSectionFound = true;
                                    PCRConfigurationObj.m_MotorControllerConfigurations.Add(MotorControllerHWConfigurationObj.m_strControllerName, MotorControllerHWConfigurationObj);
                                }
                            }
                        }
                    }
                    else if ("THERMALCONTROLLERS" == hw_setting_section_node.Name.ToUpper())
                    {
                        if (bRequiredThermalControllersSectionFound)
                        {
                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  Duplicate <ThermalControllers> entry.");
                        }
                        else
                        {
                            bRequiredThermalControllersSectionFound = true; // Required definition
                        }
                        bool bRequiredThermalControllerSectionFound = false;
                        foreach (Positional_XmlElement thermalcontrollers_subnode in hw_setting_section_node)
                        {
                            if (thermalcontrollers_subnode.NodeType == XmlNodeType.Element)
                            {
                                if ("THERMALCONTROLLER" == thermalcontrollers_subnode.Name.ToUpper())
                                {
                                    bRequiredThermalControllerSectionFound = true; // Required definition
                                    bool bRequiredNameFound = false;
                                    bool bRequiredModelFound = false;
                                    bool bRequiredPortFound = false;
                                    CSystem_Thermal_Controller_HW_Configuration ThermalControllerHWConfigurationObj = new CSystem_Thermal_Controller_HW_Configuration();

                                    foreach (Positional_XmlElement Thermal_controller_subnode in thermalcontrollers_subnode)
                                    {
                                        if (Thermal_controller_subnode.NodeType == XmlNodeType.Element)
                                        {
                                            string tag_name = Thermal_controller_subnode.Name.ToUpper();
                                            if ("PORT" == tag_name.ToUpper())
                                            {
                                                if (bRequiredPortFound)
                                                {
                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <ThermalController> section has duplicate <Port> entry.");
                                                }
                                                else
                                                {
                                                    bRequiredPortFound = true; // Required definition
                                                }
                                                ThermalControllerHWConfigurationObj.m_strPort = Thermal_controller_subnode.InnerText;
                                            }
                                            else if ("NAME" == tag_name.ToUpper())
                                            {
                                                if (bRequiredNameFound)
                                                {
                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <ThermalController> section has duplicate <Name> entry.");
                                                }
                                                else
                                                {
                                                    bRequiredNameFound = true; // Required definition
                                                }
                                                ThermalControllerHWConfigurationObj.m_strControllerName = Thermal_controller_subnode.InnerText;
                                            }
                                            else if ("MODEL" == tag_name.ToUpper())
                                            {
                                                if (bRequiredModelFound)
                                                {
                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <ThermalController> section has duplicate <Model> entry.");
                                                }
                                                else
                                                {
                                                    bRequiredModelFound = true; // Required definition
                                                }
                                                ThermalControllerHWConfigurationObj.m_strModel = Thermal_controller_subnode.InnerText;
                                            }
                                            else
                                            {
                                                // Unexpected tag defined - parse error
                                                throw new CPCRInstrumentSystemException("Configuration file parse error.  <Port> or <Model> or <Name> definition expected following <ThermalController> tag, instead found <" + tag_name + ">.");
                                            }
                                        }
                                    }
                                    // Verify required fields present
                                    if (!(bRequiredPortFound && bRequiredNameFound && bRequiredModelFound))
                                    {
                                        string strMissingElement =   (!bRequiredPortFound) ? "<Port>" :
                                                                     (!bRequiredNameFound) ? "<Name>" : "<Model>";
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <ThermalController> tag.");
                                    }
                                    bRequiredThermalControllersSectionFound = true;
                                    PCRConfigurationObj.m_ThermalControllerConfigurations.Add(ThermalControllerHWConfigurationObj.m_strControllerName, ThermalControllerHWConfigurationObj);
                                }
                            }
                        }
                        if (!bRequiredThermalControllerSectionFound)
                        {
                            string strMissingElement = "<ThermalController>";
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <ThermalControllers> tag.");
                        }
                    }
                    else if ("OPTICALFILTER_Y_MOTOR" == hw_setting_section_node.Name.ToUpper())
                    {
                        bool bRequiredMotorControllerFound = false;
                        bool bRequiredChannelFound = false;
                        bool bRequiredMotorHomeSpeedFound = false;
                        bool bRequiredMotorStartSpeedFound = false;
                        bool bRequiredMotorMaxSpeedFound = false;
                        bool bRequiredMotorAccelSpeedFound = false;
                        bool bRequiredMotorDecelSpeedFound = false;
                        bool bRequiredMotorMoveCurrentFound = false;
                        bool bRequiredMotorHoldCurrentFound = false;
                        bool bRequiredMotorJerkFound = false;
                        bool bRequiredMotorResolutionFound = false;
                        bool bRequiredMotorProfileModeFound = false;
                        bool bRequiredMotorHomeTimeoutFound = false;
                        bool bRequiredMotorPositionMoveTimeoutFound = false;
                        bool bRequiredMotorDirectionFound = false;
                        bool bRequiredMotorMaxNumLostStepsFound = false;
                        bool bRequiredMotorEncoderEnableFound = false;
                        bool bRequiredMotorEncoderMonitorTimer_ms_Found = false;
                        bool bRequiredMotorEncoderMonitorPulseChangeThresholdFound = false;
                        bool bRequiredMotorEncoderMonitorErrorCountThresholdFound = false;
                        bool bRequiredMotorEncoderDirectionPolarityFound = false;
                        bool bRequiredMotorEncoderStartOffsetFound = false;
                        bool bRequiredMotorEncoderScalingFactorFound = false;

                        if (bRequiredOpticalFilter_Y_MotorSectionFound)
                        {
                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <HWSettings> section has duplicate <OpticalFilter_Y_Motor> entry.");
                        }
                        else
                        {
                            bRequiredOpticalFilter_Y_MotorSectionFound = true;
                        }
                        foreach (Positional_XmlElement opticalfiltermotor_subnode in hw_setting_section_node)
                        {
                            if (opticalfiltermotor_subnode.NodeType == XmlNodeType.Element)
                            {
                                string tag_name = opticalfiltermotor_subnode.Name.ToUpper();
                                if ("MOTORCONTROLLER" == tag_name)
                                {
                                    if (bRequiredMotorControllerFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <MotorController> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorControllerFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_strControllerName = opticalfiltermotor_subnode.InnerText;
                                }
                                else if ("CHANNEL" == tag_name)
                                {
                                    if (bRequiredChannelFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <Channel> entry.");
                                    }
                                    else
                                    {
                                        bRequiredChannelFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiMotorChannel = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("MOTOR_HOME_SPEED" == tag_name)
                                {
                                    if (bRequiredMotorHomeSpeedFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_home_speed> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorHomeSpeedFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiMotorHomeSpeed = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }

                                else if ("MOTOR_START_SPEED" == tag_name)
                                {
                                    if (bRequiredMotorStartSpeedFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_start_speed> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorStartSpeedFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiMotorStartSpeed = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("MOTOR_MAX_SPEED" == tag_name)
                                {
                                    if (bRequiredMotorMaxSpeedFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_max_speed> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorMaxSpeedFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiMotorMaxSpeed = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("MOTOR_ACCEL_SPEED" == tag_name)
                                {
                                    if (bRequiredMotorAccelSpeedFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_accel_speed> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorAccelSpeedFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiMotorAccel = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("MOTOR_DECEL_SPEED" == tag_name)
                                {
                                    if (bRequiredMotorDecelSpeedFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_decel_speed> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorDecelSpeedFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiMotorDecel = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("MOTOR_MOVE_CURRENT" == tag_name)
                                {
                                    if (bRequiredMotorMoveCurrentFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_move_current> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorMoveCurrentFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiMotorMoveCurrent = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("MOTOR_HOLD_CURRENT" == tag_name)
                                {
                                    if (bRequiredMotorHoldCurrentFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_hold_current> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorHoldCurrentFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiMotorHoldCurrent = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("MOTOR_JERK" == tag_name)
                                {
                                    if (bRequiredMotorJerkFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_jerk> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorJerkFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiMotorJerk = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("MOTOR_RESOLUTION" == tag_name)
                                {
                                    if (bRequiredMotorResolutionFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_resolution> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorResolutionFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiMotorResolution = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("MOTOR_PROFILE_MODE" == tag_name)
                                {
                                    if (bRequiredMotorProfileModeFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_profile_mode> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorProfileModeFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiMotorProfileMode = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("MOTOR_HOME_TIMEOUT" == tag_name)
                                {
                                    if (bRequiredMotorHomeTimeoutFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_home_timeout> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorHomeTimeoutFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiMotorHomeTimeout = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("AXIS_MOVE_TIMEOUT" == tag_name)
                                {
                                    if (bRequiredMotorPositionMoveTimeoutFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <axis_move_timeout> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorPositionMoveTimeoutFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiPosition_Move_Timeout = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("MOTOR_DIRECTION" == tag_name)
                                {
                                    if (bRequiredMotorDirectionFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_direction> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorDirectionFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiMotorDirection = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("MOTOR_MAX_NUM_LOST_STEPS" == tag_name)
                                {
                                    if (bRequiredMotorMaxNumLostStepsFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_max_num_lost_steps> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorMaxNumLostStepsFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiMotorMaxNumLostSteps = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("ENCODER_ENABLE" == tag_name)
                                {
                                    if (bRequiredMotorEncoderEnableFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <encoder_enable> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorEncoderEnableFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_bEncoderEnabled = (opticalfiltermotor_subnode.InnerText == "1") ||
                                                                                                                     (opticalfiltermotor_subnode.InnerText.ToUpper() == "ON") ||
                                                                                                                     (opticalfiltermotor_subnode.InnerText.ToUpper() == "YES");
                                }
                                else if ("ENCODER_MONITOR_TIMER_MS" == tag_name)
                                {
                                    if (bRequiredMotorEncoderMonitorTimer_ms_Found)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <encoder_monitor_timer_ms> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorEncoderMonitorTimer_ms_Found = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiEncoderMonitorTimer_ms = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("ENCODER_MONITOR_PULSE_CHANGE_THRESHOLD" == tag_name)
                                {
                                    if (bRequiredMotorEncoderMonitorPulseChangeThresholdFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <encoder_monitor_pulse_change_threshold> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorEncoderMonitorPulseChangeThresholdFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiEncoderMonitorPulseChangeThreshold = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("ENCODER_MONITOR_ERROR_COUNT_THRESHOLD" == tag_name)
                                {
                                    if (bRequiredMotorEncoderMonitorErrorCountThresholdFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <encoder_monitor_error_count_threshold> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorEncoderMonitorErrorCountThresholdFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiEncoderMonitorErrorCountThreshold = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("ENCODER_DIRECTION_POLARITY" == tag_name)
                                {
                                    if (bRequiredMotorEncoderDirectionPolarityFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <encoder_direction_polarity> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorEncoderDirectionPolarityFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_uiEncoderDirectionPolarity = Convert.ToUInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("ENCODER_START_OFFSET" == tag_name)
                                {
                                    if (bRequiredMotorEncoderStartOffsetFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <encoder_start_offset> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorEncoderStartOffsetFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_iEncoderStartOffset = Convert.ToInt32(opticalfiltermotor_subnode.InnerText);
                                }
                                else if ("ENCODER_SCALING_FACTOR" == tag_name)
                                {
                                    if (bRequiredMotorEncoderScalingFactorFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <encoder_scaling_factor> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorEncoderScalingFactorFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_OpticalFilter_Y_Motor_ChannelConfiguration.m_fEncoderScalingFactor = Convert.ToSingle(opticalfiltermotor_subnode.InnerText);
                                }
                                else
                                {
                                    // Unexpected tag defined - parse error
                                    throw new CPCRInstrumentSystemException("Configuration file parse error.  <Name> and <Channel> definition expected following <OpticalFilter_Y_Motor> tag, instead found <" + tag_name + ">.");
                                }
                            }
                        }

                        // Verify required fields present
                        if (!(bRequiredMotorControllerFound &&
                              bRequiredChannelFound &&
                              bRequiredMotorHomeSpeedFound &&
                              bRequiredMotorStartSpeedFound &&
                              bRequiredMotorMaxSpeedFound &&
                              bRequiredMotorAccelSpeedFound &&
                              bRequiredMotorDecelSpeedFound &&
                              bRequiredMotorMoveCurrentFound &&
                              bRequiredMotorHoldCurrentFound &&
                              bRequiredMotorJerkFound &&
                              bRequiredMotorResolutionFound &&
                              bRequiredMotorProfileModeFound &&
                              bRequiredMotorHomeTimeoutFound &&
                              bRequiredMotorPositionMoveTimeoutFound &&
                              bRequiredMotorDirectionFound &&
                              bRequiredMotorMaxNumLostStepsFound &&
                              bRequiredMotorEncoderEnableFound &&
                              bRequiredMotorEncoderMonitorTimer_ms_Found &&
                              bRequiredMotorEncoderMonitorPulseChangeThresholdFound &&
                              bRequiredMotorEncoderMonitorErrorCountThresholdFound &&
                              bRequiredMotorEncoderDirectionPolarityFound &&
                              bRequiredMotorEncoderStartOffsetFound &&
                              bRequiredMotorEncoderScalingFactorFound
                          ))
                        {
                            string strMissingElement = (!bRequiredMotorControllerFound) ? "<MotorController>" :
                                                        (!bRequiredChannelFound) ? "<Channel>" :
                                                        (!bRequiredMotorHomeSpeedFound) ? "<motor_home_speed?" :
                                                        (!bRequiredMotorStartSpeedFound) ? "<motor_start_speed>" :
                                                        (!bRequiredMotorMaxSpeedFound) ? "<motor_max_speed>" :
                                                        (!bRequiredMotorAccelSpeedFound) ? "<motor_accel_speed>" :
                                                        (!bRequiredMotorDecelSpeedFound) ? "<motor_decel_speed>" :
                                                        (!bRequiredMotorMoveCurrentFound) ? "<motor_move_current>" :
                                                        (!bRequiredMotorHoldCurrentFound) ? "<motor_hold_current>" :
                                                        (!bRequiredMotorJerkFound) ? "<motor_jerk>" :
                                                        (!bRequiredMotorResolutionFound) ? "<motor_resolution>" :
                                                        (!bRequiredMotorProfileModeFound) ? "<motor_profile_mode>" :
                                                        (!bRequiredMotorHomeTimeoutFound) ? "<motor_home_timeout>" :
                                                        (!bRequiredMotorPositionMoveTimeoutFound) ? "<axis_move_timeout>" :
                                                        (!bRequiredMotorDirectionFound) ? "<motor_direction>" :
                                                        (!bRequiredMotorMaxNumLostStepsFound) ? "<motor_max_num_lost_steps>" :
                                                        (!bRequiredMotorEncoderEnableFound) ? "encoder_enable" :
                                                        (!bRequiredMotorEncoderMonitorTimer_ms_Found) ? "<encoder_monitor_timer_ms>" :
                                                        (!bRequiredMotorEncoderMonitorPulseChangeThresholdFound) ? "<encoder_monitor_pulse_change_threshold>" :
                                                        (!bRequiredMotorEncoderMonitorErrorCountThresholdFound) ? "<encoder_monitor_error_count_threshold>" :
                                                        (!bRequiredMotorEncoderDirectionPolarityFound) ? "<encoder_direction_polarity>" :
                                                        (!bRequiredMotorEncoderStartOffsetFound) ? "<encoder_start_offset>" :
                                                        (!bRequiredMotorEncoderScalingFactorFound) ? "encoder_scaling_factor" : "???";
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <OpticalFilter_Y_Motor> tag.");
                        }
                    }
                    else if ("CAMERA_X_MOTOR" == hw_setting_section_node.Name.ToUpper())
                    {
                        bool bRequiredMotorControllerFound = false;
                        bool bRequiredChannelFound = false;
                        bool bRequiredMotorHomeSpeedFound = false;
                        bool bRequiredMotorStartSpeedFound = false;
                        bool bRequiredMotorMaxSpeedFound = false;
                        bool bRequiredMotorAccelSpeedFound = false;
                        bool bRequiredMotorDecelSpeedFound = false;
                        bool bRequiredMotorMoveCurrentFound = false;
                        bool bRequiredMotorHoldCurrentFound = false;
                        bool bRequiredMotorJerkFound = false;
                        bool bRequiredMotorResolutionFound = false;
                        bool bRequiredMotorProfileModeFound = false;
                        bool bRequiredMotorHomeTimeoutFound = false;
                        bool bRequiredMotorMoveTimeoutFound = false;
                        bool bRequiredMotorDirectionFound = false;
                        bool bRequiredMotorMaxNumLostStepsFound = false;
                        bool bRequiredMotorEncoderEnableFound = false;
                        bool bRequiredMotorEncoderMonitorTimer_ms_Found = false;
                        bool bRequiredMotorEncoderMonitorPulseChangeThresholdFound = false;
                        bool bRequiredMotorEncoderMonitorErrorCountThresholdFound = false;
                        bool bRequiredMotorEncoderDirectionPolarityFound = false;
                        bool bRequiredMotorEncoderStartOffsetFound = false;
                        bool bRequiredMotorEncoderScalingFactorFound = false;

                        if (bRequiredCamera_X_MotorSectionFound)
                        {
                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <HWSettings> section has duplicate <Camera_X_Motor> entry.");
                        }
                        else
                        {
                            bRequiredCamera_X_MotorSectionFound = true;
                        }
                        foreach (Positional_XmlElement Camera_X_Motor_subnode in hw_setting_section_node)
                        {
                            if (Camera_X_Motor_subnode.NodeType == XmlNodeType.Element)
                            {
                                string tag_name = Camera_X_Motor_subnode.Name.ToUpper();
                                if ("MOTORCONTROLLER" == tag_name)
                                {
                                    if (bRequiredMotorControllerFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <MotorController> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorControllerFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_strControllerName = Camera_X_Motor_subnode.InnerText;
                                }
                                else if ("CHANNEL" == tag_name)
                                {
                                    if (bRequiredChannelFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <Channel> entry.");
                                    }
                                    else
                                    {
                                        bRequiredChannelFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiMotorChannel = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("MOTOR_HOME_SPEED" == tag_name)
                                {
                                    if (bRequiredMotorHomeSpeedFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <motor_home_speed> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorHomeSpeedFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiMotorHomeSpeed = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }

                                else if ("MOTOR_START_SPEED" == tag_name)
                                {
                                    if (bRequiredMotorStartSpeedFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <motor_start_speed> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorStartSpeedFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiMotorStartSpeed = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("MOTOR_MAX_SPEED" == tag_name)
                                {
                                    if (bRequiredMotorMaxSpeedFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <motor_max_speed> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorMaxSpeedFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiMotorMaxSpeed = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("MOTOR_ACCEL_SPEED" == tag_name)
                                {
                                    if (bRequiredMotorAccelSpeedFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <motor_accel_speed> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorAccelSpeedFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiMotorAccel = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("MOTOR_DECEL_SPEED" == tag_name)
                                {
                                    if (bRequiredMotorDecelSpeedFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <motor_decel_speed> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorDecelSpeedFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiMotorDecel = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("MOTOR_MOVE_CURRENT" == tag_name)
                                {
                                    if (bRequiredMotorMoveCurrentFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <motor_move_current> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorMoveCurrentFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiMotorMoveCurrent = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("MOTOR_HOLD_CURRENT" == tag_name)
                                {
                                    if (bRequiredMotorHoldCurrentFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <motor_hold_current> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorHoldCurrentFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiMotorHoldCurrent = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("MOTOR_JERK" == tag_name)
                                {
                                    if (bRequiredMotorJerkFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <motor_jerk> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorJerkFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiMotorJerk = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("MOTOR_RESOLUTION" == tag_name)
                                {
                                    if (bRequiredMotorResolutionFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <motor_resolution> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorResolutionFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiMotorResolution = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("MOTOR_PROFILE_MODE" == tag_name)
                                {
                                    if (bRequiredMotorProfileModeFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <motor_profile_mode> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorProfileModeFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiMotorProfileMode = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("MOTOR_HOME_TIMEOUT" == tag_name)
                                {
                                    if (bRequiredMotorHomeTimeoutFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <motor_home_timeout> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorHomeTimeoutFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiMotorHomeTimeout = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("AXIS_MOVE_TIMEOUT" == tag_name)
                                {
                                    if (bRequiredMotorMoveTimeoutFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <axis_move_timeout> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorMoveTimeoutFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiPosition_Move_Timeout = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("MOTOR_DIRECTION" == tag_name)
                                {
                                    if (bRequiredMotorDirectionFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <motor_direction> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorDirectionFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiMotorDirection = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("MOTOR_MAX_NUM_LOST_STEPS" == tag_name)
                                {
                                    if (bRequiredMotorMaxNumLostStepsFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <motor_max_num_lost_steps> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorMaxNumLostStepsFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiMotorMaxNumLostSteps = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("ENCODER_ENABLE" == tag_name)
                                {
                                    if (bRequiredMotorEncoderEnableFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <encoder_enable> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorEncoderEnableFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_bEncoderEnabled = (Camera_X_Motor_subnode.InnerText == "1") ||
                                                                                                                       (Camera_X_Motor_subnode.InnerText.ToUpper() == "ON") ||
                                                                                                                       (Camera_X_Motor_subnode.InnerText.ToUpper() == "YES");
                                }
                                else if ("ENCODER_MONITOR_TIMER_MS" == tag_name)
                                {
                                    if (bRequiredMotorEncoderMonitorTimer_ms_Found)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <encoder_monitor_timer_ms> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorEncoderMonitorTimer_ms_Found = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiEncoderMonitorTimer_ms = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("ENCODER_MONITOR_PULSE_CHANGE_THRESHOLD" == tag_name)
                                {
                                    if (bRequiredMotorEncoderMonitorPulseChangeThresholdFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <encoder_monitor_pulse_change_threshold> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorEncoderMonitorPulseChangeThresholdFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiEncoderMonitorPulseChangeThreshold = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("ENCODER_MONITOR_ERROR_COUNT_THRESHOLD" == tag_name)
                                {
                                    if (bRequiredMotorEncoderMonitorErrorCountThresholdFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <encoder_monitor_error_count_threshold> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorEncoderMonitorErrorCountThresholdFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiEncoderMonitorErrorCountThreshold = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("ENCODER_DIRECTION_POLARITY" == tag_name)
                                {
                                    if (bRequiredMotorEncoderDirectionPolarityFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <encoder_direction_polarity> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorEncoderDirectionPolarityFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_uiEncoderDirectionPolarity = Convert.ToUInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("ENCODER_START_OFFSET" == tag_name)
                                {
                                    if (bRequiredMotorEncoderStartOffsetFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <encoder_start_offset> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorEncoderStartOffsetFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_iEncoderStartOffset = Convert.ToInt32(Camera_X_Motor_subnode.InnerText);
                                }
                                else if ("ENCODER_SCALING_FACTOR" == tag_name)
                                {
                                    if (bRequiredMotorEncoderScalingFactorFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Camera_X_Motor> section has duplicate <encoder_scaling_factor> entry.");
                                    }
                                    else
                                    {
                                        bRequiredMotorEncoderScalingFactorFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_Camera_X_Motor_ChannelConfiguration.m_fEncoderScalingFactor = Convert.ToSingle(Camera_X_Motor_subnode.InnerText);
                                }
                                else
                                {
                                    // Unexpected tag defined - parse error
                                    throw new CPCRInstrumentSystemException("Configuration file parse error.  <Name> and <Channel> definition expected following <Camera_X_Motor> tag, instead found <" + tag_name + ">.");
                                }
                            }
                        }
                        // Verify required fields present
                        if (!(bRequiredMotorControllerFound &&
                              bRequiredChannelFound &&
                              bRequiredMotorHomeSpeedFound &&
                              bRequiredMotorStartSpeedFound &&
                              bRequiredMotorMaxSpeedFound &&
                              bRequiredMotorAccelSpeedFound &&
                              bRequiredMotorDecelSpeedFound &&
                              bRequiredMotorMoveCurrentFound &&
                              bRequiredMotorHoldCurrentFound &&
                              bRequiredMotorJerkFound &&
                              bRequiredMotorResolutionFound &&
                              bRequiredMotorProfileModeFound &&
                              bRequiredMotorHomeTimeoutFound &&
                              bRequiredMotorMoveTimeoutFound &&
                              bRequiredMotorDirectionFound &&
                              bRequiredMotorMaxNumLostStepsFound &&
                              bRequiredMotorEncoderEnableFound &&
                              bRequiredMotorEncoderMonitorTimer_ms_Found &&
                              bRequiredMotorEncoderMonitorPulseChangeThresholdFound &&
                              bRequiredMotorEncoderMonitorErrorCountThresholdFound &&
                              bRequiredMotorEncoderDirectionPolarityFound &&
                              bRequiredMotorEncoderStartOffsetFound &&
                              bRequiredMotorEncoderScalingFactorFound
                            ))
                        {
                            string strMissingElement = (!bRequiredMotorControllerFound)                         ? "<MotorController>" :
                                                       (!bRequiredChannelFound)                                 ? "<channel>" :
                                                       (!bRequiredMotorHomeSpeedFound)                          ? "<motor_home_speed>" :
                                                       (!bRequiredMotorStartSpeedFound)                         ? "<motor_start_speed>" :
                                                       (!bRequiredMotorMaxSpeedFound)                           ? "<motor_max_speed>" :
                                                       (!bRequiredMotorAccelSpeedFound)                         ? "<motor_accel_speed>" :
                                                       (!bRequiredMotorDecelSpeedFound)                         ? "<motor_decel_speed>" :
                                                       (!bRequiredMotorMoveCurrentFound)                        ? "<motor_move_current>" :
                                                       (!bRequiredMotorHoldCurrentFound)                        ? "<motor_hold_current>" :
                                                       (!bRequiredMotorJerkFound)                               ? "<motor_jerk>" : 
                                                       (!bRequiredMotorResolutionFound)                         ? "<motor_resolution>" :
                                                       (!bRequiredMotorProfileModeFound)                        ? "<motor_profile_mode>" :
                                                       (!bRequiredMotorHomeTimeoutFound)                        ? "<motor_home_timeout>" :
                                                       (!bRequiredMotorMoveTimeoutFound)                        ? "<axis_move_timeout>" :
                                                       (!bRequiredMotorDirectionFound)                          ? "<motor_direction>" :
                                                       (!bRequiredMotorMaxNumLostStepsFound)                    ? "<motor_max_num_lost_steps>" :
                                                       (!bRequiredMotorEncoderEnableFound)                      ? "<encoder_enable>" :
                                                       (!bRequiredMotorEncoderMonitorTimer_ms_Found)            ? "<encoder_monitor_timer_ms>" :
                                                       (!bRequiredMotorEncoderMonitorPulseChangeThresholdFound) ? "<encoder_monitor_pulse_change_threshold>" :
                                                       (!bRequiredMotorEncoderMonitorErrorCountThresholdFound)  ? "<encoder_monitor_error_count_threshold>" :
                                                       (!bRequiredMotorEncoderDirectionPolarityFound)           ? "<encoder_direction_polarity>" :
                                                       (!bRequiredMotorEncoderStartOffsetFound)                 ? "<encoder_start_offset>" :
                                                       (!bRequiredMotorEncoderScalingFactorFound)               ? "encoder_scaling_factor" : "???";
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Camera_X_Motor> tag.");
                        }
                    }
                    else if ("PCR_HEATSINK_FAN" == hw_setting_section_node.Name.ToUpper())
                    {
                        bool bRequiredTECControllerNameFound = false;
                        bool bRequiredAddressFound = false;
                        bool bRequiredSampletimeFound = false;
                        bool bRequiredBandsFound = false;

                        CPCR_Fan_and_HeatSink_Configuration PCR_TEC_Fan_Heatsink_ConfigurationObj = new CPCR_Fan_and_HeatSink_Configuration();
                        if (bRequiredPCR_Fan_Heatsink_SectionFound)
                        {
                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <HWSettings> section has duplicate <PCR_HeatSink_Fan> entry.");
                        }
                        else
                        {
                            bRequiredPCR_Fan_Heatsink_SectionFound = true;
                        }
                        foreach (XmlNode Fan_Heatsink_subnode in hw_setting_section_node)
                        {
                            if (Fan_Heatsink_subnode.NodeType == XmlNodeType.Element)
                            {
                                string tag_name = Fan_Heatsink_subnode.Name.ToUpper();
                                if ("TECCONTROLLER" == tag_name)
                                {
                                    if (bRequiredTECControllerNameFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_HeatSink_Fan> section has duplicate <TECController> entry.");
                                    }
                                    else
                                    {
                                        bRequiredTECControllerNameFound = true; // Required definition
                                    }
                                    PCR_TEC_Fan_Heatsink_ConfigurationObj.m_strControllerName = Fan_Heatsink_subnode.InnerText;
                                }
                                else if ("ADDRESS" == tag_name)
                                {
                                    if (bRequiredAddressFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_HeatSink_Fan> section has duplicate <Address> entry.");
                                    }
                                    else
                                    {
                                        bRequiredAddressFound = true; // Required definition
                                    }
                                    PCR_TEC_Fan_Heatsink_ConfigurationObj.m_uiTECAddress = Convert.ToUInt32(Fan_Heatsink_subnode.InnerText);
                                }
                                else if ("BANDS" == tag_name)
                                {
                                    if (bRequiredBandsFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_HeatSink_Fan> section has duplicate <Bands> entry.");
                                    }
                                    else
                                    {
                                        bRequiredBandsFound = true; // Required definition
                                    }

                                    bool bRequiredBandFound = false;
                                    List<CPCR_Fan_Control_Band> bandsList = new List<CPCR_Fan_Control_Band>();

                                    foreach (XmlNode Bands_subnode in Fan_Heatsink_subnode)
                                    {
                                        if (Bands_subnode.NodeType == XmlNodeType.Element)
                                        {
                                            string bandtag_name = Bands_subnode.Name.ToUpper();
                                            if ("BAND" == bandtag_name)
                                            {
                                                CPCR_Fan_Control_Band currentBand = new CPCR_Fan_Control_Band();

                                                bandsList.Add(currentBand);
                                                bRequiredBandFound = true; // Required definition
                                                bool bRequiredUpperFound = false;
                                                bool bRequiredLowerFound = false;
                                                bool bRequiredDeadbandFound = false;
                                                bool bRequiredDutyCycleFound = false;
                                                foreach (XmlNode Band_subnode in Bands_subnode)
                                                {
                                                    if (Band_subnode.NodeType == XmlNodeType.Element)
                                                    {
                                                        string innertag_name = Band_subnode.Name.ToUpper();
                                                        if ("UPPER" == innertag_name)
                                                        {
                                                            if (bRequiredUpperFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <PCR_HeatSink_Fan><Bands><Band> parse error.  <Band> section has duplicate <Upper> entry.");
                                                            }
                                                            else
                                                            {
                                                                bRequiredUpperFound = true; // Required definition
                                                            }
                                                            currentBand.m_fUpperTemperature = Convert.ToSingle(Band_subnode.InnerText);
                                                        }
                                                        else if ("LOWER" == innertag_name)
                                                        {
                                                            if (bRequiredLowerFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <PCR_HeatSink_Fan><Bands><Band> parse error.  <Band> section has duplicate <Lower> entry.");
                                                            }
                                                            else
                                                            {
                                                                bRequiredLowerFound = true; // Required definition
                                                            }
                                                            currentBand.m_fLowerTemperature = Convert.ToSingle(Band_subnode.InnerText);
                                                        }
                                                        else if ("DUTYCYCLE" == innertag_name)
                                                        {
                                                            if (bRequiredDutyCycleFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <PCR_HeatSink_Fan><Bands><Band> parse error.  <Band> section has duplicate <DutyCycle> entry.");
                                                            }
                                                            else
                                                            {
                                                                bRequiredDutyCycleFound = true; // Required definition
                                                            }
                                                            currentBand.m_uiFanDutyCycle = Convert.ToUInt32(Band_subnode.InnerText);
                                                        }
                                                        else if ("DEADBAND" == innertag_name)
                                                        {
                                                            if (bRequiredDeadbandFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <PCR_HeatSink_Fan><Bands><Band> parse error.  <Band> section has duplicate <Deadband> entry.");
                                                            }
                                                            else
                                                            {
                                                                bRequiredDeadbandFound = true; // Required definition
                                                            }
                                                            currentBand.m_fDeadband = Convert.ToSingle(Band_subnode.InnerText);
                                                        }
                                                        else
                                                        {
                                                            // Unexpected tag defined - parse error
                                                            throw new CPCRInstrumentSystemException("Configuration file parse error.  Unexpected tag <" + tag_name + "> following <Band> tag.");
                                                        }
                                                    }
                                                }
                                                // Verify required fields present
                                                if (!(bRequiredUpperFound &&
                                                      bRequiredLowerFound &&
                                                      bRequiredDeadbandFound &&
                                                      bRequiredDutyCycleFound
                                                   ))
                                                {
                                                    string strMissingElement = (!bRequiredUpperFound) ? "<Upper>" :
                                                                               (!bRequiredLowerFound) ? "<Lower>" :
                                                                               (!bRequiredDeadbandFound) ? "Deadband" : "<DutyCycle>";
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <PCR_HeatSink_Fan><Bands><Band> tag.");
                                                }
                                            }
                                            else
                                            {
                                                // Unexpected tag defined - parse error
                                                throw new CPCRInstrumentSystemException("Configuration file parse error.  Unexpected tag <" + tag_name + "> following <Bands> tag.");
                                            }
                                        }
                                    }
                                    if (!bRequiredBandFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error. At least one <Band> required following <PCR_HeatSink_Fan><Bands> tag.");
                                    }
                                    // Save <PCR__Fan> <Bands> information to configuration
                                    PCR_TEC_Fan_Heatsink_ConfigurationObj.m_Bands = bandsList.ToArray();
                                    Array.Sort(PCR_TEC_Fan_Heatsink_ConfigurationObj.m_Bands, (x, y) => y.m_fLowerTemperature.CompareTo(x.m_fLowerTemperature));
                                }
                                else if ("SAMPLETIMEINSECONDS" == tag_name)
                                {
                                    if (bRequiredSampletimeFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_HeatSink_Fan> section has duplicate <SampleTimeInSeconds> entry.");
                                    }
                                    else
                                    {
                                        bRequiredSampletimeFound = true; // Required definition
                                    }
                                    PCR_TEC_Fan_Heatsink_ConfigurationObj.m_fSampleTimeInSeconds = Convert.ToSingle(Fan_Heatsink_subnode.InnerText);
                                    // Clamp between min and max as per system CPCR_Defns
                                    if (PCR_TEC_Fan_Heatsink_ConfigurationObj.m_fSampleTimeInSeconds > CSystem_Defns.cfPCR_Safe_Heatsink_Temperature_Max_Sampling_Time_in_Seconds)
                                    {
                                        // Log warning - TBD

                                        PCR_TEC_Fan_Heatsink_ConfigurationObj.m_fSampleTimeInSeconds = CSystem_Defns.cfPCR_Safe_Heatsink_Temperature_Max_Sampling_Time_in_Seconds;
                                    }
                                    if (PCR_TEC_Fan_Heatsink_ConfigurationObj.m_fSampleTimeInSeconds < CSystem_Defns.cfPCR_Safe_Heatsink_Temperature_Min_Sampling_Time_in_Seconds)
                                    {
                                        // Log warning - TBD

                                        PCR_TEC_Fan_Heatsink_ConfigurationObj.m_fSampleTimeInSeconds = CSystem_Defns.cfPCR_Safe_Heatsink_Temperature_Min_Sampling_Time_in_Seconds;
                                    }
                                }
                                else
                                {
                                    // Unexpected tag defined - parse error
                                    throw new CPCRInstrumentSystemException("Configuration file parse error.  Unexpected tag <" + tag_name + "> following <PCR_HeatSink_Fan> tag.");
                                }
                            }
                        }
                        // Verify required fields present
                        if (!(bRequiredTECControllerNameFound &&
                              bRequiredAddressFound &&
                              bRequiredBandsFound &&
                              bRequiredSampletimeFound 
                           ))
                        {
                            string strMissingElement = (!bRequiredTECControllerNameFound) ? "<TECController>" :
                                                       (!bRequiredAddressFound) ? "<Address>" :
                                                       (!bRequiredBandsFound) ? "<Bands>" : "<SampleTimeInSeconds>";
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <PCR_Fan> tag.");
                        }
                        PCRConfigurationObj.m_PCR_Fan_and_HeatSink_Configuration = PCR_TEC_Fan_Heatsink_ConfigurationObj;
                    }
                    else if ("PCR_Thermal_Block_Fan_Control_Thermistor".ToUpper() == hw_setting_section_node.Name.ToUpper())
                    {
                        bool bRequiredTECControllerNameFound = false;
                        bool bRequiredAddressFound = false;
                        bool bRequiredChannelFound = false;
                        bool bRequiredThermACoefFound = false;
                        bool bRequiredThermBCoefFound = false;
                        bool bRequiredThermCCoefFound = false;

                        if (bRequiredPCR_Thermal_Block_Fan_Control_Thermistor_SectionFound)
                        {
                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <HWSettings> section has duplicate <PCR_Thermal_Block_Fan_Control_Thermistor> entry.");
                        }
                        else
                        {
                            bRequiredPCR_Thermal_Block_Fan_Control_Thermistor_SectionFound = true;
                        }
                        foreach (XmlNode Thermal_Block_Fan_Control_Thermistor_subnode in hw_setting_section_node)
                        {
                            if (Thermal_Block_Fan_Control_Thermistor_subnode.NodeType == XmlNodeType.Element)
                            {
                                string tag_name = Thermal_Block_Fan_Control_Thermistor_subnode.Name.ToUpper();
                                if ("TECCONTROLLER" == tag_name)
                                {
                                    if (bRequiredTECControllerNameFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_Thermal_Block_Fan_Control_Thermistor> section has duplicate <TECController> entry.");
                                    }
                                    else
                                    {
                                        bRequiredTECControllerNameFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration.m_strControllerName = Thermal_Block_Fan_Control_Thermistor_subnode.InnerText;
                                }
                                else if ("ADDRESS" == tag_name)
                                {
                                    if (bRequiredAddressFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_Thermal_Block_Fan_Control_Thermistor> section has duplicate <Address> entry.");
                                    }
                                    else
                                    {
                                        bRequiredAddressFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration.m_uiTECAddress = Convert.ToUInt32(Thermal_Block_Fan_Control_Thermistor_subnode.InnerText);
                                }
                                else if ("CHANNEL" == tag_name)
                                {
                                    if (bRequiredChannelFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_Thermal_Block_Fan_Control_Thermistor> section has duplicate <channel> entry.");
                                    }
                                    else
                                    {
                                        bRequiredChannelFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration.m_uiHeaterChannel = Convert.ToUInt32(Thermal_Block_Fan_Control_Thermistor_subnode.InnerText);
                                }
                                else if ("THERMA_COEFF" == tag_name)
                                {
                                    if (bRequiredThermACoefFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_Thermal_Block_Fan_Control_Thermistor> section has duplicate <ThermA_Coeff> entry.");
                                    }
                                    else
                                    {
                                        bRequiredThermACoefFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration.m_fThermA_Coeff = Convert.ToSingle(Thermal_Block_Fan_Control_Thermistor_subnode.InnerText);
                                }
                                else if ("THERMB_COEFF" == tag_name)
                                {
                                    if (bRequiredThermBCoefFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_Thermal_Block_Fan_Control_Thermistor> section has duplicate <ThermB_Coeff> entry.");
                                    }
                                    else
                                    {
                                        bRequiredThermBCoefFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration.m_fThermB_Coeff = Convert.ToSingle(Thermal_Block_Fan_Control_Thermistor_subnode.InnerText);
                                }
                                else if ("THERMC_COEFF" == tag_name)
                                {
                                    if (bRequiredThermCCoefFound)
                                    {
                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_Thermal_Block_Fan_Control_Thermistor> section has duplicate <ThermC_Coeff> entry.");
                                    }
                                    else
                                    {
                                        bRequiredThermCCoefFound = true; // Required definition
                                    }
                                    PCRConfigurationObj.m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration.m_fThermC_Coeff = Convert.ToSingle(Thermal_Block_Fan_Control_Thermistor_subnode.InnerText);
                                }
                                else
                                {
                                    // Unexpected tag defined - parse error
                                    throw new CPCRInstrumentSystemException("Configuration file parse error.  Unexpected tag <" + tag_name + "> following <PCR_Thermal_Block_Fan_Control_Thermistor> tag.");
                                }
                            }
                        }
                        // Verify required fields present
                        if (!(bRequiredTECControllerNameFound &&
                              bRequiredAddressFound &&
                              bRequiredChannelFound &&
                              bRequiredThermACoefFound &&
                              bRequiredThermBCoefFound &&
                              bRequiredThermCCoefFound
                           ))
                        {
                            string strMissingElement = (!bRequiredTECControllerNameFound) ? "<TECController>" :
                                                       (!bRequiredAddressFound) ? "<Address>" :
                                                       (!bRequiredChannelFound) ? "<Channel>" :
                                                       (!bRequiredThermACoefFound) ? "<ThermA_Coeff>" :
                                                       (!bRequiredThermBCoefFound) ? "<ThermB_Coeff>" : "<ThermC_Coeff>";
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <PCR_Thermal_Block_Fan_Control_Thermistor> tag.");
                        }
                    }
                    else if ("SYSTEM_EXHAUST_FAN_CONTROL" == hw_setting_section_node.Name.ToUpper())
                    {
                        bool bRequiredLeftFanSectionFound = false;
                        bool bRequiredRightFanSectionFound = false;

                        foreach (Positional_XmlElement exhaust_fans_node in hw_setting_section_node)
                        {
                            if (exhaust_fans_node.NodeType == XmlNodeType.Element)
                            {
                                if ("LEFT" == exhaust_fans_node.Name.ToUpper())
                                {
                                    if (!bRequiredLeftFanSectionFound)
                                    {
                                        bRequiredLeftFanSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  System Exhaust Fans Control section has duplicate <" + exhaust_fans_node.Name.ToUpper() + "> entry.");
                                    }

                                    bool bRequiredAccelDIOSectionFound = false;
                                    foreach (Positional_XmlElement accel_DIO_node in exhaust_fans_node)
                                    {
                                        if (accel_DIO_node.NodeType == XmlNodeType.Element)
                                        {
                                            if ("ACCEL_DIO" == accel_DIO_node.Name.ToUpper())
                                            {
                                                if (!bRequiredAccelDIOSectionFound)
                                                {
                                                    bRequiredAccelDIOSectionFound = true;
                                                }
                                                else
                                                {
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <System_Exhaust_Fan_Control> <Left> section has duplicate <" + accel_DIO_node.Name.ToUpper() + "> entry.");
                                                }

                                                bool bRequiredMotorControllerFound = false;
                                                bool bRequiredDOFound = false;
                                                bool bOptionalDutyCycleFound = false;
                                                foreach (Positional_XmlElement accel_do_node in accel_DIO_node)
                                                {
                                                    if (accel_do_node.NodeType == XmlNodeType.Element)
                                                    {
                                                        if ("MOTORCONTROLLER" == accel_do_node.Name.ToUpper())
                                                        {
                                                            if (bRequiredMotorControllerFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <System_Exhaust_Fan_Control> <Left> <Accel_DIO> section has duplicate <MotorController> entry.");
                                                            }
                                                            else
                                                            {
                                                                bRequiredMotorControllerFound = true; // Required definition
                                                                PCRConfigurationObj.m_SystemExhaustFans.m_LeftFanControl.m_strMotorController = accel_do_node.InnerText;
                                                            }
                                                        }
                                                        else if ("DO" == accel_do_node.Name.ToUpper())
                                                        {
                                                            if (bRequiredDOFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <System_Exhaust_Fan_Control> <Left> <Accel_DIO> section has duplicate <DO> entry.");
                                                            }
                                                            else
                                                            {
                                                                bRequiredDOFound = true; // Required definition
                                                                PCRConfigurationObj.m_SystemExhaustFans.m_LeftFanControl.m_uiDO_Channel = Convert.ToUInt32(accel_do_node.InnerText);
                                                            }
                                                        }
                                                        else if ("DUTYCYCLE" == accel_do_node.Name.ToUpper())
                                                        {
                                                            if (bOptionalDutyCycleFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <System_Exhaust_Fan_Control> <Left> <Accel_DIO> section has duplicate <DutyCycle> entry.");
                                                            }
                                                            else
                                                            {
                                                                bOptionalDutyCycleFound = true; // Required definition
                                                                PCRConfigurationObj.m_SystemExhaustFans.m_LeftFanControl.m_uiDutyCycle = Convert.ToUInt32(accel_do_node.InnerText);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // Unexpected tag defined - parse error
                                                            throw new CPCRInstrumentSystemException("Configuration file parse error.  <MotorController> and <DO> definitions expected following <System_Exhaust_Fan_Control> <Left> <Accel_DIO> tag, instead found <" + accel_do_node.Name + ">.");
                                                        }
                                                    }
                                                }
                                                // Verify required fields present
                                                if (!(bRequiredMotorControllerFound && bRequiredDOFound))
                                                {
                                                    string strMissingElement = (!bRequiredMotorControllerFound) ? "<MotorController>" : "<DO>";
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <System_Exhaust_Fan_Control> <Left> <Accel_DIO> tag.");
                                                }
                                            }
                                            else
                                            {
                                                // Unexpected tag defined - parse error
                                                throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <System_Exhaust_Fan_Control> <Left> tag, instead found <" + accel_DIO_node.Name + ">.");
                                            }
                                        }
                                    }
                                    if (!bRequiredAccelDIOSectionFound)
                                    {
                                        string strMissingElement = "<Accel_DIO>";
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <System_Exhaust_Fan_Control> <Left> tag.");
                                    }
                                }
                                else if ("RIGHT" == exhaust_fans_node.Name.ToUpper())
                                {
                                    if (!bRequiredRightFanSectionFound)
                                    {
                                        bRequiredRightFanSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  System Exhaust Fans Control section has duplicate <" + exhaust_fans_node.Name.ToUpper() + "> entry.");
                                    }

                                    bool bRequiredAccelDIOSectionFound = false;
                                    foreach (Positional_XmlElement accel_DIO_node in exhaust_fans_node)
                                    {
                                        if (accel_DIO_node.NodeType == XmlNodeType.Element)
                                        {
                                            if ("ACCEL_DIO" == accel_DIO_node.Name.ToUpper())
                                            {
                                                if (!bRequiredAccelDIOSectionFound)
                                                {
                                                    bRequiredAccelDIOSectionFound = true;
                                                }
                                                else
                                                {
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <System_Exhaust_Fan_Control> <Right> section has duplicate <" + accel_DIO_node.Name.ToUpper() + "> entry.");
                                                }

                                                bool bRequiredMotorControllerFound = false;
                                                bool bRequiredDOFound = false;
                                                bool bOptionalDutyCycleFound = false;
                                                foreach (Positional_XmlElement accel_do_node in accel_DIO_node)
                                                {
                                                    if (accel_do_node.NodeType == XmlNodeType.Element)
                                                    {
                                                        if ("MOTORCONTROLLER" == accel_do_node.Name.ToUpper())
                                                        {
                                                            if (bRequiredMotorControllerFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <System_Exhaust_Fan_Control> <Right> <Accel_DIO> section has duplicate <MotorController> entry.");
                                                            }
                                                            else
                                                            {
                                                                bRequiredMotorControllerFound = true; // Required definition
                                                                PCRConfigurationObj.m_SystemExhaustFans.m_RightFanControl.m_strMotorController = accel_do_node.InnerText;
                                                            }
                                                        }
                                                        else if ("DO" == accel_do_node.Name.ToUpper())
                                                        {
                                                            if (bRequiredDOFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <System_Exhaust_Fan_Control> <Right> <Accel_DIO> section has duplicate <DO> entry.");
                                                            }
                                                            else
                                                            {
                                                                bRequiredDOFound = true; // Required definition
                                                                PCRConfigurationObj.m_SystemExhaustFans.m_RightFanControl.m_uiDO_Channel = Convert.ToUInt32(accel_do_node.InnerText);
                                                            }
                                                        }
                                                        else if ("DUTYCYCLE" == accel_do_node.Name.ToUpper())
                                                        {
                                                            if (bOptionalDutyCycleFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <System_Exhaust_Fan_Control> <Right> <Accel_DIO> section has duplicate <DutyCycle> entry.");
                                                            }
                                                            else
                                                            {
                                                                bOptionalDutyCycleFound = true; // Required definition
                                                                PCRConfigurationObj.m_SystemExhaustFans.m_RightFanControl.m_uiDutyCycle = Convert.ToUInt32(accel_do_node.InnerText);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // Unexpected tag defined - parse error
                                                            throw new CPCRInstrumentSystemException("Configuration file parse error.  <MotorController> and <DO> definitions expected following <System_Exhaust_Fan_Control> <Right> <Accel_DIO> tag, instead found <" + accel_do_node.Name + ">.");
                                                        }
                                                    }
                                                }
                                                // Verify required fields present
                                                if (!(bRequiredMotorControllerFound && bRequiredDOFound))
                                                {
                                                    string strMissingElement = (!bRequiredMotorControllerFound) ? "<MotorController>" : "<DO>";
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <System_Exhaust_Fan_Control> <Right> <Accel_DIO> tag.");
                                                }
                                            }
                                            else
                                            {
                                                // Unexpected tag defined - parse error
                                                throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <System_Exhaust_Fan_Control> <Right> tag, instead found <" + accel_DIO_node.Name + ">.");
                                            }
                                        }
                                    }
                                    if (!bRequiredAccelDIOSectionFound)
                                    {
                                        string strMissingElement = "<Accel_DIO>";
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <System_Exhaust_Fan_Control> <Right> tag.");
                                    }
                                }
                                else
                                {
                                    // Unexpected tag defined - parse error
                                    throw new CPCRInstrumentSystemException("Configuration file parse error.  Unexpected tag <" + exhaust_fans_node.Name + "> following <System_Exhaust_Fan_Control> tag.");
                                }
                            }
                        }
                        // Verify required fields present
                        if (!(bRequiredLeftFanSectionFound && bRequiredRightFanSectionFound))
                        {
                            string strMissingElement = (!bRequiredLeftFanSectionFound) ? "<Left>" : "<Right>";
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <System_Exhaust_Fan_Control> tag.");
                        }
                    }
                    else if ("TECCONTROLLERCHANNELS" == hw_setting_section_node.Name.ToUpper())
                    {
                        Dictionary<string, bool> bTECEntriesFound = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);

                        if (bRequiredTECControllerChannelsSectionFound)
                        {
                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <HWSettings> section has duplicate <" + hw_setting_section_node + "> entry.");
                        }
                        else
                        {
                            bRequiredTECControllerChannelsSectionFound = true;
                        }

                        foreach (Positional_XmlElement TECChannel_node in hw_setting_section_node)
                        {
                            if (TECChannel_node.NodeType == XmlNodeType.Element)
                            {
                                string TEC_tag_name = TECChannel_node.Name.ToUpper();
                                foreach (string strTEC_Entry in CSystem_Defns.cstrTECEntries)
                                {
                                    bool bFound;
                                    bool bRequiredTECControllerNameFound = false;
                                    bool bRequiredAddressFound = false;
                                    bool bRequiredThermA_CoeffFound = false;
                                    bool bRequiredThermB_CoeffFound = false;
                                    bool bRequiredThermC_CoeffFound = false;
                                    bool bRequiredPIDsFound = false;
                                    bool bRequiredControlPIDSampleTimeInSecondsFound = false;
                                    bool bRequiredErrorTermBandFound = false;
                                    bool bRequiredErrorTermCountFound = false;
                                    bool bRequiredSteadyStatePowerLimitFound = false;
                                    bool bRequiredSteadyStatePowerLimitCountFound = false;
                                    CTEC_Channel_Configuration PCR_Thermal_TEC_ConfigurationObj = new CTEC_Channel_Configuration();

                                    if (strTEC_Entry.ToUpper() == TEC_tag_name)
                                    {
                                        if (bTECEntriesFound.TryGetValue(strTEC_Entry, out bFound))
                                        {
                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <TECControllerChannels> section has duplicate <" + strTEC_Entry + "> entry.");
                                        }
                                        else
                                        {
                                            bTECEntriesFound.Add(strTEC_Entry, true);
                                        }

                                        foreach (Positional_XmlElement TEC_subnode in TECChannel_node)
                                        {
                                            if (TEC_subnode.NodeType == XmlNodeType.Element)
                                            {
                                                string tag_name = TEC_subnode.Name.ToUpper();
                                                if ("TECCONTROLLER" == tag_name)
                                                {
                                                    if (bRequiredTECControllerNameFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <" + strTEC_Entry + "> section has duplicate <TECController> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredTECControllerNameFound = true; // Required definition
                                                    }
                                                    PCR_Thermal_TEC_ConfigurationObj.m_strControllerName = TEC_subnode.InnerText;
                                                }
                                                else if ("ADDRESS" == tag_name)
                                                {
                                                    if (bRequiredAddressFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <" + strTEC_Entry + "> section has duplicate <Address> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredAddressFound = true; // Required definition
                                                    }
                                                    PCR_Thermal_TEC_ConfigurationObj.m_uiTECAddress = Convert.ToUInt32(TEC_subnode.InnerText);
                                                }
                                                else if ("THERMA_COEFF" == tag_name)
                                                {
                                                    if (bRequiredThermA_CoeffFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <" + strTEC_Entry + "> section has duplicate <ThermA_Coeff> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredThermA_CoeffFound = true; // Required definition
                                                    }
                                                    PCR_Thermal_TEC_ConfigurationObj.m_fThermA_Coeff = Convert.ToSingle(TEC_subnode.InnerText);
                                                }
                                                else if ("THERMB_COEFF" == tag_name)
                                                {
                                                    if (bRequiredThermB_CoeffFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <" + strTEC_Entry + "> section has duplicate <ThermB_Coeff> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredThermB_CoeffFound = true; // Required definition
                                                    }
                                                    PCR_Thermal_TEC_ConfigurationObj.m_fThermB_Coeff = Convert.ToSingle(TEC_subnode.InnerText);
                                                }
                                                else if ("THERMC_COEFF" == tag_name)
                                                {
                                                    if (bRequiredThermC_CoeffFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <" + strTEC_Entry + "> section has duplicate <ThermC_Coeff> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredThermC_CoeffFound = true; // Required definition
                                                    }
                                                    PCR_Thermal_TEC_ConfigurationObj.m_fThermC_Coeff = Convert.ToSingle(TEC_subnode.InnerText);
                                                }
                                                else if ("CONTROLPIDSAMPLETIMEINSECONDS" == tag_name)
                                                {
                                                    if (bRequiredControlPIDSampleTimeInSecondsFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <" + strTEC_Entry + "> section has duplicate <ControlPIDSampleTimeInSeconds> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredControlPIDSampleTimeInSecondsFound = true; // Required definition
                                                    }
                                                    PCR_Thermal_TEC_ConfigurationObj.m_ControlPIDSampleTimeInSeconds = Convert.ToSingle(TEC_subnode.InnerText);
                                                }
                                                else if ("PIDS" == tag_name)
                                                {
                                                    if (bRequiredPIDsFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <" + strTEC_Entry + "> section has duplicate <PIDs> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredPIDsFound = true; // Required definition
                                                    }

                                                    bool bRequiredStepFound = false;

                                                    foreach (Positional_XmlElement Step_node in TEC_subnode)
                                                    {
                                                        if (Step_node.NodeType == XmlNodeType.Element)
                                                        {
                                                            string steptag_name = Step_node.Name.ToUpper();

                                                            if ("STEP" == steptag_name)
                                                            {
                                                                string strStepSectionName = "<Step>";
                                                                if (bRequiredStepFound)
                                                                {
                                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <" + strTEC_Entry + "> <PIDs> section has duplicate <" + tag_name + "> entry.");
                                                                }
                                                                else
                                                                {
                                                                    bRequiredStepFound = true;
                                                                }

                                                                bool bIsRampUpSection = false;
                                                                CTEC_From_Temperature_PID_Element from_temp_obj = null;
                                                                CTEC_HW_Ramping_Configuration default_temp_obj = null;
                                                                CTEC_HW_Ramping_Configuration to_temp_obj = null;
                                                                bool bRequiredRampUpFound = false;
                                                                bool bRequiredRampDownFound = false;

                                                                foreach (Positional_XmlElement Step_subnode in Step_node)
                                                                {
                                                                    if (Step_subnode.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        string Step_subnode_tag_name = Step_subnode.Name.ToUpper();

                                                                        if (("RAMPUP" == Step_subnode_tag_name) || ("RAMPDOWN" == Step_subnode_tag_name))
                                                                        {
                                                                            string rampsectionName;
                                                                            if ("RAMPUP" == Step_subnode_tag_name)
                                                                            {
                                                                                if (bRequiredRampUpFound)
                                                                                {
                                                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " section has duplicate <RampUp> entry.");
                                                                                }
                                                                                else
                                                                                {
                                                                                    bIsRampUpSection = true;
                                                                                    rampsectionName = " <RampUp>";
                                                                                    bRequiredRampUpFound = true; // Required definition
                                                                                }
                                                                            }
                                                                            else
                                                                            {   // "RampDown"
                                                                                if (bRequiredRampDownFound)
                                                                                {
                                                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " section has duplicate <RampDown> entry.");
                                                                                }
                                                                                else
                                                                                {
                                                                                    bIsRampUpSection = false;
                                                                                    rampsectionName = " <RampDown>";
                                                                                    bRequiredRampDownFound = true; // Required definition
                                                                                }
                                                                            }

                                                                            bool bRequiredFromTempsFound = false;

                                                                            foreach (Positional_XmlElement FromTemps_node in Step_subnode)
                                                                            {
                                                                                if (FromTemps_node.NodeType == XmlNodeType.Element)
                                                                                {
                                                                                    string FromTemps_node_tag_name = FromTemps_node.Name.ToUpper();

                                                                                    if ("FROMTEMPS" == FromTemps_node_tag_name)
                                                                                    {
                                                                                        if (bRequiredFromTempsFound)
                                                                                        {
                                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " section has duplicate <" + FromTemps_node_tag_name + "> entry.");
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            bRequiredFromTempsFound = true;
                                                                                        }

                                                                                        bool bOptionalFromTempFound = false;
                                                                                        bool bRequiredDefaultFound = false;

                                                                                        foreach (Positional_XmlElement FromTemps_subnode in FromTemps_node)
                                                                                        {
                                                                                            if (FromTemps_subnode.NodeType == XmlNodeType.Element)
                                                                                            {
                                                                                                string From_node_tag_name = FromTemps_subnode.Name.ToUpper();

                                                                                                if ("FROM" == From_node_tag_name)
                                                                                                {
                                                                                                    bool bRequiredLowTempFound = false;
                                                                                                    bool bRequiredHighTempFound = false;
                                                                                                    bool bRequiredToTempsFound = false;
                                                                                                    from_temp_obj = new CTEC_From_Temperature_PID_Element();

                                                                                                    bOptionalFromTempFound = true;

                                                                                                    foreach (Positional_XmlElement From_subnode in FromTemps_subnode)
                                                                                                    {
                                                                                                        if (From_subnode.NodeType == XmlNodeType.Element)
                                                                                                        {
                                                                                                            string From_subnode_tag_name = From_subnode.Name.ToUpper();

                                                                                                            if ("LOWTEMP" == From_subnode_tag_name)
                                                                                                            {
                                                                                                                if (bRequiredLowTempFound)
                                                                                                                {
                                                                                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> section has duplicate <" + From_subnode_tag_name + "> entry.");
                                                                                                                }
                                                                                                                else
                                                                                                                {
                                                                                                                    bRequiredLowTempFound = true;
                                                                                                                }
                                                                                                                from_temp_obj.m_TemperatureRange.m_fLowTemperature = Convert.ToSingle(From_subnode.InnerText);
                                                                                                            }
                                                                                                            else if ("HIGHTEMP" == From_subnode_tag_name)
                                                                                                            {
                                                                                                                if (bRequiredHighTempFound)
                                                                                                                {
                                                                                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> section has duplicate <" + From_subnode_tag_name + "> entry.");
                                                                                                                }
                                                                                                                else
                                                                                                                {
                                                                                                                    bRequiredHighTempFound = true;
                                                                                                                }
                                                                                                                from_temp_obj.m_TemperatureRange.m_fHighTemperature = Convert.ToSingle(From_subnode.InnerText);
                                                                                                            }
                                                                                                            else if ("TOTEMPS" == From_subnode_tag_name)
                                                                                                            {
                                                                                                                if (bRequiredToTempsFound)
                                                                                                                {
                                                                                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> section has duplicate <" + From_subnode_tag_name + "> entry.");
                                                                                                                }
                                                                                                                else
                                                                                                                {
                                                                                                                    bRequiredToTempsFound = true;
                                                                                                                }

                                                                                                                bool bRequiredToTempFound = false;

                                                                                                                foreach (Positional_XmlElement To_node in From_subnode)
                                                                                                                {
                                                                                                                    if (To_node.NodeType == XmlNodeType.Element)
                                                                                                                    {
                                                                                                                        string to_node_tag_name = To_node.Name.ToUpper();

                                                                                                                        if ("TO" == to_node_tag_name)
                                                                                                                        {
                                                                                                                            bool bRequiredToLowTempFound = false;
                                                                                                                            bool bRequiredToHighTempFound = false;
                                                                                                                            bool bRequiredParamsFound = false;
                                                                                                                            to_temp_obj = new CTEC_HW_Ramping_Configuration();

                                                                                                                            bRequiredToTempFound = true;

                                                                                                                            foreach (Positional_XmlElement To_subnode in To_node)
                                                                                                                            {
                                                                                                                                if (To_subnode.NodeType == XmlNodeType.Element)
                                                                                                                                {
                                                                                                                                    string To_subnode_tag_name = To_subnode.Name.ToUpper();

                                                                                                                                    if ("LOWTEMP" == To_subnode_tag_name)
                                                                                                                                    {
                                                                                                                                        if (bRequiredToLowTempFound)
                                                                                                                                        {
                                                                                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> section has duplicate <" + To_subnode_tag_name + "> entry.");
                                                                                                                                        }
                                                                                                                                        else
                                                                                                                                        {
                                                                                                                                            bRequiredToLowTempFound = true;
                                                                                                                                        }
                                                                                                                                        to_temp_obj.m_TemperatureRange.m_fLowTemperature = Convert.ToSingle(To_subnode.InnerText);
                                                                                                                                    }
                                                                                                                                    else if ("HIGHTEMP" == To_subnode_tag_name)
                                                                                                                                    {
                                                                                                                                        if (bRequiredToHighTempFound)
                                                                                                                                        {
                                                                                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> section has duplicate <" + To_subnode_tag_name + "> entry.");
                                                                                                                                        }
                                                                                                                                        else
                                                                                                                                        {
                                                                                                                                            bRequiredToHighTempFound = true;
                                                                                                                                        }
                                                                                                                                        to_temp_obj.m_TemperatureRange.m_fHighTemperature = Convert.ToSingle(To_subnode.InnerText);
                                                                                                                                    }
                                                                                                                                    else if ("PARAMS" == To_subnode_tag_name)
                                                                                                                                    {
                                                                                                                                        if (bRequiredParamsFound)
                                                                                                                                        {
                                                                                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> section has duplicate <" + To_subnode_tag_name + "> entry.");
                                                                                                                                        }
                                                                                                                                        else
                                                                                                                                        {
                                                                                                                                            bRequiredParamsFound = true;
                                                                                                                                        }

                                                                                                                                        bool bRequiredRabbitGainFound = false;
                                                                                                                                        bool bRequiredRabbitGain2Found = false;
                                                                                                                                        bool bRequiredRabbitGainOffsetFound = false;
                                                                                                                                        bool bRequiredRabbitDerivGainFound = false;
                                                                                                                                        bool bRequiredPGainFound = false;
                                                                                                                                        bool bRequiredDGainFound = false;
                                                                                                                                        bool bRequiredIGainFound = false;
                                                                                                                                        bool bRequiredDeadBandFound = false;
                                                                                                                                        bool bRequiredHighClampFound = false;
                                                                                                                                        bool bRequiredLowClampFound = false;
                                                                                                                                        bool bRequiredOvershootOffsetFound = false;
                                                                                                                                        bool bRequiredOvershootDurationFound = false;
                                                                                                                                        bool bRequiredSetpointOffsetFound = false;
                                                                                                                                        bool bRequiredPBandFound = false;

                                                                                                                                        foreach (Positional_XmlElement ToPIDParameter_subnode in To_subnode)
                                                                                                                                        {
                                                                                                                                            if (ToPIDParameter_subnode.NodeType == XmlNodeType.Element)
                                                                                                                                            {
                                                                                                                                                string ToPIDsubtag_name = ToPIDParameter_subnode.Name.ToUpper();

                                                                                                                                                if ("RABBITGAIN" == ToPIDsubtag_name)
                                                                                                                                                {
                                                                                                                                                    if (bRequiredRabbitGainFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredRabbitGainFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    string[] PIDValues = ToPIDParameter_subnode.InnerText.Split(new char[1] { ',' });
                                                                                                                                                    to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDUpParamsId]);
                                                                                                                                                    if (PIDValues.Length > 1)
                                                                                                                                                    {
                                                                                                                                                        to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDDownParamsId]);
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitGain =
                                                                                                                                                                                to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitGain;
                                                                                                                                                    }
                                                                                                                                                }
                                                                                                                                                else if ("RABBITGAIN2" == ToPIDsubtag_name)
                                                                                                                                                {
                                                                                                                                                    if (bRequiredRabbitGain2Found)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredRabbitGain2Found = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    string[] PIDValues = ToPIDParameter_subnode.InnerText.Split(new char[1] { ',' });
                                                                                                                                                    to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitGain2 = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDUpParamsId]);
                                                                                                                                                    if (PIDValues.Length > 1)
                                                                                                                                                    {
                                                                                                                                                        to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitGain2 = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDDownParamsId]);
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitGain2 =
                                                                                                                                                                                to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitGain2;
                                                                                                                                                    }
                                                                                                                                                }
                                                                                                                                                else if ("RABBITGAINOFFSET" == ToPIDsubtag_name)
                                                                                                                                                {
                                                                                                                                                    if (bRequiredRabbitGainOffsetFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredRabbitGainOffsetFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    string[] PIDValues = ToPIDParameter_subnode.InnerText.Split(new char[1] { ',' });
                                                                                                                                                    to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitGainOffset = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDUpParamsId]);
                                                                                                                                                    if (PIDValues.Length > 1)
                                                                                                                                                    {
                                                                                                                                                        to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitGainOffset = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDDownParamsId]);
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitGainOffset =
                                                                                                                                                                                to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitGainOffset;
                                                                                                                                                    }
                                                                                                                                                }
                                                                                                                                                else if ("RABBITDERIVGAIN" == ToPIDsubtag_name)
                                                                                                                                                {
                                                                                                                                                    if (bRequiredRabbitDerivGainFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredRabbitDerivGainFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    string[] PIDValues = ToPIDParameter_subnode.InnerText.Split(new char[1] { ',' });
                                                                                                                                                    to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitDerivGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDUpParamsId]);
                                                                                                                                                    if (PIDValues.Length > 1)
                                                                                                                                                    {
                                                                                                                                                        to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitDerivGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDDownParamsId]);
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitDerivGain =
                                                                                                                                                                                to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitDerivGain;
                                                                                                                                                    }
                                                                                                                                                }
                                                                                                                                                else if ("PGAIN" == ToPIDsubtag_name)
                                                                                                                                                {
                                                                                                                                                    if (bRequiredPGainFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredPGainFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    string[] PIDValues = ToPIDParameter_subnode.InnerText.Split(new char[1] { ',' });
                                                                                                                                                    to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fPGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDUpParamsId]);
                                                                                                                                                    if (PIDValues.Length > 1)
                                                                                                                                                    {
                                                                                                                                                        to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fPGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDDownParamsId]);
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fPGain =
                                                                                                                                                                                to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fPGain;
                                                                                                                                                    }
                                                                                                                                                }
                                                                                                                                                else if ("DGAIN" == ToPIDsubtag_name)
                                                                                                                                                {
                                                                                                                                                    if (bRequiredDGainFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredDGainFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    string[] PIDValues = ToPIDParameter_subnode.InnerText.Split(new char[1] { ',' });
                                                                                                                                                    to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fDGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDUpParamsId]);
                                                                                                                                                    if (PIDValues.Length > 1)
                                                                                                                                                    {
                                                                                                                                                        to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fDGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDDownParamsId]);
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fDGain =
                                                                                                                                                                                to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fDGain;
                                                                                                                                                    }
                                                                                                                                                }
                                                                                                                                                else if ("IGAIN" == ToPIDsubtag_name)
                                                                                                                                                {
                                                                                                                                                    if (bRequiredIGainFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredIGainFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    string[] PIDValues = ToPIDParameter_subnode.InnerText.Split(new char[1] { ',' });
                                                                                                                                                    to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fIGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDUpParamsId]);
                                                                                                                                                    if (PIDValues.Length > 1)
                                                                                                                                                    {
                                                                                                                                                        to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fIGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDDownParamsId]);
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fIGain =
                                                                                                                                                                                to_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fIGain;
                                                                                                                                                    }
                                                                                                                                                }
                                                                                                                                                else if ("DEADBAND" == ToPIDsubtag_name)
                                                                                                                                                {
                                                                                                                                                    if (bRequiredDeadBandFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredDeadBandFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    to_temp_obj.m_fDeadBand = Convert.ToSingle(ToPIDParameter_subnode.InnerText);
                                                                                                                                                }
                                                                                                                                                else if ("HIGHCLAMP" == ToPIDsubtag_name)
                                                                                                                                                {
                                                                                                                                                    if (bRequiredHighClampFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredHighClampFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    to_temp_obj.m_fHighClamp = Convert.ToSingle(ToPIDParameter_subnode.InnerText);
                                                                                                                                                }
                                                                                                                                                else if ("LOWCLAMP" == ToPIDsubtag_name)
                                                                                                                                                {
                                                                                                                                                    if (bRequiredLowClampFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredLowClampFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    to_temp_obj.m_fLowClamp = Convert.ToSingle(ToPIDParameter_subnode.InnerText);
                                                                                                                                                }
                                                                                                                                                else if ("OVERSHOOTOFFSET" == ToPIDsubtag_name)
                                                                                                                                                {
                                                                                                                                                    if (bRequiredOvershootOffsetFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredOvershootOffsetFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    to_temp_obj.m_fOvershootOffset = Convert.ToSingle(ToPIDParameter_subnode.InnerText);
                                                                                                                                                }
                                                                                                                                                else if ("OVERSHOOTDURATION" == ToPIDsubtag_name)
                                                                                                                                                {
                                                                                                                                                    if (bRequiredOvershootDurationFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredOvershootDurationFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    to_temp_obj.m_uiOvershootDuration = Convert.ToUInt32(ToPIDParameter_subnode.InnerText);
                                                                                                                                                }
                                                                                                                                                else if ("SETPOINTOFFSET" == ToPIDsubtag_name)
                                                                                                                                                {
                                                                                                                                                    if (bRequiredSetpointOffsetFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredSetpointOffsetFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    to_temp_obj.m_fSetpointOffset = Convert.ToSingle(ToPIDParameter_subnode.InnerText);
                                                                                                                                                }
                                                                                                                                                else if ("PBAND" == ToPIDsubtag_name)
                                                                                                                                                {
                                                                                                                                                    if (bRequiredPBandFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredPBandFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    to_temp_obj.m_fPBand = Convert.ToSingle(ToPIDParameter_subnode.InnerText);
                                                                                                                                                }
                                                                                                                                                else
                                                                                                                                                {
                                                                                                                                                    // Unexpected tag defined - parse error
                                                                                                                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> section has unexpected node <" + ToPIDsubtag_name + ">.");
                                                                                                                                                }
                                                                                                                                            }
                                                                                                                                        }

                                                                                                                                        // Verify required fields present
                                                                                                                                        if (!(
                                                                                                                                                bRequiredRabbitGainFound &&
                                                                                                                                                bRequiredRabbitGain2Found &&
                                                                                                                                                bRequiredRabbitGainOffsetFound &&
                                                                                                                                                bRequiredRabbitDerivGainFound &&
                                                                                                                                                bRequiredPGainFound &&
                                                                                                                                                bRequiredDGainFound &&
                                                                                                                                                bRequiredIGainFound &&
                                                                                                                                                bRequiredDeadBandFound &&
                                                                                                                                                bRequiredHighClampFound &&
                                                                                                                                                bRequiredLowClampFound &&
                                                                                                                                                bRequiredOvershootOffsetFound &&
                                                                                                                                                bRequiredOvershootDurationFound &&
                                                                                                                                                bRequiredSetpointOffsetFound &&
                                                                                                                                                bRequiredPBandFound
                                                                                                                                            ))
                                                                                                                                        {
                                                                                                                                            string strMissingElement = ((!bRequiredRabbitGainFound) ? "<RabbitGain>" :
                                                                                                                                                                        (!bRequiredRabbitGain2Found) ? "<RabbitGain2>" :
                                                                                                                                                                        (!bRequiredRabbitGainOffsetFound) ? "<RabbitGainOffset>" :
                                                                                                                                                                        (!bRequiredRabbitDerivGainFound) ? "<RabbitDerivGain>" :
                                                                                                                                                                        (!bRequiredPGainFound) ? "<RabbitPGain>" :
                                                                                                                                                                        (!bRequiredDGainFound) ? "<RabbitDGain>" :
                                                                                                                                                                        (!bRequiredIGainFound) ? "<RabbitIGain>" :
                                                                                                                                                                        (!bRequiredDeadBandFound) ? "<DeadBand>" :
                                                                                                                                                                        (!bRequiredHighClampFound) ? "<HighClamp>" :
                                                                                                                                                                        (!bRequiredLowClampFound) ? "<LowClamp>" :
                                                                                                                                                                        (!bRequiredOvershootOffsetFound) ? "<OvershootOffset>" :
                                                                                                                                                                        (!bRequiredOvershootDurationFound) ? "<OvershootDuration>" :
                                                                                                                                                                        (!bRequiredSetpointOffsetFound) ? "<SetpointOffset>" : "<PBand>"
                                                                                                                                                                        );
                                                                                                                                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> <Params> section.\n");
                                                                                                                                        }
                                                                                                                                    }
                                                                                                                                }
                                                                                                                            }
                                                                                                                            if (!(bRequiredToLowTempFound && bRequiredToHighTempFound && bRequiredParamsFound))
                                                                                                                            {
                                                                                                                                string strMissingElement = (!bRequiredToLowTempFound) ? "<LowTemperature>" : 
                                                                                                                                                           (!bRequiredToHighTempFound) ? "<HighTemperature>" : "<Params>";
                                                                                                                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> <To> section.\n");
                                                                                                                            }
                                                                                                                            // For QuanDx phase 2 from and to entries are assumed (must be) complete and non-conflicting
                                                                                                                            from_temp_obj.m_ToTemperatures.Add(to_temp_obj);
                                                                                                                        }
                                                                                                                        else
                                                                                                                        {
                                                                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> section has unexpected node <" + to_node_tag_name + ">.");
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                                if (!bRequiredToTempFound)
                                                                                                                {
                                                                                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. <To> required following <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> section.\n");
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                    if (!(bRequiredLowTempFound && bRequiredHighTempFound && bRequiredToTempsFound))
                                                                                                    {
                                                                                                        string strMissingElement = (!bRequiredLowTempFound) ? "<LowTemperature>" :
                                                                                                                                   (!bRequiredHighTempFound) ? "<HighTemperature>" : "<ToTemps>";
                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <From> <ToTemps> section missing " + strMissingElement + " node.");
                                                                                                    }

                                                                                                    // Save 'From' PIDs
                                                                                                    if (bIsRampUpSection)
                                                                                                    {
                                                                                                        PCR_Thermal_TEC_ConfigurationObj.m_Step_PID_RampUp_Range_List.Add(from_temp_obj);
                                                                                                    }
                                                                                                    else
                                                                                                    {   // RampDown section
                                                                                                        PCR_Thermal_TEC_ConfigurationObj.m_Step_PID_RampDown_Range_List.Add(from_temp_obj);
                                                                                                    }
                                                                                                }
                                                                                                else if ("Default".ToUpper() == From_node_tag_name)
                                                                                                {
                                                                                                    if (bRequiredDefaultFound)
                                                                                                    {
                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> section has duplicate <" + From_node_tag_name + "> entry.");
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        bRequiredDefaultFound = true;
                                                                                                    }

                                                                                                    bool bRequiredParamsFound = false;
                                                                                                    default_temp_obj = new CTEC_HW_Ramping_Configuration();

                                                                                                    foreach (Positional_XmlElement Default_node in FromTemps_subnode)
                                                                                                    {
                                                                                                        if (Default_node.NodeType == XmlNodeType.Element)
                                                                                                        {
                                                                                                            string Default_subnode_tag_name = Default_node.Name.ToUpper();

                                                                                                            if ("Params".ToUpper() == Default_subnode_tag_name)
                                                                                                            {
                                                                                                                if (bRequiredParamsFound)
                                                                                                                {
                                                                                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section has duplicate <" + Default_subnode_tag_name + "> entry.");
                                                                                                                }
                                                                                                                else
                                                                                                                {
                                                                                                                    bRequiredParamsFound = true;
                                                                                                                }

                                                                                                                bool bRequiredRabbitGainFound = false;
                                                                                                                bool bRequiredRabbitGain2Found = false;
                                                                                                                bool bRequiredRabbitGainOffsetFound = false;
                                                                                                                bool bRequiredRabbitDerivGainFound = false;
                                                                                                                bool bRequiredPGainFound = false;
                                                                                                                bool bRequiredDGainFound = false;
                                                                                                                bool bRequiredIGainFound = false;
                                                                                                                bool bRequiredDeadBandFound = false;
                                                                                                                bool bRequiredHighClampFound = false;
                                                                                                                bool bRequiredLowClampFound = false;
                                                                                                                bool bRequiredOvershootOffsetFound = false;
                                                                                                                bool bRequiredOvershootDurationFound = false;
                                                                                                                bool bRequiredSetpointOffsetFound = false;
                                                                                                                bool bRequiredPBandFound = false;

                                                                                                                foreach (Positional_XmlElement Default_PID_Params_subnode in Default_node)
                                                                                                                {
                                                                                                                    if (Default_PID_Params_subnode.NodeType == XmlNodeType.Element)
                                                                                                                    {
                                                                                                                        string Default_PID_Params_subtag_name = Default_PID_Params_subnode.Name.ToUpper();

                                                                                                                        if ("RABBITGAIN" == Default_PID_Params_subtag_name)
                                                                                                                        {
                                                                                                                            if (bRequiredRabbitGainFound)
                                                                                                                            {
                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                bRequiredRabbitGainFound = true; // Required definition
                                                                                                                            }
                                                                                                                            string[] PIDValues = Default_PID_Params_subnode.InnerText.Split(new char[1] { ',' });
                                                                                                                            default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDUpParamsId]);
                                                                                                                            if (PIDValues.Length > 1)
                                                                                                                            {
                                                                                                                                default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDDownParamsId]);
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitGain =
                                                                                                                                                        default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitGain;
                                                                                                                            }
                                                                                                                        }
                                                                                                                        else if ("RABBITGAIN2" == Default_PID_Params_subtag_name)
                                                                                                                        {
                                                                                                                            if (bRequiredRabbitGain2Found)
                                                                                                                            {
                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                bRequiredRabbitGain2Found = true; // Required definition
                                                                                                                            }
                                                                                                                            string[] PIDValues = Default_PID_Params_subnode.InnerText.Split(new char[1] { ',' });
                                                                                                                            default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitGain2 = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDUpParamsId]);
                                                                                                                            if (PIDValues.Length > 1)
                                                                                                                            {
                                                                                                                                default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitGain2 = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDDownParamsId]);
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitGain2 =
                                                                                                                                                        default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitGain2;
                                                                                                                            }
                                                                                                                        }
                                                                                                                        else if ("RABBITGAINOFFSET" == Default_PID_Params_subtag_name)
                                                                                                                        {
                                                                                                                            if (bRequiredRabbitGainOffsetFound)
                                                                                                                            {
                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                bRequiredRabbitGainOffsetFound = true; // Required definition
                                                                                                                            }
                                                                                                                            string[] PIDValues = Default_PID_Params_subnode.InnerText.Split(new char[1] { ',' });
                                                                                                                            default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitGainOffset = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDUpParamsId]);
                                                                                                                            if (PIDValues.Length > 1)
                                                                                                                            {
                                                                                                                                default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitGainOffset = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDDownParamsId]);
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitGainOffset =
                                                                                                                                                        default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitGainOffset;
                                                                                                                            }
                                                                                                                        }
                                                                                                                        else if ("RABBITDERIVGAIN" == Default_PID_Params_subtag_name)
                                                                                                                        {
                                                                                                                            if (bRequiredRabbitDerivGainFound)
                                                                                                                            {
                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                bRequiredRabbitDerivGainFound = true; // Required definition
                                                                                                                            }
                                                                                                                            string[] PIDValues = Default_PID_Params_subnode.InnerText.Split(new char[1] { ',' });
                                                                                                                            default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitDerivGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDUpParamsId]);
                                                                                                                            if (PIDValues.Length > 1)
                                                                                                                            {
                                                                                                                                default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitDerivGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDDownParamsId]);
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitDerivGain =
                                                                                                                                                        default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitDerivGain;
                                                                                                                            }
                                                                                                                        }
                                                                                                                        else if ("PGAIN" == Default_PID_Params_subtag_name)
                                                                                                                        {
                                                                                                                            if (bRequiredPGainFound)
                                                                                                                            {
                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                bRequiredPGainFound = true; // Required definition
                                                                                                                            }
                                                                                                                            string[] PIDValues = Default_PID_Params_subnode.InnerText.Split(new char[1] { ',' });
                                                                                                                            default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fPGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDUpParamsId]);
                                                                                                                            if (PIDValues.Length > 1)
                                                                                                                            {
                                                                                                                                default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fPGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDDownParamsId]);
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fPGain =
                                                                                                                                                        default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fPGain;
                                                                                                                            }
                                                                                                                        }
                                                                                                                        else if ("DGAIN" == Default_PID_Params_subtag_name)
                                                                                                                        {
                                                                                                                            if (bRequiredDGainFound)
                                                                                                                            {
                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                bRequiredDGainFound = true; // Required definition
                                                                                                                            }
                                                                                                                            string[] PIDValues = Default_PID_Params_subnode.InnerText.Split(new char[1] { ',' });
                                                                                                                            default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fDGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDUpParamsId]);
                                                                                                                            if (PIDValues.Length > 1)
                                                                                                                            {
                                                                                                                                default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fDGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDDownParamsId]);
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fDGain =
                                                                                                                                                        default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fDGain;
                                                                                                                            }
                                                                                                                        }
                                                                                                                        else if ("IGAIN" == Default_PID_Params_subtag_name)
                                                                                                                        {
                                                                                                                            if (bRequiredIGainFound)
                                                                                                                            {
                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                bRequiredIGainFound = true; // Required definition
                                                                                                                            }
                                                                                                                            string[] PIDValues = Default_PID_Params_subnode.InnerText.Split(new char[1] { ',' });
                                                                                                                            default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fIGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDUpParamsId]);
                                                                                                                            if (PIDValues.Length > 1)
                                                                                                                            {
                                                                                                                                default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fIGain = Convert.ToSingle(PIDValues[CSystem_Defns.cuiPIDDownParamsId]);
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fIGain =
                                                                                                                                                        default_temp_obj.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fIGain;
                                                                                                                            }
                                                                                                                        }
                                                                                                                        else if ("DEADBAND" == Default_PID_Params_subtag_name)
                                                                                                                        {
                                                                                                                            if (bRequiredDeadBandFound)
                                                                                                                            {
                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                bRequiredDeadBandFound = true; // Required definition
                                                                                                                            }
                                                                                                                            default_temp_obj.m_fDeadBand = Convert.ToSingle(Default_PID_Params_subnode.InnerText);
                                                                                                                        }
                                                                                                                        else if ("HIGHCLAMP" == Default_PID_Params_subtag_name)
                                                                                                                        {
                                                                                                                            if (bRequiredHighClampFound)
                                                                                                                            {
                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                bRequiredHighClampFound = true; // Required definition
                                                                                                                            }
                                                                                                                            default_temp_obj.m_fHighClamp = Convert.ToSingle(Default_PID_Params_subnode.InnerText);
                                                                                                                        }
                                                                                                                        else if ("LOWCLAMP" == Default_PID_Params_subtag_name)
                                                                                                                        {
                                                                                                                            if (bRequiredLowClampFound)
                                                                                                                            {
                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                bRequiredLowClampFound = true; // Required definition
                                                                                                                            }
                                                                                                                            default_temp_obj.m_fLowClamp = Convert.ToSingle(Default_PID_Params_subnode.InnerText);
                                                                                                                        }
                                                                                                                        else if ("OVERSHOOTOFFSET" == Default_PID_Params_subtag_name)
                                                                                                                        {
                                                                                                                            if (bRequiredOvershootOffsetFound)
                                                                                                                            {
                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                bRequiredOvershootOffsetFound = true; // Required definition
                                                                                                                            }
                                                                                                                            default_temp_obj.m_fOvershootOffset = Convert.ToSingle(Default_PID_Params_subnode.InnerText);
                                                                                                                        }
                                                                                                                        else if ("OVERSHOOTDURATION" == Default_PID_Params_subtag_name)
                                                                                                                        {
                                                                                                                            if (bRequiredOvershootDurationFound)
                                                                                                                            {
                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                bRequiredOvershootDurationFound = true; // Required definition
                                                                                                                            }
                                                                                                                            default_temp_obj.m_uiOvershootDuration = Convert.ToUInt32(Default_PID_Params_subnode.InnerText);
                                                                                                                        }
                                                                                                                        else if ("SETPOINTOFFSET" == Default_PID_Params_subtag_name)
                                                                                                                        {
                                                                                                                            if (bRequiredSetpointOffsetFound)
                                                                                                                            {
                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                bRequiredSetpointOffsetFound = true; // Required definition
                                                                                                                            }
                                                                                                                            default_temp_obj.m_fSetpointOffset = Convert.ToSingle(Default_PID_Params_subnode.InnerText);
                                                                                                                        }
                                                                                                                        else if ("PBAND" == Default_PID_Params_subtag_name)
                                                                                                                        {
                                                                                                                            if (bRequiredPBandFound)
                                                                                                                            {
                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                bRequiredPBandFound = true; // Required definition
                                                                                                                            }
                                                                                                                            default_temp_obj.m_fPBand = Convert.ToSingle(Default_PID_Params_subnode.InnerText);
                                                                                                                        }
                                                                                                                        else
                                                                                                                        {
                                                                                                                            // Unexpected tag defined - parse error
                                                                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section has unexpected node <" + Default_PID_Params_subtag_name + ">.");
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                                // Verify required fields present
                                                                                                                if (!(
                                                                                                                        bRequiredRabbitGainFound &&
                                                                                                                        bRequiredRabbitGain2Found &&
                                                                                                                        bRequiredRabbitGainOffsetFound &&
                                                                                                                        bRequiredRabbitDerivGainFound &&
                                                                                                                        bRequiredPGainFound &&
                                                                                                                        bRequiredDGainFound &&
                                                                                                                        bRequiredIGainFound &&
                                                                                                                        bRequiredDeadBandFound &&
                                                                                                                        bRequiredHighClampFound &&
                                                                                                                        bRequiredLowClampFound &&
                                                                                                                        bRequiredOvershootOffsetFound &&
                                                                                                                        bRequiredOvershootDurationFound &&
                                                                                                                        bRequiredSetpointOffsetFound &&
                                                                                                                        bRequiredPBandFound
                                                                                                                    ))
                                                                                                                {
                                                                                                                    string strMissingElement = ((!bRequiredRabbitGainFound) ? "<RabbitGain>" :
                                                                                                                                                (!bRequiredRabbitGain2Found) ? "<RabbitGain2>" :
                                                                                                                                                (!bRequiredRabbitGainOffsetFound) ? "<RabbitGainOffset>" :
                                                                                                                                                (!bRequiredRabbitDerivGainFound) ? "<RabbitDerivGain>" :
                                                                                                                                                (!bRequiredPGainFound) ? "<RabbitPGain>" :
                                                                                                                                                (!bRequiredDGainFound) ? "<RabbitDGain>" :
                                                                                                                                                (!bRequiredIGainFound) ? "<RabbitIGain>" :
                                                                                                                                                (!bRequiredDeadBandFound) ? "<DeadBand>" :
                                                                                                                                                (!bRequiredHighClampFound) ? "<HighClamp>" :
                                                                                                                                                (!bRequiredLowClampFound) ? "<LowClamp>" :
                                                                                                                                                (!bRequiredOvershootOffsetFound) ? "<OvershootOffset>" :
                                                                                                                                                (!bRequiredOvershootDurationFound) ? "<OvershootDuration>" :
                                                                                                                                                (!bRequiredSetpointOffsetFound) ? "<SetpointOffset>" : "<PBand>"
                                                                                                                                                );
                                                                                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> <Default> <Params> section.\n");
                                                                                                                }
                                                                                                                // Save 'Default' PIDs
                                                                                                                if (bIsRampUpSection)
                                                                                                                {
                                                                                                                    PCR_Thermal_TEC_ConfigurationObj.m_Step_PID_RampUp_Default = default_temp_obj;
                                                                                                                }
                                                                                                                else
                                                                                                                {   // RampDown section
                                                                                                                    PCR_Thermal_TEC_ConfigurationObj.m_Step_PID_RampDown_Default = default_temp_obj;
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> section has unexpected node <" + From_node_tag_name + ">.");
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                        if (!bRequiredDefaultFound)
                                                                                        {
                                                                                            string strMissingElement = "Default";
                                                                                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + rampsectionName + " <FromTemps> section.\n");
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " section has unexpected node <" + FromTemps_node_tag_name + ">.");
                                                                                    }
                                                                                }
                                                                                if (!bRequiredFromTempsFound)
                                                                                {
                                                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. <FromTemps> required following <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " section.\n");
                                                                                }
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> section has unexpected node <" + strStepSectionName + ">.");
                                                                        }
                                                                    }
                                                                }
                                                                if (!(bRequiredRampUpFound && bRequiredRampDownFound))
                                                                {
                                                                    string strMissingElement = (!bRequiredRampUpFound) ? "<RampUp>" : "<RampDown>";
                                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " section.\n");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> section has unexpected node <" + steptag_name + ">.");
                                                            }
                                                        }
                                                    }
                                                    if (!bRequiredStepFound)
                                                    {
                                                        string strMissingElement = "<Step>";
                                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <" + strTEC_Entry + "> <PIDs> section.\n");
                                                    }
                                                }
                                                else if ("ERRORTERMBAND" == tag_name)
                                                {
                                                    if (bRequiredErrorTermBandFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> section has duplicate <ErrorTermBand> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredErrorTermBandFound = true; // Required definition
                                                    }
                                                    PCR_Thermal_TEC_ConfigurationObj.m_fErrorTermBand = Convert.ToSingle(TEC_subnode.InnerText);
                                                }
                                                else if ("ERRORTERMCOUNT" == tag_name)
                                                {
                                                    if (bRequiredErrorTermCountFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> section has duplicate <ErrorTermCount> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredErrorTermCountFound = true; // Required definition
                                                    }
                                                    PCR_Thermal_TEC_ConfigurationObj.m_fErrorTermCount = Convert.ToSingle(TEC_subnode.InnerText);
                                                }
                                                else if ("STEADYSTATEPOWERLIMIT" == tag_name)
                                                {
                                                    if (bRequiredSteadyStatePowerLimitFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> section has duplicate <SteadyStatePowerLimit> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredSteadyStatePowerLimitFound = true; // Required definition
                                                    }
                                                    PCR_Thermal_TEC_ConfigurationObj.m_fSteadyStatePowerLimit = Convert.ToSingle(TEC_subnode.InnerText);
                                                }
                                                else if ("STEADYSTATEPOWERLIMITCOUNT" == tag_name)
                                                {
                                                    if (bRequiredSteadyStatePowerLimitCountFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> section has duplicate <SteadyStatePowerLimitCount> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredSteadyStatePowerLimitCountFound = true; // Required definition
                                                    }
                                                    PCR_Thermal_TEC_ConfigurationObj.m_fSteadyStatePowerLimitCount = Convert.ToSingle(TEC_subnode.InnerText);
                                                }
                                                else
                                                {
                                                    // Unexpected tag defined - parse error
                                                    throw new CPCRInstrumentSystemException("Configuration file parse error.  Unexpected tag <" + tag_name + "> found following <" + strTEC_Entry + "> tag.");
                                                }
                                            }
                                        }
                                        // Verify required fields present
                                        if (!(bRequiredTECControllerNameFound &&
                                              bRequiredAddressFound &&
                                              bRequiredControlPIDSampleTimeInSecondsFound &&
                                              bRequiredPIDsFound &&
                                              bRequiredErrorTermBandFound &&
                                              bRequiredErrorTermCountFound &&
                                              bRequiredSteadyStatePowerLimitFound &&
                                              bRequiredSteadyStatePowerLimitCountFound &&
                                              bRequiredThermA_CoeffFound &&
                                              bRequiredThermB_CoeffFound &&
                                              bRequiredThermC_CoeffFound
                                           ))
                                        {
                                            string strMissingElement = (!bRequiredTECControllerNameFound) ? "<TECController>" :
                                                                       (!bRequiredAddressFound) ? "<Channel>" :
                                                                       (!bRequiredControlPIDSampleTimeInSecondsFound) ? "<ControlPIDSampleTimeInSeconds>" :
                                                                       (!bRequiredErrorTermBandFound) ? "<ErrorTermBand>" :
                                                                       (!bRequiredErrorTermCountFound) ? "<ErrorTermCount>" :
                                                                       (!bRequiredSteadyStatePowerLimitFound) ? "<SteadyStatePowerLimit>" :
                                                                       (!bRequiredSteadyStatePowerLimitCountFound) ? "<SteadyStatePowerLimitCount>" :
                                                                       (!bRequiredThermA_CoeffFound) ? "<ThermA_Coeff>" :
                                                                       (!bRequiredThermB_CoeffFound) ? "<ThermB_Coeff>" : "<ThermC_Coeff>";
                                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <" + strTEC_Entry + "> tag.");
                                        }
                                        PCRConfigurationObj.m_TEC_Channel_Configurations.Add(strTEC_Entry, PCR_Thermal_TEC_ConfigurationObj);
                                    }
                                }
                            }
                        }
                    }
                    else if ("PNEUMATIC_SETTINGS" == hw_setting_section_node.Name.ToUpper())
                    {
                        if (bRequiredPneumatic_SettingsSectionFound)
                        {
                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <HWSettings> section has duplicate <" + hw_setting_section_node + "> entry.");
                        }
                        else
                        {
                            bRequiredPneumatic_SettingsSectionFound = true;
                        }

                        // Idle Pressure
                        bool bRequiredIdlePressureSectionFound = false;

                        foreach (Positional_XmlElement pneumatic_pressure_settings_node in hw_setting_section_node)
                        {
                            if (pneumatic_pressure_settings_node.NodeType == XmlNodeType.Element)
                            {
                                if ("IDLEPRESSURE" == pneumatic_pressure_settings_node.Name.ToUpper())
                                {
                                    if (!bRequiredIdlePressureSectionFound)
                                    {
                                        bRequiredIdlePressureSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Pneumatic Pressure Settings section has duplicate <" + pneumatic_pressure_settings_node.Name.ToUpper() + "> entry.");
                                    }
                                    PCRConfigurationObj.m_iIdlePressureSetpointinPSI = Convert.ToInt32(pneumatic_pressure_settings_node.InnerText);
                                }
                                else
                                {
                                    // Parse error, first node after 'Pneumatic_Settings' must be 'IdlePressure'
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument settings section contains unexpected <" + pneumatic_pressure_settings_node.Name.ToUpper() + "> tag.");
                                }
                            }
                        }
                        // Verify required fields present
                        if (!(bRequiredIdlePressureSectionFound))
                        {
                            string strMissingElement = "";
                            if (!bRequiredIdlePressureSectionFound) strMissingElement = "<IdlePressure>";
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Missing " + strMissingElement + " required following <Pneumatic_Settings> tag.");
                        }
                    }
                    else if ("PNEUMATIC_IO_HANDLING" == hw_setting_section_node.Name.ToUpper())
                    {
                        if (bRequiredPneumatic_IO_HandlingSectionFound)
                        {
                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <HWSettings> section has duplicate <" + hw_setting_section_node + "> entry.");
                        }
                        else
                        {
                            bRequiredPneumatic_IO_HandlingSectionFound = true;
                        }

                        // Pump
                        // Regulator
                        // Pressure sensor
                        // Inlet valve
                        // Outlet valve
                        // Vent valve

                        bool bRequiredPumpIOSectionFound = false;
                        bool bRequiredRegulatorIOSectionFound = false;
                        bool bRequiredPneumaticIOSectionFound = false;

                        foreach (Positional_XmlElement pneumatic_io_handling_node in hw_setting_section_node)
                        {
                            if (pneumatic_io_handling_node.NodeType == XmlNodeType.Element)
                            {
                                if ("PUMP_IO" == pneumatic_io_handling_node.Name.ToUpper())
                                {
                                    if (!bRequiredPumpIOSectionFound)
                                    {
                                        bRequiredPumpIOSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Pneumatic IO Handling section has duplicate <" + pneumatic_io_handling_node.Name.ToUpper() + "> entry.");
                                    }

                                    bool bRequiredControlSectionFound = false;
                                    foreach (Positional_XmlElement control_node in pneumatic_io_handling_node)
                                    {
                                        if (control_node.NodeType == XmlNodeType.Element)
                                        {
                                            if ("CONTROL" == control_node.Name.ToUpper())
                                            {
                                                if (!bRequiredControlSectionFound)
                                                {
                                                    bRequiredControlSectionFound = true;
                                                }
                                                else
                                                {
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Pump_IO> section has duplicate <" + control_node.Name.ToUpper() + "> entry.");
                                                }

                                                bool bRequiredAccelDIOSectionFound = false;
                                                foreach (Positional_XmlElement accel_DIO_node in control_node)
                                                {
                                                    if (accel_DIO_node.NodeType == XmlNodeType.Element)
                                                    {
                                                        if ("ACCEL_DIO" == accel_DIO_node.Name.ToUpper())
                                                        {
                                                            if (!bRequiredAccelDIOSectionFound)
                                                            {
                                                                bRequiredAccelDIOSectionFound = true;
                                                            }
                                                            else
                                                            {
                                                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Pump_IO><Control> section has duplicate <" + accel_DIO_node.Name.ToUpper() + "> entry.");
                                                            }

                                                            bool bRequiredMotorControllerFound = false;
                                                            bool bRequiredDOFound = false;
                                                            bool bOptionalDutyCycleFound = false;
                                                            foreach (Positional_XmlElement accel_do_node in accel_DIO_node)
                                                            {
                                                                if (accel_do_node.NodeType == XmlNodeType.Element)
                                                                {
                                                                    if ("MOTORCONTROLLER" == accel_do_node.Name.ToUpper())
                                                                    {
                                                                        if (bRequiredMotorControllerFound)
                                                                        {
                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Pump_IO><Control><Accel_DIO> section has duplicate <MotorController> entry.");
                                                                        }
                                                                        else
                                                                        {
                                                                            bRequiredMotorControllerFound = true; // Required definition
                                                                            PCRConfigurationObj.m_PumpIOControls.m_Control.m_strMotorController = accel_do_node.InnerText;
                                                                        }
                                                                    }
                                                                    else if ("DO" == accel_do_node.Name.ToUpper())
                                                                    {
                                                                        if (bRequiredDOFound)
                                                                        {
                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Pump_IO><Control><Accel_DIO> section has duplicate <DO> entry.");
                                                                        }
                                                                        else
                                                                        {
                                                                            bRequiredDOFound = true; // Required definition
                                                                            PCRConfigurationObj.m_PumpIOControls.m_Control.m_uiDO_Channel = Convert.ToUInt32(accel_do_node.InnerText);
                                                                        }
                                                                    }
                                                                    else if ("DUTYCYCLE" == accel_do_node.Name.ToUpper())
                                                                    {
                                                                        if (bOptionalDutyCycleFound)
                                                                        {
                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Pump_IO><Control><Accel_DIO> section has duplicate <DutyCycle> entry.");
                                                                        }
                                                                        else
                                                                        {
                                                                            bOptionalDutyCycleFound = true; // Required definition
                                                                            PCRConfigurationObj.m_PumpIOControls.m_Control.m_uiDutyCycle = Convert.ToUInt32(accel_do_node.InnerText);
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        // Unexpected tag defined - parse error
                                                                        throw new CPCRInstrumentSystemException("Configuration file parse error.  <MotorController> and <DO> definitions expected following <Pneumatic_IO_Handling><Pump_IO><Control><Accel_DIO> tag, instead found <" + accel_do_node.Name + ">.");
                                                                    }
                                                                }
                                                            }
                                                            // Verify required fields present
                                                            if (!(bRequiredMotorControllerFound && bRequiredDOFound))
                                                            {
                                                                string strMissingElement = (!bRequiredMotorControllerFound) ? "<MotorController>" : "<DO>";
                                                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Pump_IO><Control><Accel_DIO> tag.");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // Unexpected tag defined - parse error
                                                            throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <Pneumatic_IO_Handling><Pump_IO><Control> tag, instead found <" + accel_DIO_node.Name + ">.");
                                                        }
                                                    }
                                                }
                                                if (!bRequiredAccelDIOSectionFound)
                                                {
                                                    string strMissingElement = "<Accel_DIO>";
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Pump_IO><Control> tag.");
                                                }
                                            }
                                            else
                                            {
                                                // Unexpected tag defined - parse error
                                                throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <Pneumatic_IO_Handling><Pump_IO> tag, instead found <" + control_node.Name + ">.");
                                            }
                                        }
                                    }
                                    if (!bRequiredControlSectionFound)
                                    {
                                        string strMissingElement = "<Control>";
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Pump_IO> tag.");
                                    }
                                }
                                else if ("REGULATOR_IO" == pneumatic_io_handling_node.Name.ToUpper())
                                {
                                    if (!bRequiredRegulatorIOSectionFound)
                                    {
                                        bRequiredRegulatorIOSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Pneumatic IO Handling section has duplicate <Regulator_IO> entry.");
                                    }

                                    bool bRequiredControlSectionFound = false;
                                    bool bRequiredPressureReadbackSectionFound = false;
                                    foreach (Positional_XmlElement regulator_io_subnode in pneumatic_io_handling_node)
                                    {
                                        if (regulator_io_subnode.NodeType == XmlNodeType.Element)
                                        {
                                            if ("CONTROL" == regulator_io_subnode.Name.ToUpper())
                                            {
                                                if (!bRequiredControlSectionFound)
                                                {
                                                    bRequiredControlSectionFound = true;
                                                }
                                                else
                                                {
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Regulator_IO> section has duplicate <" + regulator_io_subnode.Name.ToUpper() + "> entry.");
                                                }

                                                bool bRequiredNI_AOSectionFound = false;
                                                foreach (Positional_XmlElement NI_AO_node in regulator_io_subnode)
                                                {
                                                    if (NI_AO_node.NodeType == XmlNodeType.Element)
                                                    {
                                                        if ("NI_AO" == NI_AO_node.Name.ToUpper())
                                                        {
                                                            if (!bRequiredNI_AOSectionFound)
                                                            {
                                                                bRequiredNI_AOSectionFound = true;
                                                            }
                                                            else
                                                            {
                                                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Regulator_IO><Control> section has duplicate <" + NI_AO_node.Name.ToUpper() + "> entry.");
                                                            }

                                                            bool bRequiredChannelNameFound = false;
                                                            bool bRequiredMaxSetpointFound = false;
                                                            bool bRequiredMinSetpointFound = false;
                                                            foreach (Positional_XmlElement ni_ao_subnode in NI_AO_node)
                                                            {
                                                                if (ni_ao_subnode.NodeType == XmlNodeType.Element)
                                                                {
                                                                    if ("CHANNELNAME" == ni_ao_subnode.Name.ToUpper())
                                                                    {
                                                                        if (bRequiredChannelNameFound)
                                                                        {
                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Regulator_IO><Control><NI_AO> section has duplicate <ChannelName> entry.");
                                                                        }
                                                                        else
                                                                        {
                                                                            bRequiredChannelNameFound = true; // Required definition
                                                                            PCRConfigurationObj.m_RegulatorIOControls.m_RegulatorPressureSetpoint.m_strNIControllerChannel = ni_ao_subnode.InnerText;
                                                                        }
                                                                    }
                                                                    else if ("MAXSETPOINT" == ni_ao_subnode.Name.ToUpper())
                                                                    {
                                                                        if (bRequiredMaxSetpointFound)
                                                                        {
                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Regulator_IO><Control><NI_AO> section has duplicate <MaxSetpoint> entry.");
                                                                        }
                                                                        else
                                                                        {
                                                                            bRequiredMaxSetpointFound = true; // Required definition
                                                                            PCRConfigurationObj.m_RegulatorIOControls.m_RegulatorPressureSetpoint.m_fMaxSetpoint = Convert.ToSingle(ni_ao_subnode.InnerText);
                                                                        }
                                                                    }
                                                                    else if ("MINSETPOINT" == ni_ao_subnode.Name.ToUpper())
                                                                    {
                                                                        if (bRequiredMinSetpointFound)
                                                                        {
                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Regulator_IO><Control><NI_AO> section has duplicate <MinSetpoint> entry.");
                                                                        }
                                                                        else
                                                                        {
                                                                            bRequiredMinSetpointFound = true; // Required definition
                                                                            PCRConfigurationObj.m_RegulatorIOControls.m_RegulatorPressureSetpoint.m_fMinSetpoint = Convert.ToSingle(ni_ao_subnode.InnerText);
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        // Unexpected tag defined - parse error
                                                                        throw new CPCRInstrumentSystemException("Configuration file parse error.  <ChannelName>, <MaxSetpoint> or <MinSetpoint> definitions expected following <Pneumatic_IO_Handling><Regulator_IO><Control><NI_AO> tag, instead found <" + ni_ao_subnode.Name + ">.");
                                                                    }
                                                                }
                                                            }
                                                            // Verify required fields present
                                                            if (!(bRequiredChannelNameFound && bRequiredMaxSetpointFound && bRequiredMinSetpointFound))
                                                            {
                                                                string strMissingElement = (!bRequiredChannelNameFound) ? "<ChannelName>" :
                                                                                           (!bRequiredMaxSetpointFound) ? "<MaxSetpoint>" : "<MinSetpoint>";
                                                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Regulator_IO><Control><NI_AO> tag.");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // Unexpected tag defined - parse error
                                                            throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <Pneumatic_IO_Handling><Regulator_IO><Control> tag, instead found <" + NI_AO_node.Name + ">.");
                                                        }
                                                    }
                                                }
                                                if (!bRequiredNI_AOSectionFound)
                                                {
                                                    string strMissingElement = "<NI_AO>";
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Regulator_IO><Control> tag.");
                                                }
                                            }
                                            else if ("PRESSUREREADBACK" == regulator_io_subnode.Name.ToUpper())
                                            {
                                                if (!bRequiredPressureReadbackSectionFound)
                                                {
                                                    bRequiredPressureReadbackSectionFound = true;
                                                }
                                                else
                                                {
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Regulator> section has duplicate <" + regulator_io_subnode.Name.ToUpper() + "> entry.");
                                                }

                                                bool bRequiredNI_AISectionFound = false;
                                                foreach (Positional_XmlElement NI_AI_node in regulator_io_subnode)
                                                {
                                                    if (NI_AI_node.NodeType == XmlNodeType.Element)
                                                    {
                                                        if ("NI_AI" == NI_AI_node.Name.ToUpper())
                                                        {
                                                            if (!bRequiredNI_AISectionFound)
                                                            {
                                                                bRequiredNI_AISectionFound = true;
                                                            }
                                                            else
                                                            {
                                                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Regulator_IO><PressureReadback> section has duplicate <" + NI_AI_node.Name.ToUpper() + "> entry.");
                                                            }

                                                            bool bRequiredChannelNameFound = false;
                                                            bool bRequiredMaxReadbackFound = false;
                                                            bool bRequiredMinReadbackFound = false;
                                                            foreach (Positional_XmlElement ni_ai_subnode in NI_AI_node)
                                                            {
                                                                if (ni_ai_subnode.NodeType == XmlNodeType.Element)
                                                                {
                                                                    if ("CHANNELNAME" == ni_ai_subnode.Name.ToUpper())
                                                                    {
                                                                        if (bRequiredChannelNameFound)
                                                                        {
                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Regulator_IO><PressureReadback><NI_AO> section has duplicate <ChannelName> entry.");
                                                                        }
                                                                        else
                                                                        {
                                                                            bRequiredChannelNameFound = true; // Required definition
                                                                            PCRConfigurationObj.m_RegulatorIOControls.m_RegulatorPressureReadback.m_strNIControllerChannel = ni_ai_subnode.InnerText;
                                                                        }
                                                                    }
                                                                    else if ("MAXREADBACK" == ni_ai_subnode.Name.ToUpper())
                                                                    {
                                                                        if (bRequiredMaxReadbackFound)
                                                                        {
                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Regulator_IO><PressureReadback><NI_AO> section has duplicate <MaxReadback> entry.");
                                                                        }
                                                                        else
                                                                        {
                                                                            bRequiredMaxReadbackFound = true; // Required definition
                                                                            PCRConfigurationObj.m_RegulatorIOControls.m_RegulatorPressureReadback.m_fMaxReadback = Convert.ToSingle(ni_ai_subnode.InnerText);
                                                                        }
                                                                    }
                                                                    else if ("MINREADBACK" == ni_ai_subnode.Name.ToUpper())
                                                                    {
                                                                        if (bRequiredMinReadbackFound)
                                                                        {
                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Regulator_IO><PressureReadback><NI_AI> section has duplicate <MinReadback> entry.");
                                                                        }
                                                                        else
                                                                        {
                                                                            bRequiredMinReadbackFound = true; // Required definition
                                                                            PCRConfigurationObj.m_RegulatorIOControls.m_RegulatorPressureReadback.m_fMinReadback = Convert.ToSingle(ni_ai_subnode.InnerText);
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        // Unexpected tag defined - parse error
                                                                        throw new CPCRInstrumentSystemException("Configuration file parse error.  <ChannelName>, <MaxReadback> or <MinReadback> definitions expected following <Pneumatic_IO_Handling><Regulator_IO><PressureReadback><NI_AI> tag, instead found <" + ni_ai_subnode.Name + ">.");
                                                                    }
                                                                }
                                                            }
                                                            // Verify required fields present
                                                            if (!(bRequiredChannelNameFound && bRequiredMaxReadbackFound && bRequiredMinReadbackFound))
                                                            {
                                                                string strMissingElement = (!bRequiredChannelNameFound) ? "<ChannelName>" :
                                                                                           (!bRequiredMaxReadbackFound) ? "<MaxReadback>" : "<MinReadback>";
                                                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Regulator_IO><PressureReadback><NI_AI> tag.");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // Unexpected tag defined - parse error
                                                            throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <Pneumatic_IO_Handling><Regulator_IO><PressureReadback> tag, instead found <" + NI_AI_node.Name + ">.");
                                                        }
                                                    }
                                                }
                                                if (!bRequiredNI_AISectionFound)
                                                {
                                                    string strMissingElement = "<NI_AI>";
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Regulator_IO><PressureReadback> tag.");
                                                }
                                            }
                                            else
                                            {
                                                // Unexpected tag defined - parse error
                                                throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <Pneumatic_IO_Handling><Regulator_IO> tag, instead found <" + regulator_io_subnode.Name + ">.");
                                            }
                                        }
                                    }
                                    if (!(bRequiredControlSectionFound && bRequiredPressureReadbackSectionFound))
                                    {
                                        string strMissingElement = (!bRequiredControlSectionFound) ? "<Control>" : "PressureReadback";
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Regulator_IO> tag.");
                                    }
                                }
                                else if ("PNEUMATIC_IO" == pneumatic_io_handling_node.Name.ToUpper())
                                {
                                    if (!bRequiredPneumaticIOSectionFound)
                                    {
                                        bRequiredPneumaticIOSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Pneumatic IO Handling section has duplicate <Pneumatic_IO> entry.");
                                    }

                                    bool bRequiredPressureSensorSectionFound = false;
                                    bool bRequiredInletValveSectionFound = false;
                                    bool bRequiredOutletValveSectionFound = false;
                                    bool bRequiredVentValveSectionFound = false;
                                    foreach (Positional_XmlElement pneumatic_io_subnode in pneumatic_io_handling_node)
                                    {
                                        if (pneumatic_io_subnode.NodeType == XmlNodeType.Element)
                                        {
                                            if ("PRESSURESENSOR" == pneumatic_io_subnode.Name.ToUpper())
                                            {
                                                if (!bRequiredPressureSensorSectionFound)
                                                {
                                                    bRequiredPressureSensorSectionFound = true;
                                                }
                                                else
                                                {
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Pneumatic_IO> section has duplicate <" + pneumatic_io_subnode.Name.ToUpper() + "> entry.");
                                                }

                                                bool bRequiredNI_AISectionFound = false;
                                                foreach (Positional_XmlElement NI_AI_node in pneumatic_io_subnode)
                                                {
                                                    if (NI_AI_node.NodeType == XmlNodeType.Element)
                                                    {
                                                        if ("NI_AI" == NI_AI_node.Name.ToUpper())
                                                        {
                                                            if (!bRequiredNI_AISectionFound)
                                                            {
                                                                bRequiredNI_AISectionFound = true;
                                                            }
                                                            else
                                                            {
                                                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><PressureSensor> section has duplicate <" + NI_AI_node.Name.ToUpper() + "> entry.");
                                                            }

                                                            bool bRequiredChannelNameFound = false;
                                                            bool bRequiredMaxReadbackFound = false;
                                                            bool bRequiredMinReadbackFound = false;
                                                            foreach (Positional_XmlElement ni_ai_subnode in NI_AI_node)
                                                            {
                                                                if (ni_ai_subnode.NodeType == XmlNodeType.Element)
                                                                {
                                                                    if ("CHANNELNAME" == ni_ai_subnode.Name.ToUpper())
                                                                    {
                                                                        if (bRequiredChannelNameFound)
                                                                        {
                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><PressureSensor><NI_AI> section has duplicate <ChannelName> entry.");
                                                                        }
                                                                        else
                                                                        {
                                                                            bRequiredChannelNameFound = true; // Required definition
                                                                            PCRConfigurationObj.m_PneumaticIOControls.m_UpstreamPressureSensor.m_strNIControllerChannel = ni_ai_subnode.InnerText;
                                                                        }
                                                                    }
                                                                    else if ("MAXREADBACK" == ni_ai_subnode.Name.ToUpper())
                                                                    {
                                                                        if (bRequiredMaxReadbackFound)
                                                                        {
                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><PressureSensor><NI_AI> section has duplicate <MaxReadback> entry.");
                                                                        }
                                                                        else
                                                                        {
                                                                            bRequiredMaxReadbackFound = true; // Required definition
                                                                            PCRConfigurationObj.m_PneumaticIOControls.m_UpstreamPressureSensor.m_fMaxReadback = Convert.ToSingle(ni_ai_subnode.InnerText);
                                                                        }
                                                                    }
                                                                    else if ("MINREADBACK" == ni_ai_subnode.Name.ToUpper())
                                                                    {
                                                                        if (bRequiredMinReadbackFound)
                                                                        {
                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><PressureSensor><NI_AI> section has duplicate <MinReadback> entry.");
                                                                        }
                                                                        else
                                                                        {
                                                                            bRequiredMinReadbackFound = true; // Required definition
                                                                            PCRConfigurationObj.m_PneumaticIOControls.m_UpstreamPressureSensor.m_fMinReadback = Convert.ToSingle(ni_ai_subnode.InnerText);
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        // Unexpected tag defined - parse error
                                                                        throw new CPCRInstrumentSystemException("Configuration file parse error.  <ChannelName>, <MaxSetpoint> or <MinSetpoint> definitions expected following <Pneumatic_IO_Handling><Pneumatic_IO><PressureSensor><NI_AI> tag, instead found <" + ni_ai_subnode.Name + ">.");
                                                                    }
                                                                }
                                                            }
                                                            // Verify required fields present
                                                            if (!(bRequiredChannelNameFound && bRequiredMaxReadbackFound && bRequiredMinReadbackFound))
                                                            {
                                                                string strMissingElement = (!bRequiredChannelNameFound) ? "<ChannelName>" :
                                                                                           (!bRequiredMaxReadbackFound) ? "<MaxReadback>" : "<MinReadback>";
                                                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Pneumatic_IO><PressureSensor><NI_AI> tag.");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // Unexpected tag defined - parse error
                                                            throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <Pneumatic_IO_Handling><Pneumatic_IO><PressureSensor> tag, instead found <" + NI_AI_node.Name + ">.");
                                                        }
                                                    }
                                                }
                                                if (!bRequiredNI_AISectionFound)
                                                {
                                                    string strMissingElement = "<NI_AO>";
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Pneumatic_IO><PressureSensor> tag.");
                                                }
                                            }
                                            else if ("INLETVALVE" == pneumatic_io_subnode.Name.ToUpper())
                                            {
                                                if (!bRequiredInletValveSectionFound)
                                                {
                                                    bRequiredInletValveSectionFound = true;
                                                }
                                                else
                                                {
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Pneumatic_IO> section has duplicate <" + pneumatic_io_subnode.Name.ToUpper() + "> entry.");
                                                }

                                                bool bRequiredControlSectionFound = false;
                                                foreach (Positional_XmlElement control_node in pneumatic_io_subnode)
                                                {
                                                    if (control_node.NodeType == XmlNodeType.Element)
                                                    {
                                                        if ("CONTROL" == control_node.Name.ToUpper())
                                                        {
                                                            if (!bRequiredControlSectionFound)
                                                            {
                                                                bRequiredControlSectionFound = true;
                                                            }
                                                            else
                                                            {
                                                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><InletValve> section has duplicate <" + control_node.Name.ToUpper() + "> entry.");
                                                            }

                                                            bool bRequiredAccelDIOSectionFound = false;
                                                            foreach (Positional_XmlElement accel_DIO_node in control_node)
                                                            {
                                                                if (accel_DIO_node.NodeType == XmlNodeType.Element)
                                                                {
                                                                    if ("ACCEL_DIO" == accel_DIO_node.Name.ToUpper())
                                                                    {
                                                                        if (!bRequiredAccelDIOSectionFound)
                                                                        {
                                                                            bRequiredAccelDIOSectionFound = true;
                                                                        }
                                                                        else
                                                                        {
                                                                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><InletValve><Control> section has duplicate <" + accel_DIO_node.Name.ToUpper() + "> entry.");
                                                                        }

                                                                        bool bRequiredMotorControllerFound = false;
                                                                        bool bRequiredDOFound = false;
                                                                        bool bOptionalDutyCycleFound = false;
                                                                        foreach (Positional_XmlElement accel_do_node in accel_DIO_node)
                                                                        {
                                                                            if (accel_do_node.NodeType == XmlNodeType.Element)
                                                                            {
                                                                                if ("MOTORCONTROLLER" == accel_do_node.Name.ToUpper())
                                                                                {
                                                                                    if (bRequiredMotorControllerFound)
                                                                                    {
                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><InletValve><Control><Accel_DIO> section has duplicate <MotorController> entry.");
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        bRequiredMotorControllerFound = true; // Required definition
                                                                                        PCRConfigurationObj.m_PneumaticIOControls.m_InletValveControl.m_strMotorController = accel_do_node.InnerText;
                                                                                    }
                                                                                }
                                                                                else if ("DO" == accel_do_node.Name.ToUpper())
                                                                                {
                                                                                    if (bRequiredDOFound)
                                                                                    {
                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><InletValve><Control><Accel_DIO> section has duplicate <DO> entry.");
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        bRequiredDOFound = true; // Required definition
                                                                                        PCRConfigurationObj.m_PneumaticIOControls.m_InletValveControl.m_uiDO_Channel = Convert.ToUInt32(accel_do_node.InnerText);
                                                                                    }
                                                                                }
                                                                                else if ("DUTYCYCLE" == accel_do_node.Name.ToUpper())
                                                                                {
                                                                                    if (bOptionalDutyCycleFound)
                                                                                    {
                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><InletValve><Control><Accel_DIO> section has duplicate <DutyCycle> entry.");
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        bOptionalDutyCycleFound = true; // Required definition
                                                                                        PCRConfigurationObj.m_PneumaticIOControls.m_InletValveControl.m_uiDutyCycle = Convert.ToUInt32(accel_do_node.InnerText);
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    // Unexpected tag defined - parse error
                                                                                    throw new CPCRInstrumentSystemException("Configuration file parse error.  <MotorController> and <DO> definitions expected following <Pneumatic_IO_Handling><Pneumatic_IO><InletValve><Control><Accel_DIO> tag, instead found <" + accel_do_node.Name + ">.");
                                                                                }
                                                                            }
                                                                        }
                                                                        // Verify required fields present
                                                                        if (!(bRequiredMotorControllerFound && bRequiredDOFound))
                                                                        {
                                                                            string strMissingElement = (!bRequiredMotorControllerFound) ? "<MotorController>" : "<DO>";
                                                                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Pneumatic_IO><InletValve><Control><Accel_DIO> tag.");
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        // Unexpected tag defined - parse error
                                                                        throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <Pneumatic_IO_Handling><Pneumatic_IO><InletValve><Control> tag, instead found <" + accel_DIO_node.Name + ">.");
                                                                    }
                                                                }
                                                            }
                                                            if (!bRequiredAccelDIOSectionFound)
                                                            {
                                                                string strMissingElement = "<Accel_DIO>";
                                                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Pneumatic_IO><InletValve><Control> tag.");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // Unexpected tag defined - parse error
                                                            throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <Pneumatic_IO_Handling><Pneumatic_IO><InletValve> tag, instead found <" + control_node.Name + ">.");
                                                        }
                                                    }
                                                }
                                                if (!bRequiredControlSectionFound)
                                                {
                                                    string strMissingElement = "<Control>";
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Pneumatic_IO><InletValve> tag.");
                                                }
                                            }
                                            else if ("OUTLETVALVE" == pneumatic_io_subnode.Name.ToUpper())
                                            {
                                                if (!bRequiredOutletValveSectionFound)
                                                {
                                                    bRequiredOutletValveSectionFound = true;
                                                }
                                                else
                                                {
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Pneumatic_IO> section has duplicate <" + pneumatic_io_subnode.Name.ToUpper() + "> entry.");
                                                }

                                                bool bRequiredControlSectionFound = false;
                                                foreach (Positional_XmlElement control_node in pneumatic_io_subnode)
                                                {
                                                    if (control_node.NodeType == XmlNodeType.Element)
                                                    {
                                                        if ("CONTROL" == control_node.Name.ToUpper())
                                                        {
                                                            if (!bRequiredControlSectionFound)
                                                            {
                                                                bRequiredControlSectionFound = true;
                                                            }
                                                            else
                                                            {
                                                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><OutletValve> section has duplicate <" + control_node.Name.ToUpper() + "> entry.");
                                                            }

                                                            bool bRequiredAccelDIOSectionFound = false;
                                                            foreach (Positional_XmlElement accel_DIO_node in control_node)
                                                            {
                                                                if (accel_DIO_node.NodeType == XmlNodeType.Element)
                                                                {
                                                                    if ("ACCEL_DIO" == accel_DIO_node.Name.ToUpper())
                                                                    {
                                                                        if (!bRequiredAccelDIOSectionFound)
                                                                        {
                                                                            bRequiredAccelDIOSectionFound = true;
                                                                        }
                                                                        else
                                                                        {
                                                                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><OutletValve><Control> section has duplicate <" + accel_DIO_node.Name.ToUpper() + "> entry.");
                                                                        }

                                                                        bool bRequiredMotorControllerFound = false;
                                                                        bool bRequiredDOFound = false;
                                                                        bool bOptionalDutyCycleFound = false;
                                                                        foreach (Positional_XmlElement accel_do_node in accel_DIO_node)
                                                                        {
                                                                            if (accel_do_node.NodeType == XmlNodeType.Element)
                                                                            {
                                                                                if ("MOTORCONTROLLER" == accel_do_node.Name.ToUpper())
                                                                                {
                                                                                    if (bRequiredMotorControllerFound)
                                                                                    {
                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><OutletValve><Control><Accel_DIO> section has duplicate <MotorController> entry.");
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        bRequiredMotorControllerFound = true; // Required definition
                                                                                        PCRConfigurationObj.m_PneumaticIOControls.m_OutletValveControl.m_strMotorController = accel_do_node.InnerText;
                                                                                    }
                                                                                }
                                                                                else if ("DO" == accel_do_node.Name.ToUpper())
                                                                                {
                                                                                    if (bRequiredDOFound)
                                                                                    {
                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><OutletValve><Control><Accel_DIO> section has duplicate <DO> entry.");
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        bRequiredDOFound = true; // Required definition
                                                                                        PCRConfigurationObj.m_PneumaticIOControls.m_OutletValveControl.m_uiDO_Channel = Convert.ToUInt32(accel_do_node.InnerText);
                                                                                    }
                                                                                }
                                                                                else if ("DUTYCYCLE" == accel_do_node.Name.ToUpper())
                                                                                {
                                                                                    if (bOptionalDutyCycleFound)
                                                                                    {
                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><OutletValve><Control><Accel_DIO> section has duplicate <DutyCycle> entry.");
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        bOptionalDutyCycleFound = true; // Required definition
                                                                                        PCRConfigurationObj.m_PneumaticIOControls.m_OutletValveControl.m_uiDutyCycle = Convert.ToUInt32(accel_do_node.InnerText);
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    // Unexpected tag defined - parse error
                                                                                    throw new CPCRInstrumentSystemException("Configuration file parse error.  <MotorController> and <DO> definitions expected following <Pneumatic_IO_Handling><Pneumatic_IO><OutletValve><Control><Accel_DIO> tag, instead found <" + accel_do_node.Name + ">.");
                                                                                }
                                                                            }
                                                                        }
                                                                        // Verify required fields present
                                                                        if (!(bRequiredMotorControllerFound && bRequiredDOFound))
                                                                        {
                                                                            string strMissingElement = (!bRequiredMotorControllerFound) ? "<MotorController>" : "<DO>";
                                                                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Pneumatic_IO><OutletValve><Control><Accel_DIO> tag.");
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        // Unexpected tag defined - parse error
                                                                        throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <Pneumatic_IO_Handling><Pneumatic_IO><OutletValve><Control> tag, instead found <" + accel_DIO_node.Name + ">.");
                                                                    }
                                                                }
                                                            }
                                                            if (!bRequiredAccelDIOSectionFound)
                                                            {
                                                                string strMissingElement = "<Accel_DIO>";
                                                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Pneumatic_IO><OutletValve><Control> tag.");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // Unexpected tag defined - parse error
                                                            throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <Pneumatic_IO_Handling><Pneumatic_IO><OutletValve> tag, instead found <" + control_node.Name + ">.");
                                                        }
                                                    }
                                                }
                                                if (!bRequiredControlSectionFound)
                                                {
                                                    string strMissingElement = "<Control>";
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Pneumatic_IO><OutletValve> tag.");
                                                }
                                            }
                                            else if ("VENTVALVE" == pneumatic_io_subnode.Name.ToUpper())
                                            {
                                                if (!bRequiredVentValveSectionFound)
                                                {
                                                    bRequiredVentValveSectionFound = true;
                                                }
                                                else
                                                {
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Pneumatic_IO> section has duplicate <" + pneumatic_io_subnode.Name.ToUpper() + "> entry.");
                                                }

                                                bool bRequiredControlSectionFound = false;
                                                foreach (Positional_XmlElement control_node in pneumatic_io_subnode)
                                                {
                                                    if (control_node.NodeType == XmlNodeType.Element)
                                                    {
                                                        if ("CONTROL" == control_node.Name.ToUpper())
                                                        {
                                                            if (!bRequiredControlSectionFound)
                                                            {
                                                                bRequiredControlSectionFound = true;
                                                            }
                                                            else
                                                            {
                                                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><VentValve> section has duplicate <" + control_node.Name.ToUpper() + "> entry.");
                                                            }

                                                            bool bRequiredAccelDIOSectionFound = false;
                                                            foreach (Positional_XmlElement accel_DIO_node in control_node)
                                                            {
                                                                if (accel_DIO_node.NodeType == XmlNodeType.Element)
                                                                {
                                                                    if ("ACCEL_DIO" == accel_DIO_node.Name.ToUpper())
                                                                    {
                                                                        if (!bRequiredAccelDIOSectionFound)
                                                                        {
                                                                            bRequiredAccelDIOSectionFound = true;
                                                                        }
                                                                        else
                                                                        {
                                                                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><VentValve><Control> section has duplicate <" + accel_DIO_node.Name.ToUpper() + "> entry.");
                                                                        }

                                                                        bool bRequiredMotorControllerFound = false;
                                                                        bool bRequiredDOFound = false;
                                                                        bool bOptionalDutyCycleFound = false;
                                                                        foreach (Positional_XmlElement accel_do_node in accel_DIO_node)
                                                                        {
                                                                            if (accel_do_node.NodeType == XmlNodeType.Element)
                                                                            {
                                                                                if ("MOTORCONTROLLER" == accel_do_node.Name.ToUpper())
                                                                                {
                                                                                    if (bRequiredMotorControllerFound)
                                                                                    {
                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><VentValve><Control><Accel_DIO> section has duplicate <MotorController> entry.");
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        bRequiredMotorControllerFound = true; // Required definition
                                                                                        PCRConfigurationObj.m_PneumaticIOControls.m_VentValveControl.m_strMotorController = accel_do_node.InnerText;
                                                                                    }
                                                                                }
                                                                                else if ("DO" == accel_do_node.Name.ToUpper())
                                                                                {
                                                                                    if (bRequiredDOFound)
                                                                                    {
                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><VentValve><Control><Accel_DIO> section has duplicate <DO> entry.");
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        bRequiredDOFound = true; // Required definition
                                                                                        PCRConfigurationObj.m_PneumaticIOControls.m_VentValveControl.m_uiDO_Channel = Convert.ToUInt32(accel_do_node.InnerText);
                                                                                    }
                                                                                }
                                                                                else if ("DUTYCYCLE" == accel_do_node.Name.ToUpper())
                                                                                {
                                                                                    if (bOptionalDutyCycleFound)
                                                                                    {
                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Pneumatic_IO_Handling><Pneumatic_IO><VentValve><Control><Accel_DIO> section has duplicate <DutyCycle> entry.");
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        bOptionalDutyCycleFound = true; // Required definition
                                                                                        PCRConfigurationObj.m_PneumaticIOControls.m_VentValveControl.m_uiDutyCycle = Convert.ToUInt32(accel_do_node.InnerText);
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    // Unexpected tag defined - parse error
                                                                                    throw new CPCRInstrumentSystemException("Configuration file parse error.  <MotorController> and <DO> definitions expected following <Pneumatic_IO_Handling><Pneumatic_IO><VentValve><Control><Accel_DIO> tag, instead found <" + accel_do_node.Name + ">.");
                                                                                }
                                                                            }
                                                                        }
                                                                        // Verify required fields present
                                                                        if (!(bRequiredMotorControllerFound && bRequiredDOFound))
                                                                        {
                                                                            string strMissingElement = (!bRequiredMotorControllerFound) ? "<MotorController>" : "<DO>";
                                                                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Pneumatic_IO><VentValve><Control><Accel_DIO> tag.");
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        // Unexpected tag defined - parse error
                                                                        throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <Pneumatic_IO_Handling><Pneumatic_IO><VentValve><Control> tag, instead found <" + accel_DIO_node.Name + ">.");
                                                                    }
                                                                }
                                                            }
                                                            if (!bRequiredAccelDIOSectionFound)
                                                            {
                                                                string strMissingElement = "<Accel_DIO>";
                                                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Pneumatic_IO><VentValve><Control> tag.");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // Unexpected tag defined - parse error
                                                            throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <Pneumatic_IO_Handling><Pneumatic_IO><VentValve> tag, instead found <" + control_node.Name + ">.");
                                                        }
                                                    }
                                                }
                                                if (!bRequiredControlSectionFound)
                                                {
                                                    string strMissingElement = "<Control>";
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Pneumatic_IO><VentValve> tag.");
                                                }
                                            }
                                            else
                                            {
                                                // Unexpected tag defined - parse error
                                                throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <Pneumatic_IO_Handling><Pneumatic_IO> tag, instead found <" + pneumatic_io_subnode.Name + ">.");
                                            }
                                        }
                                    }
                                    if (!(bRequiredPressureSensorSectionFound && bRequiredInletValveSectionFound && bRequiredOutletValveSectionFound && bRequiredVentValveSectionFound))
                                    {
                                        string strMissingElement = (!bRequiredPressureSensorSectionFound) ? "<PressureSensor>" :
                                                                   (!bRequiredInletValveSectionFound) ? "<InletValve>" :
                                                                   (!bRequiredOutletValveSectionFound) ? "<InletValve>" : "VentValve";
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <Pneumatic_IO_Handling><Pneumatic_IO> tag.");
                                    }
                                }
                                else
                                {
                                    // Parse error, first node after 'Measurements' must be 'Measurement'
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument settings section contains unexpected <" + pneumatic_io_handling_node.Name + "> tag.");
                                }
                            }
                        }
                        // Verify required fields present
                        if (!(bRequiredPumpIOSectionFound && bRequiredRegulatorIOSectionFound && bRequiredPneumaticIOSectionFound))
                        {
                            string strMissingElement = "";
                            if (!bRequiredPumpIOSectionFound) strMissingElement = "<Pump_IO>";
                            else if (!bRequiredRegulatorIOSectionFound) strMissingElement = "<Regulator_IO>";
                            else strMissingElement = "<Pneumatic_IO>";
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Missing " + strMissingElement + " required following <Pneumatic_IO_Handling> tag.");
                        }
                    }
                    else if ("EMITTER_IO_HANDLING" == hw_setting_section_node.Name.ToUpper())
                    {
                        if (!bRequiredEmitter_IO_Handling_SectionFound)
                        {
                            bRequiredEmitter_IO_Handling_SectionFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <HWSettings> section has duplicate <" + hw_setting_section_node.Name.ToUpper() + "> entry.");
                        }

                        bool bRequiredEnableSectionFound = false;
                        bool bRequiredSetpointSectionFound = false;
                        bool bRequiredDefaultSetpointSectionFound = false;
                        bool bRequiredFanSectionFound = false;
                        foreach (Positional_XmlElement emitter_io_handling_node in hw_setting_section_node)
                        {
                            if (emitter_io_handling_node.NodeType == XmlNodeType.Element)
                            {
                                if ("ENABLE" == emitter_io_handling_node.Name.ToUpper())
                                {
                                    if (!bRequiredEnableSectionFound)
                                    {
                                        bRequiredEnableSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <HWSettings><Emitter_IO_Handling> section has duplicate <" + emitter_io_handling_node.Name.ToUpper() + "> entry.");
                                    }

                                    bool bRequiredNIDIOSectionFound = false;
                                    foreach (Positional_XmlElement NI_DIO_node in emitter_io_handling_node)
                                    {
                                        if (NI_DIO_node.NodeType == XmlNodeType.Element)
                                        {
                                            if ("NI_DIO" == NI_DIO_node.Name.ToUpper())
                                            {
                                                if (!bRequiredNIDIOSectionFound)
                                                {
                                                    bRequiredNIDIOSectionFound = true;
                                                }
                                                else
                                                {
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <HWSettings><Emitter_IO_Handling><Enable> section has duplicate <" + NI_DIO_node.Name.ToUpper() + "> entry.");
                                                }

                                                bool bRequiredChannelNameFound = false;
                                                bool bRequiredDOFound = false;
                                                foreach (Positional_XmlElement NI_do_node in NI_DIO_node)
                                                {
                                                    if (NI_do_node.NodeType == XmlNodeType.Element)
                                                    {
                                                        if ("CHANNELNAME" == NI_do_node.Name.ToUpper())
                                                        {
                                                            if (bRequiredChannelNameFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <HWSettings><Emitter_IO_Handling><Enable><NI_DIO> section has duplicate <ChannelName> entry.");
                                                            }
                                                            else
                                                            {
                                                                bRequiredChannelNameFound = true; // Required definition
                                                                PCRConfigurationObj.m_OpticalEmitterConfiguration.m_EmitterOnOffControl.m_strNIControllerChannel = NI_do_node.InnerText;
                                                            }
                                                        }
                                                        else if ("DO" == NI_do_node.Name.ToUpper())
                                                        {
                                                            if (bRequiredDOFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <HWSettings><Emitter_IO_Handling><Enable><NI_DIO> section has duplicate <DO> entry.");
                                                            }
                                                            else
                                                            {
                                                                bRequiredDOFound = true; // Required definition
                                                                PCRConfigurationObj.m_OpticalEmitterConfiguration.m_EmitterOnOffControl.m_uiDO_Channel = Convert.ToUInt32(NI_do_node.InnerText);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // Unexpected tag defined - parse error
                                                            throw new CPCRInstrumentSystemException("Configuration file parse error.  <ChannelName> and <DO> definitions expected following <HWSettings><Emitter_IO_Handling><Enable><NI_DIO> tag, instead found <" + NI_do_node.Name + ">.");
                                                        }
                                                    }
                                                }
                                                // Verify required fields present
                                                if (!(bRequiredChannelNameFound && bRequiredDOFound))
                                                {
                                                    string strMissingElement = (!bRequiredChannelNameFound) ? "<ChannelName>" : "<DO>";
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <HWSettings><Emitter_IO_Handling><Enable><NI_DIO> tag.");
                                                }
                                            }
                                            else
                                            {
                                                // Unexpected tag defined - parse error
                                                throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <HWSettings><Emitter_IO_Handling><Enable> tag, instead found <" + NI_DIO_node.Name + ">.");
                                            }
                                        }
                                    }
                                    if (!bRequiredNIDIOSectionFound)
                                    {
                                        string strMissingElement = "<NI_DIO>";
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <HWSettings><Emitter_IO_Handling><Enable> tag.");
                                    }
                                }
                                else if ("SETPOINT" == emitter_io_handling_node.Name.ToUpper())
                                {
                                    if (!bRequiredSetpointSectionFound)
                                    {
                                        bRequiredSetpointSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <HWSettings><Emitter_IO_Handling> section has duplicate <" + emitter_io_handling_node.Name.ToUpper() + "> entry.");
                                    }

                                    bool bRequiredNI_AOSectionFound = false;
                                    foreach (Positional_XmlElement NI_AO_node in emitter_io_handling_node)
                                    {
                                        if (NI_AO_node.NodeType == XmlNodeType.Element)
                                        {
                                            if ("NI_AO" == NI_AO_node.Name.ToUpper())
                                            {
                                                if (!bRequiredNI_AOSectionFound)
                                                {
                                                    bRequiredNI_AOSectionFound = true;
                                                }
                                                else
                                                {
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <HWSettings><Emitter_IO_Handling><Setpoint> section has duplicate <" + NI_AO_node.Name.ToUpper() + "> entry.");
                                                }

                                                bool bRequiredChannelNameFound = false;
                                                bool bRequiredMaxSetpointFound = false;
                                                bool bRequiredMinSetpointFound = false;
                                                foreach (Positional_XmlElement ni_ao_subnode in NI_AO_node)
                                                {
                                                    if (ni_ao_subnode.NodeType == XmlNodeType.Element)
                                                    {
                                                        if ("CHANNELNAME" == ni_ao_subnode.Name.ToUpper())
                                                        {
                                                            if (bRequiredChannelNameFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Emitter_IO_Handling><Setpoint><NI_AO> section has duplicate <ChannelName> entry.");
                                                            }
                                                            else
                                                            {
                                                                bRequiredChannelNameFound = true; // Required definition
                                                                PCRConfigurationObj.m_OpticalEmitterConfiguration.m_EmitterAmplitudeControl.m_strNIControllerChannel = ni_ao_subnode.InnerText;
                                                            }
                                                        }
                                                        else if ("MAXSETPOINT" == ni_ao_subnode.Name.ToUpper())
                                                        {
                                                            if (bRequiredMaxSetpointFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Emitter_IO_Handling><Setpoint><NI_AO> section has duplicate <MaxSetpoint> entry.");
                                                            }
                                                            else
                                                            {
                                                                bRequiredMaxSetpointFound = true; // Required definition
                                                                PCRConfigurationObj.m_OpticalEmitterConfiguration.m_EmitterAmplitudeControl.m_fMaxSetpoint = Convert.ToSingle(ni_ao_subnode.InnerText);
                                                            }
                                                        }
                                                        else if ("MINSETPOINT" == ni_ao_subnode.Name.ToUpper())
                                                        {
                                                            if (bRequiredMinSetpointFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Emitter_IO_Handling><Setpoint><NI_AO> section has duplicate <MinSetpoint> entry.");
                                                            }
                                                            else
                                                            {
                                                                bRequiredMinSetpointFound = true; // Required definition
                                                                PCRConfigurationObj.m_OpticalEmitterConfiguration.m_EmitterAmplitudeControl.m_fMinSetpoint = Convert.ToSingle(ni_ao_subnode.InnerText);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // Unexpected tag defined - parse error
                                                            throw new CPCRInstrumentSystemException("Configuration file parse error.  <ChannelName>, <MaxSetpoint> or <MinSetpoint> definitions expected following <HWSettings><Emitter_IO_Handling><Setpoint><NI_AO> tag, instead found <" + ni_ao_subnode.Name + ">.");
                                                        }
                                                    }
                                                }
                                                // Verify required fields present
                                                if (!(bRequiredChannelNameFound && bRequiredMaxSetpointFound && bRequiredMinSetpointFound))
                                                {
                                                    string strMissingElement = (!bRequiredChannelNameFound) ? "<ChannelName>" :
                                                                               (!bRequiredMaxSetpointFound) ? "<MaxSetpoint>" : "<MinSetpoint>";
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <HWSettings><Emitter_IO_Handling><Setpoint><NI_AO> tag.");
                                                }
                                            }
                                            else
                                            {
                                                // Unexpected tag defined - parse error
                                                throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <HWSettings><Emitter_IO_Handling><Setpoint> tag, instead found <" + NI_AO_node.Name + ">.");
                                            }
                                        }
                                    }
                                    if (!bRequiredNI_AOSectionFound)
                                    {
                                        string strMissingElement = "<NI_AO>";
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <HWSettings><Emitter_IO_Handling><Setpoint> tag.");
                                    }
                                }
                                else if ("DEFAULT_SETPOINT" == emitter_io_handling_node.Name.ToUpper())
                                {
                                    if (!bRequiredDefaultSetpointSectionFound)
                                    {
                                        bRequiredDefaultSetpointSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <HWSettings><Emitter_IO_Handling> section has duplicate <" + emitter_io_handling_node.Name.ToUpper() + "> entry.");
                                    }
                                    try
                                    {
                                        PCRConfigurationObj.m_OpticalEmitterConfiguration.m_iDefaultSetpoint = Convert.ToInt32(emitter_io_handling_node.InnerText);
                                    }
                                    catch(Exception /*Exc*/)
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error. <HWSettings><Emitter_IO_Handling> <Default_Setpoint> specifies an invalid integer value.");
                                    }
                                }
                                else if ("FAN" == emitter_io_handling_node.Name.ToUpper())
                                {
                                    if (!bRequiredFanSectionFound)
                                    {
                                        bRequiredFanSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <HWSettings><Emitter_IO_Handling>  section has duplicate <" + emitter_io_handling_node.Name.ToUpper() + "> entry.");
                                    }

                                    bool bRequiredAccelDIOSectionFound = false;
                                    foreach (Positional_XmlElement accel_DIO_node in emitter_io_handling_node)
                                    {
                                        if (accel_DIO_node.NodeType == XmlNodeType.Element)
                                        {
                                            if ("ACCEL_DIO" == accel_DIO_node.Name.ToUpper())
                                            {
                                                if (!bRequiredAccelDIOSectionFound)
                                                {
                                                    bRequiredAccelDIOSectionFound = true;
                                                }
                                                else
                                                {
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <HWSettings><Emitter_IO_Handling> <Fan> section has duplicate <" + accel_DIO_node.Name.ToUpper() + "> entry.");
                                                }

                                                bool bRequiredMotorControllerFound = false;
                                                bool bRequiredDOFound = false;
                                                bool bOptionalDutyCycleFound = false;
                                                foreach (Positional_XmlElement accel_do_node in accel_DIO_node)
                                                {
                                                    if (accel_do_node.NodeType == XmlNodeType.Element)
                                                    {
                                                        if ("MOTORCONTROLLER" == accel_do_node.Name.ToUpper())
                                                        {
                                                            if (bRequiredMotorControllerFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <HWSettings><Emitter_IO_Handling><Fan><Accel_DIO> section has duplicate <MotorController> entry.");
                                                            }
                                                            else
                                                            {
                                                                bRequiredMotorControllerFound = true; // Required definition
                                                                PCRConfigurationObj.m_OpticalEmitterConfiguration.m_FanControl.m_strMotorController = accel_do_node.InnerText;
                                                            }
                                                        }
                                                        else if ("DO" == accel_do_node.Name.ToUpper())
                                                        {
                                                            if (bRequiredDOFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <HWSettings><Emitter_IO_Handling><Fan><Accel_DIO> section has duplicate <DO> entry.");
                                                            }
                                                            else
                                                            {
                                                                bRequiredDOFound = true; // Required definition
                                                                PCRConfigurationObj.m_OpticalEmitterConfiguration.m_FanControl.m_uiDO_Channel = Convert.ToUInt32(accel_do_node.InnerText);
                                                            }
                                                        }
                                                        else if ("DUTYCYCLE" == accel_do_node.Name.ToUpper())
                                                        {
                                                            if (bOptionalDutyCycleFound)
                                                            {
                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <HWSettings><Emitter_IO_Handling><Fan><Accel_DIO> section has duplicate <DutyCycle> entry.");
                                                            }
                                                            else
                                                            {
                                                                bOptionalDutyCycleFound = true; // Required definition
                                                                PCRConfigurationObj.m_OpticalEmitterConfiguration.m_FanControl.m_uiDutyCycle = Convert.ToUInt32(accel_do_node.InnerText);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // Unexpected tag defined - parse error
                                                            throw new CPCRInstrumentSystemException("Configuration file parse error.  <MotorController> and <DO> definitions expected following <HWSettings><Emitter_IO_Handling><Fan><Accel_DIO> tag, instead found <" + accel_do_node.Name + ">.");
                                                        }
                                                    }
                                                }
                                                // Verify required fields present
                                                if (!(bRequiredMotorControllerFound && bRequiredDOFound))
                                                {
                                                    string strMissingElement = (!bRequiredMotorControllerFound) ? "<MotorController>" : "<DO>";
                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <HWSettings><Emitter_IO_Handling><Fan><Accel_DIO> tag.");
                                                }
                                            }
                                            else
                                            {
                                                // Unexpected tag defined - parse error
                                                throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <HWSettings><Emitter_IO_Handling><Fan> tag, instead found <" + accel_DIO_node.Name + ">.");
                                            }
                                        }
                                    }
                                    if (!bRequiredAccelDIOSectionFound)
                                    {
                                        string strMissingElement = "<Accel_DIO>";
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <HWSettings><Emitter_IO_Handling><Fan> tag.");
                                    }
                                }
                                else
                                {
                                    // Unexpected tag defined - parse error
                                    throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <HWSettings><Emitter_IO_Handling> tag, instead found <" + emitter_io_handling_node.Name + ">.");
                                }
                            }
                        }
                        if (!(bRequiredEnableSectionFound && bRequiredSetpointSectionFound && bRequiredFanSectionFound && bRequiredDefaultSetpointSectionFound))
                        {
                            string strMissingElement = (!bRequiredEnableSectionFound) ? "<Enable>" :
                                                       (!bRequiredSetpointSectionFound) ? "<Setpoint>" :
                                                       (!bRequiredFanSectionFound) ? "<Fan>" : "<Default_Setpoint>";
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <HWSettings><Emitter_IO_Handling> tag.");
                        }
                    }
                    else if ("CAMERA_HANDLING" == hw_setting_section_node.Name.ToUpper())
                    {
                        if (!bRequiredCamera_Handling_SectionFound)
                        {
                            bRequiredCamera_Handling_SectionFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <HWSettings> section has duplicate <" + hw_setting_section_node.Name.ToUpper() + "> entry.");
                        }

                        bool bRequiredManualImageSaveFolderSectionFound = false;
                        bool bRequiredInitialSettingsFilenameSectionFound = false;
                        foreach (Positional_XmlElement camera_handling_node in hw_setting_section_node)
                        {
                            if (camera_handling_node.NodeType == XmlNodeType.Element)
                            {
                                if ("MANUALIMAGESAVEFOLDER" == camera_handling_node.Name.ToUpper())
                                {
                                    if (!bRequiredManualImageSaveFolderSectionFound)
                                    {
                                        bRequiredManualImageSaveFolderSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <HWSettings><Camera_Handling> section has duplicate <" + camera_handling_node.Name.ToUpper() + "> entry.");
                                    }
                                    PCRConfigurationObj.m_Camera_Information.m_strManualSaveFolder = camera_handling_node.InnerText;
                                    if (PCRConfigurationObj.m_Camera_Information.m_strManualSaveFolder[PCRConfigurationObj.m_Camera_Information.m_strManualSaveFolder.Length-1] != '\\')
                                    {
                                        PCRConfigurationObj.m_Camera_Information.m_strManualSaveFolder += "\\";
                                    }
                                }
                                else if ("INITIALSETTINGSFILENAME" == camera_handling_node.Name.ToUpper())
                                {
                                    if (!bRequiredInitialSettingsFilenameSectionFound)
                                    {
                                        bRequiredInitialSettingsFilenameSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  <HWSettings><Camera_Handling> section has duplicate <" + camera_handling_node.Name.ToUpper() + "> entry.");
                                    }
                                    PCRConfigurationObj.m_Camera_Information.m_strInitialSettingsFilename = camera_handling_node.InnerText;
                                }
                                else
                                {
                                    // Unexpected tag defined - parse error
                                    throw new CPCRInstrumentSystemException("Configuration file parse error.  Didn't find expected definitions following <HWSettings><Camera_Handling> tag, instead found <" + camera_handling_node.Name + ">.");
                                }
                            }
                        }
                        if (!(bRequiredManualImageSaveFolderSectionFound && bRequiredInitialSettingsFilenameSectionFound))
                        {
                            string strMissingElement = (!bRequiredManualImageSaveFolderSectionFound) ? "<ManualImageSaveFolder>" : "<InitialSettingsFilename>";
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <HWSettings><Camera_Handling> tag.");
                        }
                    }
                    else
                    {
                        // Parse error, first node after '<HWSettings>' must be '<MotorControllers>' or '<ThermalControllers>' or '<OpticalFilter_Y_Motor>' or '<Camera_X_Motor>' or '<PCR_Fan_Heatsink>' or '<TECControllerChannels>' or '<Pneumatic_Settings>' or '<Pneumatic_IO_Handling>' or '<Emitter_IO_Handling> or '<Camera_Handling>'
                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  '<MotorControllers>' or '<ThermalControllers>' or '<OpticalFilter_Y_Motor>' or '<Camera_X_Motor>' or '<PCR_Fan_Heatsink>' or '<TECControllerChannels>' or '<Pneumatic_Settings>' or '<Pneumatic_IO_Handling>' or '<Emitter_IO_Handling> or '<Camera_Handling>' section expected following <HWSettings> tag.");
                    }
                }
            }
            if (!(bRequiredMotorControllersSectionFound &&
                  bRequiredThermalControllersSectionFound &&
                  bRequiredOpticalFilter_Y_MotorSectionFound && 
                  bRequiredCamera_X_MotorSectionFound &&
                  bRequiredPCR_Fan_Heatsink_SectionFound &&
                  bRequiredTECControllerChannelsSectionFound &&
                  bRequiredPneumatic_SettingsSectionFound &&
                  bRequiredPneumatic_IO_HandlingSectionFound &&
                  bRequiredEmitter_IO_Handling_SectionFound &&
                  bRequiredCamera_Handling_SectionFound
                ))
            {
                // Parse error, first node after '<HWSettings>' must be '<FilterMotor> or <StageMeasurementMotor> or <TECHeater> or <Camera>'
                string strMissingSection = (!bRequiredMotorControllersSectionFound) ? "<MotorControllers>" :
                                           (!bRequiredPCR_Fan_Heatsink_SectionFound) ? "<PCR_HeatSink_Fan>" :
                                           (!bRequiredThermalControllersSectionFound) ? "<ThermalControllers>" :
                                           (!bRequiredOpticalFilter_Y_MotorSectionFound) ? "<OpticalFilter_Y_Motor>" :
                                           (!bRequiredCamera_X_MotorSectionFound) ? "<Camera_X_Motor>" :
                                           (!bRequiredTECControllerChannelsSectionFound) ? "<TECControllerChannels>" :
                                           (!bRequiredPneumatic_SettingsSectionFound) ? "<Pneumatic_Settings>" :
                                           (!bRequiredPneumatic_IO_HandlingSectionFound) ? "<Pneumatic_IO_Handling>" : 
                                           (!bRequiredEmitter_IO_Handling_SectionFound) ? "<Emitter_IO_Handling>" : "<Camera_Handling>";
                throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  " + strMissingSection + " section expected following <HWSettings> tag.");
            }
        }

        private void ParseCalibrationSettings(Positional_XmlElement calibrationsettings_node, CSystem_Configuration_Obj SystemConfigurationObj)
        {
            bool bRequiredOpticalFilterMotorSectionFound = false;
            bool bRequiredCameraMotorSectionFound = false;
            bool bRequiredPneumaticRegulatorSectionFound = false;

            foreach (Positional_XmlElement cal_setting_node in calibrationsettings_node)
            {
                if (cal_setting_node.NodeType == XmlNodeType.Element)
                {
                    if ("OPTICALFILTERMOTOR" == cal_setting_node.Name.ToUpper())
                    {
                        if (!bRequiredOpticalFilterMotorSectionFound)
                        {
                            bRequiredOpticalFilterMotorSectionFound = true;
                        }
                        else
                        {
                             throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument calibration section has duplicate <" + cal_setting_node.Name.ToUpper() + "> entry.");
                        }

                        bool bRequiredFilter1_Y_Offset_from_Home_In_MotorUStepsSectionFound = false;
                        bool bRequiredFilter2_Y_Offset_from_Home_In_MotorUStepsSectionFound = false;
                        bool bRequiredFilter3_Y_Offset_from_Home_In_MotorUStepsSectionFound = false;
                        bool bRequiredBlock_Emitter_Y_Offset_from_Home_In_MotorUStepsSectionFound = false;

                        foreach (Positional_XmlElement optical_filter_motor_node in cal_setting_node)
                        {
                            if (optical_filter_motor_node.NodeType == XmlNodeType.Element)
                            {
                                if ("FILTER1_Y_OFFSET_FROM_HOME_IN_MOTORUSTEPS" == optical_filter_motor_node.Name.ToUpper())
                                {
                                    if (!bRequiredFilter1_Y_Offset_from_Home_In_MotorUStepsSectionFound)
                                    {
                                        bRequiredFilter1_Y_Offset_from_Home_In_MotorUStepsSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument calibration section <OpticalFilterMotor> has duplicate <" + optical_filter_motor_node.Name.ToUpper() + "> entry.");
                                    }
                                    SystemConfigurationObj.m_OpticalFilter_Y_Motor_ChannelCalibration.m_ulFilter_1_Position_Y_Offset_from_Home_In_MotorUSteps = Convert.ToUInt32(optical_filter_motor_node.InnerText);
                                }
                                else if ("FILTER2_Y_OFFSET_FROM_HOME_IN_MOTORUSTEPS" == optical_filter_motor_node.Name.ToUpper())
                                {
                                    if (!bRequiredFilter2_Y_Offset_from_Home_In_MotorUStepsSectionFound)
                                    {
                                        bRequiredFilter2_Y_Offset_from_Home_In_MotorUStepsSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument calibration section <OpticalFilterMotor> has duplicate <" + optical_filter_motor_node.Name.ToUpper() + "> entry.");
                                    }
                                    SystemConfigurationObj.m_OpticalFilter_Y_Motor_ChannelCalibration.m_ulFilter_2_Position_Y_Offset_from_Home_In_MotorUSteps = Convert.ToUInt32(optical_filter_motor_node.InnerText);
                                }
                                else if ("FILTER3_Y_OFFSET_FROM_HOME_IN_MOTORUSTEPS" == optical_filter_motor_node.Name.ToUpper())
                                {
                                    if (!bRequiredFilter3_Y_Offset_from_Home_In_MotorUStepsSectionFound)
                                    {
                                        bRequiredFilter3_Y_Offset_from_Home_In_MotorUStepsSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument calibration section <OpticalFilterMotor> has duplicate <" + optical_filter_motor_node.Name.ToUpper() + "> entry.");
                                    }
                                    SystemConfigurationObj.m_OpticalFilter_Y_Motor_ChannelCalibration.m_ulFilter_3_Position_Y_Offset_from_Home_In_MotorUSteps = Convert.ToUInt32(optical_filter_motor_node.InnerText);
                                }
                                else if ("BLOCK_EMITTER_Y_OFFSET_FROM_HOME_IN_MOTORUSTEPS" == optical_filter_motor_node.Name.ToUpper())
                                {
                                    if (!bRequiredBlock_Emitter_Y_Offset_from_Home_In_MotorUStepsSectionFound)
                                    {
                                        bRequiredBlock_Emitter_Y_Offset_from_Home_In_MotorUStepsSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument calibration section <OpticalFilterMotor> has duplicate <" + optical_filter_motor_node.Name.ToUpper() + "> entry.");
                                    }
                                    SystemConfigurationObj.m_OpticalFilter_Y_Motor_ChannelCalibration.m_ulBlock_Emitter_Position_Y_Offset_from_Home_In_MotorUSteps = Convert.ToUInt32(optical_filter_motor_node.InnerText);
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument calibration section <OpticalFilterMotor> has unexpected <" + optical_filter_motor_node.Name.ToUpper() + "> entry.");
                                }
                            }
                        }
                        if (!(bRequiredBlock_Emitter_Y_Offset_from_Home_In_MotorUStepsSectionFound && bRequiredFilter1_Y_Offset_from_Home_In_MotorUStepsSectionFound && bRequiredFilter2_Y_Offset_from_Home_In_MotorUStepsSectionFound && bRequiredFilter3_Y_Offset_from_Home_In_MotorUStepsSectionFound))
                        {
                            string strMissingElement = (!bRequiredBlock_Emitter_Y_Offset_from_Home_In_MotorUStepsSectionFound) ? "<Block_Emitter_Y_Offset_from_Home_In_MotorUSteps>" :
                                                       (!bRequiredFilter1_Y_Offset_from_Home_In_MotorUStepsSectionFound) ? "<Filter1_Y_Offset_from_Home_In_MotorUSteps>" :
                                                       (!bRequiredFilter2_Y_Offset_from_Home_In_MotorUStepsSectionFound) ? "<Filter2_Y_Offset_from_Home_In_MotorUSteps>" : "<Filter3_Y_Offset_from_Home_In_MotorUSteps>";
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  " + strMissingElement + " section expected following  <CalibrationSettings><OpticalFilterMotor> " + strMissingElement + " tag.");
                        }
                    }
                    else if ("CAMERAMOTOR" == cal_setting_node.Name.ToUpper())
                    {
                        if (!bRequiredCameraMotorSectionFound)
                        {
                            bRequiredCameraMotorSectionFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument calibration section has duplicate <" + cal_setting_node.Name.ToUpper() + "> entry.");
                        }

                        bool bRequiredChip_1_Array_1_2_Position_X_Offset_from_Home_In_MotorUStepsSectionFound = false;
                        bool bRequiredChip_1_Array_3_4_Position_X_Offset_from_Home_In_MotorUStepsSectionFound = false;
                        bool bRequiredChip_1_Array_5_6_Position_X_Offset_from_Home_In_MotorUStepsSectionFound = false;
                        bool bRequiredChip_1_Array_7_8_Position_X_Offset_from_Home_In_MotorUStepsSectionFound = false;

                        foreach (Positional_XmlElement camera_motor_node in cal_setting_node)
                        {
                            if (camera_motor_node.NodeType == XmlNodeType.Element)
                            {
                                if ("CHIP_1_ARRAY_1_2_POSITION_X_OFFSET_FROM_HOME_IN_MOTORUSTEPS" == camera_motor_node.Name.ToUpper())
                                {
                                    if (!bRequiredChip_1_Array_1_2_Position_X_Offset_from_Home_In_MotorUStepsSectionFound)
                                    {
                                        bRequiredChip_1_Array_1_2_Position_X_Offset_from_Home_In_MotorUStepsSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument calibration section <CameraMotor> has duplicate <" + camera_motor_node.Name.ToUpper() + "> entry.");
                                    }
                                    SystemConfigurationObj.m_Camera_X_Motor_ChannelCalibration.m_ulChip_1_Array_1_2_Position_X_Offset_from_Home_In_MotorUSteps = Convert.ToUInt32(camera_motor_node.InnerText);
                                }
                                else if ("CHIP_1_ARRAY_3_4_POSITION_X_OFFSET_FROM_HOME_IN_MOTORUSTEPS" == camera_motor_node.Name.ToUpper())
                                {
                                    if (!bRequiredChip_1_Array_3_4_Position_X_Offset_from_Home_In_MotorUStepsSectionFound)
                                    {
                                        bRequiredChip_1_Array_3_4_Position_X_Offset_from_Home_In_MotorUStepsSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument calibration section <CameraMotor> has duplicate <" + camera_motor_node.Name.ToUpper() + "> entry.");
                                    }
                                    SystemConfigurationObj.m_Camera_X_Motor_ChannelCalibration.m_ulChip_1_Array_3_4_Position_X_Offset_from_Home_In_MotorUSteps = Convert.ToUInt32(camera_motor_node.InnerText);
                                }
                                else if ("CHIP_1_ARRAY_5_6_POSITION_X_OFFSET_FROM_HOME_IN_MOTORUSTEPS" == camera_motor_node.Name.ToUpper())
                                {
                                    if (!bRequiredChip_1_Array_5_6_Position_X_Offset_from_Home_In_MotorUStepsSectionFound)
                                    {
                                        bRequiredChip_1_Array_5_6_Position_X_Offset_from_Home_In_MotorUStepsSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument calibration section <CameraMotor> has duplicate <" + camera_motor_node.Name.ToUpper() + "> entry.");
                                    }
                                    SystemConfigurationObj.m_Camera_X_Motor_ChannelCalibration.m_ulChip_1_Array_5_6_Position_X_Offset_from_Home_In_MotorUSteps = Convert.ToUInt32(camera_motor_node.InnerText);
                                }
                                else if ("CHIP_1_ARRAY_7_8_POSITION_X_OFFSET_FROM_HOME_IN_MOTORUSTEPS" == camera_motor_node.Name.ToUpper())
                                {
                                    if (!bRequiredChip_1_Array_7_8_Position_X_Offset_from_Home_In_MotorUStepsSectionFound)
                                    {
                                        bRequiredChip_1_Array_7_8_Position_X_Offset_from_Home_In_MotorUStepsSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument calibration section <CameraMotor> has duplicate <" + camera_motor_node.Name.ToUpper() + "> entry.");
                                    }
                                    SystemConfigurationObj.m_Camera_X_Motor_ChannelCalibration.m_ulChip_1_Array_7_8_Position_X_Offset_from_Home_In_MotorUSteps = Convert.ToUInt32(camera_motor_node.InnerText);
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument calibration section <CameraMotor> has unexpected <" + camera_motor_node.Name.ToUpper() + "> entry.");
                                }
                            }
                        }
                        if (!(bRequiredChip_1_Array_1_2_Position_X_Offset_from_Home_In_MotorUStepsSectionFound && bRequiredChip_1_Array_3_4_Position_X_Offset_from_Home_In_MotorUStepsSectionFound && bRequiredChip_1_Array_5_6_Position_X_Offset_from_Home_In_MotorUStepsSectionFound && bRequiredChip_1_Array_3_4_Position_X_Offset_from_Home_In_MotorUStepsSectionFound))
                        {
                            string strMissingElement = (!bRequiredChip_1_Array_1_2_Position_X_Offset_from_Home_In_MotorUStepsSectionFound) ? "<Chip_1_Array_1_2_Position_X_Offset_from_Home_In_MotorUSteps>" :
                                                       (!bRequiredChip_1_Array_3_4_Position_X_Offset_from_Home_In_MotorUStepsSectionFound) ? "<Chip_1_Array_3_4_Position_X_Offset_from_Home_In_MotorUSteps>" :
                                                       (!bRequiredChip_1_Array_5_6_Position_X_Offset_from_Home_In_MotorUStepsSectionFound) ? "<Chip_1_Array_5_6_Position_X_Offset_from_Home_In_MotorUSteps>" : "<Chip_1_Array_7_8_Position_X_Offset_from_Home_In_MotorUSteps>";
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  " + strMissingElement + " section expected following  <CalibrationSettings><CameraMotor> " + strMissingElement + " tag.");
                        }
                    }
                    else if ("PNEUMATIC_REGULATOR" == cal_setting_node.Name.ToUpper())
                    {
                        if (!bRequiredPneumaticRegulatorSectionFound)
                        {
                            bRequiredPneumaticRegulatorSectionFound = true;
                        }
                        else
                        {
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument calibration section has duplicate <" + cal_setting_node.Name.ToUpper() + "> entry.");
                        }

                        bool bRequiredRegulatorSetpointOffsetSectionFound = false;

                        foreach (Positional_XmlElement regulator_cal_node in cal_setting_node)
                        {
                            if (regulator_cal_node.NodeType == XmlNodeType.Element)
                            {
                                if ("SETPOINTOFFSETINPSI" == regulator_cal_node.Name.ToUpper())
                                {
                                    if (!bRequiredRegulatorSetpointOffsetSectionFound)
                                    {
                                        bRequiredRegulatorSetpointOffsetSectionFound = true;
                                    }
                                    else
                                    {
                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument calibration section <Pneumatic_Regulator> has duplicate <" + regulator_cal_node.Name.ToUpper() + "> entry.");
                                    }
                                    SystemConfigurationObj.m_Pneumatic_Regulator_Calibration.m_fSetpointOffsetinPSI = Convert.ToSingle(regulator_cal_node.InnerText);
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument calibration section <Pneumatic_Regulator> has unexpected <" + regulator_cal_node.Name.ToUpper() + "> entry.");
                                }
                            }
                        }
                        if (!(bRequiredRegulatorSetpointOffsetSectionFound))
                        {
                            string strMissingElement = (!bRequiredRegulatorSetpointOffsetSectionFound) ? "<SetpointOffsetinPSI>" : "<???>";
                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  " + strMissingElement + " section expected following  <CalibrationSettings><Pneumatic_Regulator> " + strMissingElement + " tag.");
                        }
                    }
                    else
                    {
                        // Parse error
                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument settings section contains unexpected <" + cal_setting_node.Name.ToUpper() + "> tag.");
                    }
                }
            }
            // Verify required fields present
            if (!(bRequiredOpticalFilterMotorSectionFound && bRequiredCameraMotorSectionFound && bRequiredPneumaticRegulatorSectionFound))
            {
                string strMissingElement = (!bRequiredOpticalFilterMotorSectionFound) ? "<OpticalFilterMotor>" : 
                                           (!bRequiredCameraMotorSectionFound) ? "<CameraMotor>" : "<Pneumatic_Regulator>";
                throw new CPCRInstrumentSystemException("Instrument configuration file calibration section parse error.  Missing " + strMissingElement + " required following <CalibrationSettings> tag.");
            }
        }

        private void InstrumentSettings_ConfigurationSemanticValidation(CSystem_Configuration_Obj PCRConfigurationObj)
        {

        }

        private void MiscSettings_ConfigurationSemanticValidation(CSystem_Configuration_Obj PCRConfigurationObj)
        {

        }

        private void HWSettings_ConfigurationSemanticValidation(CSystem_Configuration_Obj PCRConfigurationObj)
        {

        }

        private void CalibrationSettings_ConfigurationSemanticValidation(CSystem_Configuration_Obj PCRConfigurationObj)
        {

        }

        public void ConfigurationSemanticValidation(CSystem_Configuration_Obj PCRConfigurationObj)
        {
            InstrumentSettings_ConfigurationSemanticValidation(PCRConfigurationObj);
            MiscSettings_ConfigurationSemanticValidation(PCRConfigurationObj);
            HWSettings_ConfigurationSemanticValidation(PCRConfigurationObj);
            CalibrationSettings_ConfigurationSemanticValidation(PCRConfigurationObj);
        }

        public void Read_and_Parse_PCRConfigFile(string strFileNamePath, out CSystem_Configuration_Obj PCRConfigObj)
        {
            CSystem_Configuration_Obj PCRConfigurationObj = null;

            // Strip all comments prior to parsing, as derived Positional_XmlElement class with line numbers can't handle XmlComment nodes
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(strFileNamePath, settings);

            try
            {
                Positional_XMLDocument doc = new Positional_XMLDocument();

                try
                {
                    doc.Load(reader);
                }
                catch (Exception Exc)
                {
                    throw new CPCRInstrumentSystemException("Configuration file read error.  Positional_XMLDocument.Load(\"" + strFileNamePath + "\") failed.  Confirm correct XML content by examining it with an XML aware editor such as XML_Notepad.\n", Exc.ToString());
                }

                Positional_XmlElement pcrsettings_node = (Positional_XmlElement) doc.DocumentElement.SelectSingleNode("/SystemSettings"); // Parse System configuration document

                PCRConfigurationObj = new CSystem_Configuration_Obj();

                // Parse Instrument settings node
                Positional_XmlElement instrumentsettings_node = (Positional_XmlElement) pcrsettings_node.SelectSingleNode("InstrumentSettings");
                ParseInstrumentSettings(instrumentsettings_node, PCRConfigurationObj);

                // Parse Miscellaneous settings node
                Positional_XmlElement miscsettings_node = (Positional_XmlElement) pcrsettings_node.SelectSingleNode("MiscSettings");
                ParseMiscSettings(miscsettings_node, PCRConfigurationObj);

                // Parse Hardware settings node entries
                Positional_XmlElement HWSettings_node = (Positional_XmlElement) pcrsettings_node.SelectSingleNode("HWSettings");
                ParseHWSettings(HWSettings_node, PCRConfigurationObj);

                // Parse Calibration settings node entries
                Positional_XmlElement Calibrations_node = (Positional_XmlElement) pcrsettings_node.SelectSingleNode("CalibrationSettings");
                ParseCalibrationSettings(Calibrations_node, PCRConfigurationObj);

                // Possible other settings here
            }
            catch (CPCRInstrumentSystemException /*Exc*/)
            {
                throw;
            }
            catch (Exception Exc)
            {
                PCRConfigurationObj = null;
                throw new Exception("Malformed PCR system configuration file contents. =>" + Exc.ToString());
            }

            PCRConfigObj = PCRConfigurationObj;
        }
    }
}
