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
    public class CRH_Thermal_Monitoring_Device : CThermal_Device
    {
        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();


        private CSystem_Configuration_File_Reader.CPCR_Thermal_Block_Fan_Control_Thermistor_Configuration _m_RHMonitorConfigObj;

        public CRH_Thermal_Monitoring_Device(CDigital_PCR_Diagnostic_Instrument instr) 
        {
            _m_Lock = new object();
            _m_instr = instr;
        }

        public override void open(CSystem_Configuration_File_Reader.CSystem_Configuration_Obj configObj)
        {
            lock (_m_Lock)
            {
                if (!_m_isOpen)
                {
                    base.open(configObj);
                    _m_isOpen = true;
                    _m_RHMonitorConfigObj = _m_configObj.m_PCR_Thermal_Block_Fan_Control_Thermistor_Configuration;
                }
                else
                {
                    throw new CPCRInstrumentSystemException("CRH_Thermal_Monitoring_Device can't be opened while currently open.", "Fault");
                }
            }
        }

        public override void init()
        {
            lock (_m_Lock)
            {
                LoadInitialParametersMonitorOnlyRH(_m_RHMonitorConfigObj.m_uiHeaterChannel.ToString(),
                                                                         _m_RHMonitorConfigObj.m_fThermA_Coeff.ToString("F99").TrimEnd("0".ToCharArray()),
                                                                         _m_RHMonitorConfigObj.m_fThermB_Coeff.ToString("F99").TrimEnd("0".ToCharArray()),
                                                                         _m_RHMonitorConfigObj.m_fThermC_Coeff.ToString("F99").TrimEnd("0".ToCharArray())
                                                                        );
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
                strResult = _m_ThermalAdapterUnit.GetTemperatureRH(_m_RHMonitorConfigObj.m_uiHeaterChannel.ToString(), out strTemp, out btranBusy, out bControl, out bCriticalError, out iCriticalErrorCode);
                if (bCriticalError || (strResult != ""))
                {
                    string strMsg = "Fault. Error[" + strResult + "] (" + iCriticalErrorCode.ToString() + ") reading Heater temperature.\n";
                    _m_instr.reportInstrumentFault(strMsg);
                    _m_instr.GetSystemDebugLogger().Log(strMsg, CLogger.LogLevel.Fault);
                    throw new System_Defns.CThermalCriticalException(strMsg, iCriticalErrorCode);
                }
                _m_fTemperatureReading = Convert.ToSingle(strTemp);
                _m_instr.reportHeatsinkTemperatureReadChange(strTemp);
                return _m_fTemperatureReading;
            }
        }

        public override void LoadInitialParametersMonitorOnlyRH(string rh_channel,
                                                               string thermistor_Coefficent_A,
                                                               string thermistor_Coefficent_B,
                                                               string thermistor_Coefficent_C
                                                              )
        {
            bool bCriticalError;
            int iCriticalErrorCode;

            _m_ThermalAdapterUnit.LoadInitialParametersMonitorOnlyRH(rh_channel, thermistor_Coefficent_A, thermistor_Coefficent_B, thermistor_Coefficent_C, out bCriticalError, out iCriticalErrorCode);
        }

    }
}
