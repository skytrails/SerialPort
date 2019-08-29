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
    public class deviceR2Piston : devicePistonPump
    {
        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public deviceR2Piston(motorBoardModel motorBoardIn, int channel, int[] channelPositions, string controllerId) :
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

                int nResult = initializeMotorParams(_channel, theModel.Config.m_R2Piston_Configuration);
                if (nResult != 0)
                {
                    // Initialization failed.

                    logger.Debug("deviceR2Piston::initialize failed.");
                }
                else
                {
                    HelixGenModel _theModel = ((HelixGen.App)(App.Current)).Model;
                    clsAMB controller = _theModel.getMotorBoard(_controllerId);

                    // Move the pump to the initial position.
                    //RNNosition = 89000;
                  
                    controller.SetDirectionPolarity(strChannel, "0");
                    controller.MoveHome(strChannel);
                    controller.WaitForCompletion(strChannel);
                    /*
                    controller.SetDirectionPolarity(strChannel, "0");
                    controller.MoveRelativePositive(strChannel, "2580");
                    controller.WaitForCompletion(strChannel);*/
                    /*
                    controller.SetDirectionPolarity(strChannel, "0");
                    controller.MoveRelativePositive(strChannel, "50064");
                    controller.WaitForCompletion(strChannel);*/

                    
                    controller.SetResolution(theModel.Config.m_R2Piston_Configuration.m_nMotorChannel.ToString("00"),
                         theModel.Config.m_R2Piston_Configuration.m_uiMotorResolution.ToString());

                    // Position = 3;
                   // RNPosition = 50000;//9.9mm
                   // RNNosition = 89000;
                }
            }
            catch(Exception)
            {
                nRetVal = 1;
            }

            return nRetVal;
        }

        public int initialize1()
        {
            int nRetVal = 0;

            try
            {
                string strChannel = _channel.ToString("00");

                // Set the motor parameters.

                HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;

                
                    HelixGenModel _theModel = ((HelixGen.App)(App.Current)).Model;
                    clsAMB controller = _theModel.getMotorBoard(_controllerId);

                    // Move the pump to the initial position.
                    //RNNosition = 89000;

                    

                    controller.SetDirectionPolarity(strChannel, "0");
                    controller.MoveRelativePositive(strChannel, "12080");
                    controller.WaitForCompletion(strChannel);
                    /*
                    controller.SetDirectionPolarity(strChannel, "0");
                    controller.MoveRelativePositive(strChannel, "68100");
                    controller.WaitForCompletion(strChannel);*/


                    controller.SetResolution(theModel.Config.m_R2Piston_Configuration.m_nMotorChannel.ToString("00"),
                         theModel.Config.m_R2Piston_Configuration.m_uiMotorResolution.ToString());

                    // Position = 3;
                    // RNPosition = 50000;//9.9mm
                    // RNNosition = 89000;
                
            }
            catch (Exception)
            {
                nRetVal = 1;
            }

            return nRetVal;
        }
    }
}
