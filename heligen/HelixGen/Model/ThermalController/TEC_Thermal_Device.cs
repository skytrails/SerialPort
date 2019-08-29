using Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System_Configuration_File_Reader;
using System_Defns;
using HelixGen;
using NLog;

namespace System_Instruments.Thermal_Controller
{

    public class CTEC_Thermal_Device : CThermal_Device
    {
        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private bool                                                                          _m_bThermalRampMonitoring; 
        private CSystem_Configuration_File_Reader.CTEC_Channel_Configuration                  _m_TECConfigObj;
        private float _m_fDefaultStartTemperature;

        public CTEC_Thermal_Device(CDigital_PCR_Diagnostic_Instrument instr, uint uiTECIndex, float fDefaultStartTemperature = 20.0F)
        {
            _m_Lock = new object();
            _m_instr = instr;
            m_uiTECIndex = uiTECIndex;
            _m_fDefaultStartTemperature = fDefaultStartTemperature;
        }

        public override void open(CSystem_Configuration_File_Reader.CSystem_Configuration_Obj configObj)
        {
            lock (_m_Lock)
            {
                if (!_m_isOpen)
                {
                    base.open(configObj);
                    _m_isOpen = true;
                    _m_TECConfigObj = _m_configObj.m_TEC_Channel_Configurations[m_strThermalConfigSectionId];
                }
                else
                {
                    throw new CPCRInstrumentSystemException("CTEC_Thermal_Device can't be opened while currently open.", "Fault");
                }
            }
        }

        public override void init()
        {
            // Initialize TEC channel details here
            try
            {
                lock (_m_Lock)
                {
                    stop();
                    loadInitialParams();
                }
            }
            catch (CThermalCriticalException CriticalExc)
            {
                string strMsg = "Fault: CTEC_Thermal_Device.init(" + CriticalExc.m_iCriticalErrorCode.ToString() + ") => error initing TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + ".\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                logger.Error(strMsg);
                //_m_instr.enterInstrumentSafeState();
                throw;
            }
            catch (CPCRInstrumentSystemException /*InstrumentExc*/)
            {
                string strMsg = "Fault: CTEC_Thermal_Device.init() => error initing TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + ".\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                logger.Error(strMsg);
                //_m_instr.enterInstrumentSafeState();
                throw;
            }
            catch (Exception Exc)
            {
                string strMsg = "Unexpected exception Fault from CTEC_Thermal_Device.init(" + Exc.ToString() + ") => error initing TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + ".\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                logger.Error(strMsg);
                //_m_instr.enterInstrumentSafeState();
                throw new System_Defns.CPCRInstrumentSystemException(strMsg);
            }
        }

        public override void start()
        {
            string stat;
            bool bCriticalError;
            int iCriticalErrorCode;
            lock (_m_Lock)
            {
                stat = _m_ThermalAdapterUnit.StartTEC(_m_TECConfigObj.m_uiTECAddress.ToString("00"), out bCriticalError, out iCriticalErrorCode);
            }
            if (bCriticalError || (stat != ""))
            {
                string strMsg = "Fault. Stat[" + stat + "] CTEC_Thermal_Device.start(" + iCriticalErrorCode.ToString() + ") => error starting TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + ".\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                logger.Error(strMsg);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                throw new System_Defns.CThermalCriticalException(strMsg, iCriticalErrorCode);
            }
            setTemperatureState(eThermalTemperatureState.Ramping_ThermalTempState);
        }

        private bool _m_bTECRampMonitorStop;
        private Task _m_PCRTECRampMonitorTask;
        private Action<object> _m_TECRampMonitor = (object obj) =>
        {
            //string sresult;
            string strTemp;
            float fTemp;
            CTEC_Thermal_Device pThis = (CTEC_Thermal_Device)obj;
            Stopwatch elapsedMonitoringTime = new Stopwatch();
            bool bIsCriticalError = false;
            int iCriticalErrorCode = 0;
            elapsedMonitoringTime.Start();
            try
            {
                // TBD - must add higher granularity sleep so cancellation works better.  Will need to use Stopwatch.
                while ((!pThis._m_bTECRampMonitorStop))
                {
                    // Timeout???
                    if (elapsedMonitoringTime.ElapsedMilliseconds > pThis._m_configObj.m_MiscellaneousConfiguration.m_fThermalRampTimeoutInSeconds * 1000)
                    {
                        pThis.setTemperatureState(eThermalTemperatureState.RampTimedout_ThermalTempState);
                        pThis._m_bThermalRampMonitoring = false;
                        try
                        {
                            pThis.stop(out bIsCriticalError, out iCriticalErrorCode);
                        }
                        catch (Exception /*Exc*/)
                        {
                            if (bIsCriticalError)
                            {
                                string strMsg = "Fault. CTEC_Thermal_Device._m_TECRampMonitor(" + iCriticalErrorCode.ToString() + ") => error stopping TEC" + pThis._m_TECConfigObj.m_uiTECAddress.ToString("00") + ".\n";
                                pThis._m_instr.m_PCR_assay_device_instances[pThis.m_uiTECIndex].reportDeviceFault(strMsg);
                                logger.Error(strMsg);
                                pThis._m_instr.m_PCR_assay_device_instances[pThis.m_uiTECIndex].enterDeviceSafeState();
                                throw new System_Defns.CThermalCriticalException(strMsg, iCriticalErrorCode);
                            }
                        }
                        return;
                    }
                    bool btranBusy, bControl;
                    fTemp = pThis.getTemperature(out btranBusy, out bControl);
                    strTemp = fTemp.ToString();
                    if (bControl && (!btranBusy))
                    {
                        // Ramp successful
                        pThis.setTemperatureState(eThermalTemperatureState.AtSetpoint_ThermalTempState);
                        pThis._m_instr.reportThermalControlTemperatureReadbackChangeEvent(pThis.m_uiTECIndex, pThis._m_fTemperatureReading, pThis.getTemperatureState());
                        pThis._m_bThermalRampMonitoring = false;
                        return;
                    }
                    pThis._m_instr.reportThermalControlTemperatureReadbackChangeEvent(pThis.m_uiTECIndex, pThis._m_fTemperatureReading, pThis.getTemperatureState());
                    Thread.Sleep((int)(pThis._m_configObj.m_MiscellaneousConfiguration.m_fThermalRampSamplePeriodInSeconds * 1000));
                }
                // Ramp cancelled.
                pThis.setTemperatureState(eThermalTemperatureState.Idle_ThermalTempState);
                pThis._m_bThermalRampMonitoring = false;
                pThis._m_instr.reportThermalControlTemperatureReadbackChangeEvent(pThis.m_uiTECIndex, pThis._m_fTemperatureReading, pThis.getTemperatureState());
            }
            catch (CThermalCriticalException CriticalExc)
            {
                string strMsg = "Fault: CTEC_Thermal_Device._m_TECRampMonitor(" + CriticalExc.m_iCriticalErrorCode.ToString() + ") => error ramping TEC" + pThis._m_TECConfigObj.m_uiTECAddress.ToString("00") + ".\n";
                pThis._m_instr.m_PCR_assay_device_instances[pThis.m_uiTECIndex].reportDeviceFault(strMsg);
                logger.Error(strMsg);
                pThis._m_instr.m_PCR_assay_device_instances[pThis.m_uiTECIndex].enterDeviceSafeState();
            }
            catch (CPCRInstrumentSystemException InstrumentExc)
            {
                pThis._m_instr.m_PCR_assay_device_instances[pThis.m_uiTECIndex].reportDeviceFault(InstrumentExc.ToString());
                logger.Error(InstrumentExc.ToString());
                string strMsg = "Fault: CTEC_Thermal_Device._m_TECRampMonitor() => error ramping TEC" + pThis._m_TECConfigObj.m_uiTECAddress.ToString("00") + ".\n";
                pThis._m_instr.m_PCR_assay_device_instances[pThis.m_uiTECIndex].reportDeviceFault(strMsg);
                logger.Error(strMsg);
                pThis._m_instr.m_PCR_assay_device_instances[pThis.m_uiTECIndex].enterDeviceSafeState();
            }
            catch (Exception Exc)
            {
                string strMsg = "Unexpected exception Fault from CTEC_Thermal_Device._m_TECRampMonitor(" + Exc.ToString() + ") => error ramping TEC" + pThis._m_TECConfigObj.m_uiTECAddress.ToString("00") + ".\n";
                pThis._m_instr.m_PCR_assay_device_instances[pThis.m_uiTECIndex].reportDeviceFault(strMsg);
                logger.Error(strMsg);
                pThis._m_instr.m_PCR_assay_device_instances[pThis.m_uiTECIndex].enterDeviceSafeState();
            }
        };

        public override void stop()
        {
            bool bIsCriticalError;
            int iCriticalErrorCode;
            stop(out bIsCriticalError, out iCriticalErrorCode);
        }

        /// <summary>
        /// Stop TEC control (power off)
        /// </summary>
        /// <param name="tec_channel">TEC channel to operate with ("00".."06")</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        override public void stop(out bool bIsCriticalError, out int iCriticalErrorCode)
        {
            bIsCriticalError = false;
            iCriticalErrorCode = 0;
            {
                string sresult = "";
                try
                {
                    {
                        sresult = _m_ThermalAdapterUnit.StopTEC(_m_TECConfigObj.m_uiTECAddress.ToString("00"), out bIsCriticalError, out iCriticalErrorCode);
                        if (_m_bThermalRampMonitoring)
                        {
                            _m_bTECRampMonitorStop = true;
                            _m_bThermalRampMonitoring = false;
                        }
                        setTemperatureState(eThermalTemperatureState.Idle_ThermalTempState);
                        if (bIsCriticalError || (sresult != ""))
                        {
                            string strMsg = "Fault. Stat[" + sresult + "] CTEC_Thermal_Device.stop(" + iCriticalErrorCode.ToString() + ") => error stopping TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + ".\n";
                            throw new System_Defns.CThermalCriticalException(strMsg, iCriticalErrorCode);
                        }
                    }
                    _m_instr.reportThermalControlTemperatureReadbackChangeEvent(m_uiTECIndex, _m_fTemperatureReading, getTemperatureState());
                }
                catch (CThermalCriticalException CriticalExc)
                {
                    string strMsg = "Fault: CTEC_Thermal_Device.stop(" + CriticalExc.m_iCriticalErrorCode.ToString() + ") => error stopping TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + ".\n";
                    _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                    logger.Error(strMsg);
                    //_m_instr.enterInstrumentSafeState();
                    _m_instr.reportThermalControlTemperatureSetpointChangeEvent(float.NaN);
                    throw;
                }
                catch (CPCRInstrumentSystemException /*InstrumentExc*/)
                {
                    string strMsg = "Fault: CTEC_Thermal_Device.stop() => error stopping TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + ".\n";
                    _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                    logger.Error(strMsg);
                    //_m_instr.enterInstrumentSafeState();
                    _m_instr.reportThermalControlTemperatureSetpointChangeEvent(float.NaN);
                    throw;
                }
                catch (Exception Exc)
                {
                    string strMsg = "Unexpected exception Fault from CTEC_Thermal_Device.stop(" + Exc.ToString() + ") => error stopping TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + ".\n";
                    _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                    logger.Error(strMsg);
                    //_m_instr.enterInstrumentSafeState();
                    _m_instr.reportThermalControlTemperatureSetpointChangeEvent(float.NaN);
                    throw new System_Defns.CPCRInstrumentSystemException(strMsg);
                }
            }
        }

        /// <summary>
        /// Set temperature for specified TEC channel
        /// </summary>
        /// <param name="tec_channel">TEC channel to operate with ("00".."06")</param>
        /// <param name="stemp">Temperature to set, C</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        override public void setTemperature(float fSp, bool bUseRangeBasedParameters = true) 
        {
            string sresult;
            bool bCriticalError;
            int iCriticalErrorCode;
            string stemp = "nan";
            float fFromTemperatureSetpoint;

            try
            {
                stemp = fSp.ToString();
                if (getTemperatureState() != eThermalTemperatureState.Idle_ThermalTempState)
                {
                    stop();
                    while (getTemperatureState() != eThermalTemperatureState.Idle_ThermalTempState)
                    {
                        Thread.Sleep(100);
                    }
                }

                fFromTemperatureSetpoint = _m_fSetpoint; // Remember starting setpoint
                _m_fSetpoint = fSp;                      // Remember new setpoint

                // Set range based ramping (directional) parameters
                setTECRampParameters(fFromTemperatureSetpoint, _m_fSetpoint, bUseRangeBasedParameters);

                sresult = _m_ThermalAdapterUnit.SetTemperatureTEC(_m_TECConfigObj.m_uiTECAddress.ToString("00"), stemp, out bCriticalError, out iCriticalErrorCode);
                if (sresult != "")
                {
                    throw new CPCRInstrumentSystemException("Failed setting TEC temperature setpoint => \"" + stemp + "\".\n", "Fault");
                }
                _m_bTECRampMonitorStop = false;
                setTemperatureState(eThermalTemperatureState.Ramping_ThermalTempState);
                start();

                try
                {
                    _m_bThermalRampMonitoring = true;
                    _m_PCRTECRampMonitorTask = new Task(_m_TECRampMonitor, this);  // Construct and start the TEC Monitor Ramp task
                    _m_PCRTECRampMonitorTask.Start();
                    //int k = 0;
                }
                catch (Exception /*Exc*/)
                {
                    //int i = 0;
                }

                _m_instr.reportThermalControlTemperatureSetpointChangeEvent(_m_fSetpoint);
                _m_instr.reportThermalControlTemperatureReadbackChangeEvent(m_uiTECIndex, _m_fTemperatureReading, getTemperatureState());
            }
            catch (CThermalCriticalException CriticalExc)
            {
                string strMsg = "Fault: CTEC_Thermal_Device.setTemperature(" + CriticalExc.m_iCriticalErrorCode.ToString() + ") => error setting TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " temperature to " + stemp + " C.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                logger.Error(strMsg);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                _m_instr.reportThermalControlTemperatureSetpointChangeEvent(float.NaN);
                throw;
            }
            catch (CPCRInstrumentSystemException /*InstrumentExc*/)
            {
                string strMsg = "Fault: CTEC_Thermal_Device.setTemperature() => error setting TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " temperature to " + stemp + " C.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                logger.Error(strMsg);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                _m_instr.reportThermalControlTemperatureSetpointChangeEvent(float.NaN);
                throw;
            }
            catch (Exception Exc)
            {
                string strMsg = "Unexpected exception Fault from CTEC_Thermal_Device.setTemperature(" + Exc.ToString() + ") => error setting TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " temperature to " + stemp + " C.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                logger.Error(strMsg);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                _m_instr.reportThermalControlTemperatureSetpointChangeEvent(float.NaN);
                throw new System_Defns.CPCRInstrumentSystemException(strMsg);
            }
        }

        /// <summary>
        /// Get current temperature in C from selected TEC channel
        /// </summary>
        /// <param name="tec_channel">TEC channel to operate with ("00".."06")</param>
        /// <param name="stemp">Current temperature, C</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        override public float getTemperature(out bool btranBusyCh0, out bool bControlCh0)
        {
            bool bCriticalError;
            int iCriticalErrorCode;
            string stemp = "";
            try
            {
//              lock (_m_Lock)
                {
                    string stat = _m_ThermalAdapterUnit.GetTemperatureTEC(_m_TECConfigObj.m_uiTECAddress.ToString("00"), out stemp, out bCriticalError, out iCriticalErrorCode, out btranBusyCh0, out bControlCh0);
                    if (bCriticalError || (stat != ""))
                    {
                        // Error reading TEC
                        string strMsg = "Fault. Error[" + stat + "] CTEC_Thermal_Device.getTemperature() => _m_ThermalAdapterUnit.GetTemperatureTEC(" + iCriticalErrorCode.ToString() + ") =>  reading TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " temperature.\n";
                        _m_instr.reportInstrumentFault(strMsg);
                        logger.Error(strMsg);
                        throw new System_Defns.CPCRInstrumentSystemException(strMsg);
                    }
                    _m_fTemperatureReading = Convert.ToSingle(stemp);
                    _m_instr.reportThermalControlTemperatureReadbackChangeEvent(m_uiTECIndex, _m_fTemperatureReading, getTemperatureState());
                    return _m_fTemperatureReading;
                }
            }
            catch (CThermalCriticalException CriticalExc)
            {
                string strMsg = "Fault: CTEC_Thermal_Device.getTemperature(" + CriticalExc.m_iCriticalErrorCode.ToString() + ") => error getting TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " temperature.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                logger.Error(strMsg);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                _m_instr.reportThermalControlTemperatureReadbackChangeEvent(m_uiTECIndex, _m_fTemperatureReading, getTemperatureState());
                throw;
            }
            catch (CPCRInstrumentSystemException /*InstrumentExc*/)
            {
                string strMsg = "Fault: CTEC_Thermal_Device.getTemperature() => error getting TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " temperature.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                logger.Error(strMsg);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                _m_instr.reportThermalControlTemperatureReadbackChangeEvent(m_uiTECIndex, _m_fTemperatureReading, getTemperatureState());
                throw;
            }
            catch (Exception Exc)
            {
                string strMsg = "Unexpected exception Fault from CTEC_Thermal_Device.getTemperature(" + Exc.ToString() + ") => error getting TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " temperature.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                logger.Error(strMsg);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                _m_instr.reportThermalControlTemperatureReadbackChangeEvent(m_uiTECIndex, _m_fTemperatureReading, getTemperatureState());
                throw new System_Defns.CPCRInstrumentSystemException(strMsg);
            }
        }

        /// <summary>
        /// Get current temperature in C from selected TEC channel
        /// </summary>
        /// <param name="tec_channel">TEC channel to operate with ("00".."06")</param>
        /// <param name="stemp">Current temperature, C</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        override public float getTemperature(out bool btranBusyCh0, out bool bControlCh0, out bool btranBusyCh1, out bool bControlCh1)
        {
            bool bCriticalError;
            int iCriticalErrorCode;
            string stemp = "";

            try
            {
//              lock (_m_Lock)
                {
                    string stat = _m_ThermalAdapterUnit.GetTemperatureTEC(_m_TECConfigObj.m_uiTECAddress.ToString("00"), out stemp, out bCriticalError, out iCriticalErrorCode, out btranBusyCh0, out bControlCh0, out btranBusyCh1, out bControlCh1);
                    if (bCriticalError || (stat != ""))
                    {
                        // Error reading TEC
                        string strMsg = "Fault: CTEC_Thermal_Device.getTemperature(" + iCriticalErrorCode.ToString() + ") => error invoking CTEC_Adapter_Driver.GetTemperatureTEC() to get TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " temperature.\n";
                        _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                        _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                        throw new System_Defns.CPCRInstrumentSystemException(strMsg);
                    }
                    _m_fTemperatureReading = Convert.ToSingle(stemp);
                    _m_instr.reportThermalControlTemperatureReadbackChangeEvent(m_uiTECIndex, _m_fTemperatureReading, getTemperatureState());
                    return _m_fTemperatureReading;
                }
            }
            catch (CThermalCriticalException /*CriticalExc*/)
            {
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                _m_instr.reportThermalControlTemperatureReadbackChangeEvent(m_uiTECIndex, _m_fTemperatureReading, getTemperatureState());
                throw;
            }
            catch (CPCRInstrumentSystemException /*InstrumentExc*/)
            {
                string strMsg = "Fault: CTEC_Thermal_Device.getTemperature() => error invoking CTEC_Adapter_Driver.GetTemperatureTEC to get TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " temperature.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                logger.Error(strMsg);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                _m_instr.reportThermalControlTemperatureReadbackChangeEvent(m_uiTECIndex, _m_fTemperatureReading, getTemperatureState());
                throw;
            }
            catch (Exception Exc)
            {
                string strMsg = "Unexpected exception Fault from CTEC_Thermal_Device.getTemperature(" + Exc.ToString() + ") => error invoking CTEC_Adapter_Driver.GetTemperatureTEC() to get TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " temperature.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                _m_instr.reportThermalControlTemperatureReadbackChangeEvent(m_uiTECIndex, _m_fTemperatureReading, getTemperatureState());
                throw new System_Defns.CPCRInstrumentSystemException(strMsg);
            }
        }

        /// <summary>
        /// Get current power in % from selected TEC channel
        /// </summary>
        /// <param name="tec_channel">TEC channel to operate with ("00".."06")</param>
        /// <param name="spwr">Current power, %</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        override public float getPower()
        {
            bool bCriticalError;
            int iCriticalErrorCode;
            string spwr;

            try
            {
//              lock (_m_Lock)
                {
                    _m_ThermalAdapterUnit.GetPowerTEC(_m_TECConfigObj.m_uiTECAddress.ToString("00"), out spwr, out bCriticalError, out iCriticalErrorCode);
                    return Convert.ToSingle(spwr);
                }
            }
            catch (CThermalCriticalException CriticalExc)
            {
                string strMsg = "Fault: CTEC_Thermal_Device.getPower(" + CriticalExc.m_iCriticalErrorCode.ToString() + ") => error invoking CTEC_Adapter_Driver.GetPowerTEC() to get TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " power.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                throw;
            }
            catch (CPCRInstrumentSystemException /*InstrumentExc*/)
            {
                string strMsg = "Fault: CTEC_Thermal_Device.getPower() => error invoking CTEC_Adapter_Driver.GetPowerTEC to get TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " power.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                throw;
            }
            catch (Exception Exc)
            {
                string strMsg = "Unexpected exception Fault from CTEC_Thermal_Device.getPower(" + Exc.ToString() + ") => error invoking CTEC_Adapter_Driver.GetPowerTEC() to get TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " power.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                throw new System_Defns.CPCRInstrumentSystemException(strMsg);
            }
        }

        /// <summary>
        /// Get output current in amps from selected TEC channel
        /// </summary>
        /// <param name="tec_channel">TEC channel to operate with ("00".."06")</param>
        /// <param name="samp">Output Current, Amps</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        override public float getCurrentOutput()
        {
            bool bCriticalError;
            int iCriticalErrorCode;
            string samp;

            try
            {
//              lock (_m_Lock)
                {
                    _m_ThermalAdapterUnit.GetOutputCurrentTEC(_m_TECConfigObj.m_uiTECAddress.ToString("00"), out samp, out bCriticalError, out iCriticalErrorCode);
                    return Convert.ToSingle(samp);
                }
            }
            catch (CThermalCriticalException CriticalExc)
            {
                string strMsg = "Fault: CTEC_Thermal_Device.getCurrentOutput(" + CriticalExc.m_iCriticalErrorCode.ToString() + ") => error invoking CTEC_Adapter_Driver.GetOutputCurrentTEC() to get TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " output current.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                throw;
            }
            catch (CPCRInstrumentSystemException /*InstrumentExc*/)
            {
                string strMsg = "Fault: CTEC_Thermal_Device.getCurrentOutput() => error invoking CTEC_Adapter_Driver.GetOutputCurrentTEC to get TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " output current.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                throw;
            }
            catch (Exception Exc)
            {
                string strMsg = "Unexpected exception Fault from CTEC_Thermal_Device.getCurrentOutput(" + Exc.ToString() + ") => error invoking CTEC_Adapter_Driver.GetOutputCurrentTEC() to get TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " output current.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                throw new System_Defns.CPCRInstrumentSystemException(strMsg);
            }
        }

        /// <summary>
        /// Initialize TEC board, selected TEC channel with parameters
        /// </summary>
        /// <param name="tec_address">TEC channel to operate with ("00".."06")</param>
        /// <param name="P_value">"P" parameter</param>
        /// <param name="I_value">"I" parameter</param>
        /// <param name="D_value">"D" parameter</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        override public void loadInitialParams()
        {
            string sresult;
            bool bCriticalError;
            int iCriticalErrorCode;

            try
            {
                lock (_m_Lock)
                {
                    sresult = _m_ThermalAdapterUnit.LoadInitialParametersTEC
                                            (
                                             _m_TECConfigObj.m_uiTECAddress.ToString("00"),
                                             _m_TECConfigObj.m_ControlPIDSampleTimeInSeconds.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_fOvershootOffset.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_uiOvershootDuration.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_fSetpointOffset.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_fDeadBand.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_fPBand.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitGain.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitGain2.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitGainOffset.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fRabbitDerivGain.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fPGain.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fIGain.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_PID_Settings[CSystem_Defns.cuiPIDUpParamsId].m_fDGain.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitGain.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitGain2.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitGainOffset.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fRabbitDerivGain.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fPGain.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fIGain.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_PID_Settings[CSystem_Defns.cuiPIDDownParamsId].m_fDGain.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_fHighClamp.ToString(),
                                             _m_TECConfigObj.m_Step_PID_RampUp_Default.m_fLowClamp.ToString(),
                                             _m_TECConfigObj.m_fErrorTermBand.ToString(),
                                             _m_TECConfigObj.m_fErrorTermCount.ToString(),
                                             _m_TECConfigObj.m_fSteadyStatePowerLimit.ToString(),
                                             _m_TECConfigObj.m_fSteadyStatePowerLimitCount.ToString(),
                                             _m_TECConfigObj.m_fThermA_Coeff.ToString("F99").TrimEnd("0".ToCharArray()),
                                             _m_TECConfigObj.m_fThermB_Coeff.ToString("F99").TrimEnd("0".ToCharArray()),
                                             _m_TECConfigObj.m_fThermC_Coeff.ToString("F99").TrimEnd("0".ToCharArray()),
                                             out bCriticalError,
                                             out iCriticalErrorCode
                                           );
                    if (bCriticalError || (sresult != ""))
                    {
                        string strMsg = "Fault: CTEC_Thermal_Device.loadInitialParams(" + iCriticalErrorCode.ToString() + ") => error invoking CTEC_Adapter_Driver.LoadInitialParametersTEC() to load TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " setup parameters.\n";
                        throw new System_Defns.CThermalCriticalException(strMsg, iCriticalErrorCode);
                    }
                }
            }
            catch (CThermalCriticalException CriticalExc)
            {
                string strMsg = "Fault: CTEC_Thermal_Device.loadInitialParams(" + CriticalExc.m_iCriticalErrorCode.ToString() + ") => error invoking CTEC_Adapter_Driver.LoadInitialParametersTEC() to load TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " setup parameters.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                //_m_instr.enterInstrumentSafeState();
                throw;
            }
            catch (CPCRInstrumentSystemException /*InstrumentExc*/)
            {
                string strMsg = "Fault: CTEC_Thermal_Device.loadInitialParams() => error invoking CTEC_Adapter_Driver.LoadInitialParametersTEC() to load TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " setup parameters.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                //_m_instr.enterInstrumentSafeState();
                throw;
            }
            catch (Exception Exc)
            {
                string strMsg = "Unexpected exception Fault from CTEC_Thermal_Device.loadInitialParams(" + Exc.ToString() + ") => error invoking CTEC_Adapter_Driver.LoadInitialParametersTEC() to load TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " setup parameters.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                //_m_instr.enterInstrumentSafeState();
                throw new System_Defns.CPCRInstrumentSystemException(strMsg);
            }
        }

        /// <summary>
        /// Load Ramp Directional TEC board parameters for selected TEC channel 
        /// </summary>
        /// <param name="tec_address">TEC channel to operate with ("00".."06")</param>
        /// <param name="P_value">"P" parameter</param>
        /// <param name="I_value">"I" parameter</param>
        /// <param name="D_value">"D" parameter</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public void loadRampDirectionalParameters(string overshootOffset,
                                                  string overshootDuration,
                                                  string setpointOffset,
                                                  string deadBand,
                                                  string pBand,
                                                  string rabbitGain_Ch0,
                                                  string rabbitGain2_Ch0,
                                                  string rabbitGainOffset_Ch0,
                                                  string rabbitDerivGain_Ch0,
                                                  string PGain_Ch0,
                                                  string IGain_Ch0,
                                                  string DGain_Ch0,
                                                  string rabbitGain_Ch1,
                                                  string rabbitGain2_Ch1,
                                                  string rabbitGainOffset_Ch1,
                                                  string rabbitDerivGain_Ch1,
                                                  string PGain_Ch1,
                                                  string IGain_Ch1,
                                                  string DGain_Ch1,
                                                  string highClamp,
                                                  string lowClamp
                                                 )
        {
            bool bCriticalError;
            int iCriticalErrorCode;
            string sresult;

            try
            {
                lock (_m_Lock)
                {
                    sresult = _m_ThermalAdapterUnit.LoadRampDirectionalAllParametersTEC(_m_TECConfigObj.m_uiTECAddress.ToString("00"),
                                                                                        overshootOffset,
                                                                                        overshootDuration,
                                                                                        setpointOffset,
                                                                                        deadBand,
                                                                                        pBand,
                                                                                        rabbitGain_Ch0,
                                                                                        rabbitGain2_Ch0,
                                                                                        rabbitGainOffset_Ch0,
                                                                                        rabbitDerivGain_Ch0,
                                                                                        PGain_Ch0,
                                                                                        IGain_Ch0,
                                                                                        DGain_Ch0,
                                                                                        rabbitGain_Ch1,
                                                                                        rabbitGain2_Ch1,
                                                                                        rabbitGainOffset_Ch1,
                                                                                        rabbitDerivGain_Ch1,
                                                                                        PGain_Ch1,
                                                                                        IGain_Ch1,
                                                                                        DGain_Ch1,
                                                                                        highClamp,
                                                                                        lowClamp,
                                                                                        out bCriticalError,
                                                                                        out iCriticalErrorCode
                                                                                       );

                    if (bCriticalError || (sresult != ""))
                    {
                        string strMsg = "Fault: CPCR_TEC_Controller.loadRampDirectionalAllParameters(" + iCriticalErrorCode.ToString() + ") => error invoking CTEC_Adapter_Driver.LoadRampDirectionalParametersTEC() to load TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " directional ramp parameters.\n";
                        throw new System_Defns.CThermalCriticalException(strMsg, iCriticalErrorCode);
                    }
                }
            }
            catch (CThermalCriticalException CriticalExc)
            {
                string strMsg = "Fault: CPCR_TEC_Controller.loadRampDirectionalAllParameters(" + CriticalExc.m_iCriticalErrorCode.ToString() + ") => error invoking CTEC_Adapter_Driver.LoadRampDirectionalParametersTEC() to load TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " directional ramp parameters.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                throw;
            }
            catch (CPCRInstrumentSystemException /*InstrumentExc*/)
            {
                string strMsg = "Fault: CPCR_TEC_Controller.loadRampDirectionalAllParameters() => error invoking CTEC_Adapter_Driver.LoadRampDirectionalParametersTEC() to load TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " directional ramp parameters.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                throw;
            }
            catch (Exception Exc)
            {
                string strMsg = "Unexpected exception Fault from CPCR_TEC_Controller.loadRampDirectionalAllParameters(" + Exc.ToString() + ") => error invoking CTEC_Adapter_Driver.LoadRampDirectionalParametersTEC() to load TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " directional ramp parameters.\n";
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                throw new System_Defns.CPCRInstrumentSystemException(strMsg);
            }
        }

#if false
        override public void loadRampDirectionalBufferedParameters(string pBand, string lowClamp)
        {
            bool bCriticalError;
            int iCriticalErrorCode;
            string sresult;

            try
            {
                lock (_m_Lock)
                {
                    sresult = _m_ThermalAdapterUnit.LoadRampDirectionalBufferedParametersTEC(_m_TECConfigObj.m_uiTECAddress.ToString("00"),
                                                                                             pBand,
                                                                                             lowClamp,
                                                                                             out bCriticalError,
                                                                                             out iCriticalErrorCode
                                                                                            );

                    if (bCriticalError || (sresult != ""))
                    {
                        string strMsg = "Fault: CTEC_Thermal_Device.loadRampDirectionalBufferedParameters(" + iCriticalErrorCode.ToString() + ") => error invoking CTEC_Adapter_Driver.LoadRampDirectionalParametersTEC() to load TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " directional ramp parameters.\n";
                        throw new System_Defns.CThermalCriticalException(strMsg, iCriticalErrorCode);
                    }
                }
            }
            catch (CThermalCriticalException CriticalExc)
            {
                _m_instr.m_PCR_assay_cartridge_instances[m_uiTECIndex].ChangeDeviceState(CDigital_PCR_Device_Chip.eDeviceStates.Faulted);
                string strMsg = "Fault: CTEC_Thermal_Device.loadRampDirectionalBufferedParameters(" + CriticalExc.m_iCriticalErrorCode.ToString() + ") => error invoking CTEC_Adapter_Driver.LoadRampDirectionalParametersTEC() to load TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " directional ramp parameters.\n";
                _m_instr.m_PCR_assay_cartridge_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                _m_instr.m_PCR_assay_cartridge_instances[m_uiTECIndex].enterDeviceSafeState();
                throw;
            }
            catch (CPCRInstrumentSystemException InstrumentExc)
            {
                _m_instr.m_PCR_assay_cartridge_instances[m_uiTECIndex].ChangeDeviceState(CDigital_PCR_Device_Chip.eDeviceStates.Faulted);
                string strMsg = "Fault: CTEC_Thermal_Device.loadRampDirectionalBufferedParameters() => error invoking CTEC_Adapter_Driver.LoadRampDirectionalParametersTEC() to load TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " directional ramp parameters.\n";
                _m_instr.m_PCR_assay_cartridge_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                _m_instr.m_PCR_assay_cartridge_instances[m_uiTECIndex].enterDeviceSafeState();
                throw;
            }
            catch (Exception Exc)
            {
                _m_instr.m_PCR_assay_cartridge_instances[m_uiTECIndex].ChangeDeviceState(CDigital_PCR_Device_Chip.eDeviceStates.Faulted);
                string strMsg = "Unexpected exception Fault from CTEC_Thermal_Device.loadRampDirectionalBufferedParameters(" + Exc.ToString() + ") => error invoking CTEC_Adapter_Driver.LoadRampDirectionalParametersTEC() to load TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " directional ramp parameters.\n";
                _m_instr.m_PCR_assay_cartridge_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                _m_instr.m_PCR_assay_cartridge_instances[m_uiTECIndex].enterDeviceSafeState();
                throw new System_Defns.CPCRInstrumentSystemException(strMsg);
            }
        }
#endif

        override public void setTECRampParameters(float fFromSetpoint, float fToSetpoint, bool bUseRangeBasedParameters = true)
        {
            {
                try
                {
                    CSystem_Configuration_File_Reader.CTEC_HW_Ramping_Configuration TEC_Ramp_Param_Set = null;
                    bool bParamsFound = false;
                    bool bIsStepOperation = false;
                    List<CSystem_Configuration_File_Reader.CTEC_From_Temperature_PID_Element> from_list;

                    if (!bUseRangeBasedParameters)
                    {
                        return; // Nothing to do
                    }

                    // Determine parameters to load from range based configuration tables
                    if (fFromSetpoint < fToSetpoint)
                    {   // Ramp Up
                        from_list = _m_TECConfigObj.m_Step_PID_RampUp_Range_List;
                        bIsStepOperation = true;

                        {
                            foreach (CSystem_Configuration_File_Reader.CTEC_From_Temperature_PID_Element from_range in from_list)
                            {
                                if ((fFromSetpoint >= from_range.m_TemperatureRange.m_fLowTemperature) && (fFromSetpoint <= from_range.m_TemperatureRange.m_fHighTemperature))
                                {
                                    foreach (CSystem_Configuration_File_Reader.CTEC_HW_Ramping_Configuration to_range in from_range.m_ToTemperatures)
                                    {
                                        if ((fToSetpoint >= to_range.m_TemperatureRange.m_fLowTemperature) && (fToSetpoint <= to_range.m_TemperatureRange.m_fHighTemperature))
                                        {
                                            TEC_Ramp_Param_Set = to_range;
                                            bParamsFound = true;
                                            break;
                                        }
                                    }
                                }
                                if (bParamsFound)
                                {
                                    break;
                                }
                            }
                            if (!bParamsFound)
                            {
                                // Use default
                                if (bIsStepOperation)
                                {
                                    TEC_Ramp_Param_Set = _m_TECConfigObj.m_Step_PID_RampUp_Default;
                                }
                            }
                        }
                    }
                    else
                    {   // Ramp Down
                        from_list = _m_TECConfigObj.m_Step_PID_RampDown_Range_List;
                        bIsStepOperation = true;

                        {
                            foreach (CSystem_Configuration_File_Reader.CTEC_From_Temperature_PID_Element from_range in from_list)
                            {
                                if ((fFromSetpoint >= from_range.m_TemperatureRange.m_fLowTemperature) && (fFromSetpoint <= from_range.m_TemperatureRange.m_fHighTemperature))
                                {
                                    foreach (CSystem_Configuration_File_Reader.CTEC_HW_Ramping_Configuration to_range in from_range.m_ToTemperatures)
                                    {
                                        if ((fToSetpoint >= to_range.m_TemperatureRange.m_fLowTemperature) && (fToSetpoint <= to_range.m_TemperatureRange.m_fHighTemperature))
                                        {
                                            TEC_Ramp_Param_Set = to_range;
                                            bParamsFound = true;
                                            break;
                                        }
                                    }
                                }
                                if (bParamsFound)
                                {
                                    break;
                                }
                            }
                            if (!bParamsFound)
                            {
                                // Use default
                                if (bIsStepOperation)
                                {
                                    TEC_Ramp_Param_Set = _m_TECConfigObj.m_Step_PID_RampDown_Default;
                                }
                            }
                        }
                    }

                    // Load directional parameters
                    this.loadRampDirectionalParameters( TEC_Ramp_Param_Set.m_fOvershootOffset.ToString(),
                                                        TEC_Ramp_Param_Set.m_uiOvershootDuration.ToString(),
                                                        TEC_Ramp_Param_Set.m_fSetpointOffset.ToString(),
                                                        TEC_Ramp_Param_Set.m_fDeadBand.ToString(),
                                                        TEC_Ramp_Param_Set.m_fPBand.ToString(),
                                                        TEC_Ramp_Param_Set.m_PID_Settings[0].m_fRabbitGain.ToString(),
                                                        TEC_Ramp_Param_Set.m_PID_Settings[0].m_fRabbitGain2.ToString(),
                                                        TEC_Ramp_Param_Set.m_PID_Settings[0].m_fRabbitGainOffset.ToString(),
                                                        TEC_Ramp_Param_Set.m_PID_Settings[0].m_fRabbitDerivGain.ToString(),
                                                        TEC_Ramp_Param_Set.m_PID_Settings[0].m_fPGain.ToString(),
                                                        TEC_Ramp_Param_Set.m_PID_Settings[0].m_fIGain.ToString(),
                                                        TEC_Ramp_Param_Set.m_PID_Settings[0].m_fDGain.ToString(),
                                                        TEC_Ramp_Param_Set.m_PID_Settings[1].m_fRabbitGain.ToString(),
                                                        TEC_Ramp_Param_Set.m_PID_Settings[1].m_fRabbitGain2.ToString(),
                                                        TEC_Ramp_Param_Set.m_PID_Settings[1].m_fRabbitGainOffset.ToString(),
                                                        TEC_Ramp_Param_Set.m_PID_Settings[1].m_fRabbitDerivGain.ToString(),
                                                        TEC_Ramp_Param_Set.m_PID_Settings[1].m_fPGain.ToString(),
                                                        TEC_Ramp_Param_Set.m_PID_Settings[1].m_fIGain.ToString(),
                                                        TEC_Ramp_Param_Set.m_PID_Settings[1].m_fDGain.ToString(),
                                                        TEC_Ramp_Param_Set.m_fHighClamp.ToString(),
                                                        TEC_Ramp_Param_Set.m_fLowClamp.ToString()
                                                      );
                }
                catch (CThermalCriticalException CriticalExc)
                {
                    string strMsg = "Fault: CTEC_Thermal_Device.setTECRampParameters(" + CriticalExc.m_iCriticalErrorCode.ToString() + ") => invoking LoadRampDirectionalParametersTEC() error setting up TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " parameters.\n";
                    _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                    _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                    _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                    throw;
                }
                catch (CPCRInstrumentSystemException /*InstrumentExc*/)
                {
                    string strMsg = "Fault: CTEC_Thermal_Device.setTECRampParameters() => invoking LoadRampDirectionalParametersTEC() error setting up TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " parameters.\n";
                    _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                    _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                    _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                    throw;
                }
                catch (Exception Exc)
                {
                    string strMsg = "Unexpected exception Fault from CTEC_Thermal_Device.setTECRampParameters(" + Exc.ToString() + ") => invoking LoadRampDirectionalParametersTEC() error setting up TEC" + _m_TECConfigObj.m_uiTECAddress.ToString("00") + " parameters.\n";
                    _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].reportDeviceFault(strMsg);
                    _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                    _m_instr.m_PCR_assay_device_instances[m_uiTECIndex].enterDeviceSafeState();
                    throw new System_Defns.CPCRInstrumentSystemException(strMsg);
                }
            }
        }
    }
}
