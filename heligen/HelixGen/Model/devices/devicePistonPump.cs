// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using ABot2;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixGen.Model;

namespace HelixGen.Model.devices
{
    /// <summary>
    /// This is an abstract class that implements common functionality
    /// used by all the piston pumps.
    /// </summary>
    abstract public class devicePistonPump
    {
        /// <summary>
        /// Reference to the motorboard this piston resides on.
        /// </summary>
        protected clsAMB _motorBoard;

        /// <summary>
        /// The channel that this piston is on.
        /// </summary>
        protected int _channel;

        /// <summary>
        /// These are the actual positions in the channel.
        /// </summary>
        protected int[] _positions;

        /// <summary>
        /// The current position.
        /// </summary>
        protected int _position;

        /// <summary>
        /// The maximum number of positions.
        /// </summary>
        protected int _npositions;

        /// <summary>
        /// The id of the controller for this device.
        /// </summary>
        protected string _controllerId;

        public delegate void evtPumpPositionChanged(int position);
        public event evtPumpPositionChanged PumpPositionChanged;


        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="motorBoardIn">A reference to the corresponding motor board.</param>
        /// <param name="channel">The channel number on the motorboard.  Should be 1...4 (inclusive)</param>
        /// <param name="positions">An array of absolute positions that correspond to the pump positions.</param>
        /// <param name="npositions">The number of positions used for this pump.  Note that this should be the
        /// size of the positions array.</param>
        public devicePistonPump(clsAMB motorBoardIn, int channel, int[] positions, string controllerId)
        {
            _motorBoard = motorBoardIn;
            _channel = channel;
            _positions = positions;
            _npositions = positions.Count();
            _controllerId = controllerId;
            
            // The Motorboard value cannot be null.

            if (motorBoardIn == null)
            {
                throw new Exception("Error; devicePistonPump::devicePistonPump the value motorBoardIn cannot be null.");
            }

            // Check the range values on the channel argument.

            if (channel < 1 || channel > 4)
            {
                throw new Exception(string.Format("Error; devicePistonPump::devicePistonPump the value for channel ({0}) must be between 1 and 4 (inclusive).",
                    channel.ToString()));
            }
        }

        /// <summary>
        /// The current position of the pump.
        /// </summary>
        public int Position
        {
            get { return _position; }
            set
            {
                // Range check the value.

                if (value < 1 || value > _npositions)
                {
                    // Throw a range error.

                    throw new Exception(string.Format("devicePistonPump::Position, range error, position request of {0} which should be no higher than {1}",
                        value.ToString(), _npositions.ToString()));
                }
                else
                {
                  //  if (_position != value)
                    {
                        HelixGenModel _theModel = ((HelixGen.App)(App.Current)).Model;
                        clsAMB controller = _theModel.getMotorBoard(_controllerId);

                        // Move the slider into position.

                        string strChannel = _channel.ToString("00");

                         controller.MoveToAbsolutePosition(strChannel,
                        _positions[value - 1].ToString());
                        
                        controller.WaitForCompletion(strChannel);
                        /*
                        controller.EncoderControlMoveToAbsolutePosition(_channel-1,
                            _positions[value - 1]);*/



                        _position = value;  // TBD; do we try to track/sych this value, or
                                            // do we just ask the board, and relate the 
                                            // position back?

                        //if (PumpPositionChanged != null)
                           // PumpPositionChanged(value);
                    }
                }
            }
        }
        public int[] Positions
        {
            get { return _positions; }
        }
        public int Speed
        {
            get
            {
                HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;
                clsAMB controller = theModel.getMotorBoard(_controllerId);
                return controller.GetMaxSpeed(_channel.ToString("00"));
            }

            set
            {
                HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;
                clsAMB controller = theModel.getMotorBoard(_controllerId);
                controller.SetMaxSpeed(_channel.ToString("00"), value.ToString());
            }
        }
        //2018
        public int RNPosition

        {

            set
            {
                HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;
                clsAMB controller = theModel.getMotorBoard(_controllerId);
                string strChannel = _channel.ToString("00");
                controller.SetStartSpeed(strChannel, "200");
                controller.SetMaxSpeed(strChannel, "200");
                controller.SetDirectionPolarity(strChannel, "0");
                controller.MoveRelativePositive(strChannel, value.ToString());
                controller.WaitForCompletion(strChannel);
            }
        }
        public int RNNosition

        {

            set
            {
                HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;
                clsAMB controller = theModel.getMotorBoard(_controllerId);
                string strChannel = _channel.ToString("00");
                controller.SetStartSpeed(strChannel, "500");
                controller.SetMaxSpeed(strChannel, "500");
                controller.SetDirectionPolarity(strChannel, "1");
                controller.MoveRelativePositive(strChannel, value.ToString());
                controller.WaitForCompletion(strChannel);
                controller.SetMaxSpeed(strChannel, "200");
            }
        }


        protected int initializeMotorParams(int axis, Configuration.CSystem_MotorBoardConfigurationItem configIn)
        {
            HelixGenModel _theModel = ((HelixGen.App)(App.Current)).Model;

            clsAMB controller = _theModel.getMotorBoard(_controllerId);

            int nRetVal = 0;

            // Set the parameters for the board on this axis.

            int iaxis = axis - 1;

            controller.axisparams_current[iaxis].strStartSpeed = configIn.m_uiMotorStartSpeed.ToString();
            controller.axisparams_current[iaxis].strMaxSpeed = configIn.m_uiMotorMaxSpeed.ToString();
            controller.axisparams_current[iaxis].strAcceleration = configIn.m_uiMotorAccel.ToString();
            controller.axisparams_current[iaxis].strDeceleration = configIn.m_uiMotorDecel.ToString();
            controller.axisparams_current[iaxis].strJerk = configIn.m_uiMotorJerk.ToString();
            controller.axisparams_current[iaxis].strMoveCurrent = configIn.m_uiMotorMoveCurrent.ToString();
            controller.axisparams_current[iaxis].strHoldCurrent = configIn.m_uiMotorHoldCurrent.ToString();
            controller.axisparams_current[iaxis].strResolution = configIn.m_uiMotorResolution.ToString();
            controller.axisparams_current[iaxis].strDirectionPolarity = configIn.m_uiMotorDirection.ToString();
            controller.axisparams_current[iaxis].strHomeTimeout = configIn.m_uiMotorHomeTimeout.ToString();
            controller.axisparams_current[iaxis].strProfileMode = configIn.m_uiMotorProfileMode.ToString();
            controller.axisparams_current[iaxis].strEncoderPresent = configIn.m_bEncoderEnabled ? "1" : "0";
            controller.axisparams_current[iaxis].strEncoderEncoderMonitorTimer_ms = configIn.m_uiEncoderMonitorTimer_ms.ToString();
            controller.axisparams_current[iaxis].strEncoderMonitorPulseChangeThreshold = configIn.m_uiEncoderMonitorPulseChangeThreshold.ToString();
            controller.axisparams_current[iaxis].strEncoderMonitorErrorCountThreshold = configIn.m_uiEncoderMonitorErrorCountThreshold.ToString();
            controller.axisparams_current[iaxis].strEncoderDirectionPolarity = configIn.m_uiEncoderDirectionPolarity.ToString();
            controller.axisparams_current[iaxis].strLostStepsLimit = configIn.m_uiMotorMaxNumLostSteps.ToString();
            controller.axisparams_current[iaxis].strEncoderStartOffset = configIn.m_iEncoderStartOffset.ToString();
            controller.axisparams_current[iaxis].strEncoderControllerConversionFactor = configIn.m_fEncoderScalingFactor.ToString();

            // Kick the parameters in.

            controller.LoadParametersIntoController(axis.ToString("00"));

            return nRetVal;
        }
    }
}
