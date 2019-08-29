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
    public class deviceOpticsMotor : devicePistonPump
    {
        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected bool isFirstIn;

        public deviceOpticsMotor(motorBoardModel motorBoardIn, int channel, int[] positions, string controllerId) :
            base(motorBoardIn.board, channel, positions, controllerId)
        {
            isFirstIn = true;
        }

        public int initialize()
        {
            int nRetVal = 0;

            try
            {
                string strChannel = _channel.ToString("00");

                // Set the motor parameters.

                HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;

                int nResult = initializeMotorParams(_channel, theModel.Config.m_OpticsMotor_Configuration);
                if (nResult != 0)
                {
                    // Initialization failed.

                    logger.Debug("deviceOpticsMotor::initialize failed.");
                }
                else
                {
                    clsAMB controller = theModel.getMotorBoard(_controllerId);

                    // Move the pump to the initial position.
                    string value;
                    controller.GetHomeSwitchStatus(strChannel,out value);
                    
                    if(Int32.Parse(value)==0)
                    {
                        controller.MoveHome(strChannel);
                        controller.WaitForCompletion(strChannel);
                    }

                   // theModel.DeviceOpticsMotor.Position = 4;
                   /*
                    for (int nLED = 0; nLED < 6; nLED++)
                    {
                        theModel.OpticsModel.SetLedCurrent(nLED + 1, 0.75);
                        //theModel.OpticsModel.SetLedCurrent(nLED + 1, 0);
                    }
                    */
                   // controller.SetStartSpeed(strChannel, "400");//200,400
                   // controller.SetMaxSpeed(strChannel, "1200");//200,1600
                   /*
                    controller.SetDirectionPolarity(strChannel, "0");
                    controller.MoveRelativePositive(strChannel, "100000000");
                    controller.WaitForCompletion(strChannel);
                    controller.MoveRelativeNegative(strChannel, "0");
                    controller.WaitForCompletion(strChannel);
                    */
                    controller.SetResolution(theModel.Config.m_OpticsMotor_Configuration.m_nMotorChannel.ToString("00"),
                         theModel.Config.m_OpticsMotor_Configuration.m_uiMotorResolution.ToString());

                   
                    

                    //if(isFirstIn)
                    //ZeroOptic();
                    isFirstIn = false;
                   // theModel.OpticsModel.TriggerPhotodetector();
                    // Position = 1;
                    /*
                    controller.MoveToAbsolutePosition(strChannel,
                             "132");
                    controller.WaitForCompletion(strChannel);*/
                }
            }
            catch (Exception)
            {
                nRetVal = 1;
            }

            return nRetVal;
        }

        public void ZeroOptic()
        {
            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;

            clsAMB controller = theModel.getMotorBoard(_controllerId);
            string strChannel = _channel.ToString("00");
            /*
            for (int nLED = 0; nLED < 6; nLED++)
            {
                theModel.OpticsModel.SetLedCurrent(nLED + 1, 0.75);
                //theModel.OpticsModel.SetLedCurrent(nLED + 1, 0);
            }*/

           // theModel.DeviceOpticsMotor.Position = 5;
            controller.MoveHome(strChannel);
            controller.WaitForCompletion(strChannel);

            // double[,] allReadings = new double[6, 4];
            // double[] baselines = new double[4];
            for (int i=0;i<15;i++)
            {

           
                for (int Position = 2; Position < 6; Position++)
                {
                    theModel.DeviceOpticsMotor.Position = Position;
                    double[] readings = theModel.opticsBoard.ReadPhotodetector();
                    if(i==14)
                         theModel.optic_zero[Position - 2]= readings[2];

                   // for (int nCartridge = 0; nCartridge < 6; nCartridge++)
                       // allReadings[nCartridge, Position - 2] = readings[nCartridge];

                    //Thread.Sleep(3000);
                }
                controller.MoveHome(strChannel);
                controller.WaitForCompletion(strChannel);
            }
            /*
            for (int nLED = 0; nLED < 6; nLED++)
            {
                theModel.OpticsModel.SetLedCurrent(nLED + 1, 0);
                //theModel.OpticsModel.SetLedCurrent(nLED + 1, 0);
            }*/
            controller.MoveHome(strChannel);
            controller.WaitForCompletion(strChannel);


        }
        public int getChannel()
        {
            return this._channel;
        }
        public string getMotorBoard()
        {
            return this._controllerId;
        }

    }
}


