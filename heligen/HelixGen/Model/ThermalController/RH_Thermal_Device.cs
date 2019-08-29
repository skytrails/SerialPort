using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System_Defns;
using System_Configuration_File_Reader;
using Nlog;

namespace System_Instruments.Thermal_Controller
{
    public class CRH_Thermal_Device : CThermal_Device
    {
        private CSystem_Configuration_File_Reader.CRH_Channel_Configuration _m_RHConfigObj;
        private float _m_fDefaultStartTemperature;
        private bool _m_ThermalRampMonitoring;

        public CRH_Thermal_Device(CAssayDiagnosticInstrument instr, uint uiSlotIndex, string strThermalElementName, float fDefaultStartTemperature = 20.0F) 
        {
            _m_Lock = new object();
            _m_instr = instr;
            m_uiSlotIndex = uiSlotIndex;
            m_strThermalElementName = strThermalElementName;
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
                    _m_RHConfigObj = _m_configObj.m_RH_Channel_Configurations[m_strThermalConfigSectionId];
                }
                else
                {
                    throw new CPCRInstrumentSystemException("CRH_Thermal_Device can't be opened while currently open.", "Fault");
                }
            }
        }

        public override void init()
        {
            lock (_m_Lock)
            {
                bool bCriticalError;
                int iCriticalErrorCode;

                stop();
                _m_ThermalAdapterUnit.LoadInitialParametersRH(_m_RHConfigObj.m_uiRHChannel.ToString(),
                                                              _m_RHConfigObj.m_fPGain.ToString(),
                                                              _m_RHConfigObj.m_fIGain.ToString(),
                                                              _m_RHConfigObj.m_fDGain.ToString(),
                                                              _m_RHConfigObj.m_fPreHeatTemperatureSetpoint.ToString(),
                                                              _m_RHConfigObj.m_fOvershootDuration.ToString(),
                                                              _m_RHConfigObj.m_fPBand.ToString(),
                                                              _m_RHConfigObj.m_fDeadBand.ToString(),
                                                              _m_RHConfigObj.m_fSetpointOffset.ToString(),
                                                              _m_fDefaultStartTemperature.ToString(),
                                                              _m_RHConfigObj.m_fThermA_Coeff.ToString("F99").TrimEnd("0".ToCharArray()),
                                                              _m_RHConfigObj.m_fThermB_Coeff.ToString("F99").TrimEnd("0".ToCharArray()),
                                                              _m_RHConfigObj.m_fThermC_Coeff.ToString("F99").TrimEnd("0".ToCharArray()),
                                                              out bCriticalError,
                                                              out iCriticalErrorCode
                                                             );
                _m_fSetpoint = _m_fDefaultStartTemperature;
                if (bCriticalError)
                {
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].ChangeCartridgeState(CAssayCartridge.eCartridgeStates.Faulted);
                    string strMsg = "Fault. Error (" + iCriticalErrorCode.ToString() + ") initializing Heater.\n";
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeFault(strMsg);
                    _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].enterCartridgeSafeState();
                    throw new System_Defns.CThermalCriticalException(strMsg, iCriticalErrorCode);
                }
            }
        }

        public override void start()
        {
            lock (_m_Lock)
            {
                string strMode = (_m_RHConfigObj.m_uiRHChannel == 0) ? "5" : "6";
                bool bCriticalError;
                int iCriticalErrorCode;
                _m_ThermalAdapterUnit.StartRH(out bCriticalError, out iCriticalErrorCode, _m_RHConfigObj.m_uiRHChannel);
                if (bCriticalError)
                {
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].ChangeCartridgeState(CAssayCartridge.eCartridgeStates.Faulted);
                    string strMsg = "Fault. Error (" + iCriticalErrorCode.ToString() + ") enabling Heater.\n";
                    _m_instr.reportInstrumentFault(strMsg);
                    _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                    _m_instr.enterInstrumentSafeState();
                    throw new System_Defns.CThermalCriticalException(strMsg,iCriticalErrorCode);
                }
                setTemperatureState(eThermalTemperatureState.Ramping_ThermalTempState);
                if (m_strThermalElementName.ToUpper() == "SAMPLEPREPTHERMCONTROL".ToUpper())
                {
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeSamplePrepTemperatureSetChange(m_uiSlotIndex, _m_fSetpoint.ToString());
                }
                _m_bRHRampMonitorStop = false;
                _m_RHRampMonitorTask = new Task(_m_RHRampMonitor, this);  // Construct and start the Heater Ramp monitor task
                _m_RHRampMonitorTask.Start();
            }
        }

        public override void stop()
        {
            bool bIsCriticalError; 
            int iCriticalErrorCode;
            stop(out bIsCriticalError, out iCriticalErrorCode);
        }

        override public void stop(out bool bIsCriticalError, out int iCriticalErrorCode)
        {
            string sresult = "";
            try
            {
                {
                    sresult = _m_ThermalAdapterUnit.StopRH(out bIsCriticalError, out iCriticalErrorCode, _m_RHConfigObj.m_uiRHChannel);
                    if (_m_ThermalRampMonitoring)
                    {
                        _m_bRHRampMonitorStop = true;
                        _m_ThermalRampMonitoring = false;
                    }
                    setTemperatureState(eThermalTemperatureState.Idle_ThermalTempState);
                    if (bIsCriticalError || (sresult != ""))
                    {
                        string strMsg = "Fault. Stat[" + sresult + "] CRH_Thermal_Device.stop(" + iCriticalErrorCode.ToString() + ") => error stopping RH" + _m_RHConfigObj.m_uiRHChannel.ToString("00") + ".\n";
                        throw new System_Defns.CThermalCriticalException(strMsg, iCriticalErrorCode);
                    }
                }
                if (m_strThermalElementName.ToUpper() == "SAMPLEPREPTHERMCONTROL".ToUpper())
                {
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeSamplePrepTemperatureSetChange(m_uiSlotIndex, "");
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeSamplePrepTemperatureReadChange(m_uiSlotIndex, _m_fTemperatureReading.ToString(), getTemperatureState());
                }
            }
            catch (CThermalCriticalException CriticalExc)
            {
                _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].ChangeCartridgeState(CAssayCartridge.eCartridgeStates.Faulted);
                string strMsg = "Fault: CRH_Thermal_Device.stop(" + CriticalExc.m_iCriticalErrorCode.ToString() + ") => error stopping RH" + _m_RHConfigObj.m_uiRHChannel.ToString("00") + ".\n";
                _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                //_m_instr.enterInstrumentSafeState();
                if (m_strThermalElementName.ToUpper() == "SAMPLEPREPTHERMCONTROL".ToUpper())
                {
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeSamplePrepTemperatureSetChange(m_uiSlotIndex, "");
                }
                throw;
            }
            catch (CPCRInstrumentSystemException InstrumentExc)
            {
                _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].ChangeCartridgeState(CAssayCartridge.eCartridgeStates.Faulted);
                string strMsg = "Fault: CRH_Thermal_Device.stop() => error stopping RH" + _m_RHConfigObj.m_uiRHChannel.ToString("00") + ".\n";
                _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                //_m_instr.enterInstrumentSafeState();
                if (m_strThermalElementName.ToUpper() == "SAMPLEPREPTHERMCONTROL".ToUpper())
                {
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeSamplePrepTemperatureSetChange(m_uiSlotIndex, "");
                }
                throw;
            }
            catch (Exception Exc)
            {
                _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].ChangeCartridgeState(CAssayCartridge.eCartridgeStates.Faulted);
                string strMsg = "Unexpected exception Fault from CRH_Thermal_Device.stop(" + Exc.ToString() + ") => error stopping TEC" + _m_RHConfigObj.m_uiRHChannel.ToString("00") + ".\n";
                _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                //_m_instr.enterInstrumentSafeState();
                if (m_strThermalElementName.ToUpper() == "SAMPLEPREPTHERMCONTROL".ToUpper())
                {
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeSamplePrepTemperatureSetChange(m_uiSlotIndex, "");
                }
                throw new System_Defns.CPCRInstrumentSystemException(strMsg);
            }
        }

        public override float getTemperature(out bool btranBusy, out bool bControl)
        {
//          lock (_m_Lock)
            {
                string strResult = "";
                string strTemp = "";
                bool bCriticalError;
                int iCriticalErrorCode;
                strResult = _m_ThermalAdapterUnit.GetTemperatureRH(_m_RHConfigObj.m_uiRHChannel.ToString(), out strTemp, out btranBusy, out bControl, out bCriticalError, out iCriticalErrorCode);
                if (bCriticalError || (strResult != ""))
                {
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].ChangeCartridgeState(CAssayCartridge.eCartridgeStates.Faulted);
                    string strMsg = "Fault. Error[" + strResult + "] (" + iCriticalErrorCode.ToString() + ") reading Heater temperature.\n";
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeFault(strMsg);
                    _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].enterCartridgeSafeState();
                    throw new System_Defns.CThermalCriticalException(strMsg, iCriticalErrorCode);
                }
                _m_fTemperatureReading = Convert.ToSingle(strTemp);
                if (m_strThermalElementName.ToUpper() == "SAMPLEPREPTHERMCONTROL".ToUpper())
                {
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeSamplePrepTemperatureReadChange(m_uiSlotIndex, strTemp, getTemperatureState());
                }
                return _m_fTemperatureReading;
            }
        }

        override public void setTemperature(float fSp, string strPrototypeOperationType = "") // Protocol operation type => "" or "STEP" or "MELTCURVE"
        {
            string sresult = "";
            bool bCriticalError;
            int iCriticalErrorCode;
            string stemp = "nan";

            try
            {
                stemp = fSp.ToString();
                while (_m_ThermalRampMonitoring)
                {
                    _m_bRHRampMonitorStop = true;
                    Thread.Sleep(100);
                }
                _m_fSetpoint = fSp;

#if false
                // Determine whether this is a RampUp or a RampDown setpoint, and use appropriate TEC parameters
                bool btranBusy, bControl;
                float fCurrentTemperature;
                fCurrentTemperature = getTemperature(out btranBusy, out bControl);
                if (fCurrentTemperature < Convert.ToSingle(stemp))
                {
                    setRampUpParameters();
                }
                else
                {
                    setRampDownParameters();
                }
#endif

                sresult = _m_ThermalAdapterUnit.SetTemperatureRH(_m_RHConfigObj.m_uiRHChannel.ToString(), stemp, out bCriticalError, out iCriticalErrorCode);
                if (sresult != "")
                {
                    throw new CPCRInstrumentSystemException("Failed setting RH temperature setpoint => \"" + stemp + "\".\n", "Fault");
                }
                _m_bRHRampMonitorStop = false;
                setTemperatureState(eThermalTemperatureState.Ramping_ThermalTempState);
                start();
                _m_RHRampMonitorTask = new Task(_m_RHRampMonitor, this);  // Construct and start the TEC Monitor Ramp task
                _m_RHRampMonitorTask.Start();
                if (m_strThermalElementName.ToUpper() == "SAMPLEPREPTHERMCONTROL".ToUpper())
                {
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeSamplePrepTemperatureSetChange(m_uiSlotIndex, stemp);
                }
            }
            catch (CThermalCriticalException CriticalExc)
            {
                _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].ChangeCartridgeState(CAssayCartridge.eCartridgeStates.Faulted);
                string strMsg = "Fault: CRH_Thermal_Device.setTemperature(" + CriticalExc.m_iCriticalErrorCode.ToString() + ") => error setting RH" + _m_RHConfigObj.m_uiRHChannel.ToString() + " temperature to " + stemp + " C.\n";
                _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].enterCartridgeSafeState();
                if (m_strThermalElementName.ToUpper() == "SAMPLEPREPTHERMCONTROL".ToUpper())
                {
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeSamplePrepTemperatureSetChange(m_uiSlotIndex, stemp);
                }
                throw;
            }
            catch (CPCRInstrumentSystemException InstrumentExc)
            {
                _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].ChangeCartridgeState(CAssayCartridge.eCartridgeStates.Faulted);
                string strMsg = "Fault: CRH_Thermal_Device.setTemperature() => error setting RH" + _m_RHConfigObj.m_uiRHChannel.ToString() + " temperature to " + stemp + " C.\n";
                _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].enterCartridgeSafeState();
                if (m_strThermalElementName.ToUpper() == "SAMPLEPREPTHERMCONTROL".ToUpper())
                {
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeSamplePrepTemperatureSetChange(m_uiSlotIndex, stemp);
                }
                throw;
            }            catch (Exception Exc)
            {
                _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].ChangeCartridgeState(CAssayCartridge.eCartridgeStates.Faulted);
                string strMsg = "Unexpected exception Fault from CRH_Thermal_Device.setTemperature(" + Exc.ToString() + ") => error setting RH" + _m_RHConfigObj.m_uiRHChannel.ToString("00") + " temperature to " + stemp + " C.\n";
                _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeFault(strMsg);
                _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].enterCartridgeSafeState();
                if (m_strThermalElementName.ToUpper() == "SAMPLEPREPTHERMCONTROL".ToUpper())
                {
                    _m_instr.m_PCR_assay_cartridge_instances[m_uiSlotIndex].reportCartridgeSamplePrepTemperatureSetChange(m_uiSlotIndex, stemp);
                }
                throw new System_Defns.CPCRInstrumentSystemException(strMsg);
            }
        }

        private bool _m_bRHRampMonitorStop;
        private Task _m_RHRampMonitorTask;
        private Action<object> _m_RHRampMonitor = (object obj) =>
        {
            string sresult = "";
            string strTemp;
            float fTemp;
            CRH_Thermal_Device pThis = (CRH_Thermal_Device)obj;
            Stopwatch elapsedMonitoringTime = new Stopwatch();
            bool bIsCriticalError = false;
            int iCriticalErrorCode = 0;
            elapsedMonitoringTime.Start();
            try
            {
                // TBD - must add higher granularity sleep so cancellation works better.  Will need to use Stopwatch.
                while ((!pThis._m_bRHRampMonitorStop))
                {
                    // Timeout???
                    if (elapsedMonitoringTime.ElapsedMilliseconds > pThis._m_configObj.m_MiscellaneousConfiguration.m_fThermalRampTimeoutInSeconds * 1000)
                    {
                        pThis.setTemperatureState(eThermalTemperatureState.RampTimedout_ThermalTempState);
                        pThis._m_bRHRampMonitorStop = false;
                        try
                        {
                            pThis.stop(out bIsCriticalError, out iCriticalErrorCode);
                        }
                        catch (Exception Exc)
                        {
                            if (bIsCriticalError)
                            {
                                pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].ChangeCartridgeState(CAssayCartridge.eCartridgeStates.Faulted);
                                string strMsg = "Fault. CRH_Thermal_Device._m_RHRampMonitor(" + iCriticalErrorCode.ToString() + ") => error stopping RH" + pThis._m_RHConfigObj.m_uiRHChannel.ToString() + ".\n";
                                pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].reportCartridgeFault(strMsg);
                                pThis._m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                                pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].enterCartridgeSafeState();
                                throw new System_Defns.CThermalCriticalException(strMsg, iCriticalErrorCode);
                            }
                        }
                        return;
                    }
                    bool btranBusy, bControl;
                    fTemp = pThis.getTemperature(out btranBusy, out bControl);
                    strTemp = fTemp.ToString();
                    if (bControl)
                    {
                        // Ramp successful
                        pThis.setTemperatureState(eThermalTemperatureState.AtSetpoint_ThermalTempState);
                        if (pThis.m_strThermalElementName.ToUpper() == "SAMPLEPREPTHERMCONTROL".ToUpper())
                        {
                            pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].reportCartridgeSamplePrepTemperatureReadChange(pThis.m_uiSlotIndex, strTemp, pThis.getTemperatureState());
                        }
                        pThis._m_bRHRampMonitorStop = false;
                        return;
                    }
                    if (pThis.m_strThermalElementName.ToUpper() == "SAMPLEPREPTHERMCONTROL".ToUpper())
                    {
                        pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].reportCartridgeSamplePrepTemperatureReadChange(pThis.m_uiSlotIndex, strTemp, pThis.getTemperatureState());
                    }
                    Thread.Sleep((int)(pThis._m_configObj.m_MiscellaneousConfiguration.m_fThermalRampSamplePeriodInSeconds * 1000));
                }
                // Ramp cancelled.
                pThis.setTemperatureState(eThermalTemperatureState.Idle_ThermalTempState);
                pThis._m_ThermalRampMonitoring = false;
                if (pThis.m_strThermalElementName.ToUpper() == "SAMPLEPREPTHERMCONTROL".ToUpper())
                {
                    pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].reportCartridgeSamplePrepTemperatureSetChange(pThis.m_uiSlotIndex, "");
                    pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].reportCartridgeSamplePrepTemperatureReadChange(pThis.m_uiSlotIndex, pThis._m_fTemperatureReading.ToString(), pThis.getTemperatureState());
                }
            }
            catch (CThermalCriticalException CriticalExc)
            {
                pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].ChangeCartridgeState(CAssayCartridge.eCartridgeStates.Faulted);
                string strMsg = "Fault: CRH_Thermal_Device._m_RHRampMonitor(" + CriticalExc.m_iCriticalErrorCode.ToString() + ") => error ramping RH" + pThis._m_RHConfigObj.m_uiRHChannel.ToString() + ".\n";
                pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].reportCartridgeFault(strMsg);
                pThis._m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].enterCartridgeSafeState();
            }
            catch (CPCRInstrumentSystemException InstrumentExc)
            {
                pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].ChangeCartridgeState(CAssayCartridge.eCartridgeStates.Faulted);
                pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].reportCartridgeFault(InstrumentExc.ToString());
                pThis._m_instr.GetSystemDebugLogger().Log(InstrumentExc.ToString(), CLogger.LogLevel.Fault);
                string strMsg = "Fault: CTERHC_Thermal_Device._m_RHRampMonitor() => error ramping RH" + pThis._m_RHConfigObj.m_uiRHChannel.ToString() + ".\n";
                pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].reportCartridgeFault(strMsg);
                pThis._m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].enterCartridgeSafeState();
            }
            catch (Exception Exc)
            {
                pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].ChangeCartridgeState(CAssayCartridge.eCartridgeStates.Faulted);
                string strMsg = "Unexpected exception Fault from CRH_Thermal_Device._m_RHRampMonitor(" + Exc.ToString() + ") => error ramping RH" + pThis._m_RHConfigObj.m_uiRHChannel.ToString() + ".\n";
                pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].reportCartridgeFault(strMsg);
                pThis._m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                pThis._m_instr.m_PCR_assay_cartridge_instances[pThis.m_uiSlotIndex].enterCartridgeSafeState();
            }
        };
    }
}
