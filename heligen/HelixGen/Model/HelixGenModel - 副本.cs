// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using ABot2;
using HelixGen;
using HelixGen.Model.devices;
using NLog;
using Stateless;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

namespace HelixGen.Model
{
    /// <summary>
    /// Arguments object sent as part of the initializationProgress event.
    /// </summary>
    public class progressEventArgs : EventArgs
    {
        protected float _percentageComplete;
        protected string _msg;
        protected bool _bCompleted;

        public progressEventArgs(float percentageIn = 0.0f, string msgIn = "", bool bCompleted = false)
        {
            _percentageComplete = percentageIn;
            _msg = msgIn;
            _bCompleted = bCompleted;
        }

        /// <summary>
        /// The percentage complete.
        /// </summary>
        public float percentageComplete
        {
            get { return _percentageComplete; }
        }

        /// <summary>
        /// The textual message associated with the progress.
        /// </summary>
        public string message
        {
            get { return _msg; }
        }

        /// <summary>
        /// Indicates whether initialization has been completed.
        /// </summary>
        public bool completed
        {
            get { return _bCompleted; }
        }
    }

    /// <summary>
    /// This is the top level model object.
    /// </summary>
    /// <remarks>
    /// The main model contains the references to the controller boards.  These
    /// are initialized here, and the individual devices are then abstracted using
    /// references to the corresponding board models.
    /// </remarks>
    public class HelixGenModel : IDisposable
    {
        #region events

        public delegate void PCRCyclerTempChangedEvent(double temperature, double power, bool controlling, bool transitioning);
        public delegate void HeaterTempChangedEvent(double temp);
        public delegate void ProtocolExecutionStep(string desc);
        public delegate void PCRCyclerSetPointChanged(double temp);

        public delegate void PCRReadingsStarted();
        public delegate void PCRReadingsTaken(double[,] readingings);

        /// <summary>
        /// Regular updates about the PCR Temperature.
        /// </summary>
        public event PCRCyclerTempChangedEvent PCRCyclerTempChanged;
        public event PCRCyclerTempChangedEvent PCRCyclerTempChanged2;
        public event PCRCyclerSetPointChanged evtPCRCyclerSetPointTemperature;

        public event PCRReadingsTaken evtPCRReadingsTaken;
        public event PCRReadingsTaken evtPCRReadingsTaken2;
        public event PCRReadingsStarted evtPCRReadingsStarted;

        /// <summary>
        /// Regular updates regarding the heater.
        /// </summary>
        public event HeaterTempChangedEvent HeaterTempChanged;

        /// <summary>
        /// Regular updates regarding the heater.
        /// </summary>
        public event ProtocolExecutionStep ProtocolExecutionStepped;

        /// <summary>
        /// Reports the progress of the startup.
        /// </summary>
        public event EventHandler initializationProgress;

        #endregion

        #region submodels
        /// <summary>
        /// The optics board portion of the model.
        /// </summary>
        protected opticsBoardModel _opticsBoard;

        /// <summary>
        /// The motor board portion of the model.
        /// </summary>
        protected motorBoardModel _motorBoard;

        /// <summary>
        /// Motorboard X, the other motor board.
        /// </summary>
        protected motorBoardModel _motorBoardX;

        protected tecBoardModel _tecBoard;
        #endregion

        #region devices
        protected deviceSlider _deviceSlider;
        protected deviceR1Piston _deviceR1Piston;
        protected deviceR2Piston _deviceR2Piston;
        protected deviceHeaterPiston _deviceHeaterPiston;
        protected devicePCRCycler _devicePCRCycler;
        protected devicePump _devicePump;
        protected deviceOpticsMotor _deviceOpticsMotor;
        protected deviceHeater _deviceHeater;
        protected deviceChassisPiston _deviceChassisPiston;
        #endregion

        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The configuration settings part of the model.
        /// </summary>
        protected Configuration _config;

        /// <summary>
        /// The current protocol.
        /// </summary>
        protected Protocol _protocol;

        /// <summary>
        /// Maps from motor controller id to the actual controller.
        /// </summary>
        protected Dictionary<string, motorBoardModel> motorControllers;

        /// <summary>
        /// A line to be shown on the progress screen.
        /// </summary>
        protected string _protocolDescLine;

        protected double _PCRCyclerSetPointTemperature;

        /// <summary>
        /// Periodic thread that updates the temperatures.
        /// </summary>
        private Timer _poll_timer;

        /// <summary>
        /// The results directory for the current run.
        /// </summary>
        public string resultsDir;

        /// <summary>
        /// Controls the background process that records temperature results.
        /// </summary>
        public bool bStopTempResults;

        /// <summary>
        /// Full Path of the currently executing script.
        /// </summary>
        public string CurrentScript;

        /// <summary>
        /// Filestream used to write results for the PCR Realtime results.
        /// </summary>
        public StreamWriter PCRFileStream;

        /// <summary>
        /// The current cycle number if we're PCR cycling.
        /// </summary>
        public int PCRCycleNum;

        /// <summary>
        /// Set to true to stop the script.
        /// </summary>
        public bool bStopScript { get; set; }

        /// <summary>
        /// True if we're operating under a temperature plan.
        /// </summary>
        public bool[] bUnderTempPlan { get; set; }

        /// <summary>
        /// True if we're currently ramping.
        /// </summary>
        public bool bRamping { get; set; }

        /// <summary>
        /// Triggers used by the instrument.
        /// </summary>
        public string anaFilePath;
        public int catrigeSelect;

        /// <summary>
        /// Triggers used by the instrument.
        /// </summary>
        protected enum instrumentStateTrigger
        {
            trigComplete
        };

        public List<double> m_data_t;//melt temp
        //m_data_t = new List<double>();
        public List<double> m_data_r1;//melt data
        public List<double> m_data_r2;
        public List<double> _slope, _slope1;  //melt slope
        public List<double> m_data_pcr, m_data_pcr1;//pcr data
        //public List<int> m_pcr_cycle;//pcr cycle
        public List<double> m_pcr_t;//pcr temp
        public double[] optic_zero;

        // public double[,] allReadings = new double[6, 4];
        public double[] readings = new double[24];


        /// <summary>
        /// The top level model object.
        /// </summary>
        public HelixGenModel()
        {
            bUnderTempPlan = new bool[6];

            for (int ndx = 0; ndx < 6; ndx++)
                bUnderTempPlan[ndx] = false;

            bRamping = false;
            _protocol = new Protocol();

            motorControllers = new Dictionary<string, motorBoardModel>();

            // Construct the configuration.

            _config = new Configuration();

            // Construct the various board models.

            _opticsBoard = new opticsBoardModel(this);
            _motorBoard = new motorBoardModel(this);
            _motorBoardX = new motorBoardModel(this);
            _tecBoard = new tecBoardModel(this);

            resultsDir = "";

            m_data_t = new List<double>();
            m_data_r1 = new List<double>();
            _slope = new List<double>();
            optic_zero = new double[4];
        }

        //theModel.Analysis();
        public void  Analysis()
        {

            FileStream fs = new FileStream("C:\\Users\\Helixgen\\Desktop\\snp\\Circuit Boards\\Melt Curve logging.csv", FileMode.Open, FileAccess.Read, FileShare.None);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(936));
            string str = "";
            str = sr.ReadLine();
            m_data_r1.Clear();       
                //string s = Console.ReadLine();
            while (str != null)
            {
                str = sr.ReadLine();
                if (str != null)
                { 
                    string[] xu = new String[25];
                    xu = str.Split(',');
                    m_data_r1.Add(double.Parse(xu[9]));
                    m_data_r1.Add(double.Parse(xu[10]));
                    m_data_r1.Add(double.Parse(xu[11]));
                    m_data_r1.Add(double.Parse(xu[12]));
                    m_data_t.Add(double.Parse(xu[0]));
                }
                //string ser = xu[9];
                //string dse = xu[10];
                //if (ser == s)
                //{ Console.WriteLine(dse); break; }
            }
            sr.Close();


            for (uint uig_sample_index = 11; uig_sample_index < m_data_r1.Count/4; uig_sample_index++)
            {
                uint h=0, i=0, j=0, number_data_points=11,columnn=0;
                double k = 0, L = 0, m = 0, n = 0, p = 0;

                //h = (uint)(number_data_points / 2);
                for (columnn = 0; columnn < 4; columnn++)
                {
                    L = 0; m = 0; n = 0; p = 0;
                    for (i = 0; i < number_data_points; i++)
                    {
                        uint temp = uig_sample_index - number_data_points + i;
                        L += i;
                        m += m_data_r1.ElementAt((int)(temp * 4 + columnn));
                        n += i * m_data_r1.ElementAt((int)(temp * 4 + columnn));
                        p += i * i;
                    }

                    if ((number_data_points * p - L * L) != 0)
                        k = -(number_data_points * n - L * m) / (number_data_points * p - L * L);

                    _slope.Add(k);
                }
            }


        }

        //theModel.m_data_r1.Add(temp_data);
        //for (uint ui = 5; ui <theModel.m_data_t.Count-5 ; ui++)
        //{

        //}

        public async Task<bool> Initialize()
        {
            bool bInitialized = true;

            logger.Debug("----- Starting -----");

            SendProgress(0, "Initializing");

            // Kickoff a process to read the configuration.

            //_state = instrumentState.isSettingUp;

            CheckFileTimeStamps();

            Task<bool> configLoadResult = _config.Load(CSystem_Defns.strDefaultSystemConfigurationPath);

            SendProgress(0.1f, "Loading system settings.");

            // Await the completion of loading config.  

            bool bResult = await configLoadResult;

            // Do a quick check to see that all the COM ports are there, and if not,
            // suggest to the user that maybe the instrument is not turned on.

            List<string> comPortLists = new List<string>();
            comPortLists.Add(Config.m_MotorControllerConfigurations["MC-1"].m_strPort);
            comPortLists.Add(Config.m_MotorControllerConfigurations["MC-2"].m_strPort);
            comPortLists.Add(Config.m_ThermalControllerConfigurations["ACCEL-TEC-CNTRL-1"].m_strPort);
            //comPortLists.Add(Config.m_OpticsController_Configuration.m_strPort);//2018

            bool bGoodPort = false;
            
            while (!bGoodPort)
            {

                foreach (string sPort in comPortLists)
                {
                    SerialPort stuntPort = new SerialPort(sPort);
                    bGoodPort = true;

                    try
                    {
                        stuntPort.Open();
                        stuntPort.Close();
                    }
                    catch (Exception)
                    {
                        bGoodPort = false;
                    }

                    if (!bGoodPort)
                        break;
                }

                if (!bGoodPort)
                {
                    MessageBox.Show("Not all ports are present in the system.  Is the instrument turned on?",
                        "Can't find all ports");
                    break;
                }
            }

            // Things that require the configuration must be after this line.

            // Initialize the optics board.
            
           // _opticsBoard.Initialize();
            SendProgress(0.2f, "the Optics Subsystem.");

            List<Task> initTasks = new List<Task>();

            // Initialize the motor controller.


            Task<bool> bMbInit = _motorBoard.Initialize(Config.m_MotorControllerConfigurations["MC-1"].m_strPort);
            initTasks.Add(bMbInit);

            SendProgress(0.3f, "Initializing the first Motor Controller.");

            // Initialize the second motor controller.

            Task<bool> bMbXInit = _motorBoardX.Initialize(Config.m_MotorControllerConfigurations["MC-2"].m_strPort);
            initTasks.Add(bMbXInit);

            SendProgress(0.4f, "Initializing the second Motor Controller.");

            // Initialize the temp controller.

            Task<bool> bTbInit = _tecBoard.Initialize();
            initTasks.Add(bTbInit);

            SendProgress(0.5f, "Initializing the Temperature controller.");

            // Wait for all the boards to init.

            await Task.WhenAll(initTasks);

            motorControllers["MC-1"] = _motorBoard;
            motorControllers["MC-2"] = _motorBoardX;

            // Construct the devices.

            _deviceSlider = new deviceSlider(motorControllers[_config.m_Slider_Configuration.m_strControllerName],
                _config.m_Slider_Configuration.m_nMotorChannel,
                _config.m_Slider_Configuration.positions,
                _config.m_Slider_Configuration.m_strControllerName);
            _deviceR2Piston = new deviceR2Piston(motorControllers[_config.m_Slider_Configuration.m_strControllerName],
                _config.m_R2Piston_Configuration.m_nMotorChannel, 
                _config.m_R2Piston_Configuration.positions,
                _config.m_R2Piston_Configuration.m_strControllerName);
            _deviceR1Piston = new deviceR1Piston(motorControllers[_config.m_R1Piston_Configuration.m_strControllerName],
                _config.m_R1Piston_Configuration.m_nMotorChannel,
                _config.m_R1Piston_Configuration.positions,
                _config.m_R1Piston_Configuration.m_strControllerName);
            _deviceHeaterPiston = new deviceHeaterPiston(motorControllers[_config.m_HeaterPiston_Configuration.m_strControllerName],
                _config.m_HeaterPiston_Configuration.m_nMotorChannel,
                _config.m_HeaterPiston_Configuration.positions,
                _config.m_HeaterPiston_Configuration.m_strControllerName);
            _deviceChassisPiston = new deviceChassisPiston(motorControllers[_config.m_ChassisPiston_Configuration.m_strControllerName],
                 _config.m_ChassisPiston_Configuration.m_nMotorChannel,
                 _config.m_ChassisPiston_Configuration.positions,
                 _config.m_ChassisPiston_Configuration.m_strControllerName);
            _devicePCRCycler = new devicePCRCycler(_tecBoard, _config.m_TEC_Channel_Configurations["TEC_5"]);
            _devicePump = new devicePump(_motorBoard, _config.m_Pump_Configuration.channel);
            _deviceOpticsMotor = new deviceOpticsMotor(motorControllers[_config.m_OpticsMotor_Configuration.m_strControllerName],
                _config.m_OpticsMotor_Configuration.m_nMotorChannel,
                _config.m_OpticsMotor_Configuration.positions,
                _config.m_OpticsMotor_Configuration.m_strControllerName);

            _deviceHeater = new deviceHeater(_tecBoard, _config.m_Heater_Configuration.channel);

            // Intialize the devices.
            // N.B. Home R2 before R1
            //_deviceOpticsMotor.initialize();
            /*
            SendProgress(0.55f, "Initializing: Positioning Slider.");
           _deviceSlider.initialize();
            SendProgress(0.65f, "Initializing: Positioning R1 Piston.");
           _deviceR2Piston.initialize();
            _deviceR1Piston.initialize();
            SendProgress(0.6f, "Initializing: Positioning R2 Piston.");
            _deviceR1Piston.initialize1();
           _deviceR2Piston.initialize1();

            SendProgress(0.7f, "Initializing: Positioning Heater Piston.");
           _deviceHeaterPiston.initialize();
            SendProgress(0.75f, "Initializing: Preparing PCR Cycler.");
            _devicePCRCycler.initialize();
            //SendProgress(0.8f, "Initializing: Positioning Pump.");
            //_devicePump.initialize();
            SendProgress(0.85f, "Initializing: Positioning Optics.");
            //_deviceOpticsMotor.initialize();
            SendProgress(0.9f, "Initializing: Preparing Heater.");
            _deviceHeater.initialize();
            SendProgress(0.95f, "Initializing: Positioning Chassis Piston.");*/
            _deviceChassisPiston.initialize(); 
             
            SendProgress(1.0f, "Initialization completed.", true);
           
            
            // Start the temperature monitor process. 

            StartTempMonitor();

            return bInitialized;
        }

        public async Task<bool> InitializeMoto()
        {
            bool bInitialized = true;
            _deviceSlider.initialize();
            //SendProgress(0.65f, "Initializing: Positioning R1 Piston.");
            _deviceR2Piston.initialize();
            _deviceR1Piston.initialize();
            //SendProgress(0.6f, "Initializing: Positioning R2 Piston.");
            _deviceR1Piston.initialize1();
            _deviceR2Piston.initialize1();


            //SendProgress(0.7f, "Initializing: Positioning Heater Piston.");
            _deviceHeaterPiston.initialize();
            //SendProgress(0.75f, "Initializing: Preparing PCR Cycler.");
            _devicePCRCycler.initialize();
            //SendProgress(0.8f, "Initializing: Positioning Pump.");
            //_devicePump.initialize();
            //SendProgress(0.85f, "Initializing: Positioning Optics.");
           // _deviceOpticsMotor.initialize();
            //SendProgress(0.9f, "Initializing: Preparing Heater.");
            _deviceHeater.initialize();
            //SendProgress(0.95f, "Initializing: Positioning Chassis Piston.");
            _deviceChassisPiston.initialize();

            //SendProgress(1.0f, "Initialization completed.", true);


            // Start the temperature monitor process. 

            //StartTempMonitor();

            return bInitialized;
        }

        /// <summary>
        /// Compare the timestamps of the local config file, and put up a warning if the stashed
        /// one is newer.
        /// </summary>
        protected void CheckFileTimeStamps()
        {
            if (File.Exists(CSystem_Defns.strSavedSystemConfigurationPath))
            {
                DateTime dtSaved = File.GetLastWriteTime(CSystem_Defns.strSavedSystemConfigurationPath);

                if (File.Exists(CSystem_Defns.strDefaultSystemConfigurationPath))
                {
                    DateTime dtCur = File.GetLastWriteTime(CSystem_Defns.strDefaultSystemConfigurationPath);

                    if (dtCur < dtSaved)
                    {
                        MessageBox.Show("Warning - The saved config file is newer than the current one.");
                    }
                }
            }
        }

        /// <summary>
        /// Sends out the progress message.
        /// </summary>
        /// <param name="percentage"></param>
        /// <param name="msg"></param>
        protected void SendProgress(float percentage, string msg, bool bComplete = false)
        {
            if (initializationProgress != null)
                initializationProgress(this, new progressEventArgs(percentage, msg, bComplete));
        }


        /// <summary>
        /// This is a bridge function so that the device object can initiate sending out
        /// the event.
        /// </summary>
        /// <param name="temp"></param>
        public void PCRCyclerTempUpdated(double temperature, double power, bool controlling, bool transitioning)
        {
            if (PCRCyclerTempChanged != null)
                PCRCyclerTempChanged(temperature, power, controlling, transitioning);
        }

        /// <summary>
        /// This is a bridge function so that the device object can initiate sending out
        /// the event.
        /// </summary>
        /// <param name="temp"></param>
        public void PCRCyclerTempUpdated2(double temperature, double power, bool controlling, bool transitioning)
        {
            if (PCRCyclerTempChanged2 != null)
                PCRCyclerTempChanged2(temperature, power, controlling, transitioning);
        }

        /// <summary>
        /// This is a bridge function so that the device object can initiate sending out
        /// the event.
        /// </summary>
        /// <param name="temp"></param>
        public void HeaterTempUpdated(double temp)
        {
            if (HeaterTempChanged != null)
                HeaterTempChanged(temp);
        }

        #region accessors
        /// <summary>
        /// Read only parameter returning a reference to the optics model.
        /// </summary>
        public opticsBoardModel OpticsModel
        {
            get { return _opticsBoard; }
        }

        /// <summary>
        /// Read only parameter returning a reference to the configuration settings.
        /// </summary>
        public Configuration Config
        {
            get { return _config; }
        }

        /// <summary>
        /// Read only accessor for the motorboard.
        /// </summary>
        public motorBoardModel motorBoard
        {
            get { return _motorBoard; }
        }

        /// <summary>
        /// Read only accessor for the motorboard.
        /// </summary>
        public motorBoardModel motorBoardx
        {
            get { return _motorBoardX; }
        }

        /// <summary>
        /// Read only accessor for the motorboard.
        /// </summary>
        public tecBoardModel tecBoard
        {
            get { return _tecBoard; }
        }

        public opticsBoardModel opticsBoard
        {
            get { return _opticsBoard; }
        }

        public deviceSlider DeviceSlider
        {
            get { return _deviceSlider;  }
        }
        public deviceChassisPiston DeviceChasis
        {
            get { return _deviceChassisPiston; }
        }

        public deviceR1Piston DeviceR1Piston
        {
            get { return _deviceR1Piston; }
        }
        public deviceR2Piston DeviceR2Piston
        {
            get { return _deviceR2Piston; }
        }
        public deviceHeaterPiston DeviceHeaterPiston
        {
            get { return _deviceHeaterPiston; }
        }

        public deviceChassisPiston DeviceChassisPiston
        {
            get { return _deviceChassisPiston; }
        }

        public devicePCRCycler DevicePCRCycler
        {
            get { return _devicePCRCycler; }
        }
        public devicePump DevicePump
        {
            get { return _devicePump; }
        }
        public deviceOpticsMotor DeviceOpticsMotor
        {
            get { return _deviceOpticsMotor; }
        }
        public deviceHeater DeviceHeater
        {
            get { return _deviceHeater; }
        }

        public string ProtocolDescLine
        {
            get { return _protocolDescLine; }
            set { _protocolDescLine = value;

                if (ProtocolExecutionStepped != null)
                    ProtocolExecutionStepped(_protocolDescLine);
            }
        }


        #endregion

        /// <summary>
        /// Executes the current script.
        /// </summary>
        public async void RunScript(string scriptName = "")
        {
            if (!string.IsNullOrEmpty(scriptName))
            {
                bool bLoadSucceeded = true;

                // Load the specified protocol.
               

                try
                {
                    Task<bool> loadResult = _protocol.Load(scriptName);
                    await loadResult;
                }
                catch(Exception ex)
                {
                    
                    bLoadSucceeded = false;
                }

                if (bLoadSucceeded)
                {
                    CurrentScript = scriptName;

                    // Put out a status message.

                    if (ProtocolExecutionStepped != null)
                        ProtocolExecutionStepped(string.Format("Starting execution of {0}",
                            Path.GetFullPath(scriptName)));

                    bStopScript = false;

                    //await InitializeMoto();

                    try
                    {
                        // Create the results subdirectory.

                        resultsDir = Path.Combine(HelixGen.CSystem_Defns.strDefaultMeasurementLogPath,
                            DateTime.Now.ToString("yyy_MM_dd_HH_mm_ss")
                            );

                        Directory.CreateDirectory(resultsDir);

                        ProtocolDescLine = string.Format("Results will be stored in: {0}", Path.GetFullPath(resultsDir));

                        // Start off a process logging the temperatures for the duration of
                        // the protocol execution.

                        bStopTempResults = false;
                        // _deviceOpticsMotor.ZeroOptic();

                        
                        

                        string TemperatureFileName = Path.Combine(resultsDir, "Temperature Log.csv");
                        logger.Debug("Opening results file to: {0}", TemperatureFileName);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(() =>
                        {
                            using (StreamWriter outStream = new StreamWriter(TemperatureFileName))
                            {
                                outStream.AutoFlush = true;

                                // Write the name of the protocol file.

                                outStream.WriteLine(string.Format("Protocol file: {0}", Path.GetFullPath(scriptName)));
                                outStream.WriteLine(string.Format("Run date.time: {0}", DateTime.Now.ToString()));
                                outStream.WriteLine("Time, Elapsed Seconds, SPREP Thermal Setpoint," +
                                    " SPREP Thermal C, " +
                                    "PCR TEC1 Setpoint, PCR TEC1 C, PCR TEC1 PWR, PRC TE1C Amps," +
                                    "PCR TEC2 Setpoint, PCR TEC2 C, PCR TEC2 PWR, PRC TEC2 Amps" +
                                     "TEC3 Temp, TEC4 Temp, TEC5 temp, TEC6 temp");

                                float htrSetPoint = 0f;
                                float htrTemp = 0f;
                                float TECSetPoint = 0f;
                                float TECTemperature = 0f;
                                float TECPower = 0f;
                                float TECCurrent = 0f;
                                float TECSetPoint2 = 0f;
                                float TECTemperature2 = 0f;
                                float TECPower2 = 0f;
                                float TECCurrent2 = 0f;
                                DateTime startTime = DateTime.Now;
                                double elapsedSeconds;
                                bool bFirstTime = true;

                                float TECTemperature3 = 0f;
                                float TECTemperature4 = 0f;
                                float TECTemperature5 = 0f;
                                float TECTemperature6 = 0f;
                                
                                while (!bStopTempResults)
                                {
                                    clsTec theBoard = tecBoard.theBoard;

                                    htrSetPoint = theBoard.GetRSHeaterSetPoint(0);
                                    htrTemp = theBoard.GetTemperature(6,0);
                                    TECSetPoint = theBoard.GetSetPoint(0);
                                    TECTemperature = theBoard.GetTemperature(0,0);
                                    TECPower = theBoard.GetPower(0);
                                    TECCurrent = theBoard.GetCurrent(0);

                                    TECSetPoint2 = theBoard.GetSetPoint(1);
                                    TECTemperature2 = theBoard.GetTemperature(1, 0);
                                    TECPower2 = theBoard.GetPower(1);
                                    TECCurrent2 = theBoard.GetCurrent(1);

                                    TECTemperature3 = theBoard.GetTemperature(2, 0);
                                    TECTemperature4 = theBoard.GetTemperature(3, 0);
                                    TECTemperature5 = theBoard.GetTemperature(4, 0);
                                    TECTemperature6 = theBoard.GetTemperature(5, 0);

                                    if (bFirstTime)
                                    {
                                        startTime = DateTime.Now;
                                        elapsedSeconds = 0;
                                        bFirstTime = false;
                                    }
                                    else
                                    {
                                        elapsedSeconds = DateTime.Now.Subtract(startTime).TotalSeconds;
                                    }

                                    outStream.WriteLine(string.Format("\"{0}\",{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                        elapsedSeconds.ToString(),
                                        htrSetPoint.ToString(),
                                        htrTemp.ToString(),
                                        TECSetPoint.ToString(),
                                        TECTemperature.ToString(),
                                        TECPower.ToString(),
                                        TECCurrent.ToString(),
                                        TECSetPoint2.ToString(),
                                        TECTemperature2.ToString(),
                                        TECPower2.ToString(),
                                        TECCurrent2.ToString(),
                                        TECTemperature3.ToString(),
                                        TECTemperature4.ToString(),
                                        TECTemperature5.ToString(),
                                        TECTemperature6.ToString()

                                        )
                                        );

                                    Thread.Sleep(250);
                                }  //2018
                            }
                        }
                        );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        

                        Task<bool> bExecute = _protocol.Execute();
                        await bExecute;
                    }
                    catch(Exception ex)
                    {
                        logger.Error("RunScript caught an exception; {0}", ex.Message);
                    }

                    bStopTempResults = true;

                    //await InitializeMoto();

                    if (ProtocolExecutionStepped != null)
                    {
                        ProtocolExecutionStepped("Execution Completed");
                    }

                   

                    StopScript();
                   // ControlComplete();//2018

                    CurrentScript = string.Empty;
                }
            }
        }

        public async void StopScript()
        {
            bStopScript = true;
        }

        public void ControlComplete()
        {
            // Turn all the TEC channels off

            for (int nController = 0; nController < 7; nController++)
                tecBoard.theBoard.SetTempControlMode(nController, 0);
        }

        /// <summary>
        /// Executes the current script.
        /// </summary>
        public void PumpOn(bool bOn = false)
        {

        }

        /// <summary>
        /// Shut Down the system.
        /// </summary>
        public void ShutDown()
        {
        }

        /// <summary>
        /// Returns a reference to the appropriate motorboard given the id.
        /// </summary>
        /// <param name="boardId"></param>
        /// <returns></returns>
        public clsAMB getMotorBoard(string boardId)
        {
            return this.motorControllers[boardId].board;
        }

        public void UpdatedPCRCyclerSetpointTemperature(double temp)
        {
            _PCRCyclerSetPointTemperature = temp;

            if (evtPCRCyclerSetPointTemperature != null)
                evtPCRCyclerSetPointTemperature(_PCRCyclerSetPointTemperature);
        }

        public void StartTempMonitor()
        {
            _poll_timer = new System.Threading.Timer(OnPoll);
            _poll_timer.Change(1000, 1000); // Set the timer to run every minute.
        }

        // Callback from polling timer
        private void OnPoll(object state)
        {
            // Read the temperatures on the TEC devices and the resistive heater.

            float tec0Temp = _tecBoard.theBoard.GetTemperature(4, 0);
            float tec1Temp = _tecBoard.theBoard.GetTemperature(5, 0);
            float tec0Power = _tecBoard.theBoard.GetPower(0);
            float tec1Power = _tecBoard.theBoard.GetPower(1);
            float RH1Temp = _tecBoard.theBoard.GetTemperature(3, 0);
            bool controlling0Tec0 = _tecBoard.theBoard.Control0[4];
            bool controlling0Tec1 = _tecBoard.theBoard.Control0[5];
            bool transitioning0Tec0 = _tecBoard.theBoard.TranBusy0[4];
            bool transitioning0Tec1 = _tecBoard.theBoard.TranBusy0[5];

#if false
            logger.Debug("OnPoll, got values; tec0; temp={0} power={1} controlling0={2} transitiong0={3}", tec0Temp.ToString(), tec0Power.ToString(),
                _tecBoard.theBoard.Control0[0] ? "true" : "false",
                _tecBoard.theBoard.TranBusy0[0] ? "true" : "false");
            logger.Debug("OnPoll, got values; tec1; temp={0} power={1}", tec1Temp.ToString(), tec1Power.ToString());
#endif

            // Send out the messages.

            if (PCRCyclerTempChanged != null)
                PCRCyclerTempChanged(tec0Temp, tec0Power, controlling0Tec0, transitioning0Tec0);

            if (PCRCyclerTempChanged2 != null)
                PCRCyclerTempChanged2(tec1Temp, tec1Power, controlling0Tec1, transitioning0Tec1);

            if (HeaterTempChanged != null)
                HeaterTempChanged(RH1Temp);
        }

        public void StopTempMonitor()
        {
            _poll_timer.Dispose();
        }

        /// <summary>
        /// Signals subscribers that we've started PCR Cycling.
        /// </summary>
        public void StartingPCRCycling()
        {
            if (evtPCRReadingsStarted != null)
                evtPCRReadingsStarted();
        }

        /// <summary>
        /// Broadcasts PD Readings
        /// </summary>
        /// <param name="readingsIn"></param>
        public void NewPDReadingsTaken(double[,] readingsIn)
        {
            if (evtPCRReadingsTaken != null)
                evtPCRReadingsTaken(readingsIn);
           // if (evtPCRReadingsTaken2 != null)
                //evtPCRReadingsTaken2(readingsIn);
        }
        
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

                StopTempMonitor();

                _deviceSlider = null;
                _deviceR1Piston = null;
                _deviceR2Piston = null;
                _deviceHeaterPiston = null;
                _devicePCRCycler = null;
                _devicePump = null;
                _deviceOpticsMotor = null;
                _deviceHeater = null;

                _opticsBoard = null;
                _motorBoard = null;
                _motorBoardX = null;
                _tecBoard.theBoard.Dispose();
                _tecBoard = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        //~HelixGenModel() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        //}

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
#endregion
    }
}
