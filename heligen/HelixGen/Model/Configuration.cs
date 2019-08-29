// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using XmlDocument_Support_Utilities;

namespace HelixGen.Model
{
    public class Configuration
    {
        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public class CSystem_Instrument_Configuration
        {
            public string m_strSerialNumber { get; set; }
            public string m_strProductNumber { get; set; }
            public string m_strProductName { get; set; }
        }

        public class CSystem_Misc_Configuration
        {
            public string m_strSystemLogPath { get; set; }
            public string m_strSystemProtocolPath { get; set; }
            public string m_strMeasurementLogPath { get; set; }
            public float m_fProtocolCycleStep_MinTemperature { get; set; }
            public float m_fProtocolCycleStep_MaxTemperature { get; set; }
            public float m_fProtocolNonCycleStep_MinTemperature { get; set; }
            public uint m_uiProtocolNonCycleStep_MinTemp_MaxHoldTimeinSeconds { get; set; }
            public float m_fProtocolNonCycleStep_MaxTemperature { get; set; }
            public float m_fThermalRampSamplePeriodInSeconds { get; set; }
            public float m_fThermalRampTimeoutInSeconds { get; set; }

            public float m_fThermalRampPCRStep { get; set;  }

            public float m_fThermalRampNumSteps { get; set; }

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

        /// <summary>
        /// The hardware configuration for the optics controller.
        /// </summary>
        public class CSystem_Optics_Controller_HW_Configuration
        {
            public string m_strPort { get; set; }
            // TBD
        }

        /// <summary>
        /// This is a super class that captures common configuration
        /// fields for items associated with a motor board.
        /// </summary>
        public class CSystem_MotorBoardConfigurationItem
        {
            public int[] positions;

            public string m_strControllerName { get; set; }
            public int m_nMotorChannel { get; set; }
            public uint m_uiMotorHomeSpeed { get; set; }
            public uint m_uiMotorStartSpeed { get; set; }
            public uint m_uiMotorMaxSpeed { get; set; }
            public uint m_uiMotorAccel { get; set; }
            public uint m_uiMotorDecel { get; set; }
            public uint m_uiMotorMoveCurrent { get; set; }
            public uint m_uiMotorHoldCurrent { get; set; }
            public uint m_uiMotorJerk { get; set; }
            public uint m_uiMotorResolution { get; set; }
            public uint m_uiMotorProfileMode { get; set; }
            public uint m_uiMotorHomeTimeout { get; set; }
            public uint m_uiPosition_Move_Timeout { get; set; }
            public uint m_uiMotorDirection { get; set; }
            public uint m_uiMotorMaxNumLostSteps { get; set; }
            public bool m_bEncoderEnabled { get; set; }
            public uint m_uiEncoderMonitorTimer_ms { get; set; }
            public uint m_uiEncoderMonitorPulseChangeThreshold { get; set; }
            public uint m_uiEncoderMonitorErrorCountThreshold { get; set; }
            public uint m_uiEncoderDirectionPolarity { get; set; }
            public int m_iEncoderStartOffset { get; set; }
            public float m_fEncoderScalingFactor { get; set; }


        }

        public class CSystem_HeaterConfiguration
        {
            /// <summary>
            /// The controller channel associated with this device.
            /// </summary>
            public int channel { get; set; }

            /// <summary>
            /// The id of the associated controller.
            /// </summary>
            public string controllerId { get; set;  }

        }

        public class CSystem_OpticsMotorConfiguration : CSystem_MotorBoardConfigurationItem
        {
        }

        public class CSystem_PumpConfiguration
        {
            public string m_strControllerName { get; set; }

            /// <summary>
            /// The controller channel associated with this device.
            /// </summary>
            public int channel { get; set; }
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
            public CTemperatureRange m_TemperatureRange;
            public List<CTEC_HW_Ramping_Configuration> m_ToTemperatures;
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

            public CTemperatureRange m_TemperatureRange;
            public CTEC_HW_PID_Configuration[] m_PID_Settings;
            public float m_fDeadBand;
            public float m_fOvershootOffset;
            public uint m_uiOvershootDuration;
            public float m_fSetpointOffset;
            public float m_fPBand;
            public float m_fLowClamp;
            public float m_fHighClamp;
        }

        public class CTemperatureRange
        {
            public CTemperatureRange() { }
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
                //m_Step_PID_RampDown_Range_List = new List<CTEC_From_Temperature_PID_Element>();
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
                //m_Step_PID_RampDown_Range_List = new List<CTEC_From_Temperature_PID_Element>();
                //for (uint ui = 0; ui < m_Step_PID_RampDown_Range_List.Count; ui++)
                //{
                //    m_Step_PID_RampDown_Range_List.Add(new CTEC_From_Temperature_PID_Element(m_Step_PID_RampDown_Range_List.ToArray()[ui]));
                //}
                m_Step_PID_RampUp_Default = new CTEC_HW_Ramping_Configuration(obj.m_Step_PID_RampUp_Default);
                //m_Step_PID_RampDown_Default = new CTEC_HW_Ramping_Configuration(obj.m_Step_PID_RampDown_Default);
                m_fErrorTermBand = obj.m_fErrorTermBand;
                m_fErrorTermCount = obj.m_fErrorTermCount;
                m_fSteadyStatePowerLimit = obj.m_fSteadyStatePowerLimit;
                m_fSteadyStatePowerLimitCount = obj.m_fSteadyStatePowerLimitCount;
            }

            public string m_strControllerName { get; set; }
            public uint m_uiTECAddress { get; set; }
            public float m_ControlPIDSampleTimeInSeconds { get; set; }
            public List<CTEC_From_Temperature_PID_Element> m_Step_PID_RampUp_Range_List;
            public CTEC_HW_Ramping_Configuration m_Step_PID_RampUp_Default;
            public List<CTEC_From_Temperature_PID_Element> m_Step_PID_RampDown_Range_List;
            public CTEC_HW_Ramping_Configuration m_Step_PID_RampDown_Default;
            public float m_fErrorTermBand { get; set; }
            public float m_fErrorTermCount { get; set; }
            public float m_fSteadyStatePowerLimit { get; set; }
            public float m_fSteadyStatePowerLimitCount { get; set; }
            public float m_fThermA_Coeff { get; set; }
            public float m_fThermB_Coeff { get; set; }
            public float m_fThermC_Coeff { get; set; }
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
            public uint m_uiTECAddress { get; set; }
            public uint m_uiHeaterChannel { get; set; }
            public float m_fThermA_Coeff { get; set; }
            public float m_fThermB_Coeff { get; set; }
            public float m_fThermC_Coeff { get; set; }
        }

        public class CPumpIOControls
        {
            public CPumpIOControls() { m_Control = new CPCR_Accel_Motor_Digital_Output(); }
            public CPCR_Accel_Motor_Digital_Output m_Control;
        }

        //public class 
        public class CMotor_Home_Input
        {
            public string m_strControllerName { get; set; }
            public uint m_uiMotorChannel { get; set; }
        }

        public class COptical_Filter_Motor_HW_Channel_Calibration
        {
            public ulong m_ulFilter_1_Position_Y_Offset_from_Home_In_MotorUSteps;
            public ulong m_ulFilter_2_Position_Y_Offset_from_Home_In_MotorUSteps;
            public ulong m_ulFilter_3_Position_Y_Offset_from_Home_In_MotorUSteps;
            public ulong m_ulBlock_Emitter_Position_Y_Offset_from_Home_In_MotorUSteps;
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

        public CSystem_Instrument_Configuration m_InstrumentConfiguration { get; }
        public CSystem_Misc_Configuration m_MiscellaneousConfiguration { get; }
        public Dictionary<string, CSystem_Motor_Controller_HW_Configuration> m_MotorControllerConfigurations { get; }
        public Dictionary<string, CSystem_Thermal_Controller_HW_Configuration> m_ThermalControllerConfigurations { get; }
        public Dictionary<string, CTEC_Channel_Configuration> m_TEC_Channel_Configurations { get; }
        public CPCR_Fan_and_HeatSink_Configuration m_PCR_Fan_and_HeatSink_Configuration { get; }
        public CPCR_Thermal_Block_Fan_Control_Thermistor_Configuration m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration { get; }
        public CSystemExhaustFans m_SystemExhaustFans { get; }
        public CSystem_Optics_Controller_HW_Configuration m_OpticsController_Configuration { get; internal set; }

        public CSystem_MotorBoardConfigurationItem m_Slider_Configuration { get; internal set; }
        public CSystem_MotorBoardConfigurationItem m_R1Piston_Configuration { get; internal set; }
        public CSystem_MotorBoardConfigurationItem m_R2Piston_Configuration { get; internal set; }

        public CSystem_MotorBoardConfigurationItem m_ChassisPiston_Configuration { get; internal set; }

        public CSystem_MotorBoardConfigurationItem m_HeaterPiston_Configuration { get; internal set; }

        public CSystem_HeaterConfiguration m_Heater_Configuration { get; }
        public CSystem_MotorBoardConfigurationItem m_OpticsMotor_Configuration { get; internal set; }

        public CSystem_PumpConfiguration m_Pump_Configuration { get; internal set; }



        private void ParseInstrumentSettings(Positional_XmlElement instrsettings_node)
        {
            bool bRequiredSerialNumberFound = false;
            bool bRequiredProductNumberFound = false;
            bool bRequiredProductNameFound = false;

            foreach (Positional_XmlElement instrument_setting_node in instrsettings_node)
            {
                if (instrument_setting_node.NodeType == XmlNodeType.Element)
                {
                    switch (instrument_setting_node.Name.ToUpper())
                    {
                        case "SERIALNO":
                            {
                                if (!bRequiredSerialNumberFound)
                                {
                                    m_InstrumentConfiguration.m_strSerialNumber = instrument_setting_node.InnerText;
                                    bRequiredSerialNumberFound = true;
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument settings section has duplicate <" + instrument_setting_node.Name.ToUpper() + "> entry.");
                                }
                            }
                            break;
                        case "PRODUCTNUMBER":
                            {
                                if (!bRequiredProductNumberFound)
                                {
                                    m_InstrumentConfiguration.m_strProductNumber = instrument_setting_node.InnerText;
                                    bRequiredProductNumberFound = true;
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument settings section has duplicate <" + instrument_setting_node.Name.ToUpper() + "> entry.");
                                }
                            }
                            break;
                        case "PRODUCTNAME":
                            {
                                if (!bRequiredProductNameFound)
                                {
                                    m_InstrumentConfiguration.m_strProductName = instrument_setting_node.InnerText;
                                    bRequiredProductNameFound = true;
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument settings section has duplicate <" + instrument_setting_node.Name.ToUpper() + "> entry.");
                                }
                            }
                            break;
                        default:
                            {
                                // Parse error, first node after 'Measurements' must be 'Measurement'
                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Instrument settings section contains unexpected <" + instrument_setting_node.Name.ToUpper() + "> tag.");
                            }
#pragma warning disable CS0162 // Unreachable code detected
                            break;
#pragma warning restore CS0162 // Unreachable code detected
                    }
                }
            }
            // Verify required fields present
            if (!(bRequiredSerialNumberFound && bRequiredProductNumberFound && bRequiredProductNameFound))
            {
                throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Missing " + (((!bRequiredSerialNumberFound) ? "<SerialNo>" : (!bRequiredProductNumberFound) ? "<ProductNumber>" : "<ProductName>")) + " required following <InstrumentSettings> tag.");
            }
        }

        private void ParseMiscSettings(Positional_XmlElement miscsettings_node)
        {
            bool bOptionalSystemLogPathFound = false;
            bool bOptionalSystemProtocolPathFound = false;
            bool bOptionalMeasurementLogPathFound = false;
            bool bRequiredProtocolCycleStep_MinTemperatureFound = false;
            bool bRequiredProtocolCycleStep_MaxTemperatureFound = false;
            bool bRequiredProtocolNonCycleStep_MinTemperatureFound = false;
            bool bRequiredProtocolNonCycleStep_MinTemp_MaxHoldTimeinSecondsFound = false;
            bool bRequiredProtocolNonCycleStep_MaxTemperatureFound = false;
            bool bRequiredThermalSamplePeriodFound = false;
            bool bRequiredThermalRampTimeoutFound = false;
            bool bRequiredThermalRampPCRStepFound = false;
            bool bRequiredThermalRampNumStepsFound = false;

            foreach (Positional_XmlElement misc_setting_node in miscsettings_node)
            {
                if (misc_setting_node.NodeType == XmlNodeType.Element)
                {
                    switch (misc_setting_node.Name.ToUpper())
                    {
                        case "SYSTEMLOGPATH":
                            {
                                if (!bOptionalSystemLogPathFound)
                                {
                                    m_MiscellaneousConfiguration.m_strSystemLogPath = Path.GetFullPath(misc_setting_node.InnerText);
                                    bOptionalSystemLogPathFound = true;
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                                }
                            }
                            break;
                        case "SYSTEMPROTOCOLPATH":
                            {
                                if (!bOptionalSystemProtocolPathFound)
                                {
                                    m_MiscellaneousConfiguration.m_strSystemProtocolPath = Path.GetFullPath(misc_setting_node.InnerText);
                                    bOptionalSystemProtocolPathFound = true;
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                                }
                            }
                            break;
                        case "MEASUREMENTLOGPATH":
                            {
                                if (!bOptionalMeasurementLogPathFound)
                                {
                                    m_MiscellaneousConfiguration.m_strMeasurementLogPath = Path.GetFullPath(misc_setting_node.InnerText);
                                    bOptionalMeasurementLogPathFound = true;
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                                }
                            }
                            break;
                        case "PROTOCOLCYCLESTEP_MINTEMPERATURE":
                            {
                                if (!bRequiredProtocolCycleStep_MinTemperatureFound)
                                {
                                    m_MiscellaneousConfiguration.m_fProtocolCycleStep_MinTemperature = Convert.ToSingle(misc_setting_node.InnerText);
                                    bRequiredProtocolCycleStep_MinTemperatureFound = true;
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                                }
                            }
                            break;
                        case "PROTOCOLCYCLESTEP_MAXTEMPERATURE":
                            {
                                if (!bRequiredProtocolCycleStep_MaxTemperatureFound)
                                {
                                    m_MiscellaneousConfiguration.m_fProtocolCycleStep_MaxTemperature = Convert.ToSingle(misc_setting_node.InnerText);
                                    bRequiredProtocolCycleStep_MaxTemperatureFound = true;
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                                }
                            }
                            break;
                        case "PROTOCOLNONCYCLESTEP_MINTEMPERATURE":
                            {
                                if (!bRequiredProtocolNonCycleStep_MinTemperatureFound)
                                {
                                    m_MiscellaneousConfiguration.m_fProtocolNonCycleStep_MinTemperature = Convert.ToSingle(misc_setting_node.InnerText);
                                    bRequiredProtocolNonCycleStep_MinTemperatureFound = true;
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                                }
                            }
                            break;
                        case "PROTOCOLNONCYCLESTEP_MINTEMP_MAXHOLDTIMEINSECONDS":
                            {
                                if (!bRequiredProtocolNonCycleStep_MinTemp_MaxHoldTimeinSecondsFound)
                                {
                                    m_MiscellaneousConfiguration.m_uiProtocolNonCycleStep_MinTemp_MaxHoldTimeinSeconds = Convert.ToUInt32(misc_setting_node.InnerText);
                                    bRequiredProtocolNonCycleStep_MinTemp_MaxHoldTimeinSecondsFound = true;
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                                }
                            }
                            break;
                        case "PROTOCOLNONCYCLESTEP_MAXTEMPERATURE":
                            {
                                if (!bRequiredProtocolNonCycleStep_MaxTemperatureFound)
                                {
                                    m_MiscellaneousConfiguration.m_fProtocolNonCycleStep_MaxTemperature = Convert.ToSingle(misc_setting_node.InnerText);
                                    bRequiredProtocolNonCycleStep_MaxTemperatureFound = true;
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                                }
                            }
                            break;
                        case "THERMALRAMPSAMPLEPERIODINSECONDS":
                            {
                                if (!bRequiredThermalSamplePeriodFound)
                                {
                                    m_MiscellaneousConfiguration.m_fThermalRampSamplePeriodInSeconds = Convert.ToSingle(misc_setting_node.InnerText);
                                    bRequiredThermalSamplePeriodFound = true;
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                                }
                            }
                            break;
                        case "THERMALRAMPTIMEOUTINSECONDS":
                            {
                                if (!bRequiredThermalRampTimeoutFound)
                                {
                                    m_MiscellaneousConfiguration.m_fThermalRampTimeoutInSeconds = Convert.ToSingle(misc_setting_node.InnerText);
                                    bRequiredThermalRampTimeoutFound = true;
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                                }
                            }
                            break;
                        case "THERMALRAMPPCRSTEP":
                            {
                                if (!bRequiredThermalRampPCRStepFound)
                                {
                                    m_MiscellaneousConfiguration.m_fThermalRampPCRStep = Convert.ToSingle(misc_setting_node.InnerText);
                                    bRequiredThermalRampPCRStepFound = true;
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                                }
                            }
                            break;
                        case "THERMALRAMPNUMSTEPS":
                            {
                                if (!bRequiredThermalRampNumStepsFound)
                                {
                                    m_MiscellaneousConfiguration.m_fThermalRampNumSteps = Convert.ToSingle(misc_setting_node.InnerText);
                                    bRequiredThermalRampNumStepsFound = true;
                                }
                                else
                                {
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section has duplicate <" + misc_setting_node.Name.ToUpper() + "> entry.");
                                }
                            }
                            break;
                        default:
                            {
                                // Parse error, first node after 'Measurements' must be 'Measurement'
                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Miscellaneous settings section contains unexpected <" + misc_setting_node.Name.ToUpper() + "> tag.");
                            }
#pragma warning disable CS0162 // Unreachable code detected
                            break;
#pragma warning restore CS0162 // Unreachable code detected
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
                  bRequiredThermalRampTimeoutFound &&
                  bRequiredThermalRampPCRStepFound &&
                  bRequiredThermalRampNumStepsFound
                  ))
            {
                string strMissingElement = (!bRequiredThermalSamplePeriodFound) ? "<ThermalSamplePeriodInSeconds>" :
                                           (!bRequiredProtocolCycleStep_MinTemperatureFound) ? "<ProtocolCycleStep_MinTemperature>" :
                                           (!bRequiredProtocolCycleStep_MaxTemperatureFound) ? "<ProtocolCycleStep_MaxTemperature>" :
                                           (!bRequiredProtocolNonCycleStep_MinTemperatureFound) ? "<ProtocolNonCycleStep_MinTemperature>" :
                                           (!bRequiredProtocolNonCycleStep_MinTemp_MaxHoldTimeinSecondsFound) ? "<ProtocolNonCycleStep_MinTemp_MaxHoldTimeinSeconds>" :
                                           (!bRequiredThermalRampPCRStepFound) ? "<ThermalRampPCRStep>" :
                                           (!bRequiredThermalRampNumStepsFound) ? "<ThermalRampNumSteps>" :
                                           (!bRequiredProtocolNonCycleStep_MaxTemperatureFound) ? "<ProtocolNonCycleStep_MaxTemperature>" : "<ThermalRampTimeoutInSeconds>";
                throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  Missing " + strMissingElement + " required following <MiscSettings> tag.");
            }
            if (!bOptionalSystemLogPathFound)
            {
                m_MiscellaneousConfiguration.m_strSystemLogPath = Path.GetFullPath(CSystem_Defns.strDefaultSystemLoggingPath);
            }
            if (!bOptionalSystemProtocolPathFound)
            {
                m_MiscellaneousConfiguration.m_strSystemProtocolPath = Path.GetFullPath(CSystem_Defns.strDefaultProtocolPath);
            }
            if (!bOptionalMeasurementLogPathFound)
            {
                m_MiscellaneousConfiguration.m_strMeasurementLogPath = Path.GetFullPath(CSystem_Defns.strDefaultMeasurementLogPath);
            }
        }

        private void ParseHWSettings(Positional_XmlElement HWSettings_node)
        {
            bool bRequiredMotorControllersSectionFound = false;
            bool bRequiredThermalControllersSectionFound = false;
            bool bRequiredPCR_Fan_Heatsink_SectionFound = false;
            bool bRequiredPCR_Thermal_Block_Fan_Control_Thermistor_SectionFound = false;
            bool bRequiredTECControllerChannelsSectionFound = false;
            bool bRequiredOpticsControllerSectionFound = false;

            foreach (Positional_XmlElement hw_setting_section_node in HWSettings_node)
            {
                if (hw_setting_section_node.NodeType == XmlNodeType.Element)
                {
                    switch (hw_setting_section_node.Name.ToUpper())
                    {
                        case "MOTORCONTROLLERS":
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
                                            CSystem_Motor_Controller_HW_Configuration MotorControllerHWConfiguration = new CSystem_Motor_Controller_HW_Configuration();
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
                                                        MotorControllerHWConfiguration.m_strPort = motorcontroller_subnode.InnerText;
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
                                                        MotorControllerHWConfiguration.m_strControllerName = motorcontroller_subnode.InnerText;
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
                                            m_MotorControllerConfigurations.Add(MotorControllerHWConfiguration.m_strControllerName, MotorControllerHWConfiguration);
                                        }
                                    }
                                }
                            }
                            break;
                        case "PUMP":
                            m_Pump_Configuration = parse_pump_settings(hw_setting_section_node);
                            break;
                        case "SLIDER_MOTOR":
                            m_Slider_Configuration = parse_motorpump_settings(hw_setting_section_node);
                            break;
                        case "R1PISTON_MOTOR":
                            m_R1Piston_Configuration = parse_motorpump_settings(hw_setting_section_node);
                            break;
                        case "R2PISTON_MOTOR":
                            m_R2Piston_Configuration = parse_motorpump_settings(hw_setting_section_node);
                            break;
                        case "HEATERPISTON_MOTOR":
                            m_HeaterPiston_Configuration = parse_motorpump_settings(hw_setting_section_node);
                            break;
                        case "CHASSISPISTON_MOTOR":
                            m_ChassisPiston_Configuration = parse_motorpump_settings(hw_setting_section_node);
                            break;
                        case "OPTICS_MOTOR":
                            m_OpticsMotor_Configuration = parse_motorpump_settings(hw_setting_section_node);
                            break;
                        case "OPTICS_CONTROLLER":
                            m_OpticsController_Configuration = parse_optics_controller_settings(hw_setting_section_node);
                            break;
                        case "THERMALCONTROLLERS":
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

                                                    switch (tag_name.ToUpper())
                                                    {
                                                        case "PORT":
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
                                                            break;
                                                        case "NAME":
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
                                                            break;
                                                        case "MODEL":
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
                                                            break;
                                                        default:
                                                            {
                                                                // Unexpected tag defined - parse error
                                                                throw new CPCRInstrumentSystemException("Configuration file parse error.  <Port> or <Model> or <Name> definition expected following <ThermalController> tag, instead found <" + tag_name + ">.");
                                                            }
#pragma warning disable CS0162 // Unreachable code detected
                                                            break;
#pragma warning restore CS0162 // Unreachable code detected
                                                    }
                                                }
                                            }
                                            // Verify required fields present
                                            if (!(bRequiredPortFound && bRequiredNameFound && bRequiredModelFound))
                                            {
                                                string strMissingElement = (!bRequiredPortFound) ? "<Port>" :
                                                                             (!bRequiredNameFound) ? "<Name>" : "<Model>";
                                                throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <ThermalController> tag.");
                                            }
                                            bRequiredThermalControllersSectionFound = true;
                                            m_ThermalControllerConfigurations.Add(ThermalControllerHWConfigurationObj.m_strControllerName, ThermalControllerHWConfigurationObj);
                                        }
                                    }
                                }
                                if (!bRequiredThermalControllerSectionFound)
                                {
                                    string strMissingElement = "<ThermalController>";
                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <ThermalControllers> tag.");
                                }
                            }
                            break;
                        case "PCR_HEATSINK_FAN":
                            {
                                bool bRequiredTECControllerNameFound = false;
                                bool bRequiredAddressFound = false;
                                bool bRequiredSampletimeFound = false;
                                bool bRequiredBandsFound = false;

                                //CPCR_Fan_and_HeatSink_Configuration PCR_TEC_Fan_Heatsink_ConfigurationObj = new CPCR_Fan_and_HeatSink_Configuration();

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
                                        switch (tag_name)
                                        {
                                            case "TECCONTROLLER":
                                                {
                                                    if (bRequiredTECControllerNameFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_HeatSink_Fan> section has duplicate <TECController> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredTECControllerNameFound = true; // Required definition
                                                    }
                                                    m_PCR_Fan_and_HeatSink_Configuration.m_strControllerName = Fan_Heatsink_subnode.InnerText;
                                                }
                                                break;
                                            case "ADDRESS":
                                                {
                                                    if (bRequiredAddressFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_HeatSink_Fan> section has duplicate <Address> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredAddressFound = true; // Required definition
                                                    }
                                                    m_PCR_Fan_and_HeatSink_Configuration.m_uiTECAddress = Convert.ToUInt32(Fan_Heatsink_subnode.InnerText);
                                                }
                                                break;
                                            case "BANDS":
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
                                                                        switch (innertag_name)
                                                                        {
                                                                            case "UPPER":
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
                                                                                break;

                                                                            case "LOWER":
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
                                                                                break;
                                                                            case "DUTYCYCLE":
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
                                                                                break;
                                                                            case "DEADBAND":
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
                                                                                break;
                                                                            default:

                                                                                {
                                                                                    // Unexpected tag defined - parse error
                                                                                    throw new CPCRInstrumentSystemException("Configuration file parse error.  Unexpected tag <" + tag_name + "> following <Band> tag.");
                                                                                }
#pragma warning disable CS0162 // Unreachable code detected
                                                                                break;
#pragma warning restore CS0162 // Unreachable code detected
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
                                                    m_PCR_Fan_and_HeatSink_Configuration.m_Bands = bandsList.ToArray();
                                                    Array.Sort(m_PCR_Fan_and_HeatSink_Configuration.m_Bands, (x, y) => y.m_fLowerTemperature.CompareTo(x.m_fLowerTemperature));
                                                }
                                                break;
                                            case "SAMPLETIMEINSECONDS":
                                                {
                                                    if (bRequiredSampletimeFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_HeatSink_Fan> section has duplicate <SampleTimeInSeconds> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredSampletimeFound = true; // Required definition
                                                    }
                                                    m_PCR_Fan_and_HeatSink_Configuration.m_fSampleTimeInSeconds = Convert.ToSingle(Fan_Heatsink_subnode.InnerText);
                                                    // Clamp between min and max as per system CPCR_Defns
                                                    if (m_PCR_Fan_and_HeatSink_Configuration.m_fSampleTimeInSeconds > CSystem_Defns.cfPCR_Safe_Heatsink_Temperature_Max_Sampling_Time_in_Seconds)
                                                    {
                                                        // Log warning - TBD

                                                        m_PCR_Fan_and_HeatSink_Configuration.m_fSampleTimeInSeconds = CSystem_Defns.cfPCR_Safe_Heatsink_Temperature_Max_Sampling_Time_in_Seconds;
                                                    }
                                                    if (m_PCR_Fan_and_HeatSink_Configuration.m_fSampleTimeInSeconds < CSystem_Defns.cfPCR_Safe_Heatsink_Temperature_Min_Sampling_Time_in_Seconds)
                                                    {
                                                        // Log warning - TBD

                                                        m_PCR_Fan_and_HeatSink_Configuration.m_fSampleTimeInSeconds = CSystem_Defns.cfPCR_Safe_Heatsink_Temperature_Min_Sampling_Time_in_Seconds;
                                                    }
                                                }
                                                break;
                                            default:
                                                {
                                                    // Unexpected tag defined - parse error
                                                    throw new CPCRInstrumentSystemException("Configuration file parse error.  Unexpected tag <" + tag_name + "> following <PCR_HeatSink_Fan> tag.");
                                                }
#pragma warning disable CS0162 // Unreachable code detected
                                                break;
#pragma warning restore CS0162 // Unreachable code detected
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
                            }
                            break;
                        case "PCR_THERMAL_BLOCK_FAN_CONTROL_THERMISTOR":
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
                                        switch (tag_name)
                                        {
                                            case "TECCONTROLLER":
                                                {
                                                    if (bRequiredTECControllerNameFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_Thermal_Block_Fan_Control_Thermistor> section has duplicate <TECController> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredTECControllerNameFound = true; // Required definition
                                                    }
                                                    m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration.m_strControllerName = Thermal_Block_Fan_Control_Thermistor_subnode.InnerText;
                                                }
                                                break;
                                            case "ADDRESS":
                                                {
                                                    if (bRequiredAddressFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_Thermal_Block_Fan_Control_Thermistor> section has duplicate <Address> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredAddressFound = true; // Required definition
                                                    }
                                                    m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration.m_uiTECAddress = Convert.ToUInt32(Thermal_Block_Fan_Control_Thermistor_subnode.InnerText);
                                                }
                                                break;
                                            case "CHANNEL":
                                                {
                                                    if (bRequiredChannelFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_Thermal_Block_Fan_Control_Thermistor> section has duplicate <channel> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredChannelFound = true; // Required definition
                                                    }
                                                    m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration.m_uiHeaterChannel = Convert.ToUInt32(Thermal_Block_Fan_Control_Thermistor_subnode.InnerText);
                                                }
                                                break;
                                            case "THERMA_COEFF":
                                                {
                                                    if (bRequiredThermACoefFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_Thermal_Block_Fan_Control_Thermistor> section has duplicate <ThermA_Coeff> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredThermACoefFound = true; // Required definition
                                                    }
                                                    m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration.m_fThermA_Coeff = Convert.ToSingle(Thermal_Block_Fan_Control_Thermistor_subnode.InnerText);
                                                }
                                                break;
                                            case "THERMB_COEFF":
                                                {
                                                    if (bRequiredThermBCoefFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_Thermal_Block_Fan_Control_Thermistor> section has duplicate <ThermB_Coeff> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredThermBCoefFound = true; // Required definition
                                                    }
                                                    m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration.m_fThermB_Coeff = Convert.ToSingle(Thermal_Block_Fan_Control_Thermistor_subnode.InnerText);
                                                }
                                                break;
                                            case "THERMC_COEFF":
                                                {
                                                    if (bRequiredThermCCoefFound)
                                                    {
                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <PCR_Thermal_Block_Fan_Control_Thermistor> section has duplicate <ThermC_Coeff> entry.");
                                                    }
                                                    else
                                                    {
                                                        bRequiredThermCCoefFound = true; // Required definition
                                                    }
                                                    m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration.m_fThermC_Coeff = Convert.ToSingle(Thermal_Block_Fan_Control_Thermistor_subnode.InnerText);
                                                }
                                                break;
                                            default:
                                                {
                                                    // Unexpected tag defined - parse error
                                                    throw new CPCRInstrumentSystemException("Configuration file parse error.  Unexpected tag <" + tag_name + "> following <PCR_Thermal_Block_Fan_Control_Thermistor> tag.");
                                                }
#pragma warning disable CS0162 // Unreachable code detected
                                                break;
#pragma warning restore CS0162 // Unreachable code detected
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
                            break;
                        case "TECCONTROLLERCHANNELS":
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
                                                        switch (tag_name)
                                                        {
                                                            case "TECCONTROLLER":
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
                                                                break;
                                                            case "ADDRESS":
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
                                                                break;
                                                            case "THERMA_COEFF":
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
                                                                break;
                                                            case "THERMB_COEFF":
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
                                                                break;
                                                            case "THERMC_COEFF":
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
                                                                break;
                                                            case "CONTROLPIDSAMPLETIMEINSECONDS":
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
                                                                break;
                                                            case "PIDS":
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

                                                                                foreach (Positional_XmlElement Step_subnode in Step_node)
                                                                                {
                                                                                    if (Step_subnode.NodeType == XmlNodeType.Element)
                                                                                    {
                                                                                        string Step_subnode_tag_name = Step_subnode.Name.ToUpper();

                                                                                        if ("RAMPUP" == Step_subnode_tag_name)
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
                                                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + "rampup section has duplicate <" + FromTemps_node_tag_name + "> entry.");
                                                                                                        }
                                                                                                        else
                                                                                                        {
                                                                                                            bRequiredFromTempsFound = true;
                                                                                                        }

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


                                                                                                                    foreach (Positional_XmlElement From_subnode in FromTemps_subnode)
                                                                                                                    {
                                                                                                                        if (From_subnode.NodeType == XmlNodeType.Element)
                                                                                                                        {
                                                                                                                            string From_subnode_tag_name = From_subnode.Name.ToUpper();

                                                                                                                            switch (From_subnode_tag_name)
                                                                                                                            {
                                                                                                                                case "LOWTEMP":
                                                                                                                                    {
                                                                                                                                        if (bRequiredLowTempFound)
                                                                                                                                        {
                                                                                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + "ramp up <FromTemps> <From> section has duplicate <" + From_subnode_tag_name + "> entry.");
                                                                                                                                        }
                                                                                                                                        else
                                                                                                                                        {
                                                                                                                                            bRequiredLowTempFound = true;
                                                                                                                                        }
                                                                                                                                        from_temp_obj.m_TemperatureRange.m_fLowTemperature = Convert.ToSingle(From_subnode.InnerText);
                                                                                                                                    }
                                                                                                                                    break;
                                                                                                                                case "HIGHTEMP":
                                                                                                                                    {
                                                                                                                                        if (bRequiredHighTempFound)
                                                                                                                                        {
                                                                                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + "rampup <FromTemps> <From> section has duplicate <" + From_subnode_tag_name + "> entry.");
                                                                                                                                        }
                                                                                                                                        else
                                                                                                                                        {
                                                                                                                                            bRequiredHighTempFound = true;
                                                                                                                                        }
                                                                                                                                        from_temp_obj.m_TemperatureRange.m_fHighTemperature = Convert.ToSingle(From_subnode.InnerText);
                                                                                                                                    }
                                                                                                                                    break;
                                                                                                                                case "TOTEMPS":
                                                                                                                                    {
                                                                                                                                        if (bRequiredToTempsFound)
                                                                                                                                        {
                                                                                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> section has duplicate <" + From_subnode_tag_name + "> entry.");
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

                                                                                                                                                            switch (To_subnode_tag_name)
                                                                                                                                                            {

                                                                                                                                                                case "LOWTEMP":
                                                                                                                                                                    {
                                                                                                                                                                        if (bRequiredToLowTempFound)
                                                                                                                                                                        {
                                                                                                                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> section has duplicate <" + To_subnode_tag_name + "> entry.");
                                                                                                                                                                        }
                                                                                                                                                                        else
                                                                                                                                                                        {
                                                                                                                                                                            bRequiredToLowTempFound = true;
                                                                                                                                                                        }
                                                                                                                                                                        to_temp_obj.m_TemperatureRange.m_fLowTemperature = Convert.ToSingle(To_subnode.InnerText);
                                                                                                                                                                    }
                                                                                                                                                                    break;
                                                                                                                                                                case "HIGHTEMP":
                                                                                                                                                                    {
                                                                                                                                                                        if (bRequiredToHighTempFound)
                                                                                                                                                                        {
                                                                                                                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> section has duplicate <" + To_subnode_tag_name + "> entry.");
                                                                                                                                                                        }
                                                                                                                                                                        else
                                                                                                                                                                        {
                                                                                                                                                                            bRequiredToHighTempFound = true;
                                                                                                                                                                        }
                                                                                                                                                                        to_temp_obj.m_TemperatureRange.m_fHighTemperature = Convert.ToSingle(To_subnode.InnerText);
                                                                                                                                                                    }
                                                                                                                                                                    break;

                                                                                                                                                                case "PARAMS":
                                                                                                                                                                    {
                                                                                                                                                                        if (bRequiredParamsFound)
                                                                                                                                                                        {
                                                                                                                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> section has duplicate <" + To_subnode_tag_name + "> entry.");
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

                                                                                                                                                                                switch (ToPIDsubtag_name)
                                                                                                                                                                                {

                                                                                                                                                                                    case "RABBITGAIN":
                                                                                                                                                                                        {
                                                                                                                                                                                            if (bRequiredRabbitGainFound)
                                                                                                                                                                                            {
                                                                                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
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
                                                                                                                                                                                        break;
                                                                                                                                                                                    case "RABBITGAIN2":
                                                                                                                                                                                        {
                                                                                                                                                                                            if (bRequiredRabbitGain2Found)
                                                                                                                                                                                            {
                                                                                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
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
                                                                                                                                                                                        break;
                                                                                                                                                                                    case "RABBITGAINOFFSET":
                                                                                                                                                                                        {
                                                                                                                                                                                            if (bRequiredRabbitGainOffsetFound)
                                                                                                                                                                                            {
                                                                                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
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
                                                                                                                                                                                        break;
                                                                                                                                                                                    case "RABBITDERIVGAIN":
                                                                                                                                                                                        {
                                                                                                                                                                                            if (bRequiredRabbitDerivGainFound)
                                                                                                                                                                                            {
                                                                                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
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
                                                                                                                                                                                        break;

                                                                                                                                                                                    case "PGAIN":
                                                                                                                                                                                        {
                                                                                                                                                                                            if (bRequiredPGainFound)
                                                                                                                                                                                            {
                                                                                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
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
                                                                                                                                                                                        break;
                                                                                                                                                                                    case "DGAIN":
                                                                                                                                                                                        {
                                                                                                                                                                                            if (bRequiredDGainFound)
                                                                                                                                                                                            {
                                                                                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
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
                                                                                                                                                                                        break;
                                                                                                                                                                                    case "IGAIN":
                                                                                                                                                                                        {
                                                                                                                                                                                            if (bRequiredIGainFound)
                                                                                                                                                                                            {
                                                                                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
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
                                                                                                                                                                                        break;
                                                                                                                                                                                    case "DEADBAND":
                                                                                                                                                                                        {
                                                                                                                                                                                            if (bRequiredDeadBandFound)
                                                                                                                                                                                            {
                                                                                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                                                            }
                                                                                                                                                                                            else
                                                                                                                                                                                            {
                                                                                                                                                                                                bRequiredDeadBandFound = true; // Required definition
                                                                                                                                                                                            }
                                                                                                                                                                                            to_temp_obj.m_fDeadBand = Convert.ToSingle(ToPIDParameter_subnode.InnerText);
                                                                                                                                                                                        }
                                                                                                                                                                                        break;
                                                                                                                                                                                    case "HIGHCLAMP":
                                                                                                                                                                                        {
                                                                                                                                                                                            if (bRequiredHighClampFound)
                                                                                                                                                                                            {
                                                                                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                                                            }
                                                                                                                                                                                            else
                                                                                                                                                                                            {
                                                                                                                                                                                                bRequiredHighClampFound = true; // Required definition
                                                                                                                                                                                            }
                                                                                                                                                                                            to_temp_obj.m_fHighClamp = Convert.ToSingle(ToPIDParameter_subnode.InnerText);
                                                                                                                                                                                        }
                                                                                                                                                                                        break;
                                                                                                                                                                                    case "LOWCLAMP":
                                                                                                                                                                                        {
                                                                                                                                                                                            if (bRequiredLowClampFound)
                                                                                                                                                                                            {
                                                                                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                                                            }
                                                                                                                                                                                            else
                                                                                                                                                                                            {
                                                                                                                                                                                                bRequiredLowClampFound = true; // Required definition
                                                                                                                                                                                            }
                                                                                                                                                                                            to_temp_obj.m_fLowClamp = Convert.ToSingle(ToPIDParameter_subnode.InnerText);
                                                                                                                                                                                        }
                                                                                                                                                                                        break;
                                                                                                                                                                                    case "OVERSHOOTOFFSET":
                                                                                                                                                                                        {
                                                                                                                                                                                            if (bRequiredOvershootOffsetFound)
                                                                                                                                                                                            {
                                                                                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                                                            }
                                                                                                                                                                                            else
                                                                                                                                                                                            {
                                                                                                                                                                                                bRequiredOvershootOffsetFound = true; // Required definition
                                                                                                                                                                                            }
                                                                                                                                                                                            to_temp_obj.m_fOvershootOffset = Convert.ToSingle(ToPIDParameter_subnode.InnerText);
                                                                                                                                                                                        }
                                                                                                                                                                                        break;
                                                                                                                                                                                    case "OVERSHOOTDURATION":
                                                                                                                                                                                        {
                                                                                                                                                                                            if (bRequiredOvershootDurationFound)
                                                                                                                                                                                            {
                                                                                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                                                            }
                                                                                                                                                                                            else
                                                                                                                                                                                            {
                                                                                                                                                                                                bRequiredOvershootDurationFound = true; // Required definition
                                                                                                                                                                                            }
                                                                                                                                                                                            to_temp_obj.m_uiOvershootDuration = Convert.ToUInt32(ToPIDParameter_subnode.InnerText);
                                                                                                                                                                                        }
                                                                                                                                                                                        break;
                                                                                                                                                                                    case "SETPOINTOFFSET":
                                                                                                                                                                                        {
                                                                                                                                                                                            if (bRequiredSetpointOffsetFound)
                                                                                                                                                                                            {
                                                                                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                                                            }
                                                                                                                                                                                            else
                                                                                                                                                                                            {
                                                                                                                                                                                                bRequiredSetpointOffsetFound = true; // Required definition
                                                                                                                                                                                            }
                                                                                                                                                                                            to_temp_obj.m_fSetpointOffset = Convert.ToSingle(ToPIDParameter_subnode.InnerText);
                                                                                                                                                                                        }
                                                                                                                                                                                        break;
                                                                                                                                                                                    case "PBAND":
                                                                                                                                                                                        {
                                                                                                                                                                                            if (bRequiredPBandFound)
                                                                                                                                                                                            {
                                                                                                                                                                                                throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> <" + ToPIDsubtag_name + "> section has duplicate <" + ToPIDsubtag_name + "> entry.");
                                                                                                                                                                                            }
                                                                                                                                                                                            else
                                                                                                                                                                                            {
                                                                                                                                                                                                bRequiredPBandFound = true; // Required definition
                                                                                                                                                                                            }
                                                                                                                                                                                            to_temp_obj.m_fPBand = Convert.ToSingle(ToPIDParameter_subnode.InnerText);
                                                                                                                                                                                        }
                                                                                                                                                                                        break;
                                                                                                                                                                                    default:
                                                                                                                                                                                        {
                                                                                                                                                                                            // Unexpected tag defined - parse error
                                                                                                                                                                                            throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> section has unexpected node <" + ToPIDsubtag_name + ">.");
                                                                                                                                                                                        }
#pragma warning disable CS0162 // Unreachable code detected
                                                                                                                                                                                        break;
#pragma warning restore CS0162 // Unreachable code detected
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
                                                                                                                                                                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> <Params> section.\n");
                                                                                                                                                                        }
                                                                                                                                                                    }
                                                                                                                                                                    break;
                                                                                                                                                            }
                                                                                                                                                        }
                                                                                                                                                    }
                                                                                                                                                    if (!(bRequiredToLowTempFound && bRequiredToHighTempFound && bRequiredParamsFound))
                                                                                                                                                    {
                                                                                                                                                        string strMissingElement = (!bRequiredToLowTempFound) ? "<LowTemperature>" :
                                                                                                                                                                                   (!bRequiredToHighTempFound) ? "<HighTemperature>" : "<Params>";
                                                                                                                                                        throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> <To> section.\n");
                                                                                                                                                    }
                                                                                                                                                    // For QuanDx phase 2 from and to entries are assumed (must be) complete and non-conflicting
                                                                                                                                                    from_temp_obj.m_ToTemperatures.Add(to_temp_obj);
                                                                                                                                                }
                                                                                                                                                else
                                                                                                                                                {
                                                                                                                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> section has unexpected node <" + to_node_tag_name + ">.");
                                                                                                                                                }
                                                                                                                                            }
                                                                                                                                        }
                                                                                                                                        if (!bRequiredToTempFound)
                                                                                                                                        {
                                                                                                                                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. <To> required following <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> section.\n");
                                                                                                                                        }
                                                                                                                                    }
                                                                                                                                    break;
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                    if (!(bRequiredLowTempFound && bRequiredHighTempFound && bRequiredToTempsFound))
                                                                                                                    {
                                                                                                                        string strMissingElement = (!bRequiredLowTempFound) ? "<LowTemperature>" :
                                                                                                                                                   (!bRequiredHighTempFound) ? "<HighTemperature>" : "<ToTemps>";
                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <From> <ToTemps> section missing " + strMissingElement + " node.");
                                                                                                                    }

                                                                                                                    // Save 'From' PIDs
                                                                                                                    if (bIsRampUpSection)
                                                                                                                    {
                                                                                                                        PCR_Thermal_TEC_ConfigurationObj.m_Step_PID_RampUp_Range_List.Add(from_temp_obj);
                                                                                                                    }
                                                                                                                    else
                                                                                                                    {   // RampDown section
                                                                                                                        //PCR_Thermal_TEC_ConfigurationObj.m_Step_PID_RampDown_Range_List.Add(from_temp_obj);
                                                                                                                    }
                                                                                                                }
                                                                                                                else if ("DEFAULT" == From_node_tag_name)
                                                                                                                {
                                                                                                                    if (bRequiredDefaultFound)
                                                                                                                    {
                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> section has duplicate <" + From_node_tag_name + "> entry.");
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

                                                                                                                            if ("PARAMS" == Default_subnode_tag_name)
                                                                                                                            {
                                                                                                                                if (bRequiredParamsFound)
                                                                                                                                {
                                                                                                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section has duplicate <" + Default_subnode_tag_name + "> entry.");
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


                                                                                                                                        switch (Default_PID_Params_subtag_name)
                                                                                                                                        {
                                                                                                                                            case "RABBITGAIN":
                                                                                                                                                {
                                                                                                                                                    if (bRequiredRabbitGainFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
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
                                                                                                                                                break;
                                                                                                                                            case "RABBITGAIN2":
                                                                                                                                                {
                                                                                                                                                    if (bRequiredRabbitGain2Found)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
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
                                                                                                                                                break;
                                                                                                                                            case "RABBITGAINOFFSET":
                                                                                                                                                {
                                                                                                                                                    if (bRequiredRabbitGainOffsetFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
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
                                                                                                                                                break;
                                                                                                                                            case "RABBITDERIVGAIN":
                                                                                                                                                {
                                                                                                                                                    if (bRequiredRabbitDerivGainFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
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
                                                                                                                                                break;

                                                                                                                                            case "PGAIN":
                                                                                                                                                {
                                                                                                                                                    if (bRequiredPGainFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
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
                                                                                                                                                break;
                                                                                                                                            case "DGAIN":
                                                                                                                                                {
                                                                                                                                                    if (bRequiredDGainFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
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
                                                                                                                                                break;
                                                                                                                                            case "IGAIN":
                                                                                                                                                {
                                                                                                                                                    if (bRequiredIGainFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
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
                                                                                                                                                break;
                                                                                                                                            case "DEADBAND":
                                                                                                                                                {
                                                                                                                                                    if (bRequiredDeadBandFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredDeadBandFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    default_temp_obj.m_fDeadBand = Convert.ToSingle(Default_PID_Params_subnode.InnerText);
                                                                                                                                                }
                                                                                                                                                break;
                                                                                                                                            case "HIGHCLAMP":
                                                                                                                                                {
                                                                                                                                                    if (bRequiredHighClampFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredHighClampFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    default_temp_obj.m_fHighClamp = Convert.ToSingle(Default_PID_Params_subnode.InnerText);
                                                                                                                                                }
                                                                                                                                                break;
                                                                                                                                            case "LOWCLAMP":
                                                                                                                                                {
                                                                                                                                                    if (bRequiredLowClampFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredLowClampFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    default_temp_obj.m_fLowClamp = Convert.ToSingle(Default_PID_Params_subnode.InnerText);
                                                                                                                                                }
                                                                                                                                                break;
                                                                                                                                            case "OVERSHOOTOFFSET":
                                                                                                                                                {
                                                                                                                                                    if (bRequiredOvershootOffsetFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredOvershootOffsetFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    default_temp_obj.m_fOvershootOffset = Convert.ToSingle(Default_PID_Params_subnode.InnerText);
                                                                                                                                                }
                                                                                                                                                break;
                                                                                                                                            case "OVERSHOOTDURATION":
                                                                                                                                                {
                                                                                                                                                    if (bRequiredOvershootDurationFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredOvershootDurationFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    default_temp_obj.m_uiOvershootDuration = Convert.ToUInt32(Default_PID_Params_subnode.InnerText);
                                                                                                                                                }
                                                                                                                                                break;
                                                                                                                                            case "SETPOINTOFFSET":
                                                                                                                                                {
                                                                                                                                                    if (bRequiredSetpointOffsetFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredSetpointOffsetFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    default_temp_obj.m_fSetpointOffset = Convert.ToSingle(Default_PID_Params_subnode.InnerText);
                                                                                                                                                }
                                                                                                                                                break;

                                                                                                                                            case "PBAND":
                                                                                                                                                {
                                                                                                                                                    if (bRequiredPBandFound)
                                                                                                                                                    {
                                                                                                                                                        throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section has duplicate <" + Default_PID_Params_subtag_name + "> entry.");
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        bRequiredPBandFound = true; // Required definition
                                                                                                                                                    }
                                                                                                                                                    default_temp_obj.m_fPBand = Convert.ToSingle(Default_PID_Params_subnode.InnerText);
                                                                                                                                                }
                                                                                                                                                break;
                                                                                                                                            default:

                                                                                                                                                {
                                                                                                                                                    // Unexpected tag defined - parse error
                                                                                                                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section has unexpected node <" + Default_PID_Params_subtag_name + ">.");
                                                                                                                                                }
#pragma warning disable CS0162 // Unreachable code detected
                                                                                                                                                break;
#pragma warning restore CS0162 // Unreachable code detected
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
                                                                                                                                    throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> <Default> <Params> section.\n");
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
                                                                                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> section has unexpected node <" + From_node_tag_name + ">.");
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                        if (!bRequiredDefaultFound)
                                                                                                        {
                                                                                                            string strMissingElement = "Default";
                                                                                                            throw new CPCRInstrumentSystemException("Instrument configuration file parse error. " + strMissingElement + " required following <" + strTEC_Entry + "> <PIDs> " + strStepSectionName + " rampup <FromTemps> section.\n");
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
                                                                                if (!bRequiredRampUpFound)
                                                                                {
                                                                                    string strMissingElement = "<RampUp>";
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
                                                                break;
                                                            case "ERRORTERMBAND":
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
                                                                break;
                                                            case "ERRORTERMCOUNT":
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
                                                                break;
                                                            case "STEADYSTATEPOWERLIMIT":
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
                                                                break;
                                                            case "STEADYSTATEPOWERLIMITCOUNT":
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
                                                                break;
                                                            default:
                                                                {
                                                                    // Unexpected tag defined - parse error
                                                                    throw new CPCRInstrumentSystemException("Configuration file parse error.  Unexpected tag <" + tag_name + "> found following <" + strTEC_Entry + "> tag.");
                                                                }
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
                                                m_TEC_Channel_Configurations.Add(strTEC_Entry, PCR_Thermal_TEC_ConfigurationObj);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case "OPTICSCONTROLLER":
                            {
                                if (bRequiredOpticsControllerSectionFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  Duplicate <MotorControllers> entry.");
                                }
                                else
                                {
                                    bRequiredOpticsControllerSectionFound = true; // Required definition
                                }

                                bool bRequiredOpticsControllerPortNumberFound = false;

                                foreach (Positional_XmlElement opticscontrollers_subnode in hw_setting_section_node)
                                {
                                    switch (opticscontrollers_subnode.Name.ToUpper())
                                    {
                                        case "PORT":
                                            {
                                                if (bRequiredOpticsControllerPortNumberFound)
                                                {
                                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error. <" + hw_setting_section_node.Name + "> section has a duplicate <PORT> entry.");
                                                }
                                                else
                                                {
                                                    bRequiredOpticsControllerPortNumberFound = true;
                                                }

                                                this.m_OpticsController_Configuration.m_strPort = opticscontrollers_subnode.InnerText;
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            if (!(bRequiredMotorControllersSectionFound &&
                  bRequiredThermalControllersSectionFound &&
                  bRequiredPCR_Fan_Heatsink_SectionFound &&
                  bRequiredTECControllerChannelsSectionFound
                ))
            {
                // Parse error, first node after '<HWSettings>' must be '<FilterMotor> or <StageMeasurementMotor> or <TECHeater> or <Camera>'
                string strMissingSection = (!bRequiredMotorControllersSectionFound) ? "<MotorControllers>" :
                                           (!bRequiredPCR_Fan_Heatsink_SectionFound) ? "<PCR_HeatSink_Fan>" :
                                           (!bRequiredThermalControllersSectionFound) ? "<ThermalControllers>" : "<TECControllerChannels>";
                throw new CPCRInstrumentSystemException("Instrument configuration file parse error.  " + strMissingSection + " section expected following <HWSettings> tag.");
            }
        }

        protected CSystem_MotorBoardConfigurationItem parse_motorpump_settings(Positional_XmlElement hw_setting_section_node)
        {
            CSystem_MotorBoardConfigurationItem configOut = new CSystem_MotorBoardConfigurationItem();

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
            bool bRequiredPositionsFound = false;

            foreach (Positional_XmlElement motor_subnode in hw_setting_section_node)
            {
                if (motor_subnode.NodeType == XmlNodeType.Element)
                {
                    switch (motor_subnode.Name.ToUpper())
                    {

                        case "MOTORCONTROLLER":
                            {
                                if (bRequiredMotorControllerFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <MotorController> entry.");
                                }
                                else
                                {
                                    bRequiredMotorControllerFound = true; // Required definition
                                }
                                configOut.m_strControllerName = motor_subnode.InnerText;
                            }
                            break;
                        case "CHANNEL":
                            {
                                if (bRequiredChannelFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <Channel> entry.");
                                }
                                else
                                {
                                    bRequiredChannelFound = true; // Required definition
                                }
                                configOut.m_nMotorChannel = int.Parse(motor_subnode.InnerText);
                            }
                            break;
                        case "MOTOR_HOME_SPEED":
                            {
                                if (bRequiredMotorHomeSpeedFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_home_speed> entry.");
                                }
                                else
                                {
                                    bRequiredMotorHomeSpeedFound = true; // Required definition
                                }
                                configOut.m_uiMotorHomeSpeed = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;

                        case "MOTOR_START_SPEED":
                            {
                                if (bRequiredMotorStartSpeedFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_start_speed> entry.");
                                }
                                else
                                {
                                    bRequiredMotorStartSpeedFound = true; // Required definition
                                }
                                configOut.m_uiMotorStartSpeed = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "MOTOR_MAX_SPEED":
                            {
                                if (bRequiredMotorMaxSpeedFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_max_speed> entry.");
                                }
                                else
                                {
                                    bRequiredMotorMaxSpeedFound = true; // Required definition
                                }
                                configOut.m_uiMotorMaxSpeed = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "MOTOR_ACCEL_SPEED":
                            {
                                if (bRequiredMotorAccelSpeedFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_accel_speed> entry.");
                                }
                                else
                                {
                                    bRequiredMotorAccelSpeedFound = true; // Required definition
                                }
                                configOut.m_uiMotorAccel = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "MOTOR_DECEL_SPEED":
                            {
                                if (bRequiredMotorDecelSpeedFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_decel_speed> entry.");
                                }
                                else
                                {
                                    bRequiredMotorDecelSpeedFound = true; // Required definition
                                }
                                configOut.m_uiMotorDecel = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "MOTOR_MOVE_CURRENT":
                            {
                                if (bRequiredMotorMoveCurrentFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_move_current> entry.");
                                }
                                else
                                {
                                    bRequiredMotorMoveCurrentFound = true; // Required definition
                                }
                                configOut.m_uiMotorMoveCurrent = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "MOTOR_HOLD_CURRENT":
                            {
                                if (bRequiredMotorHoldCurrentFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_hold_current> entry.");
                                }
                                else
                                {
                                    bRequiredMotorHoldCurrentFound = true; // Required definition
                                }
                                configOut.m_uiMotorHoldCurrent = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "MOTOR_JERK":
                            {
                                if (bRequiredMotorJerkFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_jerk> entry.");
                                }
                                else
                                {
                                    bRequiredMotorJerkFound = true; // Required definition
                                }
                                configOut.m_uiMotorJerk = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "MOTOR_RESOLUTION":
                            {
                                if (bRequiredMotorResolutionFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_resolution> entry.");
                                }
                                else
                                {
                                    bRequiredMotorResolutionFound = true; // Required definition
                                }
                                configOut.m_uiMotorResolution = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "MOTOR_PROFILE_MODE":
                            {
                                if (bRequiredMotorProfileModeFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_profile_mode> entry.");
                                }
                                else
                                {
                                    bRequiredMotorProfileModeFound = true; // Required definition
                                }
                                configOut.m_uiMotorProfileMode = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "MOTOR_HOME_TIMEOUT":
                            {
                                if (bRequiredMotorHomeTimeoutFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_home_timeout> entry.");
                                }
                                else
                                {
                                    bRequiredMotorHomeTimeoutFound = true; // Required definition
                                }
                                configOut.m_uiMotorHomeTimeout = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "AXIS_MOVE_TIMEOUT":
                            {
                                if (bRequiredMotorPositionMoveTimeoutFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <axis_move_timeout> entry.");
                                }
                                else
                                {
                                    bRequiredMotorPositionMoveTimeoutFound = true; // Required definition
                                }
                                configOut.m_uiPosition_Move_Timeout = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "MOTOR_DIRECTION":
                            {
                                if (bRequiredMotorDirectionFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_direction> entry.");
                                }
                                else
                                {
                                    bRequiredMotorDirectionFound = true; // Required definition
                                }
                                configOut.m_uiMotorDirection = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "MOTOR_MAX_NUM_LOST_STEPS":
                            {
                                if (bRequiredMotorMaxNumLostStepsFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <motor_max_num_lost_steps> entry.");
                                }
                                else
                                {
                                    bRequiredMotorMaxNumLostStepsFound = true; // Required definition
                                }
                                configOut.m_uiMotorMaxNumLostSteps = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "ENCODER_ENABLE":
                            {
                                if (bRequiredMotorEncoderEnableFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <encoder_enable> entry.");
                                }
                                else
                                {
                                    bRequiredMotorEncoderEnableFound = true; // Required definition
                                }
                                configOut.m_bEncoderEnabled = (motor_subnode.InnerText == "1") ||
                                                   (motor_subnode.InnerText.ToUpper() == "ON") ||
                                                   (motor_subnode.InnerText.ToUpper() == "YES");
                            }
                            break;
                        case "ENCODER_MONITOR_TIMER_MS":
                            {
                                if (bRequiredMotorEncoderMonitorTimer_ms_Found)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <encoder_monitor_timer_ms> entry.");
                                }
                                else
                                {
                                    bRequiredMotorEncoderMonitorTimer_ms_Found = true; // Required definition
                                }
                                configOut.m_uiEncoderMonitorTimer_ms = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "ENCODER_MONITOR_PULSE_CHANGE_THRESHOLD":
                            {
                                if (bRequiredMotorEncoderMonitorPulseChangeThresholdFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <encoder_monitor_pulse_change_threshold> entry.");
                                }
                                else
                                {
                                    bRequiredMotorEncoderMonitorPulseChangeThresholdFound = true; // Required definition
                                }
                                configOut.m_uiEncoderMonitorPulseChangeThreshold = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;

                        case "ENCODER_MONITOR_ERROR_COUNT_THRESHOLD":
                            {
                                if (bRequiredMotorEncoderMonitorErrorCountThresholdFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <encoder_monitor_error_count_threshold> entry.");
                                }
                                else
                                {
                                    bRequiredMotorEncoderMonitorErrorCountThresholdFound = true; // Required definition
                                }
                                configOut.m_uiEncoderMonitorErrorCountThreshold = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "ENCODER_DIRECTION_POLARITY":
                            {
                                if (bRequiredMotorEncoderDirectionPolarityFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <encoder_direction_polarity> entry.");
                                }
                                else
                                {
                                    bRequiredMotorEncoderDirectionPolarityFound = true; // Required definition
                                }
                                configOut.m_uiEncoderDirectionPolarity = Convert.ToUInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "ENCODER_START_OFFSET":
                            {
                                if (bRequiredMotorEncoderStartOffsetFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <encoder_start_offset> entry.");
                                }
                                else
                                {
                                    bRequiredMotorEncoderStartOffsetFound = true; // Required definition
                                }
                                configOut.m_iEncoderStartOffset = Convert.ToInt32(motor_subnode.InnerText);
                            }
                            break;
                        case "ENCODER_SCALING_FACTOR":
                            {
                                if (bRequiredMotorEncoderScalingFactorFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <encoder_scaling_factor> entry.");
                                }
                                else
                                {
                                    bRequiredMotorEncoderScalingFactorFound = true; // Required definition
                                }
                                configOut.m_fEncoderScalingFactor = Convert.ToSingle(motor_subnode.InnerText);
                            }
                            break;
                        case "POSITIONS":
                            {
                                if (bRequiredPositionsFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <encoder_scaling_factor> entry.");
                                }
                                else
                                {
                                    bRequiredPositionsFound = true; // Required definition
                                }
                                configOut.positions = motor_subnode.InnerText.Split(',').Select(str => int.Parse(str)).ToArray();
                            }
                            break;
                        default:
                            {
                                // Unexpected tag defined - parse error
                                throw new CPCRInstrumentSystemException("Configuration file parse error.  <Name> and <Channel> definition expected following <OpticalFilter_Y_Motor> tag, instead found <" + motor_subnode.Name + ">.");
                            }
#pragma warning disable CS0162 // Unreachable code detected
                            break;
#pragma warning restore CS0162 // Unreachable code detected
                    }
                }
            }

            return configOut;
        }

        protected CSystem_Optics_Controller_HW_Configuration parse_optics_controller_settings(Positional_XmlElement hw_setting_section_node)
        {
            CSystem_Optics_Controller_HW_Configuration configOut = new CSystem_Optics_Controller_HW_Configuration();

            bool bRequiredPortFound = false;

            foreach (Positional_XmlElement motor_subnode in hw_setting_section_node)
            {
                if (motor_subnode.NodeType == XmlNodeType.Element)
                {
                    switch (motor_subnode.Name.ToUpper())
                    {

                        case "PORT":
                            {
                                if (bRequiredPortFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <Optical_Controller> section has duplicate <Port> entry.");
                                }
                                else
                                {
                                    bRequiredPortFound = true; // Required definition
                                }
                                configOut.m_strPort = motor_subnode.InnerText;
                            }
                            break;

                        default:
                            {
                                // Unexpected tag defined - parse error
                                throw new CPCRInstrumentSystemException("Configuration file parse error.  <Name> and <Channel> definition expected following <OpticalFilter_Y_Motor> tag, instead found <" + motor_subnode.Name + ">.");
                            }
#pragma warning disable CS0162 // Unreachable code detected
                            break;
#pragma warning restore CS0162 // Unreachable code detected
                    }
                }
            }

            return configOut;
        }

        protected CSystem_PumpConfiguration parse_pump_settings(Positional_XmlElement hw_setting_section_node)
        {
            CSystem_PumpConfiguration configOut = new CSystem_PumpConfiguration();

            bool bRequiredMotorControllerFound = false;
            bool bRequiredChannelFound = false;

            foreach (Positional_XmlElement motor_subnode in hw_setting_section_node)
            {
                if (motor_subnode.NodeType == XmlNodeType.Element)
                {
                    switch (motor_subnode.Name.ToUpper())
                    {

                        case "MOTORCONTROLLER":
                            {
                                if (bRequiredMotorControllerFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <MotorController> entry.");
                                }
                                else
                                {
                                    bRequiredMotorControllerFound = true; // Required definition
                                }
                                configOut.m_strControllerName = motor_subnode.InnerText;
                            }
                            break;
                        case "CHANNEL":
                            {
                                if (bRequiredChannelFound)
                                {
                                    throw new CPCRInstrumentSystemException("Configuration file <HWSettings> parse error.  <OpticalFilter_Y_Motor> section has duplicate <Channel> entry.");
                                }
                                else
                                {
                                    bRequiredChannelFound = true; // Required definition
                                }
                                configOut.channel = int.Parse(motor_subnode.InnerText);
                            }
                            break;
                        default:
                            {
                                // Unexpected tag defined - parse error
                                throw new CPCRInstrumentSystemException("Configuration file parse error.  <Name> and <Channel> definition expected following <OpticalFilter_Y_Motor> tag, instead found <" + motor_subnode.Name + ">.");
                            }
#pragma warning disable CS0162 // Unreachable code detected
                            break;
#pragma warning restore CS0162 // Unreachable code detected
                    }
                }
            }

            return configOut;
        }

        public Configuration()
        {
            m_InstrumentConfiguration = new CSystem_Instrument_Configuration();
            m_MiscellaneousConfiguration = new CSystem_Misc_Configuration();
            m_MotorControllerConfigurations = new Dictionary<string, CSystem_Motor_Controller_HW_Configuration>(StringComparer.InvariantCultureIgnoreCase);
            m_ThermalControllerConfigurations = new Dictionary<string, CSystem_Thermal_Controller_HW_Configuration>(StringComparer.InvariantCultureIgnoreCase);
            m_TEC_Channel_Configurations = new Dictionary<string, CTEC_Channel_Configuration>(StringComparer.InvariantCultureIgnoreCase);
            m_PCR_Fan_and_HeatSink_Configuration = new CPCR_Fan_and_HeatSink_Configuration();
            m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration = new CPCR_Thermal_Block_Fan_Control_Thermistor_Configuration();
            m_SystemExhaustFans = new CSystemExhaustFans();
            m_OpticsController_Configuration = new CSystem_Optics_Controller_HW_Configuration();
            m_Slider_Configuration = new CSystem_MotorBoardConfigurationItem();
            m_R1Piston_Configuration = new CSystem_MotorBoardConfigurationItem();
            m_R2Piston_Configuration = new CSystem_MotorBoardConfigurationItem();
            m_HeaterPiston_Configuration = new CSystem_MotorBoardConfigurationItem();
            m_Heater_Configuration = new CSystem_HeaterConfiguration();
            m_OpticsMotor_Configuration = new CSystem_MotorBoardConfigurationItem();
            m_Pump_Configuration = new CSystem_PumpConfiguration();
            m_ChassisPiston_Configuration = new CSystem_MotorBoardConfigurationItem();
        }

        /// <summary>
        /// Reads in the configuration.
        /// </summary>
        /// <param name="strFileNamePath">The path to the configuration file.</param>
        /// <returns>True if the operation succeeded, false otherwise.</returns>
        public async Task<bool> Load(string strFileNamePath)
        {
            bool bResult = true;

            // Strip all comments prior to parsing, as derived Positional_XmlElement class with line numbers can't handle XmlComment nodes

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = null;

            try
            {
                reader = XmlReader.Create(strFileNamePath, settings);
            }
            catch (Exception ex)
            {
                logger.Error("Configuration::Load Caught an exception; {0}", ex.Message);
                bResult = false;
            }

            if (bResult)
            {
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

                    Positional_XmlElement pcrsettings_node = (Positional_XmlElement)doc.DocumentElement.SelectSingleNode("/SystemSettings"); // Parse System configuration document

                    // Parse Instrument settings node
                    Positional_XmlElement instrumentsettings_node = (Positional_XmlElement)pcrsettings_node.SelectSingleNode("InstrumentSettings");
                    ParseInstrumentSettings(instrumentsettings_node);

                    // Parse Miscellaneous settings node
                    Positional_XmlElement miscsettings_node = (Positional_XmlElement)pcrsettings_node.SelectSingleNode("MiscSettings");
                    ParseMiscSettings(miscsettings_node);

                    // Parse Hardware settings node entries
                    Positional_XmlElement HWSettings_node = (Positional_XmlElement)pcrsettings_node.SelectSingleNode("HWSettings");
                    ParseHWSettings(HWSettings_node);

                    // Possible other settings here
                }
                catch (CPCRInstrumentSystemException ex)
                {
                    logger.Error("Configuration::Load Caught an exception; {0}", ex.Message);
                    throw;
                }
                catch (Exception Exc)
                {
                    throw new Exception("Malformed PCR system configuration file contents. =>" + Exc.ToString());
                }
            }

            return bResult;
        }
    }
}
