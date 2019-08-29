// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using ABot2;

namespace HelixGen.Model.devices
{
    /// <summary>
    /// Arguments object sent as part of the pump position event.
    /// </summary>
    public class pumpEventArgs : EventArgs
    {
        /// <summary>
        /// The position the pump is now at.
        /// </summary>
        public int position { get; internal set; }

        public pumpEventArgs(int positionIn = 0)
        {
            position = positionIn;
        }
    }

    public class devicePump
    {
        /// <summary>
        /// Reference to the motorboard this piston resides on.
        /// </summary>
        protected clsAMB _motorBoard;

        /// <summary>
        /// The channel that this pump is on.
        /// </summary>
        protected int _diochannel;

        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();


 

        public devicePump(motorBoardModel motorBoardIn, int diochannel)
        {
            _motorBoard = motorBoardIn.board;
            _diochannel = diochannel;

            // The Motorboard value cannot be null.

            if (motorBoardIn == null)
            {
                throw new Exception("Error; devicePump::devicePump the value motorBoardIn cannot be null.");
            }

            // Check the range values on the channel argument.

            if (diochannel < 1 || diochannel > 8)
            {
                throw new Exception(string.Format("Error; devicePump::devicePump the value for diochannel ({0}) must be between 1 and 8 (inclusive).",
                    diochannel.ToString()));
            }
        }

        public int initialize()
        {
            int nRetVal = 0;

            // Set the value to zed.

            //_motorBoard.SetDIOStatus(_diochannel.ToString(), "0");
            return nRetVal;
        }

        public bool dispense
        {
            set
            {
                try
                {
                    //_motorBoard.SetDIOStatus(_diochannel.ToString("00"), (value) ? "1" : "0");
                }
                catch (Exception ex)
                {
                    logger.Error("devicePump::dispense({0}) caught an exception: {1}",
                        value ? "true" : "false", ex.Message);
                }
            }

            get
            {
                bool bResult = false;

                try
                {
                    //bResult = _motorBoard.GetDIOStatus(_diochannel.ToString("00"));
                }
                catch (Exception ex)
                {
                    logger.Error("devicePump::dispense() get caught an exception: {0}", ex.Message);
                }

                return bResult;
            }
        }
    }
}
