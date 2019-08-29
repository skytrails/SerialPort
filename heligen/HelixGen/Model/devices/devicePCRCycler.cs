// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using Accel.HeaterBoard;
using HelixGen.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ABot2;

namespace HelixGen.Model.devices
{
    public class devicePCRCycler
    {
        const double NO_SETPOINT = -999;

        /// <summary>
        /// Reference to the lower level tec board.
        /// </summary>
        protected clsTec _tecBoard;

        /// <summary>
        /// The channel used by this heater on the board.
        /// </summary>
        protected int _nChannel;

        /// <summary>
        /// The current setPoint temperature.
        /// </summary>
        protected double[] _setPoint;
        protected double[] _prev_setPoint;

        /// <summary>
        /// Convenient reference to the model.
        /// </summary>
        protected HelixGenModel _theModel;

        /// <summary>
        /// A copy of the TEC config for this channel.
        /// </summary>
        protected Configuration.CTEC_Channel_Configuration _tecConfig;

        /// <summary>
        /// The current low and high temp ranges we're targeting.
        /// </summary>
        protected double[] _rampingLowTemp;
        protected double[] _rampingHighTemp;

        /// <summary>
        /// True if we're at temperature (according to the controller).
        /// </summary>
        protected bool[] _bAtTemp;

        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public devicePCRCycler(tecBoardModel modelIn, Configuration.CTEC_Channel_Configuration tecConfig)
        {
            _setPoint = new double[6];
            _prev_setPoint = new double[6];
            _rampingLowTemp = new double[6];
            _rampingHighTemp = new double[6];
            _bAtTemp = new bool[6];

            _theModel = ((HelixGen.App)(App.Current)).Model;

            _tecBoard = modelIn.theBoard;
            for (int ndx = 0; ndx < 6; ndx++)
            {
                _setPoint[ndx] = _tecBoard.GetTemperature(ndx, 0);
                 _prev_setPoint[ndx] = 0;
                //_prev_setPoint[ndx] = _tecBoard.GetTemperature(ndx, 0);
                _rampingLowTemp[ndx] = NO_SETPOINT;
                _rampingHighTemp[ndx] = NO_SETPOINT;
                _bAtTemp[ndx] = false;
                _theModel.bUnderTempPlan[ndx] = true;

            }
            
            _nChannel = (int)(tecConfig.m_uiTECAddress) - 1;
            _tecConfig = tecConfig;

            // Stash a reference to the model while we're at it.

        }

        public int initialize()
        {
            int nRetVal = 0;
            return nRetVal;
        }

        private void DevicePCRCycler_ThermalProgress(Accel.IHeater sender, Accel.ThermalProgressArgs e)
        {
            logger.Debug("PCRCycler thermal progress; isControlling: {0} isTransitioning: {1}", e.isControlling.ToString(), e.isTransitioning.ToString());
        }

        private void DevicePCRCycler_ThermalProgress2(Accel.IHeater sender, Accel.ThermalProgressArgs e)
        {
            logger.Debug("PCRCycler thermal progress2; isControlling: {0} isTransitioning: {1}", e.isControlling.ToString(), e.isTransitioning.ToString());
        }

        private void DevicePCRCycler_ErrorReceived(Accel.INamed sender, ErrEventArgs e)
        {
            logger.Debug("PCRCycler Got an error; {0}", e.Error.ToString());
        }

        private void DevicePCRCycler_ErrorReceived2(Accel.INamed sender, ErrEventArgs e)
        {
            logger.Debug("PCRCycler Got an error2; {0}", e.Error.ToString());
        }


        /// <summary>
        /// Sets the temperature parameters to the appropriate settings for this temperature band.
        /// </summary>
        /// <param name="curTemp"></param>
        /// <param name="rangeList"></param>
        public void SetToAppropriateBand(int nChannel, float curTemp, double toTemp, List<HelixGen.Model.Configuration.CTEC_From_Temperature_PID_Element> rangeList)
        {
            //logger.Debug("DevicePCRCycler SetToAppropriateBand starting");

            // If our target is outside our current targets.

            if (!_theModel.bUnderTempPlan[nChannel] && !_theModel.bStopScript)
            {
                logger.Debug("DevicePCRCycler SetToAppropriateBand, finding a new band, curTemp is {0} to temp is {1}", curTemp.ToString(),
                    toTemp.ToString());
                // Find a new plan.
                HelixGen.Model.Configuration.CTEC_HW_Ramping_Configuration rampingConfig = SelectThermalParameters(curTemp, (float)toTemp, rangeList);

                if (rampingConfig != null)
                {
                    SetChannelSettings(nChannel, rampingConfig);

                   // _theModel.tecBoard.theBoard.SetFanDutyCycle(100);
                   // _theModel.tecBoard.theBoard.EnableSystemFan(true);
                   // _theModel.tecBoard.theBoard.EnableTECFan(true);

                    // Record the range we've currently set up.

                    _rampingLowTemp[nChannel] = rampingConfig.m_TemperatureRange.m_fLowTemperature;
                    _rampingHighTemp[nChannel] = rampingConfig.m_TemperatureRange.m_fHighTemperature;

                    _theModel.bUnderTempPlan[nChannel] = true;
                }
                else
                {
                    // TBD; there is a default somewhere.
                }
            }

            //logger.Debug("DevicePCRCycler SetToAppropriateBand exitting");
        }

        /// <summary>
        /// Selects the PID parameters for the specified temperature range.
        /// </summary>
        /// <returns></returns>
        public HelixGen.Model.Configuration.CTEC_HW_Ramping_Configuration SelectThermalParameters(float fromTemp, float toTemp, List<HelixGen.Model.Configuration.CTEC_From_Temperature_PID_Element> rangeList)
        {
            HelixGen.Model.Configuration.CTEC_HW_Ramping_Configuration configOut = null;

            foreach (HelixGen.Model.Configuration.CTEC_From_Temperature_PID_Element element in rangeList)
            {
                if (fromTemp >= element.m_TemperatureRange.m_fLowTemperature &&
                    fromTemp <= element.m_TemperatureRange.m_fHighTemperature
                    )
                {
                    foreach (HelixGen.Model.Configuration.CTEC_HW_Ramping_Configuration rampingConfig in element.m_ToTemperatures)
                    {
                        if (toTemp >= rampingConfig.m_TemperatureRange.m_fLowTemperature &&
                            toTemp <= rampingConfig.m_TemperatureRange.m_fHighTemperature)
                        {
                            configOut = rampingConfig;

                            logger.Debug("DevicePCRCycler SelectThermalParameters, found a plan; curTemp is {4} making settings for the temperature range: From: {0}-{1} To: {2}-{3}",
                                 element.m_TemperatureRange.m_fLowTemperature.ToString(),
                                 element.m_TemperatureRange.m_fHighTemperature.ToString(),
                                 configOut.m_TemperatureRange.m_fLowTemperature.ToString(),
                                 configOut.m_TemperatureRange.m_fHighTemperature.ToString(),
                                 fromTemp.ToString()
                                 );
                        }
                    }
                }
            }

            // Hopefully we've got our plan now.

            if (configOut == null)
            {
                logger.Debug("DevicePCRCycler SelectThermalParameters did not find a plan for {0} to {1}.",
                    fromTemp.ToString(), toTemp.ToString());
            }

            return configOut;
        }

#if false
        /// <summary>
        /// Selects the PID parameters for the specified temperature range.
        /// </summary>
        /// <returns></returns>
        public HelixGen.Model.Configuration.CTEC_HW_Ramping_Configuration SelectThermalParameters(float fromTemp, float toTemp, List<HelixGen.Model.Configuration.CTEC_From_Temperature_PID_Element> rangeList)
        {
            HelixGen.Model.Configuration.CTEC_HW_Ramping_Configuration configOut = null;

            // Scan one, locate the plan that has the tightest from range.

            float fromRangeDelta = -1;
            int ndxRange = -1;
            int ndx = 0;

            foreach (HelixGen.Model.Configuration.CTEC_From_Temperature_PID_Element element in rangeList)
            {
                if (fromTemp >= element.m_TemperatureRange.m_fLowTemperature &&
                    fromTemp <= element.m_TemperatureRange.m_fHighTemperature
                    )
                {
                    if (fromRangeDelta == -1)
                    {
                        // Grab this one if it's the first one.

                        fromRangeDelta = Math.Abs(element.m_TemperatureRange.m_fHighTemperature - element.m_TemperatureRange.m_fLowTemperature);
                        ndxRange = ndx;
                    }
                    else
                    {
                        // Use this one if it's tighter.

                        float localFromRangeDelta = Math.Abs(element.m_TemperatureRange.m_fHighTemperature - element.m_TemperatureRange.m_fLowTemperature);

                        if (localFromRangeDelta < fromRangeDelta)
                        {
                            fromRangeDelta = localFromRangeDelta;
                            ndxRange = ndx;
                        }
                    }
                }

                ndx++;
            }

            // If we found a range.

            if (ndxRange != -1)
            {
                HelixGen.Model.Configuration.CTEC_From_Temperature_PID_Element configElement = rangeList[ndxRange];

                // Find the tightest to range.

                float toRangeDelta = -1;
                int ndxToRange = -1;
                ndx = 0;

                foreach (HelixGen.Model.Configuration.CTEC_HW_Ramping_Configuration rampingConfig in configElement.m_ToTemperatures)
                {
                    if (toTemp >= rampingConfig.m_TemperatureRange.m_fLowTemperature &&
                        toTemp <= rampingConfig.m_TemperatureRange.m_fHighTemperature)
                    {
                        if (toRangeDelta == -1)
                        {
                            toRangeDelta = Math.Abs(rampingConfig.m_TemperatureRange.m_fHighTemperature - rampingConfig.m_TemperatureRange.m_fLowTemperature);
                            ndxToRange = ndx;
                        }
                        else
                        {
                            float localToRangeDelta = Math.Abs(rampingConfig.m_TemperatureRange.m_fHighTemperature - rampingConfig.m_TemperatureRange.m_fLowTemperature);

                            if (localToRangeDelta < toRangeDelta)
                            {
                                toRangeDelta = Math.Abs(rampingConfig.m_TemperatureRange.m_fHighTemperature - rampingConfig.m_TemperatureRange.m_fLowTemperature);
                                ndxToRange = ndx;
                            }
                        }
                    }

                    ndx++;
                }

                // Hopefully we've got our plan now.

                if (ndxToRange != -1)
                {
                    configOut = configElement.m_ToTemperatures[ndxToRange];

                    logger.Debug("DevicePCRCycler SelectThermalParameters, found a plan; curTemp is {4} making settings for the temperature range: From: {0}-{1} To: {2}-{3}",
                         configElement.m_TemperatureRange.m_fLowTemperature.ToString(),
                         configElement.m_TemperatureRange.m_fHighTemperature.ToString(),
                         configOut.m_TemperatureRange.m_fLowTemperature.ToString(),
                         configOut.m_TemperatureRange.m_fHighTemperature.ToString(),
                         fromTemp.ToString()
                         );
                }
                else
                {
                    logger.Debug("DevicePCRCycler SelectThermalParameters did not find a plan for {0} to {1}.",
                        fromTemp.ToString(), toTemp.ToString());
                }
            }

            return configOut;
        }
#endif


        /// <summary>
        /// Takes the device to the specified temperature and holds it there.
        /// </summary>
        /// <param name="temperature">The desired temperature.</param>
        /// <param name="millisecondsAtTemp">Milliseconds at the temperature.</param>
        public void gotoTempAndStay(int nController, double temperature, int millisecondsAtTemp)
        {
            logger.Debug("gotoTempAndStay: temperature = {0} time = {1} ms", temperature.ToString(), millisecondsAtTemp.ToString());

            _theModel.ProtocolDescLine = string.Format("  setting temperature to {0}", temperature.ToString());

            //_theModel.DevicePCRCycler.Temperature = temperature;
           // Temperature = temperature;
            this[nController] = temperature;
            this[nController-1] = temperature;
            //_theModel.DevicePCRCycler.Temperature2 = temperature;

            // Wait for the device to reach the appointed temperature.

            if (temperature != 0)
            {
                bool bControlling0Tec1 = _theModel.tecBoard.theBoard.Control0[nController];
                bool bTransitioning0Tec1 = _theModel.tecBoard.theBoard.TranBusy0[nController];
               // bool bControlling0Tec2 = _theModel.tecBoard.theBoard.Control0[nController + 1];
                //bool bTransitioning0Tec2 = _theModel.tecBoard.theBoard.TranBusy0[nController + 1];

                while (!_theModel.bStopScript &&
                    !((bControlling0Tec1 && !bTransitioning0Tec1) 
                    //&&(bControlling0Tec2 && !bTransitioning0Tec2)
                    )
                    )
                {
                    bControlling0Tec1 = _theModel.tecBoard.theBoard.Control0[nController];
                    bTransitioning0Tec1 = _theModel.tecBoard.theBoard.TranBusy0[nController];
                   // bControlling0Tec2 = _theModel.tecBoard.theBoard.Control0[nController + 1];
                   // bTransitioning0Tec2 = _theModel.tecBoard.theBoard.TranBusy0[nController + 1];
                    Thread.Sleep(100);//1000 to 100
                }

                // We're at temperature.  Clear out the information on the current plan.

                _rampingLowTemp[nController] = double.MaxValue;
                _rampingHighTemp[nController] = double.MinValue;
                _bAtTemp[nController] = true;

                _rampingLowTemp[nController + 1] = double.MaxValue;
                _rampingHighTemp[nController + 1] = double.MinValue;
                _bAtTemp[nController + 1] = true;

                logger.Debug("At temperature.");
            }

            _theModel.ProtocolDescLine = string.Format("  at temperature; {0}", temperature.ToString());

            logger.Debug("gotoTempAndStay: at temperature = {0}, sleeping ...", temperature.ToString());
        }

        public void gotoTemp(int nController, double temperature, int millisecondsAtTemp)
        {
            logger.Debug("gotoTempAndStay: temperature = {0} time = {1} ms", temperature.ToString(), millisecondsAtTemp.ToString());

            _theModel.ProtocolDescLine = string.Format("  over temperature to {0}", temperature.ToString());

           // _theModel.DevicePCRCycler.Temperature = temperature;
            //_theModel.DevicePCRCycler.Temperature2 = temperature;

            // Wait for the device to reach the appointed temperature.

            if (temperature != 0)
            {
                float temp = 0;

                temp = _tecBoard.GetTemperature(nController, 0);
                //_theModel.DevicePCRCycler.Temperature = temperature;
                this[nController] = temperature;
                this[nController-1] = temperature;
                if (temp > temperature)
                 {
                    while (temp > (temperature+10))
                    {
                      temp = _tecBoard.GetTemperature(nController, 0);
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    while (temp < (temperature-2))
                    {
                        temp = _tecBoard.GetTemperature(nController, 0);
                        Thread.Sleep(1000);
                    }

                }

                // We're at temperature.  Clear out the information on the current plan.
                /*
                _rampingLowTemp[nController] = double.MaxValue;
                _rampingHighTemp[nController] = double.MinValue;
                _bAtTemp[nController] = true;

                _rampingLowTemp[nController + 1] = double.MaxValue;
                _rampingHighTemp[nController + 1] = double.MinValue;
                _bAtTemp[nController + 1] = true;

                logger.Debug("At temperature.");*/
            }

            _theModel.ProtocolDescLine = string.Format("  at temperature; {0}", temperature.ToString());

            logger.Debug("gotoTempAndStay: at temperature = {0}, sleeping ...", temperature.ToString());
        }

        protected void SetChannelSettings(int channelIn, HelixGen.Model.Configuration.CTEC_HW_Ramping_Configuration rampingConfigIn)
        {
            logger.Debug("SetChannelSettings; Channel: {0} OvershootOffset: {1}, OvershootDuration: {2} P: {3} I: {4} D: {5}",
                channelIn.ToString(),
                rampingConfigIn.m_fOvershootOffset.ToString(),
                rampingConfigIn.m_uiOvershootDuration.ToString(),
                rampingConfigIn.m_PID_Settings[0].m_fPGain.ToString(),
                rampingConfigIn.m_PID_Settings[0].m_fIGain.ToString(),
                rampingConfigIn.m_PID_Settings[0].m_fDGain.ToString()
                );

            _theModel.tecBoard.theBoard.SetSampleTime(channelIn, 0.1f);

            _theModel.tecBoard.theBoard.SetOvershootDuration(channelIn, 0, rampingConfigIn.m_uiOvershootDuration);
            _theModel.tecBoard.theBoard.SetOvershootOffset(channelIn, 0, rampingConfigIn.m_fOvershootOffset);

            _theModel.tecBoard.theBoard.SetOvershootDuration(channelIn, 1, rampingConfigIn.m_uiOvershootDuration);
            _theModel.tecBoard.theBoard.SetOvershootOffset(channelIn, 1, rampingConfigIn.m_fOvershootOffset);

            _theModel.tecBoard.theBoard.SetSetPointOffset(channelIn, rampingConfigIn.m_fSetpointOffset);

            _theModel.tecBoard.theBoard.TECSetPID_D(channelIn, 0, rampingConfigIn.m_PID_Settings[0].m_fDGain);
            _theModel.tecBoard.theBoard.TECSetPID_I(channelIn, 0, rampingConfigIn.m_PID_Settings[0].m_fIGain);
            _theModel.tecBoard.theBoard.TECSetPID_P(channelIn, 0, rampingConfigIn.m_PID_Settings[0].m_fPGain);
            _theModel.tecBoard.theBoard.SetRabbitGain(channelIn, 0, rampingConfigIn.m_PID_Settings[0].m_fRabbitGain);
            _theModel.tecBoard.theBoard.SetRabbitGain2(channelIn, 0, rampingConfigIn.m_PID_Settings[0].m_fRabbitGain2);
            _theModel.tecBoard.theBoard.SetRabbitGainDeriv(channelIn, 0, rampingConfigIn.m_PID_Settings[0].m_fRabbitDerivGain);

            _theModel.tecBoard.theBoard.TECSetPID_D(channelIn, 1, rampingConfigIn.m_PID_Settings[1].m_fDGain);
            _theModel.tecBoard.theBoard.TECSetPID_I(channelIn, 1, rampingConfigIn.m_PID_Settings[1].m_fIGain);
            _theModel.tecBoard.theBoard.TECSetPID_P(channelIn, 1, rampingConfigIn.m_PID_Settings[1].m_fPGain);
            _theModel.tecBoard.theBoard.SetRabbitGain(channelIn, 1, rampingConfigIn.m_PID_Settings[1].m_fRabbitGain);
            _theModel.tecBoard.theBoard.SetRabbitGain2(channelIn, 1, rampingConfigIn.m_PID_Settings[1].m_fRabbitGain2);
            _theModel.tecBoard.theBoard.SetRabbitGainDeriv(channelIn, 1, rampingConfigIn.m_PID_Settings[1].m_fRabbitDerivGain);

            _theModel.tecBoard.theBoard.SetErrorTermBand(channelIn, 50);
            _theModel.tecBoard.theBoard.SetErrorTermCounts(channelIn, 255);
            _theModel.tecBoard.theBoard.SetPBand(channelIn, 6);
            _theModel.tecBoard.theBoard.SetPowerLimitCounts(channelIn, 255);
            _theModel.tecBoard.theBoard.SetTempControlMode(channelIn, 1);
            _theModel.tecBoard.theBoard.SetLowClamp(channelIn, -100);
            _theModel.tecBoard.theBoard.SetHighClamp(channelIn, 300);

        }

        /// <summary>
        /// The current device temperature.
        /// </summary>
        /// <remarks>
        /// Note that this function actually queries the device, rather
        /// than returning the last reported temperature.
        /// </remarks>

        public double Temperature
        {
            get
            {
                return _tecBoard.GetTemperature(_nChannel, 0);
            }

            set
            {
                logger.Debug("Setting the temperature to {0}", value.ToString());

                if (value < _setPoint[_nChannel])
                {
                    _theModel.tecBoard.theBoard.SetFanDutyCycle(100);
                    _theModel.tecBoard.theBoard.EnableSystemFan(true);
                    _theModel.tecBoard.theBoard.EnableTECFan(true);
                }
                else
                {
                    // _theModel.tecBoard.theBoard.SetFanDutyCycle(100);
                    // _theModel.tecBoard.theBoard.EnableSystemFan(true);
                    _theModel.tecBoard.theBoard.EnableTECFan(false);
                }

                _prev_setPoint[_nChannel] = _setPoint[_nChannel];
                _setPoint[_nChannel] = value;
                _bAtTemp[_nChannel] = false;
                _theModel.bUnderTempPlan[_nChannel] = false;
                _theModel.UpdatedPCRCyclerSetpointTemperature(_setPoint[_nChannel]);

                _tecBoard.SetSetPoint(_nChannel, (float)value);

                if (!_theModel.bRamping)
                {
                    
                    if (_rampingLowTemp[_nChannel] == NO_SETPOINT ||
                     _rampingHighTemp[_nChannel] == NO_SETPOINT ||
                     _setPoint[_nChannel] < _rampingLowTemp[_nChannel] ||
                     _setPoint[_nChannel] > _rampingHighTemp[_nChannel]
                     )
                    {
                        float presetValue=0;
                        double setValue=0;
                        if(_prev_setPoint[_nChannel] < _setPoint[_nChannel])
                        {
                            setValue = 95;
                            presetValue = (float)_prev_setPoint[_nChannel];
                        }
                        else
                        {
                            presetValue = 95;
                            setValue = _setPoint[_nChannel];
                        }
                        SetToAppropriateBand(_nChannel, presetValue,
                           setValue, _theModel.Config.m_TEC_Channel_Configurations["TEC_5"].m_Step_PID_RampUp_Range_List);
                    }

                    _tecBoard.SetTempControlMode(_nChannel, 1);
                }
                
            }
        }

        public double this[int nchannel]
        {
            get
            {
                return _tecBoard.GetTemperature(nchannel, 0);
            }

            set
            {
                logger.Debug("Setting the temperature to {0}", value.ToString());

                if (value < _setPoint[nchannel])
                {
                    _theModel.tecBoard.theBoard.SetFanDutyCycle(100);
                    _theModel.tecBoard.theBoard.EnableSystemFan(true);
                    _theModel.tecBoard.theBoard.EnableTECFan(true);
                }
                else
                {
                    // _theModel.tecBoard.theBoard.SetFanDutyCycle(100);
                    // _theModel.tecBoard.theBoard.EnableSystemFan(true);
                    _theModel.tecBoard.theBoard.EnableTECFan(false);
                }

                _prev_setPoint[nchannel] = _setPoint[nchannel];
                _setPoint[nchannel] = value;
                _bAtTemp[nchannel] = false;
                _theModel.bUnderTempPlan[nchannel] = false;
                _theModel.UpdatedPCRCyclerSetpointTemperature(_setPoint[nchannel]);

                _tecBoard.SetSetPoint(nchannel, (float)value);

                if (!_theModel.bRamping)
                {

                    if (_rampingLowTemp[nchannel] == NO_SETPOINT ||
                     _rampingHighTemp[nchannel] == NO_SETPOINT ||
                     _setPoint[nchannel] < _rampingLowTemp[nchannel] ||
                     _setPoint[nchannel] > _rampingHighTemp[nchannel]
                     )
                    {
                        float presetValue = 0;
                        double setValue = 0;
                        if (_prev_setPoint[nchannel] < _setPoint[nchannel])
                        {
                            setValue = 95;
                            presetValue = (float)_prev_setPoint[nchannel];
                        }
                        else
                        {
                            presetValue = 95;
                            setValue = _setPoint[nchannel];
                        }
                        SetToAppropriateBand(nchannel, presetValue,

                           setValue, _theModel.Config.m_TEC_Channel_Configurations["TEC_5"].m_Step_PID_RampUp_Range_List);
                    }

                    _tecBoard.SetTempControlMode(nchannel, 1);
                }

            }
        }

        /// <summary>
        /// The current device temperature.
        /// </summary>
        /// <remarks>
        /// Note that this function actually queries the device, rather
        /// than returning the last reported temperature.
        /// </remarks>
        public double Temperature2
        {
            get
            {
                return _tecBoard.GetTemperature(_nChannel + 1, 0);
            }

            set
            {
                logger.Debug("Setting the temperature2 to {0}", value.ToString());
                _prev_setPoint[1] = _setPoint[1];
                _setPoint[1] = value;
                _bAtTemp[1] = false;
                _theModel.bUnderTempPlan[1] = false;
                _theModel.UpdatedPCRCyclerSetpointTemperature(_setPoint[1]);

                _tecBoard.SetSetPoint(_nChannel + 1, (float)value);

                if (!_theModel.bRamping)
                {
                    if (_rampingLowTemp[1] == NO_SETPOINT ||
                     _rampingHighTemp[1] == NO_SETPOINT ||
                     _setPoint[1] < _rampingLowTemp[1] ||
                     _setPoint[1] > _rampingHighTemp[1]
                     )
                    {
                        SetToAppropriateBand(_nChannel + 1, (float)_prev_setPoint[1],
                           _setPoint[1], _theModel.Config.m_TEC_Channel_Configurations["TEC_1"].m_Step_PID_RampUp_Range_List);
                    }

                    _tecBoard.SetTempControlMode(_nChannel + 1, 1);
                }

                _theModel.tecBoard.theBoard.SetFanDutyCycle(100);
                _theModel.tecBoard.theBoard.EnableSystemFan(true);
                _theModel.tecBoard.theBoard.EnableTECFan(true);
            }
        }
    }
}
