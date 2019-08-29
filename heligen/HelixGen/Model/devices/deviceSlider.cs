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
    public class deviceSlider : devicePistonPump
    {
        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public deviceSlider(motorBoardModel motorBoardIn, int channel, int[] channelPositions, string controllerId) :
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

                int nResult = initializeMotorParams(_channel, theModel.Config.m_Slider_Configuration);
                if (nResult != 0)
                {
                    // Initialization failed.

                    logger.Debug("deviceSlider::initialize failed.");
                }
                else
                {
                    clsAMB controller = theModel.getMotorBoard(_controllerId);

                    // Move the pump to the initial position.
                     controller.SetDirectionPolarity(strChannel, "0");
                    controller.MoveHome(strChannel);
                    controller.WaitForCompletion(strChannel);


                   

                    controller.MoveRelativePositive(strChannel, "4510");   

                    controller.WaitForCompletion(strChannel);
                    /*
                      //controller.SetDirectionPolarity(strChannel, "0");
                      controller.MoveRelativePositive(strChannel, "9500");
                      controller.WaitForCompletion(strChannel);


                      controller.SetDirectionPolarity(strChannel, "1");
                       //controller.MoveRelativePositive(strChannel, "6156");
                       controller.MoveRelativePositive(strChannel, "5800");
                       controller.WaitForCompletion(strChannel);

                             controller.MoveRelativePositive(strChannel, "4300");
                      controller.WaitForCompletion(strChannel);

                     controller.SetDirectionPolarity(strChannel, "0");
                   */
                    /* controller.MoveToAbsolutePosition(strChannel,
                             "11114");
                     controller.WaitForCompletion(strChannel);*/
                    //controller.SetDirectionPolarity(strChannel, "1");

                    controller.SetResolution(theModel.Config.m_Slider_Configuration.m_nMotorChannel.ToString("00"),
                         theModel.Config.m_Slider_Configuration.m_uiMotorResolution.ToString());

                    //Position = 2;
                    // Position = 1;
                    //RNPosition = 9614;
                    //RNNosition = 6307;
                    //RNNosition = 3307;
                }
            }
            catch (Exception ex)
            {
                logger.Debug("deviceSlider::initialize() Caught an exception: {0}", ex.Message);
                nRetVal = 1;
            }

            return nRetVal;
        }
    }
}
