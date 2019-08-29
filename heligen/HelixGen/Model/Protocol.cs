// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using XmlDocument_Support_Utilities;

using ABot2;

namespace HelixGen.Model
{
    public class sliderAction : ProtocolAction
    {
        /// <summary>
        /// The position to set the slider to.
        /// </summary>
        protected int _position;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public sliderAction(int position = 0)
        {
            _position = position;
        }
        public override bool Execute()
        {
            bool bRetVal = true;
            _theModel.DeviceSlider.Position = _position;
            return bRetVal;
        }

        #region accessors
        public int position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion
    }
    public class chasisAction : ProtocolAction
    {
        /// <summary>
        /// The position to set the slider to.
        /// </summary>
        protected int _position;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public chasisAction(int position = 0)
        {
            _position = position;
        }
        public override bool Execute()
        {
            bool bRetVal = true;
            _theModel.DeviceChasis.Position = _position;
            return bRetVal;
        }

        #region accessors
        public int position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion
    }

    public class r1Action : ProtocolAction
    {
        /// <summary>
        /// The position to set the slider to.
        /// </summary>
        protected int _position;

        /// <summary>
        /// Speed to move at.  -999 if value should be ignored.
        /// </summary>
        protected int _speed;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public r1Action(int position = 0, int speed = -999)
        {
            _position = position;
            _speed = speed;
        }
        public override bool Execute()
        {
            bool bRetVal = true;
            int previousSpeed = _theModel.DeviceR1Piston.Speed;

            if (_speed != -999)
            {
                _theModel.DeviceR1Piston.Speed = _speed;
            }

            _theModel.DeviceR1Piston.Position = _position;

            // Put the speed back if we changed it.

            if (_speed != -999)
            {
                _theModel.DeviceR1Piston.Speed = previousSpeed;
            }

            return bRetVal;
        }

        #region accessors
        public int position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion
    }
    //2018
    public class r1RNPosition : ProtocolAction
    {
        /// <summary>
        /// The position
        /// to set the slider to.
        /// </summary>
        protected int _position;

        public r1RNPosition(int position = 0)
        {
            _position = position;

        }
        public override bool Execute()
        {
            bool bRetVal = true;
            _theModel.DeviceR1Piston.RNPosition = _position;

            return bRetVal;
        }

        #region accessors
        public int position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion
    }

    public class r2RNNPosition : ProtocolAction
    {
        /// <summary>
        /// The position
        /// to set the slider to.
        /// </summary>
        protected int _position;

        public r2RNNPosition(int position = 0)
        {
            _position = position;

        }
        public override bool Execute()
        {
            bool bRetVal = true;
            _theModel.DeviceR2Piston.RNNosition = _position;

            return bRetVal;
        }

        #region accessors
        public int position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion
    }
    public class r1RNNPosition : ProtocolAction
    {
        /// <summary>
        /// The position
        /// to set the slider to.
        /// </summary>
        protected int _position;

        public r1RNNPosition(int position = 0)
        {
            _position = position;

        }
        public override bool Execute()
        {
            bool bRetVal = true;
            _theModel.DeviceR1Piston.RNNosition = _position;

            return bRetVal;
        }

        #region accessors
        public int position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion
    }
    public class r2RNPosition : ProtocolAction
    {
        /// <summary>
        /// The position
        /// to set the slider to.
        /// </summary>
        protected int _position;

        public r2RNPosition(int position = 0)
        {
            _position = position;

        }
        public override bool Execute()
        {
            bool bRetVal = true;
            _theModel.DeviceR2Piston.RNPosition = _position;

            return bRetVal;
        }

        #region accessors
        public int position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion
    }

    public class HeatNNPosition : ProtocolAction
    {
        /// <summary>
        /// The position
        /// to set the slider to.
        /// </summary>
        protected int _position;

        public HeatNNPosition(int position = 0)
        {
            _position = position;

        }
        public override bool Execute()
        {
            bool bRetVal = true;
            _theModel.DeviceHeaterPiston.RNNosition = _position;

            return bRetVal;
        }

        #region accessors
        public int position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion
    }
    public class SlideNNPosition : ProtocolAction
    {
        /// <summary>
        /// The position
        /// to set the slider to.
        /// </summary>
        protected int _position;

        public SlideNNPosition(int position = 0)
        {
            _position = position;

        }
        public override bool Execute()
        {
            bool bRetVal = true;
            _theModel.DeviceSlider.RNNosition = _position;
            return bRetVal;
        }

        #region accessors
        public int position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion
    }
    public class HeatNPosition : ProtocolAction
    {
        /// <summary>
        /// The position
        /// to set the slider to.
        /// </summary>
        protected int _position;

        public HeatNPosition(int position = 0)
        {
            _position = position;

        }
        public override bool Execute()
        {
            bool bRetVal = true;
            _theModel.DeviceHeaterPiston.RNPosition = _position;

            return bRetVal;
        }

        #region accessors
        public int position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion
    }

    public class SlideNPosition : ProtocolAction
    {
        /// <summary>
        /// The position
        /// to set the slider to.
        /// </summary>
        protected int _position;

        public SlideNPosition(int position = 0)
        {
            _position = position;

        }
        public override bool Execute()
        {
            bool bRetVal = true;
            _theModel.DeviceSlider.RNPosition = _position;

            return bRetVal;
        }

        #region accessors
        public int position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion
    }

    public class r2Action : ProtocolAction
    {
        /// <summary>
        /// The position to set the slider to.
        /// </summary>
        protected int _position;

        /// <summary>
        /// Speed to move at.  -999 if value should be ignored.
        /// </summary>
        protected int _speed;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public r2Action(int position = 0, int speed = -999)
        {
            _position = position;
            _speed = speed;
        }
        public override bool Execute()
        {
            bool bRetVal = true;
            int previousSpeed = _theModel.DeviceR2Piston.Speed;

            if (_speed != -999)
            {
                _theModel.DeviceR2Piston.Speed = _speed;
            }

            _theModel.DeviceR2Piston.Position = _position;

            // Put the speed back if we changed it.

            if (_speed != -999)
            {
                _theModel.DeviceR2Piston.Speed = previousSpeed;
            }

            return bRetVal;
        }

        #region accessors
        public int position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion
    }

    public class heaterPistonAction : ProtocolAction
    {
        /// <summary>
        /// The position to set the slider to.
        /// </summary>
        protected int _position;

        /// <summary>
        /// Speed to move at.  -999 if value should be ignored.
        /// </summary>
        protected int _speed;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public heaterPistonAction(int position = 0, int speed = -999)
        {
            _position = position;
            _speed = speed;
        }
        public override bool Execute()
        {
            bool bRetVal = true;
            int previousSpeed = _theModel.DeviceHeaterPiston.Speed;


            if (_speed != -999)
            {
                _theModel.DeviceHeaterPiston.Speed = _speed;
            }

            _theModel.DeviceHeaterPiston.Position = _position;

            // Put the speed back if we changed it.

            if (_speed != -999)
            {
                _theModel.DeviceHeaterPiston.Speed = previousSpeed;
            }

            return bRetVal;
        }

        #region accessors
        public int position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion
    }

    public class heaterAction : ProtocolAction
    {
        /// <summary>
        /// The position to set the slider to.
        /// </summary>
        protected double _temperature;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public heaterAction(double temperature = 0.0)
        {
            _temperature = temperature;
        }
        public override bool Execute()
        {
            bool bRetVal = true;
            _theModel.DeviceHeater.Temperature = _temperature;
            //_theModel.DevicePCRCycler.Temperature = _temperature;

            // Wait for the device to reach the appointed temperature.

            if (_temperature != 0)
            {
#if false
                while (_theModel.DeviceHeater.Temperature != _temperature)
                {
                    Thread.Sleep(1000);
                }
#endif
            }

            return bRetVal;
        }

        #region accessors
        public double Ttemperature
        {
            get { return _temperature; }
            set { _temperature = value; }
        }
        #endregion
    }
    public class heaterCtrlAction : ProtocolAction
    {
        /// <summary>
        /// The position to set the slider to.
        /// </summary>
        protected double _temperature;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public heaterCtrlAction(double temperature = 0.0)
        {
            _temperature = temperature;
        }
        public override bool Execute()
        {
            bool bRetVal = true;
            _theModel.DeviceHeater.DeviceHeaterCtrl(_temperature);

            // Wait for the device to reach the appointed temperature.
            return bRetVal;
        }

        #region accessors
        public double Ttemperature
        {
            get { return _temperature; }
            set { _temperature = value; }
        }
        #endregion
    }

    public class pcrAction : ProtocolAction
    {
        /// <summary>
        /// The position to set the slider to.
        /// </summary>
        protected double _temperature;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public pcrAction(double temperature = 0.0)
        {
            _temperature = temperature;
        }
        public override bool Execute()
        {

            bool bRetVal = true;

            if (_temperature != 0)
            {
                // Enable the system fan.

                //_theModel.tecBoard.theBoard.SetFanDutyCycle(100);//2018
                //Thread.Sleep(5);
                //_theModel.tecBoard.theBoard.EnableTECFan(true);
                //Thread.Sleep(5);

                //_theModel.tecBoard.theBoard.EnableSystemFan(true);
                //Thread.Sleep(5);

                _theModel.DevicePCRCycler. gotoTempAndStay(4, _temperature, 0);
               // _theModel.DevicePCRCycler.gotoTempAndStay(3, _temperature, 0);
            }

            return bRetVal;
        }

        #region accessors
        public double Ttemperature
        {
            get { return _temperature; }
            set { _temperature = value; }
        }
        #endregion
    }
    public class overTempAction : ProtocolAction
    {
        /// <summary>
        /// The position to set the slider to.
        /// </summary>
        protected double _temperature;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public overTempAction(double temperature = 0.0)
        {
            _temperature = temperature;
        }
        public override bool Execute()
        {

            bool bRetVal = true;

            if (_temperature != 0)
            {
                // Enable the system fan.

                //_theModel.tecBoard.theBoard.SetFanDutyCycle(100);//2018
                //Thread.Sleep(5);
                //_theModel.tecBoard.theBoard.EnableTECFan(true);
                //Thread.Sleep(5);

                //_theModel.tecBoard.theBoard.EnableSystemFan(true);
                //Thread.Sleep(5);

                _theModel.DevicePCRCycler.gotoTemp(4, _temperature, 0);
                //_theModel.DevicePCRCycler.gotoTemp(3, _temperature, 0);
            }

            return bRetVal;
        }

        #region accessors
        public double Ttemperature
        {
            get { return _temperature; }
            set { _temperature = value; }
        }
        #endregion
    }

    public class pumpAction : ProtocolAction
    {
        /// <summary>
        /// The position to set the slider to.
        /// </summary>
        protected bool _bOn;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public pumpAction(bool bOn = false)
        {
            _bOn = bOn;
        }
        public override bool Execute()
        {
            bool bRetVal = true;
            _theModel.DevicePump.dispense = _bOn;
            return bRetVal;
        }

        #region accessors
        public bool On
        {
            get { return _bOn; }
            set { _bOn = value; }
        }
        #endregion
    }

    public class timeAction : ProtocolAction
    {
        /// <summary>
        /// The position to set the slider to.
        /// </summary>
        protected int _nMicroseconds;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public timeAction(int microSeconds = 0)
        {
            _nMicroseconds = microSeconds;
        }
        public override bool Execute()
        {
            bool bRetVal = true;

            Thread.Sleep(_nMicroseconds);
            return bRetVal;
        }

        #region accessors
        public int timeInMicroseconds
        {
            get { return _nMicroseconds; }
            set { _nMicroseconds = value; }
        }
        #endregion
    }

    public abstract class ProtocolAction
    {
        protected HelixGenModel _theModel;

        /// <summary>
        /// Instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();


        public ProtocolAction()
        {
            _theModel = ((HelixGen.App)(App.Current)).Model;

        }

        public virtual bool Execute()
        {
            bool bRetVal = true;
            return bRetVal;
        }
    }

    /// <summary>
    /// Represents an individual protocol step.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class ProtocolStep
    {
        /// <summary>
        /// A textual comment on what the step does, this is 
        /// used to display something to the user.
        /// </summary>
        protected string _description;

        /// <summary>
        /// The time span over which this action should execute.
        /// </summary>
        protected TimeSpan _executionTime;

        /// <summary>
        /// The line number in the protocol file.
        /// </summary>
        protected int _lineNo;

        /// <summary>
        /// List of actions that make up the protocol.
        /// </summary>
        protected List<ProtocolAction> _actions;

        /// <summary>
        /// A list of PCR actions.  These are executed after the _actions are executed.
        /// </summary>
        protected List<ProtocolStep> _pcrActions;

        private static Logger logger = LogManager.GetCurrentClassLogger();


        public ProtocolStep(int nLine)
        {
            _lineNo = nLine;
            _actions = new List<ProtocolAction>();
            _pcrActions = new List<ProtocolStep>();
        }

        /// <summary>
        /// Executes the specified protocol step.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> Execute()
        {
            bool bRetVal = true;
            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;

#if false
            Parallel.ForEach(_actions, (anAction) =>
            {
                anAction.Execute();
            });
#endif
            foreach (ProtocolAction theAction in _actions)
            {
                logger.Debug("Executing the action: {0}", theAction.ToString());

                if (theModel.bStopScript)
                    break;

                theAction.Execute();
            }

            foreach (ProtocolStep theStep in _pcrActions)
            {
                logger.Debug("Executing the step from _pcrActions: {0}", theStep.ToString());

                if (theModel.bStopScript)
                    break;

                theStep.Execute();
            }

            return bRetVal;
        }

        #region accessors
        public string description
        {
            get { return _description; }
            set { _description = value; }
        }

        public int line
        {
            get { return _lineNo; }
        }

        public List<ProtocolAction> Actions
        {
            get { return _actions; }
        }

        public List<ProtocolStep> PCRActions
        {
            get { return _pcrActions; }
            set { _pcrActions = value; }
        }
        #endregion
    }

    /// <summary>
    /// Represents an individual protocol step.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class CycleStep : ProtocolStep
    {
        public int repetitions = 1;

        private static Logger logger = LogManager.GetCurrentClassLogger();


        public CycleStep(int nLine, int repetitionsIn = 1) : base(nLine)
        {
            repetitions = repetitionsIn;
        }

        /// <summary>
        /// Executes the specified protocol step.
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> Execute()
        {
            bool bRetVal = true;

#if false
            Parallel.ForEach(_actions, (anAction) =>
            {
                anAction.Execute();
            });
#endif
            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;



            //theModel.StartingPCRCycling(); // Clear the PCR graph.

            for (int nCycle = 0; nCycle < repetitions; nCycle++)
            {
                theModel.ProtocolDescLine = string.Format(" Cycle {0} of {1}", (nCycle + 1).ToString(), repetitions.ToString());//2018


                foreach (ProtocolAction theAction in _actions)
                {
                    if (theModel.bStopScript)
                        break;

                    logger.Debug("Cycle {1}: Executing the action: {0}", theAction.ToString(),
                        nCycle.ToString());
                    //theModel.ProtocolDescLine = string.Format(" Cycle {0} of {1}: Executing the action: {0}", (nCycle + 1).ToString(), repetitions.ToString(), theAction.ToString());
                    theAction.Execute();
                }

                foreach (ProtocolStep theStep in _pcrActions)
                {
                    if (theModel.bStopScript)
                        break;

                    logger.Debug("Cycle {1}: Executing the step: {0}", theStep.ToString(),
                        nCycle.ToString());

                    //theModel.ProtocolDescLine = string.Format(" Cycle {0} of {1}: Executing the step:{2}"
                        //, (nCycle + 1).ToString(), repetitions.ToString(), theStep.ToString());//2018

                    theStep.Execute();
                }
            }

            theModel.ProtocolDescLine = " Cycling Completed";
            return bRetVal;
        }
    }

    /// <summary>
    /// Represents an individual protocol step.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// 

    public class SampleStep : ProtocolStep
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        public SampleStep(int nLine) : base(nLine)
        {
        }

        /// <summary>
        /// Executes the specified protocol step.
        /// </summary>
        /// <returns></returns>
        /// 
        /*
        public override async Task<bool> Execute()
        {
            bool bRetVal = true;

            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;

            StringBuilder outLine = new StringBuilder(256);

            outLine.Append(++(theModel.PCRCycleNum)).ToString();

            double[,] allReadings = new double[6, 4];
            double[] baselines = new double[4];

           // theModel.DeviceOpticsMotor.Position = 4;//2018

            for (int Position = 6; Position < 7; Position++)//2,6
            {
                theModel.DeviceOpticsMotor.Position = Position;
               

                Thread.Sleep(3000);
            }
            
            for (int nCartridgeRep = 0; nCartridgeRep < 6; nCartridgeRep++)
            {
                for (int nChannelRep = 0; nChannelRep < 4; nChannelRep++)
                    outLine.Append("," + allReadings[nCartridgeRep, nChannelRep].ToString());
            }

            theModel.NewPDReadingsTaken(allReadings);

            double[] temperatures = theModel.opticsBoard.ReadTemperatures();

            outLine.Append("," + string.Join(",", temperatures));

            theModel.PCRFileStream.WriteLine(outLine);
            //theModel.DeviceOpticsMotor.Position = 1;


            string strChannel = theModel.DeviceOpticsMotor.getChannel().ToString("00");
            clsAMB controller = theModel.getMotorBoard(theModel.DeviceOpticsMotor.getMotorBoard());
            // Move the pump to the initial position.

           // controller.SetDirectionPolarity(strChannel, "0");
            

            controller.MoveHome(strChannel);
            controller.WaitForCompletion(strChannel);

            //theModel.DeviceOpticsMotor.Speed = 400;
            Thread.Sleep(3000);

            return bRetVal;
        }*/
        public override async Task<bool> Execute()
        {
            bool bRetVal = true;

            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;

            StringBuilder outLine = new StringBuilder(256);

            string strChannel = theModel.DeviceOpticsMotor.getChannel().ToString("00");
            clsAMB controller = theModel.getMotorBoard(theModel.DeviceOpticsMotor.getMotorBoard());

            outLine.Append(++(theModel.PCRCycleNum)).ToString();

            // double[,] allReadings = new double[6, 4];
            // double[] baselines = new double[4];

            // theModel.DeviceOpticsMotor.Position = 4;//2018

            theModel.OpticsModel.SetLedCurrent(1, 0.2);
            controller.MoveRelativePositive(strChannel, "1536000");//1200*1.6s*8=15360
            theModel.opticsBoard.TriggerPhotodetector(false);//begin resolve sample

            double[,] allReadings = new double[6, 4];
            double[] temp_data = new double[4];

            //theModel.opticsBoard._devOptics.sPacketEnd0aGot = false;
            theModel.opticsBoard._devOptics.sCurrPacketLen = 0;
            theModel.opticsBoard._devOptics.sPacketIdGot = false;

            theModel.opticsBoard._devOptics.sPacketEnd0aGot = false;
            theModel.opticsBoard._devOptics.gCommErr = false;

            // string value;
            // int counter_cycle_moto = 0;
            int samplCounter = 0;
            while (!theModel.bStopScript)
            {

                if (theModel.opticsBoard.UnpackFrame())
                {
                    bool isDataRight = true;
                    
                    for (int i = 0; i < 9; i++)
                    {
                        if (theModel.readings[i] == 0)
                        {
                            isDataRight = false;
                            break;
                        }
                    }
                    //if (theModel.readings[0] != 0 && theModel.readings[1] != 0 && theModel.readings[2] != 0 && theModel.readings[3] != 0 && theModel.readings[4] == 0)
                    if(isDataRight == true)
                    {
                        for (int nCartridgeRep = 0; nCartridgeRep < 6; nCartridgeRep++)
                        {
                            for (int nChannelRep = 0; nChannelRep < 4; nChannelRep++)
                            {
                                allReadings[nCartridgeRep, nChannelRep] = theModel.readings[nCartridgeRep * 4 + nChannelRep];//- theModel.optic_zero[nChannel - 2];
                                if (nCartridgeRep == 0)
                                    theModel.m_data_r1.Add(allReadings[nCartridgeRep, nChannelRep]);
                            }

                        }

                        for (int nCartridgeRep = 0; nCartridgeRep < 6; nCartridgeRep++)
                        {
                            for (int nChannelRep = 0; nChannelRep < 4; nChannelRep++)
                                outLine.Append("," + allReadings[nCartridgeRep, nChannelRep].ToString());
                        }

                        theModel.NewPDReadingsTaken(allReadings);
                        // double[] temperatures = theModel.opticsBoard.ReadTemperatures();

                        // outLine.Append("," + string.Join(",", temperatures));

                        theModel.PCRFileStream.WriteLine(outLine);
                    }
                    break;
                }
                Thread.Sleep(100);
                samplCounter++;
                if(samplCounter>=200)
                {
                    samplCounter = 0;
                    break;
                }

            }

            //theModel.OpticsModel.SetLedCurrent(1, 0);//turn off led
            controller.Terminate(strChannel);              // Turn off the moto

            

            return bRetVal;
        }
    }

    /// <summary>
    /// Represents an individual protocol step.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class MeltCurveStep : ProtocolStep
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected double startTemp;
        protected double endTemp;
        protected double rampRate;

        


        public MeltCurveStep(int nLine, double startTempIn, double endTempIn, double rampRateIn) : base(nLine)
        {
            startTemp = startTempIn;
            endTemp = endTempIn;
            rampRate = rampRateIn;

           
        }
        /*
        public override async Task<bool> Execute()
        {
            bool bRetVal = true;

            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;

            List<Task> initTasks = new List<Task>();
            
            // Execute a ramp up.

            theModel.DevicePCRCycler.gotoTempAndStay(4, startTemp, 60);


            //theModel.DevicePCRCycler.SetToAppropriateBand((float)this.startTemp, this.endTemp, theModel.Config.m_TEC_Channel_Configurations["TEC_1"].m_Step_PID_RampUp_Range_List);

            //theModel.bUnderTempPlan = true;

            // Set us up for a linear ramp.

            float oldPBand0 = theModel.tecBoard.theBoard.GetPBand(4);
            // float oldPBand1 = theModel.tecBoard.theBoard.GetPBand(5);
            theModel.tecBoard.theBoard.SetPBand(4, 0f);
            // theModel.tecBoard.theBoard.SetPBand(5, 0f);
            logger.Debug("Meltcurve: setting the ramprate to {0}", rampRate.ToString());
            theModel.tecBoard.theBoard.SetHeatRate(4, (float)rampRate);
            //theModel.tecBoard.theBoard.SetHeatRate(5, (float)rampRate);
            theModel.tecBoard.theBoard.SetCurveProfile(4, 0);
            //theModel.tecBoard.theBoard.SetCurveProfile(5, 0);

            // The temperature is not making it there, so we are intentionally setting a 
            // value 10 degrees higher.
            // Doing a Max here so we don't fry anything.

            double excessiveTargetTemperature = Math.Min(endTemp + 10f, 105);

            theModel.bRamping = true;
            theModel.DevicePCRCycler.Temperature = excessiveTargetTemperature;
            //theModel.DevicePCRCycler.Temperature2 = excessiveTargetTemperature;

            theModel.ProtocolDescLine = string.Format("     ramping started");

            string meltLogFileName = Path.Combine(theModel.resultsDir, "Melt Curve logging.csv");
            logger.Debug("Opening melt results file to: {0}", meltLogFileName);
            //meltLogFileName.ElementAt(0);

            using (StreamWriter meltStream = new StreamWriter(meltLogFileName))
            {
                meltStream.WriteLine("MeltCurveTemperature," +
                    "Cartridge 1 Channel 1, Cartridge 1 Channel 2, Cartridge 1 Channel 3, Cartridge 1 Channel 4," +
                    "Cartridge 2 Channel 1, Cartridge 2 Channel 2, Cartridge 2 Channel 3, Cartridge 2 Channel 4," +
                    "Cartridge 3 Channel 1, Cartridge 3 Channel 2, Cartridge 3 Channel 3, Cartridge 3 Channel 4," +
                    "Cartridge 4 Channel 1, Cartridge 4 Channel 2, Cartridge 4 Channel 3, Cartridge 4 Channel 4," +
                    "Cartridge 5 Channel 1, Cartridge 5 Channel 2, Cartridge 5 Channel 3, Cartridge 5 Channel 4," +
                    "Cartridge 6 Channel 1, Cartridge 6 Channel 2, Cartridge 6 Channel 3, Cartridge 6 Channel 4"
                    );

                //string outLine = "";
                StringBuilder outLine = new StringBuilder(256);

                double curTemp = 0;

                for (int nReadChannel = 0; nReadChannel < 4; nReadChannel++)
                {
                    theModel.OpticsModel.SetLedCurrent(nReadChannel + 1, 1.0);
                }

                // This is the meltcurve loop.
                //theModel.DeviceOpticsMotor.Position = 3;//2018

                double[,] allReadings = new double[6, 4];
                double[] temp_data = new double[4];
                while (curTemp <= endTemp && (!theModel.bStopScript))
                {
                    curTemp = theModel.DevicePCRCycler.Temperature;
                    logger.Debug("In meltcurve, curTemp = {0}", curTemp.ToString());

                    outLine.Clear();
                    outLine.Append(curTemp.ToString());

                    theModel.m_data_t.Add(curTemp);

                    for (int nChannel = 2; nChannel < 6; nChannel++)
                    {
                        theModel.DeviceOpticsMotor.Position = nChannel;
                        double[] readings = theModel.opticsBoard.ReadPhotodetector();

                        for (int nCartridge = 0; nCartridge < 6; nCartridge++)
                        //allReadingnCartridge[nCartridge, nChannel - 2] = readings[nCartridge];
                        {
                            if (nCartridge == 2)
                                allReadings[nCartridge, nChannel - 2] = readings[nCartridge] - theModel.optic_zero[nChannel - 2];
                            else
                                allReadings[nCartridge, nChannel - 2] = readings[nCartridge];
                        }

                        theModel.m_data_r1.Add(allReadings[2, nChannel - 2]);
                        //temp_data[nChannel - 2] = allReadings[2, nChannel - 2];
                        // Thread.Sleep(3000);

                    }
                    //Thread.Sleep(1000);
                    //theModel.m_data_r1.Add(temp_data);


                    for (int nCartridgeRep = 0; nCartridgeRep < 6; nCartridgeRep++)
                    {
                        for (int nChannelRep = 0; nChannelRep < 4; nChannelRep++)
                            outLine.Append("," + allReadings[nCartridgeRep, nChannelRep].ToString());

                        //theModel. m_data_t = new List<double>();

                    }

                    theModel.NewPDReadingsTaken(allReadings);

                    string strChannel = theModel.DeviceOpticsMotor.getChannel().ToString("00");
                    clsAMB controller = theModel.getMotorBoard(theModel.DeviceOpticsMotor.getMotorBoard());
                    // Move the pump to the initial position.

                    controller.MoveToAbsolutePosition(strChannel,
                     "2000");
                    controller.WaitForCompletion(strChannel);

                    // controller.SetDirectionPolarity(strChannel, "0");
                    theModel.DeviceOpticsMotor.Speed = 200;

                    controller.MoveHome(strChannel);
                    controller.WaitForCompletion(strChannel);
                    theModel.DeviceOpticsMotor.Speed = 400;

                    meltStream.WriteLineAsync(outLine.ToString());
                    meltStream.FlushAsync();
                    Thread.Sleep(1000);
                }
            }

            theModel.ProtocolDescLine = string.Format("     ramping completed");

            // Turn down the LEDS

            //for (int nReadChannel = 0; nReadChannel < 4; nReadChannel++)
            //{
            //   theModel.OpticsModel.SetLedCurrent(nReadChannel+1, 0.75);
            // }

            // Put the settings back.

            theModel.tecBoard.theBoard.SetPBand(4, oldPBand0);
            //theModel.tecBoard.theBoard.SetPBand(5, oldPBand1);
            theModel.tecBoard.theBoard.SetCurveProfile(4, 1);
            //theModel.tecBoard.theBoard.SetCurveProfile(5, 1);

            theModel.bRamping = false;

            return bRetVal;
        }*/
        /// <summary>
        /// Executes the specified protocol step.
        /// </summary>
        /// <returns></returns>
        
        public override async Task<bool>    Execute()
        {
            bool bRetVal = true;

            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;

            List<Task> initTasks = new List<Task>();
            
            // Execute a ramp up.

            //theModel.DevicePCRCycler.gotoTempAndStay(3, startTemp, 60);
            theModel.DevicePCRCycler.gotoTempAndStay(4, startTemp, 60);


            //theModel.DevicePCRCycler.SetToAppropriateBand((float)this.startTemp, this.endTemp, theModel.Config.m_TEC_Channel_Configurations["TEC_1"].m_Step_PID_RampUp_Range_List);

            //theModel.bUnderTempPlan = true;

            // Set us up for a linear ramp.

            /*
            float oldPBand0 = theModel.tecBoard.theBoard.GetPBand(4);
           // float oldPBand1 = theModel.tecBoard.theBoard.GetPBand(5);
            theModel.tecBoard.theBoard.SetPBand(4, 0f);
           // theModel.tecBoard.theBoard.SetPBand(5, 0f);
            logger.Debug("Meltcurve: setting the ramprate to {0}", rampRate.ToString());
            theModel.tecBoard.theBoard.SetHeatRate(4, (float)rampRate);
            //theModel.tecBoard.theBoard.SetHeatRate(5, (float)rampRate);
            theModel.tecBoard.theBoard.SetCurveProfile(4, 0);
            //theModel.tecBoard.theBoard.SetCurveProfile(5, 0);

            */
            float oldPBand0 =0;
            for (int i = 3; i < 5; i++)
            {
                 oldPBand0 = theModel.tecBoard.theBoard.GetPBand(i);
                // float oldPBand1 = theModel.tecBoard.theBoard.GetPBand(5);
                theModel.tecBoard.theBoard.SetPBand(i, 0f);
                // theModel.tecBoard.theBoard.SetPBand(5, 0f);
                logger.Debug("Meltcurve: setting the ramprate to {0}", rampRate.ToString());
                if(i==3)
                theModel.tecBoard.theBoard.SetHeatRate(i, (float)(rampRate));
                else
                    theModel.tecBoard.theBoard.SetHeatRate(i, (float)rampRate);
                //theModel.tecBoard.theBoard.SetHeatRate(5, (float)rampRate);
                theModel.tecBoard.theBoard.SetCurveProfile(i, 0);
            }
            

            // The temperature is not making it there, so we are intentionally setting a 
            // value 10 degrees higher.
            // Doing a Max here so we don't fry anything.

            double excessiveTargetTemperature = Math.Min(endTemp + 10f, 105);

            theModel.bRamping = true;
            
            theModel.DevicePCRCycler[3] = excessiveTargetTemperature;
            theModel.DevicePCRCycler.Temperature = excessiveTargetTemperature;


            theModel.ProtocolDescLine = string.Format("     ramping started");

            string meltLogFileName = Path.Combine(theModel.resultsDir, "Melt Curve logging.csv");
            logger.Debug("Opening melt results file to: {0}", meltLogFileName);
            

            string strChannel = theModel.DeviceOpticsMotor.getChannel().ToString("00");
            clsAMB controller = theModel.getMotorBoard(theModel.DeviceOpticsMotor.getMotorBoard());

            using (StreamWriter meltStream = new StreamWriter(meltLogFileName))
            {
                
                meltStream.WriteLine("MeltCurveTemperature," +
                    "Cartridge 1 Channel 1, Cartridge 1 Channel 2, Cartridge 1 Channel 3, Cartridge 1 Channel 4," +
                    "Cartridge 2 Channel 1, Cartridge 2 Channel 2, Cartridge 2 Channel 3, Cartridge 2 Channel 4," +
                    "Cartridge 3 Channel 1, Cartridge 3 Channel 2, Cartridge 3 Channel 3, Cartridge 3 Channel 4," +
                    "Cartridge 4 Channel 1, Cartridge 4 Channel 2, Cartridge 4 Channel 3, Cartridge 4 Channel 4," +
                    "Cartridge 5 Channel 1, Cartridge 5 Channel 2, Cartridge 5 Channel 3, Cartridge 5 Channel 4,"  +
                    "Cartridge 6 Channel 1, Cartridge 6 Channel 2, Cartridge 6 Channel 3, Cartridge 6 Channel 4"
                    );

                //string outLine = "";
                StringBuilder outLine = new StringBuilder(256);

                double curTemp = 0;

                // This is the meltcurve loop.
                //theModel.DeviceOpticsMotor.Position = 3;//2018
                
                theModel.OpticsModel.SetLedCurrent(1, 0.2);
                controller.MoveRelativePositive(strChannel, "100000000");
                theModel.opticsBoard.TriggerPhotodetector(true);//begin resolve sample

                double[,] allReadings = new double[6, 4];
                double[] temp_data = new double[4];

                //theModel.opticsBoard._devOptics.sPacketEnd0aGot = false;
                theModel.opticsBoard._devOptics.sCurrPacketLen = 0;
                theModel.opticsBoard._devOptics.sPacketIdGot = false;

                theModel.opticsBoard._devOptics.sPacketEnd0aGot = false;
                theModel.opticsBoard._devOptics.gCommErr = false;

               // string value;
               // int counter_cycle_moto = 0;

                while (curTemp <= endTemp && (!theModel.bStopScript))
                {

                    curTemp = theModel.DevicePCRCycler.Temperature;
                    //curTemp = theModel.DevicePCRCycler[3];
                    logger.Debug("In meltcurve, curTemp = {0}", curTemp.ToString());
                    outLine.Clear();
                    outLine.Append(curTemp.ToString()); 
                    theModel.m_data_t.Add(curTemp);

                    
                    if(theModel.opticsBoard.UnpackFrame())
                    {
                        
                        bool isDataRight = true;
                        
                        for (int i=0;i<9;i++)
                        {
                            if(theModel.readings[i] >= 0)
                            {
                                isDataRight = false;
                                break;
                            }
                        }
                       // if (theModel.readings[0]!=0&& theModel.readings[1]!=0 && theModel.readings[2] != 0 && theModel.readings[3] != 0 && theModel.readings[4] == 0)
                       if(isDataRight==true)
                        {
                            for (int nCartridgeRep = 0; nCartridgeRep < 6; nCartridgeRep++)
                            {
                                for (int nChannelRep = 0; nChannelRep < 4; nChannelRep++)
                                {
                                    allReadings[nCartridgeRep, nChannelRep] = theModel.readings[nCartridgeRep * 4 + nChannelRep];//- theModel.optic_zero[nChannel - 2];
                                    if (nCartridgeRep == 0)
                                        theModel.m_data_r1.Add(allReadings[nCartridgeRep, nChannelRep]);
                                }

                            }

                            // theModel.m_data_r1.Add(allReadings[0, 0]);

                            for (int nCartridgeRep = 0; nCartridgeRep < 6; nCartridgeRep++)
                            {
                                for (int nChannelRep = 0; nChannelRep < 4; nChannelRep++)
                                    outLine.Append("," + allReadings[nCartridgeRep, nChannelRep].ToString());

                                //theModel. m_data_t = new List<double>();

                            }

                            theModel.NewPDReadingsTaken(allReadings);


                            
                            try {  meltStream.WriteLineAsync(outLine.ToString()); }
                            catch (Exception ex)
                            {
                                logger.Debug("stingProcess, got an exception: {0}", ex.Message);

                            }
                             meltStream.FlushAsync();
                        }
                        
                        Thread.Sleep(100);

                    }
                    
                    //Thread.Sleep(1000);
                    //theModel.m_data_r1.Add(temp_data);


                    
                }
            }
            controller.Terminate(strChannel);              // Turn off the moto
            theModel.OpticsModel.StopSample();              //stop sample
            theModel.OpticsModel.SetLedCurrent(1, 0);      // Turn down the LEDS
            
            theModel.ProtocolDescLine = string.Format("     ramping completed");

            for (int i = 3; i < 5; i++)
            {
                // Put the settings back.
                theModel.tecBoard.theBoard.SetPBand(i, oldPBand0);
                //theModel.tecBoard.theBoard.SetPBand(5, oldPBand1);
                theModel.tecBoard.theBoard.SetCurveProfile(i, 1);
                //theModel.tecBoard.theBoard.SetCurveProfile(5, 1);

            }

            theModel.bRamping = false;

            return bRetVal;
        }
    }

    /// <summary>
    /// Contains code to load and execute a protocol.
    /// </summary>
    public class Protocol
    {
        protected List<ProtocolStep> _steps;

        /// <summary>
        /// The author of the script.
        /// </summary>
        protected string _author;

        /// <summary>
        /// The script date.
        /// </summary>
        protected DateTime _date;

        /// <summary>
        /// The description of the protocol, as stated in the xml file.
        /// </summary>
        protected string _description;

        private static Logger logger = LogManager.GetCurrentClassLogger();


        public Protocol()
        {
            _steps = new List<ProtocolStep>();
        }

        /// <summary>
        /// Accepts a parent node and creates a list of ProtocolSteps from it.
        /// </summary>
        /// <param name="steps_node"></param>
        /// <returns></returns>
        public List<ProtocolStep> ProcessCodeRun(Positional_XmlElement steps_node)
        {
            List<ProtocolStep> steps = new List<ProtocolStep>();

            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;

            foreach (Positional_XmlElement step_node in steps_node.ChildNodes)
            {
                switch (step_node.Name.ToLower())
                {
                    case "step":

                        ProtocolStep currentStep = new ProtocolStep(step_node.LineNumber);

                        // Run down the list of attributes

                        foreach (XmlAttribute attr in step_node.Attributes)
                        {
                            // Note that in the code below, the actions are being pushed onto the 
                            // front of the list of actions, only to ensure that the time action is
                            // last on the list.

                            switch (attr.Name.ToLower())
                            {
                                case "desc":
                                    currentStep.description = attr.InnerText;
                                    break;
                                case "heater":
                                    {
                                        double temperature = 0;

                                        try
                                        {
                                            temperature = double.Parse(attr.InnerText);
                                        }
                                        catch (Exception /*ex*/)
                                        {
                                            logger.Error("Protocol Line: {1} Incorrectly formed temperature value; \"{0}\", expecting a real value.",
                                                attr.InnerText,
                                                step_node.LineNumber.ToString());
                                            throw;
                                        }

                                        heaterAction ha = new heaterAction(temperature);
                                        currentStep.Actions.Insert(0, ha);
                                    }
                                    break;
                                case "heaterctrl":
                                    {
                                        double temperature = 0;

                                        try
                                        {
                                            temperature = double.Parse(attr.InnerText);
                                        }
                                        catch (Exception /*ex*/)
                                        {
                                            logger.Error("Protocol Line: {1} Incorrectly formed temperature value; \"{0}\", expecting a real value.",
                                                attr.InnerText,
                                                step_node.LineNumber.ToString());
                                            throw;
                                        }

                                        heaterCtrlAction hctrl = new heaterCtrlAction(temperature);
                                        currentStep.Actions.Insert(0, hctrl);
                                    }
                                    break;
                                case "heaterpiston":
                                    {
                                        int speed = -999;

                                        int cntPositions = theModel.Config.m_HeaterPiston_Configuration.positions.Count();

                                        int position = ConvertAndRangeCheckPosition(attr, 1, cntPositions, step_node.LineNumber, ref speed);
                                        heaterPistonAction hpa = new heaterPistonAction(position, speed);
                                        currentStep.Actions.Insert(0, hpa);
                                    }
                                    break;
                                case "heaterprn":
                                    {
                                        int speed = -999;
                                        int cntPositions = 200000;//theModel.Config.m_Slider_Configuration.positions.Count();

                                        int position = ConvertAndRangeCheckPosition(attr, 1, cntPositions, step_node.LineNumber, ref speed);
                                        HeatNPosition hprn = new HeatNPosition(position);
                                        currentStep.Actions.Insert(0, hprn);
                                    }
                                    break;
                                case "heaterprnn":
                                    {
                                        int speed = -999;
                                        int cntPositions = 200000;//theModel.Config.m_Slider_Configuration.positions.Count();

                                        int position = ConvertAndRangeCheckPosition(attr, 1, cntPositions, step_node.LineNumber, ref speed);
                                        HeatNNPosition hprnn = new HeatNNPosition(position);
                                        currentStep.Actions.Insert(0, hprnn);
                                    }
                                    break;
                                case "pcr":
                                    {
                                        double temperature = 0;

                                        try
                                        {
                                            temperature = double.Parse(attr.InnerText);
                                        }
                                        catch (Exception /*ex*/)
                                        {
                                            logger.Error("Protocol Line: {1} Incorrectly formed temperature value; \"{0}\", expecting a real value.",
                                                attr.InnerText,
                                                step_node.LineNumber.ToString());
                                            throw;
                                        }

                                        pcrAction pcra = new pcrAction(temperature);
                                        currentStep.Actions.Insert(0, pcra);
                                    }
                                    break;
                                case "overtemp":
                                    {
                                        double temperature = 0;

                                        try
                                        {
                                            temperature = double.Parse(attr.InnerText);
                                        }
                                        catch (Exception /*ex*/)
                                        {
                                            logger.Error("Protocol Line: {1} Incorrectly formed temperature value; \"{0}\", expecting a real value.",
                                                attr.InnerText,
                                                step_node.LineNumber.ToString());
                                            throw;
                                        }

                                        overTempAction overTemp = new overTempAction(temperature);
                                        currentStep.Actions.Insert(0, overTemp);
                                    }
                                    break;
                                case "pump":
                                    {
                                        string value = attr.InnerText.ToLower();

                                        if (value != "on" && value != "off")
                                        {
                                            string msg = string.Format("Protocol Line: {1}, value: \"{0}\" is invalid, expecting either \"on\" or \"off\".",
                                                 attr.InnerText,
                                                 step_node.LineNumber.ToString());
                                            logger.Error(msg);
                                            throw new Exception(msg);
                                        }

                                        pumpAction pa = new pumpAction(value == "on");
                                        currentStep.Actions.Insert(0, pa);
                                    }
                                    break;
                                case "r1":
                                    {
                                        int speed = -999;

                                        int cntPositions = theModel.Config.m_R1Piston_Configuration.positions.Count();
                                        int position = ConvertAndRangeCheckPosition(attr, 1, cntPositions, step_node.LineNumber, ref speed);
                                        r1Action r1a = new r1Action(position, speed);
                                        currentStep.Actions.Insert(0, r1a);
                                    }
                                    break;
                                    //2018
                                case "r1rn":
                                    {
                                        int speed = -999;
                                        int cntPositions = 200000;//theModel.Config.m_Slider_Configuration.positions.Count();

                                        int position = ConvertAndRangeCheckPosition(attr, 1, cntPositions, step_node.LineNumber, ref speed);
                                        r1RNPosition r1rn = new r1RNPosition(position);
                                        currentStep.Actions.Insert(0, r1rn);
                                    }
                                    break;
                                case "r1rnn":
                                    {
                                        int speed = -999;
                                        int cntPositions = 200000;//theModel.Config.m_Slider_Configuration.positions.Count();

                                        int position = ConvertAndRangeCheckPosition(attr, 1, cntPositions, step_node.LineNumber, ref speed);
                                        r1RNNPosition r1rnn = new r1RNNPosition(position);
                                        currentStep.Actions.Insert(0, r1rnn);
                                    }
                                    break;
                                case "r2rn":
                                    {
                                        int speed = -999;
                                        int cntPositions = 200000;// theModel.Config.m_Slider_Configuration.positions.Count();

                                        int position = ConvertAndRangeCheckPosition(attr, 1, cntPositions, step_node.LineNumber, ref speed);
                                        r2RNPosition r2rn = new r2RNPosition(position);
                                        currentStep.Actions.Insert(0, r2rn);
                                    }
                                    break;
                                case "r2rnn":
                                    {
                                        int speed = -999;
                                        int cntPositions = 200000;//theModel.Config.m_Slider_Configuration.positions.Count();

                                        int position = ConvertAndRangeCheckPosition(attr, 1, cntPositions, step_node.LineNumber, ref speed);
                                        r2RNNPosition r2rnn = new r2RNNPosition(position);
                                        currentStep.Actions.Insert(0, r2rnn);
                                    }
                                    break;

                                case "r2":
                                    {
                                        int speed = -999;
                                        int cntPositions = theModel.Config.m_R2Piston_Configuration.positions.Count();

                                        int position = ConvertAndRangeCheckPosition(attr, 1, cntPositions, step_node.LineNumber, ref speed);
                                        r2Action r2a = new r2Action(position, speed);
                                        currentStep.Actions.Insert(0, r2a);
                                    }
                                    break;
                                case "slider":
                                    {
                                        int speed = -999;
                                        int cntPositions = theModel.Config.m_Slider_Configuration.positions.Count();

                                        int position = ConvertAndRangeCheckPosition(attr, 1, cntPositions, step_node.LineNumber, ref speed);
                                        sliderAction slidera = new sliderAction(position);
                                        currentStep.Actions.Insert(0, slidera);
                                    }
                                    break;
                                case "chasis":
                                    {
                                        int speed = -999;
                                        int cntPositions = theModel.Config.m_ChassisPiston_Configuration.positions.Count();

                                        int position = ConvertAndRangeCheckPosition(attr, 1, cntPositions, step_node.LineNumber, ref speed);
                                        chasisAction chasisera = new chasisAction(position);
                                        currentStep.Actions.Insert(0, chasisera);
                                    }
                                    break;
                                case "sliderrn":
                                    {
                                        int speed = -999;
                                        int cntPositions = 200000;//theModel.Config.m_Slider_Configuration.positions.Count();

                                        int position = ConvertAndRangeCheckPosition(attr, 1, cntPositions, step_node.LineNumber, ref speed);
                                        SlideNPosition slidern = new SlideNPosition(position);
                                        currentStep.Actions.Insert(0, slidern);
                                    }
                                    break;
                                case "sliderrnn":
                                    {
                                        int speed = -999;
                                        int cntPositions = 200000;//theModel.Config.m_Slider_Configuration.positions.Count();

                                        int position = ConvertAndRangeCheckPosition(attr, 1, cntPositions, step_node.LineNumber, ref speed);
                                        SlideNNPosition slidernn = new SlideNNPosition(position);
                                        currentStep.Actions.Insert(0, slidernn);
                                    }
                                    break;
                                case "time":
                                    {
                                        int time = 0;

                                        try
                                        {
                                            time = int.Parse(attr.InnerText);
                                        }
                                        catch (Exception /*ex*/)
                                        {
                                            logger.Error("Protocol Line: {1} Incorrectly formed time value; \"{0}\", expecting a real value.",
                                                attr.InnerText,
                                                step_node.LineNumber.ToString());
                                            throw;
                                        }

                                        timeAction timea = new timeAction(time);
                                        currentStep.Actions.Add(timea);
                                    }
                                    break;
                            }
                        }

                        // Process children nodes if there are any.

                        if (step_node.ChildNodes.Count > 0)
                        {
                            Positional_XmlElement pcr_nodes = (Positional_XmlElement)step_node.SelectSingleNode("pcr");
                            //Positional_XmlElement pcr_nodes = (Positional_XmlElement)step_node.ChildNodes;

                            currentStep.PCRActions = ProcessCodeRun(pcr_nodes);
                        }

                        steps.Add(currentStep);
                        break;
                    case "cycle":
                        int nRepetitions = (int)(theModel.Config.m_MiscellaneousConfiguration.m_fThermalRampNumSteps);

                        foreach (XmlAttribute attr in step_node.Attributes)
                        {
                            switch (attr.Name.ToLower())
                            {
                                case "repetitions":
                                    try
                                    {
                                        nRepetitions = int.Parse(attr.Value);
                                    }
                                    catch (Exception ex)
                                    {
                                        // TODO; badly formed value, do something about it.
                                    }
                                    break;
                                default:
                                    // TODO; erroneous value, do something about it.
                                    break;
                            }
                        }

                        currentStep = new CycleStep(step_node.LineNumber, nRepetitions);

                        if (step_node.ChildNodes.Count > 0)
                        {
                            Positional_XmlElement pcr_nodes = (Positional_XmlElement)step_node.SelectSingleNode("steps");
                            //Positional_XmlElement pcr_nodes = (Positional_XmlElement)step_node.ChildNodes;

                            currentStep.PCRActions = ProcessCodeRun(pcr_nodes);
                        }
                        steps.Add(currentStep);
                        break;
                    case "sample":
                        currentStep = new SampleStep(step_node.LineNumber);
                        steps.Add(currentStep);
                        break;
                    case "meltcurve":

                        double startTemp = 0f;
                        double endTemp = 0f;
                        double rampRate = theModel.Config.m_MiscellaneousConfiguration.m_fThermalRampPCRStep;

                        foreach (XmlAttribute attr in step_node.Attributes)
                        {
                            switch (attr.Name.ToLower())
                            {
                                case "start":
                                    try
                                    {
                                        startTemp = double.Parse(attr.Value);
                                    }
                                    catch (Exception ex)
                                    {
                                        // TODO; badly formed value, do something about it.
                                    }
                                    break;
                                case "end":
                                    try
                                    {
                                        endTemp = double.Parse(attr.Value);
                                    }
                                    catch (Exception ex)
                                    {
                                        // TODO; badly formed value, do something about it.
                                    }
                                    break;
                                case "ramprate":
                                    try
                                    {
                                        rampRate = double.Parse(attr.Value);
                                    }
                                    catch (Exception ex)
                                    {
                                        // TODO; badly formed value, do something about it.
                                    }
                                    break;
                                default:
                                    // TODO; erroneous value, do something about it.
                                    break;
                            }
                        }

                        currentStep = new MeltCurveStep(step_node.LineNumber, startTemp, endTemp, rampRate);
                        steps.Add(currentStep);
                        break;
                    default:
                        break;
                }


            }

            return steps;
        }

        /// <summary>
        /// Loads a protocol from the specified file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public async Task<bool> Load(string filename)
        {
            bool bRetVal = true;

            // Strip all comments prior to parsing, as derived Positional_XmlElement class with line numbers can't handle XmlComment nodes

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = null;

            try
            {
                reader = XmlReader.Create(filename, settings);
            }
            catch (Exception ex)
            {
                logger.Error("Configuration::Load Caught an exception; {0}", ex.Message);
                bRetVal = false;
            }

            if (bRetVal)
            {
                HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;

                Positional_XMLDocument doc = new Positional_XMLDocument();

                try
                {
                    doc.Load(reader);
                }
                catch (Exception Exc)
                {
                    throw new CPCRInstrumentSystemException("Configuration file read error.  Positional_XMLDocument.Load(\"" + filename + "\") failed.  Confirm correct XML content by examining it with an XML aware editor such as XML_Notepad.\n", Exc.ToString());
                }

                // Process the description node.

                Positional_XmlElement desc_node = (Positional_XmlElement)doc.DocumentElement.SelectSingleNode("/protocol/description");

                _author = desc_node.GetAttribute("author");

                try
                {
                    _date = DateTime.Parse(desc_node.GetAttribute("date"));
                }
                catch (Exception /*ex*/)
                {
                    // There was an error parsing the date format.
                    // This is not a show stopper.
                    // TBD; Deal with this.
                }

                _description = desc_node.GetAttribute("text");

                // Process the steps.

                Positional_XmlElement steps_node = (Positional_XmlElement)doc.DocumentElement.SelectSingleNode("/protocol/steps");

                _steps = ProcessCodeRun(steps_node);
            }

            return bRetVal;
        }

        public async Task<bool> Execute()
        {
            bool bRetVal = true;

            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;

            // Setup the PCR Realtime stream in the model.

            string PCRFileName = Path.Combine(theModel.resultsDir, "Real time PCR data.csv");
            theModel.PCRFileStream = new StreamWriter(PCRFileName);

            theModel.PCRFileStream.AutoFlush = true;
            theModel.PCRFileStream.WriteLine(string.Format("Protocol file: {0}", Path.GetFullPath(theModel.CurrentScript)));

            theModel.PCRFileStream.WriteLine("PCR Cycle#, " +
                "Cartridge 1 Channel 1, Cartridge 1 Channel 2, Cartridge 1 Channel 3, Cartridge 1 Channel 4," +
                "Cartridge 2 Channel 1, Cartridge 2 Channel 2, Cartridge 2 Channel 3, Cartridge 2 Channel 4," +
                "Cartridge 3 Channel 1, Cartridge 3 Channel 2, Cartridge 3 Channel 3, Cartridge 3 Channel 4," +
                "Cartridge 4 Channel 1, Cartridge 4 Channel 2, Cartridge 4 Channel 3, Cartridge 4 Channel 4," +
                "Cartridge 5 Channel 1, Cartridge 5 Channel 2, Cartridge 5 Channel 3, Cartridge 5 Channel 4," +
                "Cartridge 6 Channel 1, Cartridge 6 Channel 2, Cartridge 6 Channel 3, Cartridge 6 Channel 4," +
                "Sensor Temp 1, Sensor Temp 2, Sensor Temp 3, Sensor Temp 4, Sensor Temp 5, Sensor Temp 6"

                );

            // Turn on the LEDS//2018
            /*
            for ( int nLED = 0; nLED < 6; nLED++)
            {
                theModel.OpticsModel.SetLedCurrent(nLED+1, 0.75);
                //theModel.OpticsModel.SetLedCurrent(nLED + 1, 0);
            }*/

            theModel.PCRCycleNum = 0;
            /*
            for (int i = 0; i < 15; i++)
            {
                SampleStep currentStep = new SampleStep(0);
                currentStep.Execute();
            }*/

            // Put out a status message that we're starting the protocol.

            foreach (ProtocolStep currentStep in _steps)
            {
                if (theModel.bStopScript)
                {
                    logger.Debug("Protocol: Execute stopping the script.");
                    break;
                }

                try
                {
                    theModel.ProtocolDescLine = currentStep.description;
                    logger.Debug("Protocol: Execute step: {0} line: {1}", currentStep.ToString(),
                        currentStep.line.ToString());
                    currentStep.Execute();
                }
                catch (Exception ex)
                {
                    logger.Debug("Protocol::Execute caught an exception: {0}", ex.Message);
                    bRetVal = false;
                    break;
                }
            }

            theModel.PCRFileStream.Close();
            theModel.ControlComplete();

            // Turn the LEDS off.
            /*
            for (int nLED = 0; nLED < 6; nLED++)
            {
                theModel.OpticsModel.SetLedCurrent(nLED+1, 0.00);
            }
            */
            return bRetVal;
        }


        /// <summary>
        /// Convenience function that range checks the text value.
        /// </summary>
        /// <remarks>
        /// Function first tries to convert textValue to an integer.  If this
        /// fails for some reason, it prints a log message and throws an expection.
        /// 
        /// The function then range checks the value, as per the parameters, and 
        /// if this fails, it prints a log message and throws an exception.
        /// 
        /// This parsing allows for two forms;
        /// 
        /// 1. Just an integer.
        /// 
        /// or 2. An integer with a speed specifier.  This is of the form 2@300
        /// meaning, position 2 at a speed of 300.
        /// </remarks>
        /// <param name="attrIn">The text to be checked.</param>
        /// <param name="rangeMin">The lower range to be checked.  This is inclusive.</param>
        /// <param name="rangeMax">The upper range value to be check.  This is inclusive.</param>
        /// <param name="lineNo">The XML line number.</param>
        /// <returns>The valid position value.</returns>
        protected int ConvertAndRangeCheckPosition(XmlAttribute attrIn, int rangeMin, int rangeMax, int lineNo, ref int speed)
        {
            speed = -999;
            int position = 0;

            // Attempt the conversion.

            try
            {
                string[] theText = attrIn.InnerText.Split('@');
                position = int.Parse(theText[0]);

                if (theText.Count() > 1)
                {
                    speed = int.Parse(theText[1]);
                }
            }
            catch (Exception /*ex*/)
            {
                logger.Error("Protocol Line: {1}, Incorrectly formed position value; \"{0}\", expecting a real value.",
                    attrIn.InnerText, lineNo.ToString());
                throw;
            }

            // Perform the range check.

            if (position < rangeMin || position > rangeMax)
            {
                string msg = string.Format("Protocol Line: {1}, Position value out of range \"{0}\", expecting a value between {2} and {3} (inclusive).",
                     attrIn.InnerText, lineNo.ToString(), rangeMin.ToString(), rangeMax.ToString());
                logger.Error(msg);
                throw new Exception(msg);
            }

            return position;
        }

        /// <summary>
        /// Convenience function that returns the value of an attribute, or blank
        /// if the attribute is not present.
        /// </summary>
        /// <param name="elementIn">The xml node.</param>
        /// <param name="attribute">The attribute name</param>
        /// <returns>The value of the attribute if it exists, or blank.</returns>
        string getAttribute(Positional_XmlElement elementIn, string attribute)
        {
            string strOut = string.Empty;

            try
            {
                strOut = elementIn.Attributes[attribute].InnerText;
            }
            catch (Exception /*ex*/)
            {

            }

            return strOut;
        }

    }
}
