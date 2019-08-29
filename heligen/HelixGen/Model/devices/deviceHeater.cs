// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using Accel.HeaterBoard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using ABot2;

namespace HelixGen.Model.devices
{
    public class deviceHeater
    {
        /// <summary>
        /// Reference to the lower level tec board.
        /// </summary>
        protected clsTec _tecBoard;

        /// <summary>
        /// The channel used by this heater on the board.
        /// </summary>
        protected int _nChannel;

        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected HelixGenModel _theModel;

        public deviceHeater(tecBoardModel modelIn, int nChannelIn)
        {
            _tecBoard = modelIn.theBoard;
            _nChannel = nChannelIn;

            // Stash a reference to the model while we're at it.

            _theModel = ((HelixGen.App)(App.Current)).Model;
        }

        public int initialize()
        {
            int nRetVal = 0;

            // Subscribe to temperature updates.

            //_tecBoard.ResHeater(_nChannel).PollInterval = 1000;
            //_tecBoard.ResHeater(_nChannel).PropertyChanged += DeviceHeater_PropertyChanged;
            //_tecBoard.ResHeater(_nChannel).ErrorReceived += DeviceHeater_ErrorReceived;
            //_tecBoard.ResHeater(_nChannel).Activate(5);

            return nRetVal;
        }

        private void DeviceHeater_ErrorReceived(Accel.INamed sender, ErrEventArgs e)
        {
            logger.Debug("Heater Got an error; {0}", e.Error.ToString());
        }
        public void DeviceHeaterCtrl(double ctrl)
        {
            
            if(ctrl==0)
                _tecBoard.SetTempControlMode(6, 0);// tec 4
            else
            {
                _tecBoard.SetTempControlMode(6, 1);
                _tecBoard.SetRSHeaterP(0,120);
                _tecBoard.SetRSHeaterI(0,(float)0.05);
                _tecBoard.SetRSHeaterD(0,400);
                _tecBoard.SetRSHeaterSetPointOffset(0, (float)0.1);
                _tecBoard.SetRSHeaterPropBand(0,5);
                _tecBoard.SetRSHeaterPreHeatSetPoint(0,50);
                
                _tecBoard.SetRSHeaterP(1, 40);
                _tecBoard.SetRSHeaterI(1, (float)0.05);
                _tecBoard.SetRSHeaterD(1, 400);
                _tecBoard.SetRSHeaterSetPointOffset(1, (float)0.1);
                _tecBoard.SetRSHeaterPropBand(1, 5);
                _tecBoard.SetRSHeaterPreHeatSetPoint(1, 50);
            }
               
            

        }

#if false

        private void DeviceHeater_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Temperature":
                    double curTemp = (double)(_tecBoard.ResHeater(_nChannel).Temperature);
                    _theModel.HeaterTempUpdated(curTemp);
                    break;

            }
        }
#endif

        /// <summary>
        /// The current device temperature.
        /// </summary>
        /// <remarks>
        /// Note that this function actually queries the device, rather
        /// than returning the last reported temperature.
        /// </remarks>
        public double Temperature
        {
            get {
               // return _tecBoard.GetTemperature(6, _nChannel);
                return _tecBoard.GetTemperature(6, 0);
            }

            set
            {
                 _tecBoard.SetTempControlMode(6, 1);
                _tecBoard.SetTempControlMode(6, 5);
                _tecBoard.SetTempControlMode(6, 6);
                _tecBoard.SetTempControlMode(6, 7);
                _tecBoard.SetRSHeaterSetPoint(0, (float)value);

                 _tecBoard.SetRSHeaterSetPoint(1, (float)value);

               // _tecBoard.SetSetPoint(6, (float)value);

                // _theModel.DevicePCRCycler.SetToAppropriateBand(6, 0,
                //90, _theModel.Config.m_TEC_Channel_Configurations["TEC_5"].m_Step_PID_RampUp_Range_List);


                // _tecBoard.SetTempControlMode(3, 1);
                // _tecBoard.SetSetPoint(3, (float)value);




                //if (curTemp < value)
                //{
                //_tecBoard.ResHeater(_nChannel).Activate(5);
                //}
                //else
                //{
                //    _tecBoard.ResHeater(_nChannel).Activate(0);
                //}
            }
        }
    }
}
