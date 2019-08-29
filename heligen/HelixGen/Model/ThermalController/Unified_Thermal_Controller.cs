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
using HelixGen;

namespace System_Instruments.Thermal_Controller
{
    public class CUnified_Thermal_Controller
    {
        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();


        private CDigital_PCR_Diagnostic_Instrument _m_instr;
        private CTEC_Thermal_Device[] _m_TEC_Controllers;
        private CSystem_Configuration_File_Reader.CSystem_Configuration_Obj _m_configObj;

        public CUnified_Thermal_Controller(CDigital_PCR_Diagnostic_Instrument instr)
        {
            _m_instr = instr;
            _m_TEC_Controllers = new CTEC_Thermal_Device[CSystem_Defns.cuiTotalTECs];
            for (uint uiChannel = 0; uiChannel < _m_TEC_Controllers.Length; uiChannel++)
            {
                _m_TEC_Controllers[uiChannel] = new CTEC_Thermal_Device(_m_instr, uiChannel);
            }            
        }

        public void open(CSystem_Configuration_File_Reader.CSystem_Configuration_Obj configObj)
        {
            _m_configObj = configObj;
            for (uint uiChannel = 0; uiChannel < _m_TEC_Controllers.Length; uiChannel++)
            {
                _m_TEC_Controllers[uiChannel].open(_m_configObj);
            }
        }

        public void init()
        {
            for (uint uiChannel = 0; uiChannel < _m_TEC_Controllers.Length; uiChannel++)
            {
                _m_TEC_Controllers[uiChannel].init();
            }
        }

        public void start(uint uiChannel)
        {
            _m_TEC_Controllers[uiChannel].start();
        }

        public void start()
        {   // All TEC channels
            for (uint uiChannel = 0; uiChannel < _m_TEC_Controllers.Length; uiChannel++)
            {
                _m_TEC_Controllers[uiChannel].start();
            }
        }

        public void stop(uint uiChannel, out bool bIsCriticalError, out int iCriticalErrorCode)
        {
            _m_TEC_Controllers[uiChannel].stop(out bIsCriticalError, out iCriticalErrorCode);
        }

        public void stop()
        {   // All TEC channels
            bool[] bIsCriticalError = new bool[_m_TEC_Controllers.Length];
            int[] iCriticalErrorCode = new int[_m_TEC_Controllers.Length];

            for (uint uiChannel = 0; uiChannel < _m_TEC_Controllers.Length; uiChannel++)
            {
                _m_TEC_Controllers[uiChannel].stop(out bIsCriticalError[uiChannel], out iCriticalErrorCode[uiChannel]);
            }
        }

        public void setTemperatures(float fSp, bool bUseRangeBasedParameters = true)
        {   // All TEC channels with same set-point
            for (uint uiChannel = 0; uiChannel < _m_TEC_Controllers.Length; uiChannel++)
            {
                _m_TEC_Controllers[uiChannel].setTemperature(fSp, bUseRangeBasedParameters);
            }
        }

        public void setTemperature(float[] fSp, bool bUseRangeBasedParameters = true)
        {    // All TEC channels to separate set-points
            for (uint uiChannel = 0; uiChannel < _m_TEC_Controllers.Length; uiChannel++)
            {
                _m_TEC_Controllers[uiChannel].setTemperature(fSp[uiChannel], bUseRangeBasedParameters);
            }
        }

        public float getTemperature(uint uiChannel, out bool btranBusyCh0, out bool bControlCh0)
        {
            return _m_TEC_Controllers[uiChannel].getTemperature(out btranBusyCh0, out bControlCh0);
        }

        public void getTemperatures(out float[] fTemps, out bool[] btranBusyCh0, out bool[] bControlCh0)
        {   // All TEC channels
            fTemps = new float[_m_TEC_Controllers.Length];
            btranBusyCh0 = new bool[_m_TEC_Controllers.Length];
            bControlCh0 = new bool[_m_TEC_Controllers.Length];
            for (uint uiChannel = 0; uiChannel < _m_TEC_Controllers.Length; uiChannel++)
            {
                fTemps[uiChannel] = _m_TEC_Controllers[uiChannel].getTemperature(out btranBusyCh0[uiChannel], out bControlCh0[uiChannel]);
            }
        }

        public eThermalTemperatureState getTemperatureState(uint uiChannel)
        {
            return _m_TEC_Controllers[uiChannel].getTemperatureState();
        }

        public void getTemperatureStates(out eThermalTemperatureState[] states)
        {
            states = new eThermalTemperatureState[_m_TEC_Controllers.Length];
            for (uint uiChannel = 0; uiChannel < _m_TEC_Controllers.Length; uiChannel++)
            {
                states[uiChannel] = _m_TEC_Controllers[uiChannel].getTemperatureState();
            }
        }

        public bool IsTemperatureRampTimedout(uint uiChannel)
        {
            return _m_TEC_Controllers[uiChannel].IsTemperatureRampTimedout();
        }

        public bool IsTemperatureRampTimedout()
        {   // Is any TEC channel ramp timed-out 
            bool bResult = false;
            for (uint uiChannel = 0; uiChannel < _m_TEC_Controllers.Length; uiChannel++)
            {
                bResult = bResult || _m_TEC_Controllers[uiChannel].IsTemperatureRampTimedout();
            }
            return bResult;
        }

        public bool IsTemperaturatSetpoint(uint uiChannel)
        {
            return _m_TEC_Controllers[uiChannel].IsTemperaturatSetpoint();
        }

        public bool IsTemperaturatSetpoint()
        {   // All TEC channels are at temperature set-point
            bool bResult = true;
            for (uint uiChannel = 0; uiChannel < _m_TEC_Controllers.Length; uiChannel++)
            {
                bResult = bResult && _m_TEC_Controllers[uiChannel].IsTemperaturatSetpoint();
            }
            return bResult;
        }

        public float getTemperatureSetpoint(uint uiChannel)
        {
            return _m_TEC_Controllers[uiChannel].getTemperatureSetpoint();
        }

        public void getTemperatureSetpoints(out float[] fSetpoints)
        {   // All TEC channels
            fSetpoints = new float[_m_TEC_Controllers.Length];
            for (uint uiChannel = 0; uiChannel < _m_TEC_Controllers.Length; uiChannel++)
            {
                fSetpoints[uiChannel] = _m_TEC_Controllers[uiChannel].getTemperatureSetpoint();
            }
            return;
        }

        public float getPower(uint uiChannel)
        {
            return _m_TEC_Controllers[uiChannel].getPower();
        }

        public void getPowers(out float[] fPowers)
        {   // All TEC channels
            fPowers = new float[_m_TEC_Controllers.Length];
            for (uint uiChannel = 0; uiChannel < _m_TEC_Controllers.Length; uiChannel++)
            {
                fPowers[uiChannel] = _m_TEC_Controllers[uiChannel].getPower();
            }
            return;
        }

        public float getCurrentOutput(uint uiChannel)
        {
            return _m_TEC_Controllers[uiChannel].getPower();
        }

        public void getCurrentOutputs(out float[] fCurrents)
        {   // All TEC channels
            fCurrents = new float[_m_TEC_Controllers.Length];
            for (uint uiChannel = 0; uiChannel < _m_TEC_Controllers.Length; uiChannel++)
            {
                fCurrents[uiChannel] =  _m_TEC_Controllers[uiChannel].getPower();
            }
            return;
        }

        public void loadInitialParams(uint uiChannel)
        {
            _m_TEC_Controllers[uiChannel].loadInitialParams();
        }

        public void loadInitialParams()
        {   // All TEC channels
            for (uint uiChannel = 0; uiChannel < _m_TEC_Controllers.Length; uiChannel++)
            {
                _m_TEC_Controllers[uiChannel].loadInitialParams();
            }
        }

        private void loadRampDirectionalParameters(uint   uiChannel,
                                                   string overshootOffset,
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
            _m_TEC_Controllers[uiChannel].loadRampDirectionalParameters(overshootOffset,
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
                                                                        lowClamp
                                                                       );
        }
    }
}
