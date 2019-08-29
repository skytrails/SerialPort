// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using ABot2;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelixGen.Model.devices
{
    public class deviceHeaterPiston : devicePistonPump
    {
        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public deviceHeaterPiston(motorBoardModel motorBoardIn, int channel, int[] channelPositions, string controllerId) :
            base(motorBoardIn.board, channel, channelPositions, controllerId)
        {
        }

        public int initialize()
        {
            int nRetVal = 0;

            try
            {
                string strChannel = _channel.ToString("00");

                // Set the motor parameters.

                HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;

                int nResult = initializeMotorParams(_channel, theModel.Config.m_HeaterPiston_Configuration);
                if (nResult != 0)
                {
                    // Initialization failed.

                    logger.Debug("deviceHeaterPiston::initialize failed.");
                }
                else
                {
                    clsAMB controller = theModel.getMotorBoard(theModel.Config.m_HeaterPiston_Configuration.m_strControllerName);

                    // RNPosition = 40000;
                    // RNNosition = 111000;
                    // Move the pump to the initial position.
                    /*
                    controller.SetDirectionPolarity(strChannel, "1");
                    controller.MoveRelativePositive(strChannel, "90000");
                    controller.WaitForCompletion(strChannel);*/


                    controller.SetDirectionPolarity(strChannel, "0");
                    controller.MoveHome(strChannel);
                    controller.WaitForCompletion(strChannel);
                    /*
                    controller.MoveRelativePositive(strChannel, "104354");
                    controller.WaitForCompletion(strChannel);

                    controller.MoveRelativePositive(strChannel, "45020");
                    controller.WaitForCompletion(strChannel);
                    */
                    // controller.SetDirectionPolarity(strChannel, "0");
                    /*
                    controller.MoveRelativePositive(strChannel, "83612"); 
                    controller.WaitForCompletion(strChannel);
                    
                    controller.MoveRelativePositive(strChannel, "30020");
                    controller.WaitForCompletion(strChannel);*/
                    /*
                    controller.SetDirectionPolarity(strChannel, "1");
                    controller.MoveRelativePositive(strChannel, "90000");
                    controller.WaitForCompletion(strChannel);*/

                    controller.SetResolution(theModel.Config.m_HeaterPiston_Configuration.m_nMotorChannel.ToString("00"),
                        theModel.Config.m_HeaterPiston_Configuration.m_uiMotorResolution.ToString());

                    //Position = 3;
                    //RNPosition = 53000;//10mm
                    //RNPosition = 40000;
                    //RNNosition = 90000;
                }
            }
            catch (Exception ex)
            {
                logger.Debug("deviceHeaterPiston::initialize() Caught an exception: {0}", ex.Message);

                nRetVal = 1;
            }

            return nRetVal;
        }
    }
}
