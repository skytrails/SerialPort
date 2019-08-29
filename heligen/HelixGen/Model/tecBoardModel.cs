// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using ABot2;
using Accel.HeaterBoard;
using HelixGen.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelixGen.Model
{
    public class tecBoardModel : IDisposable
    {
        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// A reference back to the parent model.
        /// </summary>
        protected HelixGenModel _model;

        /// <summary>
        /// Reference to the actual tec board.
        /// </summary>
        protected clsTec _tecBoard;

        public tecBoardModel(HelixGenModel modelIn)
        {
            _model = modelIn;
            _tecBoard = new clsTec();
        }

        public async Task<bool> Initialize()
        {
            bool bResult = false;

            try
            {
                BoardCfgSec config = new BoardCfgSec();

                // Bridge the configuration fields to the corresponding TEC configuration
                // fields.

                config.Port = _model.Config.m_ThermalControllerConfigurations["ACCEL-TEC-CNTRL-1"].m_strPort;

                _tecBoard.Initialize(config.Port);

                foreach(KeyValuePair<string, Configuration.CTEC_Channel_Configuration> item in _model.Config.m_TEC_Channel_Configurations)
                {
                    int controller = (int)item.Value.m_uiTECAddress - 1;

                    clsTec.TecParameters tecCfgElement = new clsTec.TecParameters();

                    tecCfgElement.SampleTime = item.Value.m_ControlPIDSampleTimeInSeconds;
                    tecCfgElement.ErrorBand = 50; // item.Value.m_fErrorTermBand;
                    tecCfgElement.ErrorCountLimit = 255; // (int)item.Value.m_fErrorTermCount;
                    tecCfgElement.PowerLimitHigh = 100;  //item.Value.m_fSteadyStatePowerLimit;
                    tecCfgElement.PowerLimitCount = 255; //  (int)item.Value.m_fSteadyStatePowerLimitCount;
                    tecCfgElement.sheq_A = item.Value.m_fThermA_Coeff;
                    tecCfgElement.sheq_B = item.Value.m_fThermB_Coeff;
                    tecCfgElement.sheq_C = item.Value.m_fThermC_Coeff;
                    tecCfgElement.Propband = item.Value.m_Step_PID_RampUp_Default.m_fPBand;
                    tecCfgElement.SetpointOffset = item.Value.m_Step_PID_RampUp_Default.m_fSetpointOffset;
                    tecCfgElement.HeatPid_P = item.Value.m_Step_PID_RampUp_Default.m_PID_Settings[0].m_fPGain;
                    tecCfgElement.HeatPid_I = item.Value.m_Step_PID_RampUp_Default.m_PID_Settings[0].m_fIGain;
                    tecCfgElement.HeatPid_D = item.Value.m_Step_PID_RampUp_Default.m_PID_Settings[0].m_fDGain;
                    tecCfgElement.CoolPid_P = item.Value.m_Step_PID_RampUp_Default.m_PID_Settings[0].m_fPGain;
                    tecCfgElement.CoolPid_I = item.Value.m_Step_PID_RampUp_Default.m_PID_Settings[0].m_fIGain;
                    tecCfgElement.CoolPid_D = item.Value.m_Step_PID_RampUp_Default.m_PID_Settings[0].m_fDGain;

                    _tecBoard.LoadParametersIntoTec(controller, tecCfgElement);
                }

                bResult = true;
            }
            catch (Exception ex)
            {
                logger.Error("tecBoardModel.Initialize caught an exception; {0}", ex.Message);
                bResult = false;
            }

            return bResult;
        }

        #region accessors

        public clsTec theBoard
        {
            get { return _tecBoard; }
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.


                _tecBoard.Dispose();

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~tecBoardModel() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
