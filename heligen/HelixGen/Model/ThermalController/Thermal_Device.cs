using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System_Defns;
using System_Configuration_File_Reader;
using Logger;

namespace System_Instruments.Thermal_Controller
{
    public enum eThermalTemperatureState { Idle_ThermalTempState, Ramping_ThermalTempState, AtSetpoint_ThermalTempState, RampTimedout_ThermalTempState, Error_ThermalTempState };

    public class CThermal_Device
    {
        public uint                                                            m_uiTECIndex;
        public string                                                          m_strThermalControllerId;
        public string                                                          m_strThermalConfigSectionId;
        protected CThermal_Adapter_Unit                                        _m_ThermalAdapterUnit;
        protected object                                                      _m_Lock;
        protected CDigital_PCR_Diagnostic_Instrument                          _m_instr;
        protected bool                                                        _m_isOpen;
        protected eThermalTemperatureState                                    _m_ThermalTemperatureState;
        protected CSystem_Configuration_File_Reader.CSystem_Configuration_Obj _m_configObj;
        protected float                                                       _m_fSetpoint;
        protected float                                                       _m_fTemperatureReading;
        
        virtual public void open(CSystem_Configuration_File_Reader.CSystem_Configuration_Obj configObj)
        {
            _m_configObj = configObj;

            // TEC
            m_strThermalConfigSectionId = CSystem_Defns.cstrTECEntries[m_uiTECIndex];
            m_strThermalControllerId = configObj.m_TEC_Channel_Configurations[m_strThermalConfigSectionId].m_strControllerName;                        
            _m_ThermalAdapterUnit = CThermal_Adapter_Unit.Instance(configObj.m_ThermalControllerConfigurations[m_strThermalControllerId].m_strPort);
            
            _m_isOpen = false;
            _m_fSetpoint = 0.0F;
            _m_fTemperatureReading = 0.0F;
            _m_ThermalTemperatureState = eThermalTemperatureState.Idle_ThermalTempState;
            _m_ThermalAdapterUnit.open();
            return;
        }

        virtual public void init()
        {
            return;
        }

        virtual public void start()
        {
            return;
        }

        virtual public void stop(out bool bIsCriticalError, out int iCriticalErrorCode)
        {
            bIsCriticalError = true;
            iCriticalErrorCode = 0;
            return;
        }

        virtual public void stop()
        {
            return;
        }

        virtual public void setTemperature(float fSp, bool bUseRangeBasedParameters = true) 
        {
            return;
        }

        /// <summary>
        /// Get temperature setpoint 
        /// </summary>
        /// <param name="tec_channel">TEC channel to operate with ("00".."06")</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        virtual public float getTemperatureSetpoint()
        {
            return _m_fSetpoint;
        }

        virtual public float getTemperature(out bool bTranBusyCh0, out bool bControlCh0)
        {
            bTranBusyCh0 = false;
            bControlCh0 = false;
            return 0.0F;
        }

        virtual public float getTemperature(out bool bTranBusyCh0, out bool bControlCh0, out bool bTranBusyCh1, out bool bControlCh1)
        {
            bTranBusyCh0 = false;
            bControlCh0 = false;
            bTranBusyCh1 = false;
            bControlCh1 = false;
            return 0.0F;
        }

        virtual public float getPower()
        {
            return 0.0F;
        }

        virtual public float getCurrentOutput()
        {
            return 0.0F;
        }

        virtual public void loadInitialParams()
        {
            return;
        }

        virtual public void loadRampDirectionalAllParameters(string tec_address,
                                                             string overshootOffset,
                                                             string overshootDuration,
                                                             string setpointOffset,
                                                             string pBand,
                                                             string lowClamp
                                                            )
        {
            return;
        }

        virtual public void LoadInitialParametersMonitorOnlyRH(string rh_channel,
                                                               string thermistor_Coefficent_A,
                                                               string thermistor_Coefficent_B,
                                                               string thermistor_Coefficent_C
                                                              )
        {
            return;
        }

        virtual public void setTECRampParameters(float fFromSetpoint, float fToSetpoint, bool bUseRangeBasedParameters = true)
        {
            return;
        }

        virtual public eThermalTemperatureState getTemperatureState()
        {
            return _m_ThermalTemperatureState;
        }

        virtual protected void setTemperatureState(eThermalTemperatureState nextState)
        {
            _m_ThermalTemperatureState = nextState;
        }

        virtual public bool IsTemperaturatSetpoint()
        {
            return getTemperatureState() == eThermalTemperatureState.AtSetpoint_ThermalTempState;
        }

        virtual public bool IsTemperatureRampTimedout()
        {
            return getTemperatureState() == eThermalTemperatureState.RampTimedout_ThermalTempState;
        }
    }
}
