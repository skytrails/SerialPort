using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Xml.Serialization;
using HelixGen;
using NLog;

namespace ABot2
{
    public class clsAMB: IDisposable
    {
        private SerialPort objSerialPort = new SerialPort();
        private Object lockobj = new Object();

        private string tmpsSPData = "";

        private int statusByte;
        private bool statusBusy;
        private bool statusError;
        private int errorcode;

        public static Logger logger = LogManager.GetCurrentClassLogger();

        //-------------------------------------------------------------------

        public class BotParameters
        {
            public string ExtensionBoardPresets { get; set; }
        }

        public class AxisParameters
        {
            public string strAxis;
            public string strStartSpeed;
            public string strMaxSpeed;
            public string strAcceleration;
            public string strDeceleration;
            public string strJerk;
            public string strMoveCurrent;
            public string strHoldCurrent;
            public string strResolution;
            public string strDirectionPolarity;
            public string strHomeTimeout;
            public string strProfileMode;
            public string strEncoderPresent;
            public string strEncoderEncoderMonitorTimer_ms;
            public string strEncoderMonitorPulseChangeThreshold;
            public string strEncoderMonitorErrorCountThreshold;
            public string strEncoderDirectionPolarity;
            public string strEncoderStartOffset; // Logic part, no load into controller
            public string strEncoderControlEnabled; // Logic part, no load into controller
            public string strEncoderControlMode; // Logic part, no load into controller
            public string strEncoderControllerConversionFactor; // Logic part, no load into controller
            public string strLostStepsLimit; // Logic part, no load into controller
            public string strLostStepsCheckMode; // Logic part, no load into controller
        }

        public AxisParameters[] axisparams_current = new AxisParameters[4]
         {
            new AxisParameters(),
            new AxisParameters(),
            new AxisParameters(),
            new AxisParameters()
         };

        public class AMBParameters
        {
            public BotParameters botparams_current { get; set; }
            public AxisParameters[] axisparams_current { get; set; }
        }

        public AMBParameters ambparams_current = new AMBParameters()
        {
            botparams_current = new BotParameters(),
            axisparams_current = new AxisParameters[4] { 
                new AxisParameters(),
                new AxisParameters(),
                new AxisParameters(),
                new AxisParameters()
            }
        };

        public AMBParameters ambparams_defaults = new AMBParameters()
        {
            botparams_current = new BotParameters
            {
                ExtensionBoardPresets = "0"
            },

            axisparams_current = new AxisParameters[4] { 
            new AxisParameters() {
                strAxis = "01",
                strStartSpeed = "50",
                strMaxSpeed = "1200",
                strAcceleration = "3000",
                strDeceleration = "3000",
                strJerk = "30000",
                strMoveCurrent = "80",
                strHoldCurrent = "25",
                strResolution = "3",
                strDirectionPolarity = "0",
                strHomeTimeout = "60",
                strProfileMode = "0",
                strEncoderPresent = "0",
                strEncoderDirectionPolarity = "0",
                strEncoderStartOffset = "0",
                strEncoderControlEnabled = "0",
                strEncoderControlMode = "0",
                strEncoderControllerConversionFactor = "1",
                strLostStepsLimit = "0",
                strLostStepsCheckMode = "0"
            },
            new AxisParameters() { 
                strAxis = "02",
                strStartSpeed = "50",
                strMaxSpeed = "1200",
                strAcceleration = "3000",
                strDeceleration = "3000",
                strJerk = "30000",
                strMoveCurrent = "80",
                strHoldCurrent = "25",
                strResolution = "3",
                strDirectionPolarity = "0",
                strHomeTimeout = "60",
                strProfileMode = "0",
                strEncoderPresent = "0",
                strEncoderDirectionPolarity = "0",
                strEncoderStartOffset = "0",
                strEncoderControlEnabled = "0",
                strEncoderControlMode = "0",
                strEncoderControllerConversionFactor = "1",
                strLostStepsLimit = "0",
                strLostStepsCheckMode = "0"
            },
            new AxisParameters() {
                strAxis = "03",
                strStartSpeed = "50",
                strMaxSpeed = "1200",
                strAcceleration = "3000",
                strDeceleration = "3000",
                strJerk = "30000",
                strMoveCurrent = "80",
                strHoldCurrent = "25",
                strResolution = "3",
                strDirectionPolarity = "0",
                strHomeTimeout = "60",
                strProfileMode = "0",
                strEncoderPresent = "0",
                strEncoderDirectionPolarity = "0",
                strEncoderStartOffset = "0",
                strEncoderControlEnabled = "0",
                strEncoderControlMode = "0",
                strEncoderControllerConversionFactor = "1",
                strLostStepsLimit = "0",
                strLostStepsCheckMode = "0"
            },
            new AxisParameters() {
                strAxis = "04",
                strStartSpeed = "50",
                strMaxSpeed = "1200",
                strAcceleration = "3000",
                strDeceleration = "3000",
                strJerk = "30000",
                strMoveCurrent = "80",
                strHoldCurrent = "25",
                strResolution = "3",
                strDirectionPolarity = "0",
                strHomeTimeout = "60",
                strProfileMode = "0",
                strEncoderPresent = "0",
                strEncoderDirectionPolarity = "0",
                strEncoderStartOffset = "0",
                strEncoderControlEnabled = "0",
                strEncoderControlMode = "0",
                strEncoderControllerConversionFactor = "1",
                strLostStepsLimit = "0",
                strLostStepsCheckMode = "0"
            },
            }
        };

        public bool[] DI_State = new bool[8];
        public bool[] DO_State = new bool[8];

        public class AxisCoordinates
        {
            public string ControllerCoordinate;
            public string EncoderCoordinate;
        }

        public AxisCoordinates[] Robot_Position = new AxisCoordinates[4] {
            new AxisCoordinates(){
                ControllerCoordinate = "-", // undefined, until homing
                EncoderCoordinate = "-"
            },

            new AxisCoordinates(){
                ControllerCoordinate = "-",
                EncoderCoordinate = "-"
            },

            new AxisCoordinates(){
                ControllerCoordinate = "-",
                EncoderCoordinate = "-"
            },

            new AxisCoordinates(){
                ControllerCoordinate = "-",
                EncoderCoordinate = "-"
            },
        };

        // default config file
        // JAD public string cfgfilepath_defaults = Path.Combine(System.Windows.Forms.Application.StartupPath, "ambcfg.xml");

        //-------------------------------------------------------------------

        /// <summary>
        /// Initialize and open serial port
        /// </summary>
        /// <param name="sPort">Port symbolic name to operate with</param>
        public void Initialize(string sPort)
        {
            if (objSerialPort.IsOpen)
            {
                objSerialPort.Close();
            }

            objSerialPort.PortName = sPort;
            objSerialPort.BaudRate = 115200;
            objSerialPort.ReadTimeout = 1000;
            objSerialPort.WriteTimeout = 1000;
            objSerialPort.StopBits = StopBits.One;
            objSerialPort.DataBits = 8;
            objSerialPort.Handshake = Handshake.None;
            objSerialPort.Parity = Parity.None;

            objSerialPort.Open();
        }

        /// <summary>
        /// Execute data exchange with serial port
        /// </summary>
        /// <param name="indata">Input data - command to send to serial port</param>
        /// <param name="outdata">Output data from serial port</param>
        /// <param name="do_response_parsing">Flag, forces to perform data parsing and status byte definition</param>
        /// <param name="do_skip_read">Flag, forces to skip output data read from serial port</param>
        private void ExecCmd(string indata, out string outdata, bool do_response_parsing = true, bool do_skip_read = false)
        {
            string msg;

            lock (lockobj)
            {
                outdata = "";

                int irepeat_count = 3;
                do
                {
                    if (clsGlobals.bGUIStopSign)
                    {
                        return;
                    }

                    try
                    {
                        //logger.Debug("ABot2::clsAMB::ExecCMD sending command; \"{0}\"",
                        //    indata);
                        objSerialPort.WriteLine(indata + '\r');

                        if (do_skip_read)
                        {
                            return;
                        }

                        do
                        {
                            outdata = objSerialPort.ReadLine();
                        }
                        while (objSerialPort.BytesToRead > 0);

                        if (!do_response_parsing)
                        {
                            return;
                        }

                        if ((outdata[0] != '/') && (outdata[1] != '0'))
                        {
                            msg = "Error: (AMB) response parsing error. Data: " + outdata + "; command: " + indata + "; iteration: " + irepeat_count.ToString();
                            logger.Error("ABot2::clsAMB::ExecCMD throwing Exception {0}", msg);
                            throw new Exception(msg);
                        }

                        statusByte = (int)outdata[2];
                        statusBusy = (!((statusByte & 32) > 0));
                        statusError = ((statusByte & 31) > 0);
                        errorcode = (statusByte & 31);
                        /*
                        if (errorcode != 0)
                        {
                            msg = string.Format("(AMB )Error: {0}  Data: {1}; command: {2}; iteration: {3}", errorcode.ToString(), outdata, indata, irepeat_count.ToString());
                            logger.Error("ABot2::clsAMB::ExecCMD throwing Exception {0}", msg);

                            if (errorcode == 7)
                            {
                                ;//ReinitializeController();
                            }
                            else
                                throw new Exception(msg);
                        }*/

                        if (outdata.Length > 3)
                        {
                            outdata = outdata.Remove(0, 3).Replace("\r", "").Replace("\n", "");
                            return;
                        }

                        msg = "Error: (AMB) empty data returned; command: " + indata + "; iteration: " + irepeat_count.ToString();
                        logger.Error("ABot2::clsAMB::ExecCMD throwing Exception {0}",
                            msg);
                        throw new Exception(msg);
                    }
                    catch (Exception ex)
                    {
                        objSerialPort.DiscardInBuffer();
                        objSerialPort.DiscardOutBuffer();

                        if (--irepeat_count > 0)
                        {
                            Thread.Sleep(50);
                            continue;
                        }
                        else
                        {
                            msg = "Error: (AMB): " + ex.Message + "; source command: " + indata + "; iteration: " + irepeat_count.ToString() + "; error code: " + errorcode;
                            logger.Error("ABot2::clsAMB::ExecCMD throwing Exception {0}",
                                msg);
                            throw new Exception(msg, ex);
                        }
                    }
                } while (true);
            } // end of lock
        }

        /// <summary>
        /// Function attempts to respond to Controller not initialized errors by rehoming.
        /// </summary>
        public void ReinitializeController()
        {
            logger.Debug("ABot2::clsAMB::ReinitalizeController starting");

            string str;
  
            // Clear the errors on all the axes

            foreach (string axis in "01,02,03,04".Split(','))
            {
                GetClearErrorCode(axis, out str);
            }

            // Wait for robot to complete any ongoing actions.

            // N.B.; Order is important here.

            while (GetStatus("04") != "00")    // Wait for the axis to become not busy.
            {
                System.Threading.Thread.Sleep(1000);
                GetClearErrorCode("04", out str);
                logger.Debug("ABot2::clsAMB::ReinitalizeController clearerror 04");
            }

            MoveHome("04");
            WaitForCompletion("04");

            while (GetStatus("03") != "00")    // Wait for the axis to become not busy.
            {
                System.Threading.Thread.Sleep(100);
                GetClearErrorCode("03", out str);
                logger.Debug("ABot2::clsAMB::ReinitalizeController clearerror 03");
            }

            MoveHome("03");
            WaitForCompletion("03");

            while (GetStatus("02") != "00")    // Wait for the axis to become not busy.
            {
                System.Threading.Thread.Sleep(100);
                GetClearErrorCode("02", out str);
                logger.Debug("ABot2::clsAMB::ReinitalizeController clearerror 02");
            }

            while (GetStatus("01") != "00")    // Wait for the axis to become not busy.
            {
                System.Threading.Thread.Sleep(100);
                GetClearErrorCode("01", out str);
                logger.Debug("ABot2::clsAMB::ReinitalizeController clearerror 01");
            }

            MoveHome("02");
            MoveHome("01");
            WaitForCompletion("01,02");

            logger.Debug("ABot2::clsAMB::ReinitalizeController finishing");
        }

        /// <summary>
        /// Execute command free style type
        /// </summary>
        /// <param name="indata">Input command</param>
        /// <param name="outdata">Output data</param>
        public void ExecuteFreeStyleCommand(string indata, out string outdata)
        {
            ExecCmd(indata, out outdata);
        }

        /// <summary>
        /// Perform AMB poll for busy bit and errors
        /// </summary>
        /// <param name="axes">Axes to poll (operate with) (01 .. 04).
        /// Format for multiple axes polling: ["01,02,03,04"] (comma delimited string with axes)</param>
        public void WaitForCompletion(string axes = "01,02,03,04")
        {
            if (axes == "")
            {
                axes = "01,02,03,04";
            }

            string[] spaxes = axes.Split(',');
            foreach (string axis in spaxes)
            {
                do
                {
                    if (clsGlobals.bGUIStopSign)
                    {
                        return; // end of operation; stop event would be managed in calling function.
                    }

                    ExecCmd("/" + axis + "Q", out tmpsSPData);

                    if (!statusBusy)
                    {
                        break;
                    }

                    Thread.Sleep(100);

                } while (true);
            }
        }

        /// <summary>
        /// Perform lost steps check based on comparision reading from motor controller and encoder
        /// </summary>
        /// <param name="axis">Axes to operate with ('01'|'02'|'03'|'04')</param>
        /// Format for multiple axes polling: ["01,02,03,04"] (comma delimited string with axes)</param>
        public void LostStepsCheckEncoderVsController(string axes = "01,02,03,04")
        {
            string spos_val, spos_enc_val;
            int ipos_ctrl_val, ipos_enc_val, ictrlpos_convertedfrom_encpos;
            int ilsidx;
            int ilost_steps_limit;
            string serr = "";

            string[] spaxes = axes.Split(',');
            foreach (string axis in spaxes)
            {
                // conversion 
                ilsidx = Convert.ToInt32(axis) - 1;

                // extra check - override external call
                if (ambparams_current.axisparams_current[ilsidx].strLostStepsCheckMode == "0")
                {
                    continue;
                }

                if (ambparams_current.axisparams_current[ilsidx].strLostStepsCheckMode != "1")
                {
                    string msg = string.Format("Wrong Lost steps check mode parameter value: {0}.",
                        ambparams_current.axisparams_current[ilsidx].strLostStepsCheckMode);
                    logger.Error("ABot2::LostStepsCheckEncoderVsController throwing Exception {0}",
                        msg);
                    throw new Exception(msg);
                }

                // limit for current axis - from configuration
                ilost_steps_limit = Convert.ToInt32(ambparams_current.axisparams_current[ilsidx].strLostStepsLimit);

                // get current positions for current axis
                GetCurrentPosition(axis, out spos_val);
                GetCurrentPositionByEncoder(axis, out spos_enc_val);
                ipos_ctrl_val = Convert.ToInt32(spos_val);
                ipos_enc_val = Convert.ToInt32(spos_enc_val);

                // convert from encoder to controller steps
                ConvertStepsEncoderToController(ilsidx, ipos_enc_val, false, out ictrlpos_convertedfrom_encpos);

                // check
                if (Math.Abs(ipos_ctrl_val - ictrlpos_convertedfrom_encpos) >= ilost_steps_limit)
                {
                    serr += "Lost steps limit was exceeded, axis: " + axis + "\r";
                }
            }

            if (serr != "")
            {
                string msg = "Error: " + serr;
                logger.Error("ABot2::LostStepsCheckEncoderVsController throwing Exception {0}",
                    msg);
                throw new Exception(msg);
            }


        }

        /// <summary>
        /// Read stepper motor missed steps
        /// Note: move home operation required before this function call
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">A value representing the stepper motor missed steps</param>
        /// <returns>The number of missed steps.</returns>
        public int GetMissedSteps(string axis, out string value)
        {
            value = "";

            string cmd = "/" + axis + "?E";
            ExecCmd(cmd, out value);

            return Int32.Parse(value);
        }

        /// Read stepper motor missed steps
        /// Note: move home operation required before this function call
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">A value representing the stepper motor missed steps</param>
        /// <returns>The number of missed steps.</returns>
        public int GetMissedSteps(string axis)
        {
            string value = "";

            string cmd = "/" + axis + "?E";
            ExecCmd(cmd, out value);

            return Int32.Parse(value);
        }

        /// <summary>
        /// Move Stepper Motor to absolute position
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Position to move to in microsteps (0-134217727)</param>
        public void MoveToAbsolutePosition(string axis, string value)
        {
            string cmd = "/" + axis + "A" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Move Stepper Motor relative in positive direction
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Distance to move in microsteps (0-134217727)</param>
        public void MoveRelativePositive(string axis, string value)
        {
            string cmd = "/" + axis + "P" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Move Stepper Motor relative in negative direction
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Distance to move in microsteps (0-134217727)</param>
        public void MoveRelativeNegative(string axis, string value)
        {
            string cmd = "/" + axis + "D" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Set Stepper Motor direction polarity
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Direction to set (0,1; default: 0)</param>
        public void SetDirectionPolarity(string axis, string value)
        {
            string cmd = "/" + axis + "o" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Set Stepper Motor position without moving
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Position to set (0-134217727)</param>
        public void SetPosition(string axis, string value)
        {
            string cmd = "/" + axis + "S" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Home stepper motor 
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Number of steps to move before runaway exception occurs (1-134217727)</param>
        public void MoveHome(string axis, string value = "134217727")
        {
            string cmd = "/" + axis + "Z" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Set stepper motor home routine watchdog timeout
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Watchdog timeout, sec (1-6000; default: 120)</param>
        public void SetHomeTimeout(string axis, string value)
        {
            string cmd = "/" + axis + "W" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Set Start speed of stepper motor
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Start velocity in units of steps/sec (0-10,000; default: 50)</param>
        public void SetStartSpeed(string axis, string value)
        {
            string cmd = "/" + axis + "v" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Set Max/Slew Speed of stepper motor
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Velocity in units of steps/sec (0-100,000; default: 2,500)</param>
        public void SetMaxSpeed(string axis, string value)
        {
            string cmd = "/" + axis + "V" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Set Acceleration factor of stepper motor
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Acceleration in units of steps/sec2 (0-150,000; default: 15,000)</param>
        public void SetAcceleration(string axis, string value)
        {
            string cmd = "/" + axis + "L" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Set Deceleration factor of stepper motor
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Deceleration in units of steps/sec2 (0-150,000; default: 15,000)</param>
        public void SetDeceleration(string axis, string value)
        {
            string cmd = "/" + axis + "d" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Set Jerk parameter of stepper motor
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Jerk in units of steps/sec3 (0-1,500,000; default: 150,000)</param>
        public void SetJerk(string axis, string value)
        {
            string cmd = "/" + axis + "J" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Set profile mode used to calculate motor trajectory of stepper motor
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Profile mode (0 = trapezoidal, 1 = S-curve, 2-velocity; default: 0)</param>
        public void SetProfileMode(string axis, string value)
        {
            string cmd = "/" + axis + "p" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Sets Move Current on a scale of 0 to 100% of max current for stepper motor (Full scale motor current is 2.0A)
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Move Current (0-100%, default: 30)</param>
        public void SetMoveCurrent(string axis, string value)
        {
            string cmd = "/" + axis + "m" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Sets Hold Current on a scale of 0 to 100% of max current for stepper motor (Full scale motor current is 2.0A)
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Hold Current (0-50%, default: 12)</param>
        public void SetHoldCurrent(string axis, string value)
        {
            string cmd = "/" + axis + "h" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Set micro step mode (resolution)
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Mode:
        /// 0 = Full stepping
        /// 1 = Half stepping
        /// 2 = Quarter stepping
        /// 3 = Sixteenth stepping
        /// NB: Moves are still made in full step increments when microstepping. Motor must be initialized (in home state) to change stepping mode
        /// </param>
        public void SetResolution(string axis, string value)
        {
            string cmd = "/" + axis + "N" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Enable / disable the encoder
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Mode to set:
        /// 1 = Enable
        /// 0 = Disable
        /// </param>
        public void SetEncoderState(string axis, string value)
        {
            string cmd = "/" + axis + "k" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Set encoder direction polarity
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Mode to set:
        /// 1 = Negative
        /// 0 = Positive
        public void SetEncoderDirectionPolarity(string axis, string value)
        {
            string cmd = "/" + axis + "e" + value + "R";
            ExecCmd(cmd, out tmpsSPData);
        }

        /// <summary>
        /// Terminate current axis move (command execution). Multiple axes could be used.
        /// </summary>
        /// <param name="axis">Axes to operate with (01 .. 04)</param>
        public void Terminate(string axes)
        {
            string[] spaxes = axes.Split(',');
            foreach (string axis in spaxes)
            {
                string cmd = "/" + axis + "T";
                ExecCmd(cmd, out tmpsSPData, false, false);
            }
        }

        /// <summary>
        /// Get the current commanded stepper motor position
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Current (commanded) position (output value)</param>
        public void GetCurrentPosition(string axis, out string value)
        {
            string cmd = "/" + axis + "?0";
            ExecCmd(cmd, out value);
        }

        /// <summary>
        /// Get the current commanded stepper motor position
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        public int GetCurrentPosition(string axis)
        {
            string value;

            string cmd = "/" + axis + "?0";
            ExecCmd(cmd, out value);

            return Int32.Parse(value);
        }

        /// <summary>
        /// Get the current stepper Start Velocity
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Current start speed (output value)</param>
        public void GetStartSpeed(string axis, out string value)
        {
            string cmd = "/" + axis + "?1";
            ExecCmd(cmd, out value);
        }

        /// <summary>
        /// Get the current stepper Slew/Max Speed
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Current max speed (output value)</param>
        public void GetMaxSpeed(string axis, out string value)
        {
            string cmd = "/" + axis + "?2";
            ExecCmd(cmd, out value);
        }

        /// <summary>
        /// Get the current stepper Slew/Max Speed
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Current max speed (output value)</param>
        public int GetMaxSpeed(string axis)
        {
            string value;

            string cmd = "/" + axis + "?2";
            ExecCmd(cmd, out value);

            return Int32.Parse(value);
        }

        /// <summary>
        /// Get the status of the stepper Home Switch
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Current status (1=closed, 0=open)</param>
        public void GetHomeSwitchStatus(string axis, out string value)
        {
            string cmd = "/" + axis + "?4";
            ExecCmd(cmd, out value);
            //return Int32.Parse(value);
        }

        /// <summary>
        /// Get the current stepper Acceleration
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Current acceleration (output value)</param>
        public void GetAcceleration(string axis, out string value)
        {
            string cmd = "/" + axis + "?8";
            ExecCmd(cmd, out value);
        }

        /// <summary>
        /// Get the current stepper Acceleration
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Current acceleration (output value)</param>
        public int GetAcceleration(string axis)
        {
            string value;
            string cmd = "/" + axis + "?8";
            ExecCmd(cmd, out value);
            return Int32.Parse(value);
        }

        /// <summary>
        /// Get the current stepper Deceleration
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Current deceleration (output value)</param>
        public void GetDeceleration(string axis, out string value)
        {
            string cmd = "/" + axis + "?9";
            ExecCmd(cmd, out value);
        }

        /// <summary>
        /// Get the current stepper Deceleration
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Current deceleration (output value)</param>
        public int GetDeceleration(string axis)
        {
            string value;
            string cmd = "/" + axis + "?9";
            ExecCmd(cmd, out value);
            return Int32.Parse(value);
        }

        /// <summary>
        /// Get the current stepper Jerk
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Current jerk (output value)</param>
        public void GetJerk(string axis, out string value)
        {
            string cmd = "/" + axis + "?A";
            ExecCmd(cmd, out value);
        }

        /// <summary>
        /// Get the current stepper Profile Mode
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Current mode (output value)
        /// 0 = trapezoidal
        /// 1 = S-curve
        /// 2 = velocity
        /// </param>
        public void GetProfileMode(string axis, out string value)
        {
            string cmd = "/" + axis + "?B";
            ExecCmd(cmd, out value);
        }

        /// <summary>
        /// Get instruction error code (and clears error) 
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Current error (output value)</param>
        public void GetClearErrorCode(string axis, out string value)
        {
            string cmd = "/" + axis + "?C";
            ExecCmd(cmd, out value);
        }

        /// <summary>
        /// Get micro stepping mode 
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Current mode (output value)</param>
        public void GetResolution(string axis, out string value)
        {
            string cmd = "/" + axis + "?D";
            ExecCmd(cmd, out value);
        }

        /// <summary>
        /// Returns number of missed steps from stepper motor home routine
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Number of missed steps (output value)</param>
        public void GetNumberOfMissedSteps(string axis, out string value)
        {
            string cmd = "/" + axis + "?E";
            ExecCmd(cmd, out value);
        }

        /// <summary>
        /// Returns move current value for stepper motor
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Move current (%) (output value)</param>
        public void GetMoveCurrent(string axis, out string value)
        {
            string cmd = "/" + axis + "?F";
            ExecCmd(cmd, out value);
        }

        /// <summary>
        /// Returns hold current value for stepper motor
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Hold current (%) (output value)</param>
        public void GetHoldCurrent(string axis, out string value)
        {
            string cmd = "/" + axis + "?G";
            ExecCmd(cmd, out value);
        }

        /// <summary>
        /// Returns DC motor home routine timeout in seconds
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Home timeout (seconds) (output value)</param>
        public void GetHomeTimeoutDC(string axis, out string value)
        {
            string cmd = "/" + axis + "?H";
            ExecCmd(cmd, out value);
        }

        /// <summary>
        /// Returns Stepper motor home routine timeout in seconds
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Home timeout (seconds) (output value)</param>
        public void GetHomeTimeoutStepper(string axis, out string value)
        {
            string cmd = "/" + axis + "?I";
            ExecCmd(cmd, out value);
        }

        /// <summary>
        /// Get the Ready/Busy status as well as any error conditions in the “Status” byte of the return string
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Status data (output value):
        /// 1st character: busy status: "0"-free, "1"-busy;
        /// 2nd character: error status: "0"-no error, "1"-error occurred
        /// </param>
        public void GetStatus(string axis, out string value)
        {
            string cmd = "/" + axis + "Q";
            ExecCmd(cmd, out value);

            // reset

            value = statusBusy ? "1" : "0";
            value += statusError ? "1" : "0";
        }

        /// <summary>
        /// Get the Ready/Busy status as well as any error conditions in the “Status” byte of the return string
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Status data (output value):
        /// 1st character: busy status: "0"-free, "1"-busy;
        /// 2nd character: error status: "0"-no error, "1"-error occurred
        /// </param>
        public string GetStatus(string axis)
        {
            string valueOut;

            string cmd = "/" + axis + "Q";
            ExecCmd(cmd, out valueOut);

            // SWE; N.B. Note below that valueOut is reset.
            // StatusBusy and status error are set in ExecCmd as side effects.

            valueOut = statusBusy ? "1" : "0";
            valueOut += statusError ? "1" : "0";

            return valueOut;
        }

        /// <summary>
        /// Returns actual position set by the encoder
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Current position (output value)</param>
        public void GetCurrentPositionByEncoder(string axis, out string value)
        {
            string cmd = "/" + axis + "?L";
            ExecCmd(cmd, out value);
        }

        /// <summary>
        /// Returns the current Firmware revision and date
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Current revision and date (output value)</param>
        public void GetCurrentRevisionDate(string axis, out string value)
        {
            string cmd = "/" + axis + "&";
            ExecCmd(cmd, out value);
        }

        /// <summary>
        /// Get digital input channel status
        /// </summary>
        /// <param name="channel">Channel to operate with [1..8]</param>
        /// <param name="status">Input channel status:
        /// 0 = Off, open
        /// 1 = On, triggered
        /// </param>
        public void GetDIOStatus(string channel, out string status)
        {
            status = "";

            int ich;
            string odata;

            // reformat and check input data
            if (!int.TryParse(channel, out ich))
            {
                string msg = string.Format("Error: (DIO get) wrong channel number: {0}.",
                     channel);
                logger.Error("ABot2::GetDIOStatus throwing Exception {0}",
                    msg);
                throw new Exception(msg);
            }
            if (ich < 1 || ich > 8)
            {
                string msg = "Error: (DIO get) wrong channel number: " + channel;
                logger.Error("ABot2::GetDIOStatus throwing Exception {0}",
                    msg);
                throw new Exception(msg);
            }

            // shift (board design)
            ich = ich + 18;

            // data exchange
            string cmd = "/" + ich.ToString() + "E" + "0" + "R";
            ExecCmd(cmd, out odata);

            // parse
            if (odata[0] == 48)
            {
                status = "0"; // off
            }
            else if (odata[0] == 49)
            {
                status = "1"; // on
            }
            else
            {
                string msg = "Error: (DIO get) read data error.";
                logger.Error("ABot2::GetDIOStatus throwing Exception {0}",
                    msg);
                throw new Exception(msg);
            }
        }

        /// <summary>
        /// Get digital input channel status
        /// </summary>
        /// <param name="channel">Channel to operate with [1..8]</param>
        /// <returns>
        /// false if the value is off, true otherwise.
        /// </returns>
        public bool GetDIOStatus(string channel)
        {
            bool bRetVal;

            int ich;
            string odata;

            // reformat and check input data
            if (!int.TryParse(channel, out ich))
            {
                string msg = string.Format("Error: (DIO get) wrong channel number: {0}.",
                     channel);
                logger.Error("ABot2::GetDIOStatus throwing Exception {0}",
                    msg);
                throw new Exception(msg);
            }
            if (ich < 1 || ich > 8)
            {
                string msg = "Error: (DIO get) wrong channel number: " + channel;
                logger.Error("ABot2::GetDIOStatus throwing Exception {0}",
                    msg);
                throw new Exception(msg);
            }

            // shift (board design)
            ich = ich + 18;

            // data exchange
            string cmd = "/" + ich.ToString() + "E" + "0" + "R";
            ExecCmd(cmd, out odata);

            // parse
            bRetVal = odata[0] != 48;

            return bRetVal;
        }

        /// <summary>
        /// Set digital output channel status
        /// </summary>
        /// <param name="channel">Channel to operate with [1..8]</param>
        /// <param name="status">Output channel status to set:
        /// “1” –> “On”, 24 volts output
        /// “0” –> “Off”, 0 volts output
        /// </param>
        public void SetDIOStatus(string channel, string status)
        {
            logger.Debug("clsAMD::SetDOStatus: channel {0} status {1}", channel, status);

            int ich;
            string odata;

            // reformat and check input data
            if (!int.TryParse(channel, out ich))
            {
                string msg = "Error: (DIO set) wrong channel number: " + channel;
                logger.Error("ABot2::SetDIOStatus throwing Exception {0}",
                    msg);
                throw new Exception(msg);
            }
            if (ich < 1 || ich > 8)
            {
                string msg = "Error: (DIO set) wrong channel number: " + ich.ToString();
                logger.Error("ABot2::SetDIOStatus throwing Exception {0}",
                    msg);
                throw new Exception(msg);
            }

            if ((status != "0") && (status != "1"))
            {
                string msg = "Error: (DIO set) wrong channel status: " + status;
                logger.Error("ABot2::SetDIOStatus throwing Exception {0}",
                    msg);
                throw new Exception(msg);
            }

            // invert status
            if (status == "0")
            {
                status = "1"; // board treats "1" as "off"
            }
            else if (status == "1")
            {
                status = "0";  // board treats "0" as "on"
            }

            // exec
            string cmd = "/1" + channel + "E" + status + "R";
            ExecCmd(cmd, out odata);

            // update internal status (ZZ::n.b.:inverted!)
            if (status == "1")
            {
                DO_State[ich - 1] = true; // software treats "1" as "on"
            }
            else
            {
                DO_State[ich - 1] = false; // software treats "0" as "off"
            }

            logger.Debug("clsAMD::SetDOStatus returning");
        }

        /// <summary>
        /// Set digital output channel status
        /// </summary>
        /// <param name="channel">Channel to operate with [1..8]</param>
        /// <param name="status">Output channel status to set:
        /// “1” –> “On”, 24 volts output
        /// “0” –> “Off”, 0 volts output
        /// </param>
        public void SetDOStatusSpecial(string channel, string status)
        {
            int ich;
            string odata;

            // reformat and check input data
            if (!int.TryParse(channel, out ich))
            {
                string msg = "Error: (DIO set) wrong channel number: " + channel;
                logger.Error("ABot2::SetDIOStatus throwing Exception {0}", msg);
                throw new Exception(msg);
            }
            if (ich < 1 || ich > 8)
            {
                string msg = "Error: (DIO set) wrong channel number: " + ich.ToString();
                logger.Error("ABot2::SetDIOStatus throwing Exception {0}", msg);
                throw new Exception(msg);
            }

            // exec
            string cmd = "/1" + channel + "E" + status + "R";
            ExecCmd(cmd, out odata);

            //// update internal status (ZZ::n.b.:inverted!)
            //if (status == "1")
            //{
            //    DO_State[ich - 1] = true; // software treats "1" as "on"
            //}
            //else
            //{
            //    DO_State[ich - 1] = false; // software treats "0" as "off"
            //}
        }

        //------------------------------------------------------------------------------------------
        //DIO functionality:

        // write control: 
        // example:
        // "/11E0R\r"
        // "/" - control symbol;
        // "1" - board number;
        // "1" - channel number (1..8);
        // "E" - control command;
        // "0" - action: "1"->off (no power), "0"->on (24 volts output);
        // "R" - command finish control symbol;
        // "\r" - carriage return;

        // read control: 
        // example:
        // "/26E0R\r"
        // "/" - control symbol;
        // "26": address - is sum of "18" (fixed const) and "8"->channel number to read in this example.
        // "E" - control command;
        // "0" - control command - yes, always "0"!;
        // "R" - command finish control symbol;
        // "\r" - carriage return;

        //------------------------------------------------------------------------------------------
        // service, encoder

        /// <summary>
        /// Get resolution multiplication factor
        /// </summary>
        /// <param name="axis">Axis , 0-based</param>
        /// <param name="resolution">Current resolution (multiplication factor)</param>
        public void GetCurrentResolution(int axis, out int resolution)
        {
            resolution = 0;
            string sresolution = ambparams_current.axisparams_current[axis].strResolution;
            switch (sresolution)
            {
                case "0":
                    resolution = 1;
                    break;

                case "1":
                    resolution = 2;
                    break;

                case "2":
                    resolution = 4;
                    break;

                case "3":
                    resolution = 16;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Convert controller steps to encoder
        /// </summary>
        /// <param name="iaxis">Axis, 0-based</param>
        /// <param name="icontrollerposition">Controller steps</param>
        /// <param name="brelativemove">Mode: relative move (true) or absolute position (false)</param>
        /// <param name="iencoderposition">Encoder steps</param>
        public void ConvertStepsControllerToEncoder(int iaxis, int icontrollerposition, bool brelativemove, out int iencoderposition)
        {
            iencoderposition = 0;

            double dencoderconversionfactor = Convert.ToDouble(ambparams_current.axisparams_current[iaxis].strEncoderControllerConversionFactor);
            double dencoderstartoffset = Convert.ToDouble(ambparams_current.axisparams_current[iaxis].strEncoderStartOffset);

            //int iresolution = 0;
            //GetCurrentResolution(iaxis, out iresolution);

            // convert for better rounding
            double dcontrollerposition = Convert.ToDouble(icontrollerposition);
            double dresolution = 1; // Convert.ToDouble(iresolution); ZZ::no use resolution convertion for now..

            // start offset counts?
            double dencoderposition;
            if (brelativemove)
            {
                dencoderposition = (dcontrollerposition) * dencoderconversionfactor / dresolution;
            }
            else
            {
                dencoderposition = (dcontrollerposition - dencoderstartoffset) * dencoderconversionfactor / dresolution;
            }

            iencoderposition = Convert.ToInt32(dencoderposition);
        }

        /// <summary>
        /// Convert encoder steps to controller steps
        /// </summary>
        /// <param name="iaxis">Axis, 0-based</param>
        /// <param name="iencoderposition">Encoder steps - destination</param>
        /// <param name="brelativemove">Mode: relative move (true) or absolute position (false)</param>
        /// <param name="icontrollerposition">Position or distance, depends on mode</param>
        public void ConvertStepsEncoderToController(int iaxis, int iencoderposition, bool brelativemove, out int icontrollerposition)
        {
            icontrollerposition = 0;

            //  double dencoderconversionfactor = Convert.ToDouble(ambparams_current.axisparams_current[iaxis].strEncoderControllerConversionFactor);
            //  double dencoderstartoffset = Convert.ToDouble(ambparams_current.axisparams_current[iaxis].strEncoderStartOffset);

            double dencoderconversionfactor = Convert.ToDouble(axisparams_current[iaxis].strEncoderControllerConversionFactor);
             double dencoderstartoffset = Convert.ToDouble(axisparams_current[iaxis].strEncoderStartOffset);

            //ZZ:: skip resolution factor for now..
            //int iresolution = 0;
            //GetCurrentResolution(iaxis, out iresolution);
            //double dresolution = Convert.ToDouble(iresolution);

            // convert for better rounding
            double dencoderposition = Convert.ToDouble(iencoderposition);
            double dresolution = 1; //ZZ::fake resolutioin, for implemented algorithm consistency

            // mode: relative or absolute move..
            double dcontrollerposition;
            if (brelativemove)
            {
                dcontrollerposition = (dencoderposition / dencoderconversionfactor * dresolution);
            }
            else
            {
                dcontrollerposition = (dencoderposition / dencoderconversionfactor * dresolution) + dencoderstartoffset;
            }

            icontrollerposition = Convert.ToInt32(dcontrollerposition);
        }

        /// <summary>
        /// Check encoder control mode and call appropriate control function for encoder move
        /// </summary>
        /// <param name="iaxis">Axis to move, 0-based (0..3)</param>
        /// <param name="iencposition_abs_desired">Absolute encoder position (target)</param>
        public void EncoderControlMoveToAbsolutePosition(int iaxis, int iencposition_abs_desired)
        {
            // check if encoder is enabled
            /*
            //if ((ambparams_current.axisparams_current[iaxis].strEncoderPresent != "1") || (ambparams_current.axisparams_current[iaxis].strEncoderControlEnabled != "1"))
            if ((axisparams_current[iaxis].strEncoderPresent != "1") || (axisparams_current[iaxis].strEncoderControlEnabled != "1"))
            {
                string msg = "Error: EncoderControlMoveToAbsolutePosition: Encoder for axis: " + iaxis.ToString() + " is absent or disabled. ";
                logger.Error("ABot2::EncoderControlMoveToAbsolutePosition throwing Exception {0}",
                    msg);
                throw new Exception(msg);
            }
            

            // select control mode
            if (ambparams_current.axisparams_current[iaxis].strEncoderControlMode == "0")
            {
                EncoderControlMoveToAbsolutePosition_StepByStep(iaxis, iencposition_abs_desired);
            }
            else
            {
                EncoderControlMoveToAbsolutePosition_Continuous(iaxis, iencposition_abs_desired);
            }
            */
            EncoderControlMoveToAbsolutePosition_StepByStep(iaxis, iencposition_abs_desired);
        }

        /// <summary>
        /// Move to encoder position. For final position approaching use "step-by-step mode" algorithm.
        /// </summary>
        /// <param name="axis">Axis, 0-based (0..3)</param>
        /// <param name="iencposition_abs_desired">Destination - encoder position</param>
        public void EncoderControlMoveToAbsolutePosition_StepByStep(int iaxis, int iencposition_abs_desired)
        {
            // 1. move to pre-destination (10 steps BEFORE destination (+ verify!!))
            // 2. move step-by-step towards destination.

            // prerequisites
            int ictrlposition_abs_predesired = 0;
            int iencposition_abs_predesired = iencposition_abs_desired - 2; // 10 steps of encoder before destination

            int iencposition_current;
            string sencposition_current;

            // n of tries to reach destination
            int ntries = 0;
            //axis
            string saxis = (iaxis + 1).ToString("00");
           // string saxis = (iaxis + 0).ToString("00");

            // pre-destination calc: 10 steps of encoder
            int ienc_predestination_correctionmove_distance = 1;
            int ictrl_predestination_correctionmove_distance = 0;
            ConvertStepsEncoderToController(iaxis, ienc_predestination_correctionmove_distance, true, out ictrl_predestination_correctionmove_distance);

        _OneMoreTry:

            //1. 

            // fast profile
            SetControllerProfile_NormalMove(iaxis);
            WaitForCompletion(saxis); // wait for current operation is done (to make future read from encoder consistent)

            // calc and move to pre-destination
            ConvertStepsEncoderToController(iaxis, iencposition_abs_predesired, false, out ictrlposition_abs_predesired);
            MoveToAbsolutePosition(saxis, ictrlposition_abs_predesired.ToString());
            WaitForCompletion(saxis); // wait until controller is done
            Thread.Sleep(100); // delay to finish move

            // check and make adjustments if necessary
            while (true)
            {
                // stop sign?
                if (clsGlobals.bGUIStopSign) { return; }

                // get current position by enc
                 GetCurrentPositionByEncoder(saxis, out sencposition_current);
                //GetCurrentPositionByEncoder("01", out sencposition_current);
                iencposition_current = Convert.ToInt32(sencposition_current);

                // reached desired pre-destination?
                if ((iencposition_abs_desired - iencposition_current) <= (10 * 2))
                {
                    break;
                }
                // no.. adjustments..
                else
                {
                    if (iencposition_abs_desired > iencposition_current)
                    {
                        MoveRelativePositive(saxis, ictrl_predestination_correctionmove_distance.ToString());
                    }
                    else
                    {
                        MoveRelativeNegative(saxis, ictrl_predestination_correctionmove_distance.ToString());
                    }
                    WaitForCompletion(saxis);
                    Thread.Sleep(100);
                }
            }

            // 2.

            // slow profile
            SetControllerProfile_SlowMove(iaxis);
            WaitForCompletion(saxis); // wait for current operation is done (to make future read from encoder consistent)

            // fine move
            int nsteps = 0;
            while (true)
            {
                // stop sign?
                if (clsGlobals.bGUIStopSign) { return; }

                // get current position by enc
                GetCurrentPositionByEncoder(saxis, out sencposition_current);
                iencposition_current = Convert.ToInt32(sencposition_current);

                // reached desired destination?
                if (iencposition_abs_desired == iencposition_current)
                {
                    break;
                }
                else
                {
                    MoveRelativePositive(saxis, 1.ToString()); // move by 1 step towards destination
                    WaitForCompletion(saxis);
                    Thread.Sleep(100);
                    nsteps++;
                }

                // not found, hard-break
                if (nsteps > 25)
                {
                    break;
                }
            }

            // error occurred?
            if (nsteps > 25)
            {
                ntries++;
                if (ntries <= 3)
                {
                    goto _OneMoreTry;
                }
                else
                {
                    string msg = "Error: Encoder control move: could not reach target position.";
                    logger.Error("ABot2::EncoderControlMoveToAbsolutePosition throwing Exception {0}",
                        msg);
                    throw new Exception(msg);
                }
            }
        }

        /// <summary>
        /// Move to encoder position. For final position approaching use "continuous mode" algorithm.
        /// </summary>
        /// <param name="axis">Axis, 0-based (0..3)</param>
        /// <param name="iencposition_abs_desired">Destination - encoder position</param>
        public void EncoderControlMoveToAbsolutePosition_Continuous(int iaxis, int iencposition_abs_desired)
        {
            //int icuur_pos_ctrl;
            string sencposition_current = "";
            int iencposition_current = 0;
            int ictrlposition_predestination = 0;

            //-- get distance to target where we stop motion with fast profile and switch to slow one..
            int icenc_predestination_distance = 25; // 25 encoder steps before destination
            int icenc_predestination;

            // move distance
            int icontmovedistance_enc = icenc_predestination_distance * 5 * 2; // continuous move mode - test distance distance
            int icontmovedistance_ctrl = 0;

            // continuous mode - precision 
            int iencmaxpossibleprecisionstep_continious = 0;
            // abs precision
            int iencmaxpossibleprecision = 1;
            // to monitor direction of slow move
            int idistancetotarget = 0;
            int idistancetotarget_old = 0;

            // flag found
            bool bpositionfound = false;

            // delay between move, in ms
            int idelay = 100;
            int ngtries = 3;
            int ngtries_c = 0;

            // axis
            string saxis = (iaxis + 1).ToString("00");

            // move distance (work window)
            ConvertStepsEncoderToController(iaxis, icontmovedistance_enc, true, out icontmovedistance_ctrl);

        _OneMoreTry:

            if (clsGlobals.bGUIStopSign) { return; }

            //-- coarse move

            // get current position by enc
            GetCurrentPositionByEncoder(saxis, out sencposition_current);
            iencposition_current = Convert.ToInt32(sencposition_current);

            // calc pre-destination position (we move only in positive direction (more reliable readings from encoder))
            icenc_predestination = iencposition_abs_desired - icenc_predestination_distance;

            // fast profile
            SetControllerProfile_NormalMove(iaxis);
            WaitForCompletion(saxis); // wait for current operation is done (to make future read from encoder consistent)

            // move (abs) to that pre-destination position.
            ConvertStepsEncoderToController(iaxis, icenc_predestination, false, out ictrlposition_predestination);
            MoveToAbsolutePosition(saxis, ictrlposition_predestination.ToString());
            WaitForCompletion(saxis); // wait until controller is done
            Thread.Sleep(idelay); // delay to finish move

            // distance to target (reset)
            idistancetotarget_old = icenc_predestination_distance * 5;

            //-- fine move

            // 3 attempts
            for (int i = 0; i < 3; i++)
            {
                // stop sign?
                if (clsGlobals.bGUIStopSign) { return; }

                // get current position by enc
                GetCurrentPositionByEncoder(saxis, out sencposition_current);
                iencposition_current = Convert.ToInt32(sencposition_current);

                // slow profile
                SetControllerProfile_SlowMove(iaxis);
                WaitForCompletion(saxis); // wait for current operation is done (to make future read from encoder consistent)

                // start moving in positiove direction (always positive)
                MoveRelativePositive(saxis, icontmovedistance_ctrl.ToString());

                // monitor position while moving
                while (true)
                {
                    // stop sign?
                    if (clsGlobals.bGUIStopSign) { return; }

                    // get current pos
                    GetCurrentPositionByEncoder(saxis, out sencposition_current);
                    iencposition_current = Convert.ToInt32(sencposition_current);

                    // distance to target
                    idistancetotarget = Math.Abs(iencposition_current - iencposition_abs_desired);

                    // reached destination? 
                    if (idistancetotarget <= (iencmaxpossibleprecisionstep_continious + 1))
                    {
                        Terminate(saxis); // stop
                        WaitForCompletion(saxis); // wait for current operation is done (to make future read from encoder consistent)
                        Thread.Sleep(idelay); // delay to finish all moves

                        bpositionfound = true; // to full quit
                        break; // to the main cycle
                    }

                    // are we skipped our target? or end of move?
                    if ((idistancetotarget > idistancetotarget_old) | (!statusBusy))
                    {
                        Terminate(saxis); // stop
                        WaitForCompletion(saxis); // wait for current operation is done (to make future read from encoder consistent)
                        Thread.Sleep(idelay); // delay to finish move

                        SetControllerProfile_NormalMove(iaxis);
                        WaitForCompletion(saxis); // wait for current operation is done (to make future read from encoder consistent)

                        icenc_predestination_distance = icenc_predestination_distance * (i + 2); // move our target a bit more to the left..
                        icenc_predestination = iencposition_abs_desired - icenc_predestination_distance;
                        ConvertStepsEncoderToController(iaxis, icenc_predestination, false, out ictrlposition_predestination);

                        MoveToAbsolutePosition(saxis, ictrlposition_predestination.ToString()); // reset the position, to move over again
                        WaitForCompletion(saxis); // wait for current operation is done (to make future read from encoder consistent)
                        Thread.Sleep(idelay); // delay to finish move

                        idistancetotarget_old = icenc_predestination_distance * 5 * 2 * (i + 2);

                        break; // to the 3-try cycle
                    }
                    else
                    {
                        idistancetotarget_old = idistancetotarget;
                    }
                }

                // quit, pos found
                if (bpositionfound)
                {
                    break;
                }
            }

            // wait for current operation is done (to make future read from encoder consistent)
            SetControllerProfile_NormalMove(iaxis); // switch back to normal profile
            WaitForCompletion(saxis); // wait for current operation is done (to make future read from encoder consistent)

            GetCurrentPositionByEncoder(saxis, out sencposition_current);
            iencposition_current = Convert.ToInt32(sencposition_current);

            // one more try.?
            if (Math.Abs(iencposition_current - iencposition_abs_desired) > iencmaxpossibleprecision)
            {
                ngtries_c++;
                if (ngtries_c >= ngtries)
                {
                    string msg = "Error: Encoder control move: could not reach target position.";
                    logger.Error("ABot2::EncoderControlMoveToAbsolutePosition throwing Exception {0}",
                        msg);
                    throw new Exception(msg);
                }
                goto _OneMoreTry;
            }

            // so we exit from here with all moves done, at exact desired position 
        }

        public void SetControllerProfile_SlowMove(int iaxis)
        {
            string saxis = (iaxis + 1).ToString("00");

            string sSlowStartSpeed = "1";
            string sSlowMaxSpeed = "1";
            string sSlowAcceleration = "10000";
            string sSlowDeceleration = "10000";

            SetStartSpeed(saxis, sSlowStartSpeed);
            SetMaxSpeed(saxis, sSlowMaxSpeed);
            SetAcceleration(saxis, sSlowAcceleration);
            SetDeceleration(saxis, sSlowDeceleration);
        }
        /*
        public void SetControllerProfile_NormalMove(int iaxis)
        {
            string saxis = (iaxis + 1).ToString("00");
            SetStartSpeed(saxis, ambparams_current.axisparams_current[iaxis].strStartSpeed);
            SetMaxSpeed(saxis, ambparams_current.axisparams_current[iaxis].strMaxSpeed);
            SetAcceleration(saxis, ambparams_current.axisparams_current[iaxis].strAcceleration);
            SetDeceleration(saxis, ambparams_current.axisparams_current[iaxis].strDeceleration);
        }*/

        public void SetControllerProfile_NormalMove(int iaxis)
        {
            string saxis = (iaxis + 1).ToString("00");
            SetStartSpeed(saxis, axisparams_current[iaxis].strStartSpeed);
            SetMaxSpeed(saxis, axisparams_current[iaxis].strMaxSpeed);
            SetAcceleration(saxis, axisparams_current[iaxis].strAcceleration);
            SetDeceleration(saxis, axisparams_current[iaxis].strDeceleration);
        }

        //------------------------------------------------------------------------------------------
        // configuration

        public void LoadCfgFile(string cfg_fname)
        {
            XmlSerializer ser = new XmlSerializer(typeof(AMBParameters));
            using (FileStream reader = new FileStream(cfg_fname, FileMode.Open))
            {
                ambparams_current = ser.Deserialize(reader) as AMBParameters;
            }
        }

        public void SaveCfgFile(string cfg_fname)
        {
            XmlSerializer ser = new XmlSerializer(typeof(AMBParameters));
            using (TextWriter writer = new StreamWriter(cfg_fname))
            {
                ser.Serialize(writer, ambparams_current);
            }
        }

        public void LoadCfgDefaults()
        {
            ambparams_current = ambparams_defaults;
        }

        /// <summary>
        /// Load parameters to motor controller
        /// </summary>
        /// <param name="axes">Axis to operate with (01 .. 04), could be list of</param>
        /// <param name="round">Type of parameters to load, workaround of "Resolution", which could be loaded only after homing
        /// "0" - load all parameters;
        /// "1" - load all parameters, except "Resolution"
        /// "2" - load "Resolution" parameter only
        /// </param>
        public void LoadParametersIntoController(string axes = "01,02,03,04", string round = "0")
        {
            int iaxis;
            string[] spaxes = axes.Split(',');
            foreach (string axis in spaxes)
            {
                if (clsGlobals.bGUIStopSign) { return; }

                iaxis = Convert.ToInt32(axis) - 1;

                // handle parameters that require to home axis first..
                if ((round == "0") || (round == "2"))
                {
                    if (axisparams_current[iaxis].strEncoderPresent != "0")
                    {
                        SetResolution(axis, axisparams_current[iaxis].strResolution);
                        SetEncoderState(axis, axisparams_current[iaxis].strEncoderPresent);
                        SetEncoderDirectionPolarity(axis, axisparams_current[iaxis].strEncoderDirectionPolarity);

                        if (round == "2")
                        {
                            continue;
                        }
                    }
                }

                // parameters that do not require homing
                //SetResolution(axis, axisparams_current[iaxis].strResolution);

                SetDirectionPolarity(axis, axisparams_current[iaxis].strDirectionPolarity); // the most important actually
                SetProfileMode(axis, axisparams_current[iaxis].strProfileMode);
                SetStartSpeed(axis, axisparams_current[iaxis].strStartSpeed);
                SetMaxSpeed(axis, axisparams_current[iaxis].strMaxSpeed);
                SetAcceleration(axis, axisparams_current[iaxis].strAcceleration);
                SetDeceleration(axis, axisparams_current[iaxis].strDeceleration);
                SetJerk(axis, axisparams_current[iaxis].strJerk);
                SetMoveCurrent(axis, axisparams_current[iaxis].strMoveCurrent);
                SetHoldCurrent(axis, axisparams_current[iaxis].strHoldCurrent);
                SetHomeTimeout(axis, axisparams_current[iaxis].strHomeTimeout);
            }
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
                // TODO: set large fields to null.

                objSerialPort.Close();

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~clsAMB() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
