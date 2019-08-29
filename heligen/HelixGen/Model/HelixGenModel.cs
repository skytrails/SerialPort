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

        public delegate void BeginAnalysis();
        public event BeginAnalysis evtBeginAnalysis;

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
        public event EventHandler runProgress;

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
        protected Timer _poll_timer1;

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

        public float runSecondMeter,percentageofTime,percentageStep;
        protected bool inWaitStas;
        protected float timeStep;

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

        public double[] readings = new double[24];
        public int[] checkres = new int[6];
        public int[] checkres1 = new int[6];

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
            m_data_r2 = new List<double>();
            _slope = new List<double>();
            _slope1 = new List<double>();
            m_data_pcr = new List<double>();
            m_data_pcr1 = new List<double>();
            m_pcr_t = new List<double>();
            // anaFilePath = "C:\\snp_new\\HelixGen\\logs\\measurement\\2018_05_16_14_20_20";
            anaFilePath = "C:\\HelixGen\\logs\\measurement\\2018_11_28_13_31_44";
            //anaFilePath = "C:\\HelixGen\\logs\\measurement";
            optic_zero = new double[4];
            catrigeSelect = 0;

        }

        //theModel.Analysis();
        public void Analysis()
        {

            m_data_r1.Clear();
            m_data_r2.Clear();
            m_data_t.Clear();
            _slope.Clear();
            _slope1.Clear();
            m_data_pcr.Clear();
            m_data_pcr1.Clear();
            m_pcr_t.Clear();
            if (catrigeSelect == 0)
                checkres[1]= 0;
            else if (catrigeSelect == 1)
                checkres[3] = 0;
            if (catrigeSelect == 0)
                checkres1[1] = 0;
            else if (catrigeSelect == 1)
                checkres1[3] = 0;

            int line = 0;
            string protocolFile = System.IO.Path.Combine(anaFilePath,
                 "Melt Curve logging.csv");
            try
            {
                //FileStream fs = new FileStream("C:\\Users\\Helixgen\\Desktop\\snp\\Circuit Boards\\Melt Curve logging.csv", FileMode.Open, FileAccess.Read, FileShare.None);
                FileStream fs = new FileStream(protocolFile, FileMode.Open, FileAccess.Read, FileShare.None);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(936));
            sr.DiscardBufferedData();
            string str = "";
            for (int i = 0; i < 20; i++)
            {
                str = sr.ReadLine();
            }
            //string s = Console.ReadLine();
            while (str != null)
            {
                //line++;
                str = sr.ReadLine();
                if (str != null)
                {
                    string[] xu = new String[25];
                    xu = str.Split(',');
                        /*   
                       m_data_r1.Add(double.Parse(xu[9]));//4*x+1,9
                       m_data_r1.Add(double.Parse(xu[10]));
                       m_data_r1.Add(double.Parse(xu[11]));
                       m_data_r1.Add(double.Parse(xu[12]));
                       */
                        /*
                        m_data_r1.Add(double.Parse(xu[1]));//4*x+1,9
                        m_data_r1.Add(double.Parse(xu[2]));
                        m_data_r1.Add(double.Parse(xu[3]));
                        m_data_r1.Add(double.Parse(xu[4]));
                         */
                        /*
                    m_data_r1.Add(double.Parse(xu[16]));//4*x+1,9
                    m_data_r1.Add(double.Parse(xu[17]));
                    m_data_r1.Add(double.Parse(xu[18]));
                    m_data_r1.Add(double.Parse(xu[19]));
                     */
                        switch (catrigeSelect)
                        {
                            case 0:
                                m_data_r1.Add(double.Parse(xu[1]));//4*x+1,9
                                m_data_r1.Add(double.Parse(xu[2]));
                                m_data_r1.Add(double.Parse(xu[3]));
                                m_data_r1.Add(double.Parse(xu[4]));
                                break;
                            case 1:
                                m_data_r1.Add(double.Parse(xu[5]));//4*x+1,9
                                m_data_r1.Add(double.Parse(xu[6]));
                                m_data_r1.Add(double.Parse(xu[7]));
                                m_data_r1.Add(double.Parse(xu[8]));
                                break;
                            case 2:
                                m_data_r1.Add(double.Parse(xu[9]));//4*x+1,9
                                m_data_r1.Add(double.Parse(xu[10]));
                                m_data_r1.Add(double.Parse(xu[11]));
                                m_data_r1.Add(double.Parse(xu[12]));
                                break;
                            case 3:
                                m_data_r1.Add(double.Parse(xu[13]));//4*x+1,9
                                m_data_r1.Add(double.Parse(xu[14]));
                                m_data_r1.Add(double.Parse(xu[15]));
                                m_data_r1.Add(double.Parse(xu[16]));
                                break;
                            case 4:
                                m_data_r1.Add(double.Parse(xu[17]));//4*x+1,9
                                m_data_r1.Add(double.Parse(xu[18]));
                                m_data_r1.Add(double.Parse(xu[19]));
                                m_data_r1.Add(double.Parse(xu[20]));
                                break;
                            case 5:
                                m_data_r1.Add(double.Parse(xu[21]));//4*x+1,9
                                m_data_r1.Add(double.Parse(xu[22]));
                                m_data_r1.Add(double.Parse(xu[23]));
                                m_data_r1.Add(double.Parse(xu[24]));
                                break;
                            default:
                                break;
                        }
                       
                    m_data_t.Add(double.Parse(xu[0]));
                }
            }
            sr.Close();
        }
            catch (Exception ex)
            {
                logger.Error("fileOperation caught an exception; {0}", ex.Message);
            }

            //calibtare
            /*
            for (uint uig_sample_index = 0; uig_sample_index < m_data_r1.Count / 4; uig_sample_index++)
            {
                uint columnn = 0;
                //h = (uint)(number_data_points / 2);
                for (columnn = 0; columnn < 4; columnn++)
                {
                   
                    double temp1 = 0;
                    switch (columnn)
                    {
                        case 0:
                            temp1= m_data_r1.ElementAt(0) *(87/37.52);
                            
                            m_data_r1.RemoveAt(0);
                            m_data_r1.Add(temp1);
                           
                            break;
                        case 1:
                           
                            temp1 = m_data_r1.ElementAt(0) * 1;
                            m_data_r1.RemoveAt(0);
                            m_data_r1.Add(temp1);
                            break;
                        case 2:
                            temp1 = m_data_r1.ElementAt(0) * (87 / 37.52);
                           
                            m_data_r1.RemoveAt(0);
                            m_data_r1.Add(temp1);
                            break;
                        case 3:
                             temp1 = m_data_r1.ElementAt(0) * (87 / 30.9);
                            
                            m_data_r1.RemoveAt(0);
                            m_data_r1.Add(temp1);
                            break;
                        default:
                            break;
                    }
                }
            }
            */


            //move base
            for (uint uig_sample_index = 1; uig_sample_index < m_data_r1.Count / 4; uig_sample_index++)
            {
                uint columnn = 0;
                //h = (uint)(number_data_points / 2);
                for (columnn = 0; columnn < 4; columnn++)
                {

                    double temp1 = 0;
                   
                    switch (columnn)
                    {
                        case 0:
                            temp1 = m_data_r1.ElementAt(4) - m_data_r1.ElementAt(0);
                            temp1 = -temp1;
                            m_data_r1.RemoveAt(4);
                            m_data_r1.Add(temp1);

                            break;
                        case 1:

                            temp1 = m_data_r1.ElementAt(4) - m_data_r1.ElementAt(1);
                            temp1 = -temp1;
                            m_data_r1.RemoveAt(4);
                            m_data_r1.Add(temp1);
                            break;
                        case 2:
                            temp1 = m_data_r1.ElementAt(4) - m_data_r1.ElementAt(2);
                            temp1 = -temp1;
                            m_data_r1.RemoveAt(4);
                            m_data_r1.Add(temp1);
                            break;
                        case 3:
                            temp1 = m_data_r1.ElementAt(4) - m_data_r1.ElementAt(3);
                            temp1 = -temp1;
                            m_data_r1.RemoveAt(4);
                            m_data_r1.Add(temp1);
                            break;
                        default:
                            break;
                    }
                    
                }
            }
            if (m_data_r1.Count >= 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    m_data_r1.RemoveAt(0);
                }
            }

            double[] m_data_max = new double[4] { 0,0,0,0};

            //calibrate
            for (uint uig_sample_index = 0; uig_sample_index < m_data_r1.Count / 4; uig_sample_index++)
            {
                uint columnn = 0;
                //h = (uint)(number_data_points / 2);
                for (columnn = 0; columnn < 4; columnn++)
                {

                    double temp1 = 0;
                    switch (columnn)
                    {
                        case 0:

                            temp1 = m_data_r1.ElementAt(0) /0.06;
                            if (m_data_max[0] < System.Math.Abs(temp1))
                                m_data_max[0] = System.Math.Abs(temp1);
                           // if (temp1 < 0)
                           //   temp1 /= 100;
                            m_data_r1.RemoveAt(0);
                            m_data_r1.Add(temp1);

                            break;
                        case 1:

                            //temp1 = m_data_r1.ElementAt(0) / 1.41;
                            temp1 = m_data_r1.ElementAt(0) / 0.06;
                            if (m_data_max[1] < System.Math.Abs(temp1))
                                m_data_max[1] = System.Math.Abs(temp1);
                            m_data_r1.RemoveAt(0);
                            m_data_r1.Add(temp1);
                            break;
                        case 2:
                            //temp1 = m_data_r1.ElementAt(0) /0.44;
                            temp1 = m_data_r1.ElementAt(0) / 0.06;
                            if (m_data_max[2] < System.Math.Abs(temp1))
                                m_data_max[2] = System.Math.Abs(temp1);
                            m_data_r1.RemoveAt(0);
                            m_data_r1.Add(temp1);
                            break;
                        case 3:
                            temp1 = m_data_r1.ElementAt(0)/0.06;
                            // temp1 = m_data_r1.ElementAt(0) / 1;
                            if (m_data_max[3] < System.Math.Abs(temp1))
                                m_data_max[3] = System.Math.Abs(temp1);
                            m_data_r1.RemoveAt(0);
                            m_data_r1.Add(temp1);
                            break;
                        default:
                            break;
                    }
                }
            }

            for (uint uig_sample_index = 0; uig_sample_index < m_data_r1.Count / 4; uig_sample_index++)
            {
                uint columnn = 0;
                //h = (uint)(number_data_points / 2);
                for (columnn = 0; columnn < 4; columnn++)
                {

                    double temp1 = 0;
                    switch (columnn)
                    {
                        case 0:


                            if (m_data_max[0] < 400000)
                            {
                                //m_data_r1.ElementAt(0) = 0;
                                 m_data_r1.RemoveAt((int)uig_sample_index*4+ (int)columnn);
                                m_data_r1.Insert((int)uig_sample_index * 4 + (int)columnn,0);
                                // m_data_r1.RemoveAt(0);
                                // m_data_r1.Add(temp1);

                            }

                            break;
                        case 1:

                            if (m_data_max[1] < 100000)
                            {
                                //m_data_r1.ElementAt(0) = 0;
                                //m_data_r1.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_r1.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_r1.Insert((int)uig_sample_index * 4 + (int)columnn, 0);

                            }
                            break;
                        case 2:
                            if (m_data_max[2] < 100000)
                            {
                                //m_data_r1.ElementAt(0) = 0;
                                //m_data_r1.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_r1.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_r1.Insert((int)uig_sample_index * 4 + (int)columnn, 0);

                            }
                            break;
                        case 3:
                            if (m_data_max[3] < 500000)
                            {
                                //m_data_r1.ElementAt(0) = 0;
                                //m_data_r1.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_r1.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_r1.Insert((int)uig_sample_index * 4 + (int)columnn, 0);

                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            if (m_data_max[0] < 500000 && m_data_max[1] < 500000 && m_data_max[2] < 500000 && m_data_max[3] < 500000)
                m_data_r1.Clear();
            // melt data move noise
            if (m_data_r1.Count/4>=12)
            { 
            for(uint preCounnt=1; preCounnt < 12; preCounnt++)
            { 
            for (uint uig_sample_index = 1; uig_sample_index < preCounnt; uig_sample_index++)
            {
                uint i = 0, number_data_points = preCounnt, columnn = 0;
                double k = 0, m = 0;
                for (columnn = 0; columnn < 4; columnn++)
                {
                    m = 0;
                    for (i = 0; i < number_data_points; i++)
                    {
                        //uint temp = uig_sample_index - number_data_points + i;
                        m += m_data_r1.ElementAt((int)(i * 4 + columnn));
                    }
                    k = m / number_data_points;
                    m_data_r2.Add(k);
                }
            }
            }
            }

            for (uint uig_sample_index = 12; uig_sample_index < m_data_r1.Count / 4; uig_sample_index++)
            {
                uint i = 0, number_data_points = 12, columnn = 0;
                double k = 0, m = 0;
                for (columnn = 0; columnn < 4; columnn++)
                {
                    m = 0;
                    for (i = 0; i < number_data_points; i++)
                    {
                        uint temp = uig_sample_index - number_data_points + i;
                        m += m_data_r1.ElementAt((int)(temp * 4 + columnn));
                    }
                    k = m / number_data_points;
                    m_data_r2.Add(k);
                }
            }

            //to slope
            for (uint uig_sample_index = 15; uig_sample_index < m_data_r2.Count / 4; uig_sample_index++)
           {
               uint h = 0, i = 0, j = 0, number_data_points = 15, columnn = 0;
               double k = 0, L = 0, m = 0, n = 0, p = 0;

               //h = (uint)(number_data_points / 2);
               for (columnn = 0; columnn < 4; columnn++)
               {
                   L = 0; m = 0; n = 0; p = 0;
                   for (i = 0; i < number_data_points; i++)
                   {
                       uint temp = uig_sample_index - number_data_points + i;
                       L += i;
                       m += m_data_r2.ElementAt((int)(temp * 4 + columnn));
                       n += i * m_data_r2.ElementAt((int)(temp * 4 + columnn));
                       p += i * i;
                   }

                   if ((number_data_points * p - L * L) != 0)
                       k = -(number_data_points * n - L * m) / (number_data_points * p - L * L);
                    if (k < 0)
                        k /=10;
                   _slope.Add(k);
               }
           }
            // slope move noise
            for (uint uig_sample_index = 17; uig_sample_index < _slope.Count / 4; uig_sample_index++)
            {
                uint  i = 0, number_data_points = 17, columnn = 0;
                double k = 0, m = 0;
                for (columnn = 0; columnn < 4; columnn++)
                {
                     m = 0; 
                    for (i = 0; i < number_data_points; i++)
                    {
                        uint temp = uig_sample_index - number_data_points + i;
                        m += _slope.ElementAt((int)(temp * 4 + columnn));
                    }
                    k = m / number_data_points;
                    _slope1.Add(k);
                }
            }

            double[] m_data_max1 = new double[4] { 0, 0, 0, 0 };
            
            for (uint uig_sample_index = 0; uig_sample_index < _slope1.Count / 4; uig_sample_index++)
            {
                uint columnn = 0;
                //h = (uint)(number_data_points / 2);
                for (columnn = 0; columnn < 4; columnn++)
                {

                    double temp1 = 0;
                    switch (columnn)
                    {
                        case 0:

                            temp1 = _slope1.ElementAt(0) / 1;
                            if (m_data_max1[0] < System.Math.Abs(temp1))
                                m_data_max1[0] = System.Math.Abs(temp1);
                            // if (temp1 < 0)
                            //   temp1 /= 100;
                            _slope1.RemoveAt(0);
                            _slope1.Add(temp1);

                            break;
                        case 1:

                            //temp1 = m_data_r1.ElementAt(0) / 1.41;
                            temp1 = _slope1.ElementAt(0) / 1;
                            if (m_data_max1[1] < System.Math.Abs(temp1))
                                m_data_max1[1] = System.Math.Abs(temp1);
                            _slope1.RemoveAt(0);
                            _slope1.Add(temp1);
                            break;
                        case 2:
                            //temp1 = m_data_r1.ElementAt(0) /0.44;
                            temp1 = _slope1.ElementAt(0) / 1;
                            if (m_data_max1[2] < System.Math.Abs(temp1))
                                m_data_max1[2] = System.Math.Abs(temp1);
                            _slope1.RemoveAt(0);
                            _slope1.Add(temp1);
                            break;
                        case 3:
                            temp1 = _slope1.ElementAt(0) / 1;
                            // temp1 = m_data_r1.ElementAt(0) / 1;
                            if (m_data_max1[3] < System.Math.Abs(temp1))
                                m_data_max1[3] = System.Math.Abs(temp1);
                            _slope1.RemoveAt(0);
                            _slope1.Add(temp1);
                            break;
                        default:
                            break;
                    }
                }
            }
            
            for (uint uig_sample_index = 0; uig_sample_index < _slope1.Count / 4; uig_sample_index++)
            {
                uint columnn = 0;
                //h = (uint)(number_data_points / 2);
                for (columnn = 0; columnn < 4; columnn++)
                {

                    double temp1 = 0;
                    switch (columnn)
                    {
                        case 0:


                            // if (m_data_max[0] < 500000)
                            {
                                //m_data_r1.ElementAt(0) = 0;
                                temp1 = _slope1.ElementAt((int)uig_sample_index * 4 + (int)columnn) / m_data_max1[0];
                                _slope1.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                _slope1.Insert((int)uig_sample_index * 4 + (int)columnn, temp1);
                                // m_data_r1.RemoveAt(0);
                                // m_data_r1.Add(temp1);

                            }

                            break;
                        case 1:

                            // if (m_data_max[0] < 500000)
                            {
                                //m_data_r1.ElementAt(0) = 0;
                                temp1 = _slope1.ElementAt((int)uig_sample_index * 4 + (int)columnn) / m_data_max1[1];
                                _slope1.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                _slope1.Insert((int)uig_sample_index * 4 + (int)columnn, temp1);
                                // m_data_r1.RemoveAt(0);
                                // m_data_r1.Add(temp1);

                            }
                            break;
                        case 2:
                            // if (m_data_max[0] < 500000)
                            {
                                //m_data_r1.ElementAt(0) = 0;
                                temp1 = _slope1.ElementAt((int)uig_sample_index * 4 + (int)columnn) / m_data_max1[2];
                                _slope1.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                _slope1.Insert((int)uig_sample_index * 4 + (int)columnn, temp1);
                                // m_data_r1.RemoveAt(0);
                                // m_data_r1.Add(temp1);

                            }
                            break;
                        case 3:
                            // if (m_data_max[0] < 500000)
                            {
                                //m_data_r1.ElementAt(0) = 0;
                                temp1 = _slope1.ElementAt((int)uig_sample_index * 4 + (int)columnn) / m_data_max1[3];
                                _slope1.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                _slope1.Insert((int)uig_sample_index * 4 + (int)columnn, temp1);
                                // m_data_r1.RemoveAt(0);
                                // m_data_r1.Add(temp1);

                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            // double[] m_data_max1 = new double[4] { 0, 0, 0, 0 };
            int[,] peak_index = new int[4,20];
            int[] counter_peak = new int[4] { 0, 0, 0, 0 };
            for (uint uig_sample_index = 1; uig_sample_index < _slope1.Count / 4-1; uig_sample_index++)
            {
                uint columnn = 0;
                //h = (uint)(number_data_points / 2);
                for (columnn = 0; columnn < 4; columnn++)
                {

                    double temp1 = 0;
                    switch (columnn)
                    {
                        case 0:

                           if( _slope1.ElementAt((int)uig_sample_index * 4 + (int)columnn)> _slope1.ElementAt((int)(uig_sample_index-1) * 4 + (int)columnn)
                                && _slope1.ElementAt((int)uig_sample_index * 4 + (int)columnn) > _slope1.ElementAt((int)(uig_sample_index+1) * 4 + (int)columnn)
                                && _slope1.ElementAt((int)uig_sample_index * 4 + (int)columnn)>0.35)
                            {
                                counter_peak[0]++;
                                peak_index[0, counter_peak[0] - 1] = (int)uig_sample_index;
                            }
                            

                            break;
                        case 1:
                            if (_slope1.ElementAt((int)uig_sample_index * 4 + (int)columnn) > _slope1.ElementAt((int)(uig_sample_index - 1) * 4 + (int)columnn)
                               && _slope1.ElementAt((int)uig_sample_index * 4 + (int)columnn) > _slope1.ElementAt((int)(uig_sample_index + 1) * 4 + (int)columnn)
                               && _slope1.ElementAt((int)uig_sample_index * 4 + (int)columnn) > 0.35)
                            {
                                counter_peak[1]++;
                                peak_index[1, counter_peak[1] - 1] = (int)uig_sample_index;
                            }

                            break;
                        case 2:
                            if (_slope1.ElementAt((int)uig_sample_index * 4 + (int)columnn) > _slope1.ElementAt((int)(uig_sample_index - 1) * 4 + (int)columnn)
                               && _slope1.ElementAt((int)uig_sample_index * 4 + (int)columnn) > _slope1.ElementAt((int)(uig_sample_index + 1) * 4 + (int)columnn)
                               && _slope1.ElementAt((int)uig_sample_index * 4 + (int)columnn) > 0.35)
                            {
                                counter_peak[2]++;
                                peak_index[2, counter_peak[2] - 1] = (int)uig_sample_index;
                            }
                            break;
                        case 3:
                            if (_slope1.ElementAt((int)uig_sample_index * 4 + (int)columnn) > _slope1.ElementAt((int)(uig_sample_index - 1) * 4 + (int)columnn)
                               && _slope1.ElementAt((int)uig_sample_index * 4 + (int)columnn) > _slope1.ElementAt((int)(uig_sample_index + 1) * 4 + (int)columnn)
                               && _slope1.ElementAt((int)uig_sample_index * 4 + (int)columnn) > 0.35)
                            {
                                counter_peak[3]++;
                                peak_index[3, counter_peak[3] - 1] = (int)uig_sample_index;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            if(counter_peak[0]==2)
            {
                if(catrigeSelect==0)
                    checkres[1] = 2;
                else if (catrigeSelect == 1)
                    checkres[3] = 2;
            }
            else  if (counter_peak[0] == 1)
            {
                if(peak_index[0,0]>50 && peak_index[0, 0] <90)
                {
                    if (catrigeSelect == 0)
                        checkres[1] = 1;
                    else if (catrigeSelect == 1)
                        checkres[3] = 1;
                }
                else if (peak_index[0, 0] > 90)
                {
                    if (catrigeSelect == 0)
                        checkres[1] = 3;
                    else if (catrigeSelect == 1)
                        checkres[3] = 3;
                }

            }

            if (counter_peak[1] == 2)
            {
                if (catrigeSelect == 0)
                    checkres1[1] = 2;
                else if (catrigeSelect == 1)
                    checkres1[3] = 2;
            }
            else if (counter_peak[1] == 1)
            {
                if (peak_index[1, 0] > 50 && peak_index[1, 0] < 90)
                {
                    if (catrigeSelect == 0)
                        checkres1[1] = 1;
                    else if (catrigeSelect == 1)
                        checkres1[3] = 1;
                }
                else if (peak_index[1, 0] > 90)
                {
                    if (catrigeSelect == 0)
                        checkres1[1] = 3;
                    else if (catrigeSelect == 1)
                        checkres1[3] = 3;
                }

            }
            for (uint uig_sample_index = 0; uig_sample_index < m_data_r2.Count / 4; uig_sample_index++)
            {
                uint columnn = 0;
                //h = (uint)(number_data_points / 2);
                for (columnn = 0; columnn < 4; columnn++)
                {

                    double temp1 = 0;
                    switch (columnn)
                    {
                        case 0:


                           // if (m_data_max[0] < 500000)
                            {
                                //m_data_r1.ElementAt(0) = 0;
                                temp1 = m_data_r2.ElementAt((int)uig_sample_index * 4 + (int)columnn) / m_data_max[0];
                                m_data_r2.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_r2.Insert((int)uig_sample_index * 4 + (int)columnn, temp1);
                                // m_data_r1.RemoveAt(0);
                                // m_data_r1.Add(temp1);

                            }

                            break;
                        case 1:

                            // if (m_data_max[0] < 500000)
                            {
                                //m_data_r1.ElementAt(0) = 0;
                                temp1 = m_data_r2.ElementAt((int)uig_sample_index * 4 + (int)columnn) / m_data_max[1];
                                m_data_r2.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_r2.Insert((int)uig_sample_index * 4 + (int)columnn, temp1);
                                // m_data_r1.RemoveAt(0);
                                // m_data_r1.Add(temp1);

                            }
                            break;
                        case 2:
                            // if (m_data_max[0] < 500000)
                            {
                                //m_data_r1.ElementAt(0) = 0;
                                temp1 = m_data_r2.ElementAt((int)uig_sample_index * 4 + (int)columnn) / m_data_max[2];
                                m_data_r2.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_r2.Insert((int)uig_sample_index * 4 + (int)columnn, temp1);
                                // m_data_r1.RemoveAt(0);
                                // m_data_r1.Add(temp1);

                            }
                            break;
                        case 3:
                            // if (m_data_max[0] < 500000)
                            {
                                //m_data_r1.ElementAt(0) = 0;
                                temp1 = m_data_r2.ElementAt((int)uig_sample_index * 4 + (int)columnn) / m_data_max[3];
                                m_data_r2.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_r2.Insert((int)uig_sample_index * 4 + (int)columnn, temp1);
                                // m_data_r1.RemoveAt(0);
                                // m_data_r1.Add(temp1);

                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            protocolFile = System.IO.Path.Combine(anaFilePath,
                "Real time PCR data.csv");
            try { 
           //FileStream fs = new FileStream("C:\\Users\\Helixgen\\Desktop\\snp\\Circuit Boards\\Melt Curve logging.csv", FileMode.Open, FileAccess.Read, FileShare.None);
           FileStream fs = new FileStream(protocolFile, FileMode.Open, FileAccess.Read, FileShare.None);
           StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(936));
           sr.DiscardBufferedData();
            string str = "";
           str = sr.ReadLine();
           str = sr.ReadLine();
           //string s = Console.ReadLine();
           while (str != null)
           {
               //line++;
               str = sr.ReadLine();
               if (str != null)
               {
                   string[] xu = new String[25];
                   xu = str.Split(',');

                        switch (catrigeSelect)
                        {
                            case 0:
                                m_data_pcr.Add(double.Parse(xu[1]));//4*x+1,9
                                m_data_pcr.Add(double.Parse(xu[2]));
                                m_data_pcr.Add(double.Parse(xu[3]));
                                m_data_pcr.Add(double.Parse(xu[4]));
                                break;
                            case 1:
                                m_data_pcr.Add(double.Parse(xu[5]));//4*x+1,9
                                m_data_pcr.Add(double.Parse(xu[6]));
                                m_data_pcr.Add(double.Parse(xu[7]));
                                m_data_pcr.Add(double.Parse(xu[8]));
                                break;
                            case 2:
                                m_data_pcr.Add(double.Parse(xu[9]));//4*x+1,9
                                m_data_pcr.Add(double.Parse(xu[10]));
                                m_data_pcr.Add(double.Parse(xu[11]));
                                m_data_pcr.Add(double.Parse(xu[12]));
                                break;
                            case 3:
                                m_data_pcr.Add(double.Parse(xu[13]));//4*x+1,9
                                m_data_pcr.Add(double.Parse(xu[14]));
                                m_data_pcr.Add(double.Parse(xu[15]));
                                m_data_pcr.Add(double.Parse(xu[16]));
                                break;
                            case 4:
                                m_data_pcr.Add(double.Parse(xu[17]));//4*x+1,9
                                m_data_pcr.Add(double.Parse(xu[18]));
                                m_data_pcr.Add(double.Parse(xu[19]));
                                m_data_pcr.Add(double.Parse(xu[20]));
                                break;
                            case 5:
                                m_data_pcr.Add(double.Parse(xu[21]));//4*x+1,9
                                m_data_pcr.Add(double.Parse(xu[22]));
                                m_data_pcr.Add(double.Parse(xu[23]));
                                m_data_pcr.Add(double.Parse(xu[24]));
                                break;
                            default:
                                break;
                        }
                    }
           }
           sr.Close();
            }
            catch(Exception ex)
            {
                logger.Error("fileOperation caught an exception; {0}", ex.Message);
            }

            //move base
            for (uint uig_sample_index = 1; uig_sample_index < m_data_pcr.Count / 4; uig_sample_index++)
            {
                uint columnn = 0;
                //h = (uint)(number_data_points / 2);
                for (columnn = 0; columnn < 4; columnn++)
                {

                    double temp1 = 0;

                    switch (columnn)
                    {
                        case 0:
                            temp1 = m_data_pcr.ElementAt(4) - m_data_pcr.ElementAt(0);
                            temp1 = -temp1;
                            m_data_pcr.RemoveAt(4);
                            m_data_pcr.Add(temp1);

                            break;
                        case 1:
                            temp1 = m_data_pcr.ElementAt(4) - m_data_pcr.ElementAt(1);
                            temp1 = -temp1;
                            m_data_pcr.RemoveAt(4);
                            m_data_pcr.Add(temp1);
                            break;
                        case 2:
                            temp1 = m_data_pcr.ElementAt(4) - m_data_pcr.ElementAt(2);
                            temp1 = -temp1;
                            m_data_pcr.RemoveAt(4);
                            m_data_pcr.Add(temp1);
                            break;
                        case 3:
                            temp1 = m_data_pcr.ElementAt(4) - m_data_pcr.ElementAt(3);
                            temp1 = -temp1;
                            m_data_pcr.RemoveAt(4);
                            m_data_pcr.Add(temp1);
                            break;
                        default:
                            break;
                    }

                }
            }
            if(m_data_pcr.Count>=4)
            { 
                for (int i = 0; i < 4; i++)
                {
                    m_data_pcr.RemoveAt(0);
                }
            }
            double[] m_data1_max = new double[4] { 0, 0, 0, 0 };
            //calibrate
            for (uint uig_sample_index = 0; uig_sample_index < m_data_pcr.Count / 4; uig_sample_index++)
            {
                uint columnn = 0;
                //h = (uint)(number_data_points / 2);
                for (columnn = 0; columnn < 4; columnn++)
                {

                    double temp1 = 0;
                    switch (columnn)
                    {
                        case 0:
                            temp1 = m_data_pcr.ElementAt(0) * 1;
                            if (m_data1_max[0] < System.Math.Abs(temp1))
                                m_data1_max[0] = System.Math.Abs(temp1);
                            m_data_pcr.RemoveAt(0);
                            m_data_pcr.Add(temp1);

                            break;
                        case 1:

                            temp1 = m_data_pcr.ElementAt(0) / 1;
                            if (m_data1_max[1] < System.Math.Abs(temp1))
                                m_data1_max[1] = System.Math.Abs(temp1);
                            m_data_pcr.RemoveAt(0);
                            m_data_pcr.Add(temp1);
                            break;
                        case 2:
                            temp1 = m_data_pcr.ElementAt(0) / 1;
                            if (m_data1_max[2] < System.Math.Abs(temp1))
                                m_data1_max[2] = System.Math.Abs(temp1);
                            m_data_pcr.RemoveAt(0);
                            m_data_pcr.Add(temp1);
                            break;
                        case 3:
                            temp1 = m_data_pcr.ElementAt(0) / 1;
                            if (m_data1_max[3] < System.Math.Abs(temp1))
                                m_data1_max[3] = System.Math.Abs(temp1);
                            m_data_pcr.RemoveAt(0);
                            m_data_pcr.Add(temp1);
                            break;
                        default:
                            break;
                    }
                }
            }
            for (uint uig_sample_index = 0; uig_sample_index < m_data_pcr.Count / 4; uig_sample_index++)
            {
                uint columnn = 0;
                //h = (uint)(number_data_points / 2);
                for (columnn = 0; columnn < 4; columnn++)
                {

                    double temp1 = 0;
                    switch (columnn)
                    {
                        case 0:


                            if (m_data1_max[0] < 40000)
                            {
                                //m_data_r1.ElementAt(0) = 0;
                                m_data_pcr.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_pcr.Insert((int)uig_sample_index * 4 + (int)columnn, 0);
                                // m_data_r1.RemoveAt(0);
                                // m_data_r1.Add(temp1);

                            }

                            break;
                        case 1:

                            if (m_data1_max[1] < 40000)
                            {
                                //m_data_r1.ElementAt(0) = 0;
                                //m_data_r1.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_pcr.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_pcr.Insert((int)uig_sample_index * 4 + (int)columnn, 0);

                            }
                            break;
                        case 2:
                            if (m_data1_max[2] < 40000)
                            {
                                //m_data_r1.ElementAt(0) = 0;
                                //m_data_r1.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_pcr.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_pcr.Insert((int)uig_sample_index * 4 + (int)columnn, 0);

                            }
                            break;
                        case 3:
                            if (m_data1_max[3] < 40000)
                            {
                                //m_data_r1.ElementAt(0) = 0;
                                //m_data_r1.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_pcr.RemoveAt((int)uig_sample_index * 4 + (int)columnn);
                                m_data_pcr.Insert((int)uig_sample_index * 4 + (int)columnn, 0);

                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            if (m_data1_max[0] < 40000 && m_data1_max[1] < 40000 && m_data1_max[2] < 40000 && m_data1_max[3] < 40000)
                m_data_pcr.Clear();
            if (m_data_pcr.Count / 4 >= 11)
            {
                for (uint preCounnt = 1; preCounnt < 11; preCounnt++)
                {
                    //for (uint uig_sample_index = 1; uig_sample_index < preCounnt; uig_sample_index++)
                    {
                        uint i = 0, number_data_points = preCounnt, columnn = 0;
                        double k = 0, m = 0;
                        for (columnn = 0; columnn < 4; columnn++)
                        {
                            m = 0;
                            for (i = 0; i < number_data_points; i++)
                            {
                                //uint temp = uig_sample_index - number_data_points + i;
                                m += m_data_pcr.ElementAt((int)(i * 4 + columnn));
                            }
                            k = m / number_data_points;
                            m_data_pcr1.Add(k);
                        }
                    }
                }
            }
            //move noise
            for (uint uig_sample_index = 11; uig_sample_index < m_data_pcr.Count / 4; uig_sample_index++)
            {
                uint i = 0, number_data_points = 11, columnn = 0;
                double k = 0, m = 0;
                for (columnn = 0; columnn < 4; columnn++)
                {
                    m = 0;
                    for (i = 0; i < number_data_points; i++)
                    {
                        uint temp = uig_sample_index - number_data_points + i;
                        m += m_data_pcr.ElementAt((int)(temp * 4 + columnn));
                    }
                    k = m / number_data_points;
                    m_data_pcr1.Add(k);
                }
            }


            protocolFile = System.IO.Path.Combine(anaFilePath,
                 "Temperature Log.csv");
            try { 
            //FileStream fs = new FileStream("C:\\Users\\Helixgen\\Desktop\\snp\\Circuit Boards\\Melt Curve logging.csv", FileMode.Open, FileAccess.Read, FileShare.None);
                  FileStream  fs = new FileStream(protocolFile, FileMode.Open, FileAccess.Read, FileShare.None);
                   StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(936));
                    sr.DiscardBufferedData();
                    string str = "";
                    str = sr.ReadLine();
                    str = sr.ReadLine();
                    str = sr.ReadLine();
                    //string s = Console.ReadLine();
                    while (str != null)
                    {
                        //line++;
                        str = sr.ReadLine();
                        if (str != null)
                        {
                            string[] xu = new String[25];
                            xu = str.Split(',');
                            m_pcr_t.Add(double.Parse(xu[14]));
                            //m_data_pcr.Add(double.Parse(xu[10]));
                            //m_data_pcr.Add(double.Parse(xu[11]));
                            //m_data_pcr.Add(double.Parse(xu[12]));
                            //m_data_t.Add(double.Parse(xu[0]));
                        }
                    }
                    sr.Close();
            }
            catch(Exception ex)
            {
                logger.Error("fileOperation caught an exception; {0}", ex.Message);
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
           comPortLists.Add(Config.m_OpticsController_Configuration.m_strPort);//2018

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
               }
           }
           


            // Things that require the configuration must be after this line.

            // Initialize the optics board.
            SendProgress(0.2f, "the Optics Subsystem.");
            _opticsBoard.Initialize();
            

            List<Task> initTasks = new List<Task>();

            // Initialize the motor controller.
            
            SendProgress(0.3f, "Initializing the first Motor Controller.");
            Task<bool> bMbInit = _motorBoard.Initialize(Config.m_MotorControllerConfigurations["MC-1"].m_strPort);
            initTasks.Add(bMbInit);



            // Initialize the second motor controller.
            SendProgress(0.4f, "Initializing the second Motor Controller.");
            Task<bool> bMbXInit = _motorBoardX.Initialize(Config.m_MotorControllerConfigurations["MC-2"].m_strPort);
            initTasks.Add(bMbXInit);



            // Initialize the temp controller.
            SendProgress(0.5f, "Initializing the Temperature controller.");
            Task<bool> bTbInit = _tecBoard.Initialize();
             initTasks.Add(bTbInit);
            
            

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
            
             _deviceOpticsMotor.initialize();

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
            SendProgress(0.95f, "Initializing: Positioning Chassis Piston.");
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
            _deviceOpticsMotor.initialize();
            //SendProgress(0.9f, "Initializing: Preparing Heater.");
            _deviceHeater.initialize();
            //SendProgress(0.95f, "Initializing: Positioning Chassis Piston.");
            _deviceChassisPiston.initialize();

            //SendProgress(1.0f, "Initialization completed.", true);


            // Start the temperature monitor process. 

            StartTempMonitor();

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
        /// 
        protected void SendProgress(float percentage, string msg, bool bComplete = false)
        {
            if (initializationProgress != null)
                initializationProgress(this, new progressEventArgs(percentage, msg, bComplete));
        }




        protected void SendRunProgress(float percentage, string msg, bool bComplete = false)
        {
            if (runProgress != null)
                runProgress(this, new progressEventArgs(percentage, msg, bComplete));
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
        public void modeBeginAna()
        {
            if (evtBeginAnalysis != null)
                evtBeginAnalysis();
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
            get { return _deviceSlider; }
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

        public Timer GetTimer
        {
            get { return _poll_timer1; }
        }

        public string ProtocolDescLine
        {
            get { return _protocolDescLine; }
            set { _protocolDescLine = value;

                if (ProtocolExecutionStepped != null)
                    ProtocolExecutionStepped(_protocolDescLine);
            }
        }
        public Protocol GetProtocol
        {

            get { return _protocol; }
        }
        //_protocol = new Protocol();


        #endregion

        /// <summary>
        /// Executes the current script.
        /// </summary>
        public async void RunScript(string scriptName = "")
        {
            if (!string.IsNullOrEmpty(scriptName))
            {
                bool bLoadSucceeded = true;

                //_protocol = new Protocol();

                // Load the specified protocol.

                try
                {
                    Task<bool> loadResult = _protocol.Load(scriptName);
                    await loadResult;
                }
                catch(Exception ex)
                {
                    
                    bLoadSucceeded = false;
                    MessageBox.Show("file formate wrong");
                }

                if (bLoadSucceeded)
                {
                    CurrentScript = scriptName;

                    // Put out a status message.

                    if (ProtocolExecutionStepped != null)
                        ProtocolExecutionStepped(string.Format("Starting execution of {0}",
                            Path.GetFullPath(scriptName)));

                    bStopScript = false;

                    try
                    {
                        // Create the results subdirectory.

                        resultsDir = Path.Combine(HelixGen.CSystem_Defns.strDefaultMeasurementLogPath,
                            DateTime.Now.ToString("yyy_MM_dd_HH_mm_ss")
                            );

                        Directory.CreateDirectory(resultsDir);

                        anaFilePath = resultsDir;

                        ProtocolDescLine = string.Format("Results will be stored in: {0}", Path.GetFullPath(resultsDir));

                        // Start off a process logging the temperatures for the duration of
                        // the protocol execution.

                        bStopTempResults = false;

                        runSecondMeter = 0;
                        percentageofTime = 0;
                        percentageStep = 0.00012f;
                        inWaitStas = false;
                       

                        timeStep = 1;
                        //StartRunMonitor();
                        
                        //await InitializeMoto();

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
                                }//2018
                            }
                        }
                        );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        //Task<bool> bExecute = _protocol.Execute();
                        //await bExecute;
                        await _protocol.Execute();
                    }
                    catch(Exception ex)
                    {
                        logger.Error("RunScript caught an exception; {0}", ex.Message);
                    }

                    bStopTempResults = true;
                    anaFilePath = resultsDir;
                    modeBeginAna();

                  // await InitializeMoto();
                  /*
                  if (_protocol.GetStatus != 3)
                  {
                      percentageofTime = 0.95f;
                      percentageStep = 0.001f;
                      _protocol.GetStatus = 3;
                      _protocol.SendStatusProgress(3);
                  }*/

                    if (ProtocolExecutionStepped != null)
                    {
                        ProtocolExecutionStepped("Execution Completed");
                    }

                    // StopScript();
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

        protected void StartTempMonitor()
        {
            _poll_timer = new System.Threading.Timer(OnPoll);
            _poll_timer.Change(1000, 1000); // Set the timer to run every minute.
        }
        /*
        protected void StartRunMonitor()
        {
            _poll_timer1 = new System.Threading.Timer(OnPoll1);
            _poll_timer1.Change(1000, 1000); // Set the timer to run every minute.
        }
        */
        // Callback from polling timer
        private void OnPoll(object state)
        {
            // Read the temperatures on the TEC devices and the resistive heater.

            float tec0Temp = _tecBoard.theBoard.GetTemperature(4, 0);
            float tec1Temp = _tecBoard.theBoard.GetTemperature(5, 0);
            float tec0Power = _tecBoard.theBoard.GetPower(0);
            float tec1Power = _tecBoard.theBoard.GetPower(1);
            float RH1Temp = _tecBoard.theBoard.GetTemperature(6, 0);
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
        /*
        private void OnPoll1(object state)
        {
            // Read the temperatures on the TEC devices and the resistive heater.

            //float runProgress = 0;
            string runProg;
            int notElapTime = 0;

            runSecondMeter++;
            percentageofTime += percentageStep;
            notElapTime =(int) (runSecondMeter / percentageofTime - runSecondMeter);
            //notElapTime = (int)(1 / percentageStep - runSecondMeter);

            //percentageStep = 0.001f;
            
            if (!inWaitStas && percentageofTime >= 0.995f && _protocol.GetStatus < 3)
            {
                percentageofTime = 0.995f;
                percentageStep = 0.00002f;
                inWaitStas = true;
            }
            if (percentageofTime >= 1.0f)
            {
                percentageofTime = 1.0f;
                _poll_timer1.Dispose();
            }

            runProg = (percentageofTime * 100).ToString("F3");
            TimeSpan ts = new TimeSpan(0, 0, notElapTime);
            //richTextBox2.Text = ts.Hours + "小时" + ts.Minutes + "分钟" + ts.Seconds + "秒";
            if(!bStopScript)
                runProg = "总进度："+ runProg+"%     剩余时间："+ ts.Hours + "小时" + ts.Minutes + "分" + ts.Seconds + "秒";
            else
            {
                //runProg = "Ending,please wait......";
                runProg = "结束中，请耐心等待......";
                //percentageofTime = 1;
            }
            
            SendRunProgress(percentageofTime, runProg);
        }
        */
        protected void StopTempMonitor()
        {

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
