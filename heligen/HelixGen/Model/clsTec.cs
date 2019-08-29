using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Xml.Serialization;
using HelixGen;
using NLog;

namespace ABot2
{
    public class clsTec: IDisposable
    {
        private SerialPort objSerialPort = new SerialPort();
        private static readonly Object _m_lockobj = new Object();

        //private string tmpsSPData = "";

#if false
        private int _m_iStatusByte;
        public bool ErrorBit { get; internal set; } = false;
        public bool ErrorCommBit { get; internal set; } = false;
        public bool ResetBit { get; internal set; } = false;
        public bool BusyBit { get; internal set; } = false;
        public bool TranBusy0 { get; internal set; } = false;
        public bool Control0 { get; internal set; } = false;
        public bool TranBusy1 { get; internal set; } = false;
        public bool Control1 { get; internal set; } = false;
        public bool CheckSumError { get; internal set; } = false;
#endif

        public int[] _m_iStatusByte;
        public bool[] ErrorBit;
        public bool[] ErrorCommBit;
        public bool[] ResetBit;
        public bool[] BusyBit;
        public bool[] TranBusy0 ;
        public bool[] Control0;
        public bool[] TranBusy1;
        public bool[] Control1;
        public bool[] CheckSumError;



        public static Logger logger = LogManager.GetCurrentClassLogger();

        //-------------------------------------------------------------------


        public struct TecParameters
        {
            public float OvershootOffset;
            public float ErrorBand;
            public int ErrorCountLimit;
            public float PowerLimit;
            public int PowerLimitCount;
            public float PowerLimitLow;
            public float PowerLimitHigh;
            public float HeatPid_P;
            public float HeatPid_I;
            public float HeatPid_D;
            public float CoolPid_P;
            public float CoolPid_I;
            public float CoolPid_D;
            public float HeatRabbit_A;
            public float HeatRabbit_B;
            public float HeatRabbit_C;
            public float HeatRabbit_D;
            public float CoolRabbit_A;
            public float CoolRabbit_B;
            public float CoolRabbit_C;
            public float CoolRabbit_D;
            public float SampleTime;
            public float sheq_A;
            public float sheq_B;
            public float sheq_C;
            public float Propband;
            public float SetpointOffset;

        };

        /// <summary>
        /// The parameters for an entire board.
        /// </summary>
        public class HeaterAMBParameters
        {

        }

        public clsTec()
        {
            _m_iStatusByte = new int[8];
            ErrorBit = new bool[8];
            ErrorCommBit = new bool[8];
            ResetBit = new bool[8];
            BusyBit = new bool[8];
            TranBusy0 = new bool[8];
            Control0 = new bool[8];
            TranBusy1 = new bool[8];
            Control1 = new bool[8];
            CheckSumError = new bool[8];
        }

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
            //objSerialPort.StopBits = StopBits.One;
            // objSerialPort.DataBits = 8;
            
            objSerialPort.StopBits = StopBits.One;

            objSerialPort.DataBits = 8;
            objSerialPort.Handshake = Handshake.None;
            objSerialPort.Parity = Parity.None;

            objSerialPort.Open();

            // Clear out any stuff in the buffers.

            objSerialPort.DiscardInBuffer();
            objSerialPort.DiscardOutBuffer();
        }

        /// <summary>
        /// Write data to serial port
        /// </summary>
        /// <param name="scmd">String to send (command)</param>
        /// <param name="do_checksum">Flag determines if checksum needs to be done</param>
        /// <param name="sresp">Response from comm port</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        private string WriteCmd(string scmd, int nController, bool checkSumActive, out string sresp)
        {
            sresp = "";

            lock (_m_lockobj)
            {
                bool gotIt = false;
                string sres = "";

                // Form the command.

                scmd = (">" + scmd);

                //logger.Debug("TEC: Sending the command \"{0}\"", scmd);

                if (checkSumActive)
                {
                    try
                    {
                        int checkSum = 0;
                        for (int i = 0; i < scmd.Length; i++)
                        {
                            checkSum += (int)scmd[i];
                        }
                        checkSum = checkSum % 255;
                        scmd = scmd + String.Format("{0:X2}", checkSum);
                    }
                    catch (Exception ex)
                    {
                        return "Error: TEC board: control sum error: " + ex.Message;
                    }
                }

                scmd += "\r\n";

                while (!gotIt)
                {
                    try
                    {
                        objSerialPort.DiscardInBuffer();
                        objSerialPort.DiscardOutBuffer();

                        objSerialPort.Write(scmd);
                    }
                    catch (Exception ex)
                    {
                        return "Error: TEC board: port write error: " + ex.Message;
                    }

                    sres = ReadCmd(out sresp, nController, checkSumActive);

                    if (ErrorBit[nController])
                    {
                        // Clear the error bit.
                        //logger.Debug("Clearing the error.");
                        GetErrorCode(nController);
                    }
                    else
                        gotIt = true;
                }

                return sres;
            }
        }


        /// <summary>
        /// Read data from serial port
        /// </summary>
        /// <param name="sres"></param>
        /// <param name="do_checksum"></param>
        /// <returns>Error message or empty string if there is no errors</returns>
        private string ReadCmd(out string sres, int nController, bool checkSumActive = true)
        {
            sres = "";
            string sresp = "";

            ErrorBit[nController] = false;
            ErrorCommBit[nController] = false;
            ResetBit[nController] = false;
            BusyBit[nController] = false;
            TranBusy0[nController] = false;
            Control0[nController] = false;
            TranBusy1[nController] = false;
            Control1[nController] = false;
            CheckSumError[nController] = false;

            // moved to "Delay()"
            //if (!clsGlobals.bOverrideStopReason)
            //{
            //    if (clsGlobals.bGUIStopSign)
            //    {
            //        throw new Exception("GUI: Stop request.");
            //    }

            //    if (clsGlobals.bLidIsOpen)
            //    {
            //        throw new Exception("System: Lid is open.");
            //    }
            //}

            uint uiMaxRetries = 3;
            uint uiRetries = 0;
            string strEntireStr = "";
            while (uiRetries < uiMaxRetries)
            {
                try
                {
                    sresp = objSerialPort.ReadLine();
                    strEntireStr += sresp;
                    //logger.Debug("Retry: {1} Response string is \"{0}\"", strEntireStr, uiRetries.ToString());
                    break;
                }
                catch (Exception ex)
                {
                    logger.Debug("ReadCmd, got an exception: {0}", ex.Message);
                    strEntireStr += sresp;
                    uiRetries++;
                    if (uiRetries >= uiMaxRetries)
                    {
                        return "Error: TEC board: port read timeout error" + ex.Message;
                    }
                }
            }
            sresp = strEntireStr;

            try
            {
                //// start = resp.find("<")
                int start;
                start = sresp.IndexOf("<");
                if ((start == -1) || (sresp.Length < 4))
                {
                    logger.Debug("Error: TEC board: wrong response from port, read error");
                    return "Error: TEC board: wrong response from port, read error";
                }

                //// resp = resp[start:]
                sresp = sresp.Substring(start);

                ////# Read and convert status byte
                ////statusChars = resp[1:3]
                ////statusByte = int(statusChars,16)
                string statusChars;
                statusChars = sresp.Substring(1, 2);
                UInt16 statusByte;
                _m_iStatusByte[nController] = statusByte = Convert.ToUInt16(statusChars, 16);

#if false
                logger.Debug("statusChars are; \"{0}\" status byte is; 0x{1}", statusChars,
                    statusByte.ToString("X"));
#endif

                ErrorBit[nController] = ((statusByte & 0x01) > 0);
                ErrorCommBit[nController] = ((statusByte & 0x02) > 0);
                ResetBit[nController] = ((statusByte & 0x04) > 0);
                // N.B.; This is always on.
                BusyBit[nController] = ((statusByte & 0x80) > 0);
                TranBusy0[nController] = ((statusByte & 0x08) > 0);
                Control0[nController] = ((statusByte & 0x10) > 0);
                TranBusy1[nController] = ((statusByte & 0x20) > 0);
                Control1[nController] = ((statusByte & 0x40) > 0);

#if false
                logger.Debug("Flags are now; errorBit; {0} errorComBit: {1} ResetBit: {2} BusyBit: {3}",
                     ErrorBit[nController].ToString(),
                     ErrorCommBit[nController].ToString(),
                     ResetBit[nController].ToString(),
                     BusyBit[nController].ToString());
#endif




#if false
                logger.Debug("Flags are now; transition0; {0} control0: {1} Transition1: {2} Control1: {3}",
                    TranBusy0[nController].ToString(),
                    Control0[nController].ToString(),
                    TranBusy1[nController].ToString(),
                    Control1[nController].ToString());
#endif

                //# Get any data
                //for i in range(3,len(resp)):
                //    if ((resp[i] != '\n') and (resp[i] != '\r')):
                //        data += resp[i]
                string data = "";
                for (int i = 3; i < sresp.Length; i++)
                {
                    if ((sresp[i].ToString() != "\n") & (sresp[i].ToString() != "\r"))
                    {
                        data += sresp[i];
                    }
                }

                if (checkSumActive)
                {
                    //# Check checksum if enabled
                    //if enCheckSum == True:
                    //    checkSum = int(data[-2:],16)        # Get message checksum
                    //    data = data[:-2]                    # Remove checksum from data
                    //    respStr = "<" + statusChars + data  # Response string
                    int checkSum; 
                    checkSum = Convert.ToInt32(data.Substring(data.Length - 2), 16);
                    data = data.Substring(0, data.Length - 2);
                    string respStr;
                    respStr = "<" + statusChars + data;

                    //# Calculate and test checksum
                    //calc_checkSum = 0
                    //for w in respStr:
                    //    calc_checkSum += ord(w)
                    //calc_checkSum = calc_checkSum % 255
                    //if calc_checkSum != checkSum:
                    //    checkSumError = True
                    int calc_checkSum = 0;
                    for (int i = 0; i < respStr.Length; i++)
                    {
                        calc_checkSum += (int)respStr[i];
                    }
                    calc_checkSum = calc_checkSum % 255;
                    if (calc_checkSum != checkSum)
                    {
                        CheckSumError[nController] = true;
                    }
                }

                // response data
                sres = data;

            }
            catch (Exception ex)
            {
                return "Error: TEC board: data parse error: " + ex.ToString();
            }

            return "";
        }

        /// <summary>
        /// Returns a string with a formatted float value (single-precision) at maximum precision,
        /// but using a string of 0s after the decimal, instead of an explicit exponent.
        /// There may be up to six zeros between the period and the first nonzero digit,
        /// depending on the value.
        /// For very small values, limit the precision so as not to overrun the 18-character
        /// limit imposed by the TEC controller's message structure.
        /// </summary>
        internal string FloatFormat(float value)
        {
            const int SinglePrecision = 7;
            const int MaxFractionLength = 15;   // 18 places total, less "-0."
            string floatSpec = "g";

            float absval = Math.Abs(value);
            if (absval > 0 && absval < 0.0001f)
            {
                int leading = -(int)Math.Truncate(Math.Log10(absval));
                if (leading >= MaxFractionLength) // Zeros to fifteen or more places?
                {
                    leading = 1 - SinglePrecision;  // Limit output to a single place of precision
                }
                else if (leading > MaxFractionLength - SinglePrecision)
                {
                    leading = MaxFractionLength - SinglePrecision;
                }

                floatSpec = string.Format("f{0}", leading + SinglePrecision);
            }

            string strOut = string.Format("{0:" + floatSpec + "}", value).Trim();

            return strOut;
        }

        /// <summary>
        /// Read stepper motor missed steps
        /// Note: move home operation required before this function call
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">A value representing the stepper motor missed steps</param>
        /// <returns>The number of missed steps.</returns>
        public int GetErrorCode(int nController)
        {
            string value;
            WriteCmd(string.Format("{0}RE0", (nController + 1).ToString("00")), nController, true, out value);
            WriteCmd(string.Format("{0}RE0=0", (nController + 1).ToString("00")), nController, true, out value);

            int nResult = 0;

            if (!string.IsNullOrEmpty(value))
                nResult = int.Parse(value);

            return nResult;
        }

        /// Read stepper motor missed steps
        /// Note: move home operation required before this function call
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">A value representing the stepper motor missed steps</param>
        /// <returns>The number of missed steps.</returns>
        public string GetFirmwareVersion(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}VR0", (nController + 1).ToString("00")), nController, true, out value);
            return value;
        }

        public float TECGetPID_P(int nController, int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("{0}KP{1}", (nController + 1).ToString("00"), nChannel.ToString("0")),
                nController,
                true,
                out value);
            return float.Parse(value);
        }

        public void TECSetPID_P(int nController, int nChannel, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}KP{1}={2}", (nController + 1).ToString("00"), 
                nChannel.ToString("0"), 
                FloatFormat(valueIn)), nController, true, out value);
        }

        public float TECGetPID_I(int nController, int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("{0}KI{1}", (nController + 1).ToString("00"), 
                nChannel.ToString("0")), nController, true, out value);
            return float.Parse(value);
        }

        public void TECSetPID_I(int nController, int nChannel, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}KI{1}={2}", (nController + 1).ToString("00"), 
                nChannel.ToString("0"), FloatFormat(valueIn)), nController, true, out value);
        }

        public float TECGetPID_D(int nController, int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("{0}KD{1}", (nController + 1).ToString("00"), nChannel.ToString("0")),
                nController, true, out value);
            return float.Parse(value);
        }

        public void TECSetPID_D(int nController, int nChannel, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}KD{1}={2}", (nController + 1).ToString("00"), nChannel.ToString("0"),
                FloatFormat(valueIn)), nController, true, out value);
        }

        public float GetSampleTime(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}TP0", (nController + 1).ToString("00")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetSampleTime(int nController, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}TP0={1}", (nController + 1).ToString("00"), FloatFormat(valueIn)),
                nController, true, out value);
        }

        public float GetSetPoint(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}SP0", (nController + 1).ToString("00")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetSetPoint(int nController, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}SP0={1}", (nController + 1).ToString("00"), FloatFormat(valueIn)), nController, true, out value);
        }

        public int GetTempControlMode(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}MD0", (nController + 1).ToString("00")), nController, true, out value);
            return int.Parse(value);
        }

        public void SetTempControlMode(int nController, int valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}MD0={1}", (nController + 1).ToString("00"), valueIn.ToString()), nController, true, out value);
            //WriteCmd(string.Format("{0}MD1={1}", (nController + 1).ToString("00"), valueIn.ToString()), nController, true, out value);
        }

        public int GetTECStatus(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}ST0", (nController + 1).ToString("00")), nController, true, out value);
            return int.Parse(value);
        }

        public float GetTemperature(int nController, int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("{0}TE{1}", (nController + 1).ToString("00"), nChannel.ToString("0")), nController, true, out value);

            float fResult = 0;

            if (!string.IsNullOrEmpty(value))
                fResult = float.Parse(value);

            return fResult;
        }

        public float GetSetPointOffset(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}OF0", (nController + 1).ToString("00")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetSetPointOffset(int nController, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}OF0={1}", (nController + 1).ToString("00"), FloatFormat(valueIn)), nController, true, out value);
        }

        public float GetDeadBand(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}DB0", (nController + 1).ToString("00")), nController, true, out value);
            return float.Parse(value);
        }

        public float SetDeadBand(int nController, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}DB0={1}", (nController + 1).ToString("00"), FloatFormat(valueIn)), nController, true, out value);
            return float.Parse(value);
        }

        public void SetPBand(int nController, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}PB0={1}", (nController + 1).ToString("00"), FloatFormat(valueIn)), nController, true, out value);
        }

        public float GetPBand(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}PB0", (nController + 1).ToString("00")), nController, true, out value);
            return float.Parse(value);  
        }

        public float GetPower(int nController) 
        {

            string value = "";
            WriteCmd(string.Format("{0}PR0", (nController + 1).ToString("00")), nController, true, out value);
             return float.Parse(value);
        } 

        public float GetCurrent(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}RA0", (nController + 1).ToString("00")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetThermACoeff(int nController, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}AC0={1}", (nController + 1).ToString("00"), FloatFormat(valueIn)), nController, true, out value);
        }

        public float GetThermACoeff(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}AC0", (nController + 1).ToString("00")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetThermBCoeff(int nController, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}BC0={1}", (nController + 1).ToString("00"), FloatFormat(valueIn)), nController, true, out value);
        }

        public float GetThermBCoeff(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}BC0", (nController + 1).ToString("00")), nController, true, out value);
            return float.Parse(value);
        }
         
        public void SetThermCCoeff(int nController, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}CC0={1}", (nController + 1).ToString("00"), FloatFormat(valueIn)), nController, true, out value);
        }

        public float GetThermCCoeff(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}CC0", (nController + 1).ToString("00")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetThermDCoeff(int nController, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}DC0={1}", (nController + 1).ToString("00"), FloatFormat(valueIn)), nController, true, out value);
        }

        public float GetThermDCoeff(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}DC0", (nController + 1).ToString("00")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetLowClamp(int nController, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}LC0={1}", (nController + 1).ToString("00"), FloatFormat(valueIn)), nController, true, out value);
        }

        public float GetLowClamp(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}LC0", (nController + 1).ToString("00")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetHighClamp(int nController, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}HC0={1}", (nController + 1).ToString("00"), FloatFormat(valueIn)), nController, true, out value);
        }

        public int GetHighClamp(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}HC0", (nController + 1).ToString("00")), nController, true, out value);
            return int.Parse(value);
        }

        public float GetHeatRate(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}HR0", (nController + 1).ToString("00")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetHeatRate(int nController, float heatRate)
        {
            string value = "";
            WriteCmd(string.Format("{0}HR0={1}", (nController + 1).ToString("00"), FloatFormat(heatRate)), nController, true, out value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nController"></param>
        /// <returns>1 for S-Curve (default) or 0 for Linear.</returns> 
        public int GetCurveProfile(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}LR0", (nController + 1).ToString("00")), nController, true, out value);
            return int.Parse(value);
        }

        public void SetCurveProfile(int nController, int profile)
        {
            string value = "";
            WriteCmd(string.Format("{0}LR0={1}", (nController + 1).ToString("00"), profile.ToString()), nController, true, out value);
        }

        public void SetRSHeaterSetPoint(int nChannel, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("07RS{0}={1}",nChannel.ToString("0"), FloatFormat(valueIn)), 6, true, out value);
        }

        public float GetRSHeaterSetPoint(int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("07RS{0}", nChannel.ToString("0")), 6, true, out value);
            return float.Parse(value);
        } 

        public void SetRSHeaterP(int nChannel, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("07RP{0}={1}", nChannel.ToString("0"), FloatFormat(valueIn)), 6, true, out value);
        }

        public float GetRSHeaterP(int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("07RP{1}", nChannel.ToString("0")), 6, true, out value);
            return float.Parse(value);
        }

        public void SetRSHeaterI(int nChannel, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("07RI{0}={1}", nChannel.ToString("0"), FloatFormat(valueIn)), 6, true, out value);
        }

        public float GetRSHeaterI(int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("07RI{0}", nChannel.ToString("0")), 6, true, out value);
            return float.Parse(value);
        }

        public void SetRSHeaterD(int nChannel, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("07RD{0}={1}", nChannel.ToString("0"), FloatFormat(valueIn)), 6, true, out value);
        }

        public float GetRSHeaterD(int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("07RD{0}", nChannel.ToString("0")), 6, true, out value);
            return float.Parse(value);
        }

        public void SetRSHeaterPropBand(int nChannel, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("07RO{0}={1}", nChannel.ToString("0"), FloatFormat(valueIn)), 6, true, out value);
        }

        public float GetRSHeaterPropBand(int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("07RO{0}", nChannel.ToString("0")), 6, true, out value);
            return float.Parse(value);
        }

        public int GetRSHeaterStatus(int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("07HS{0}", nChannel.ToString("0")), 6, true, out value);
            return int.Parse(value);
        }


        public void SetRSHeaterSetPointOffset(int nChannel, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("07HO{0}={1}", nChannel.ToString("0"), FloatFormat(valueIn)), 6, true, out value);
        }

        public float GetRSHeaterSetPointOffset(int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("07HO{0}", nChannel.ToString("0")), 6, true, out value);
            return float.Parse(value);
        }

        public void SetRSHeaterPreHeatSetPoint(int nChannel, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("07PT{0}={1}", nChannel.ToString("0"), FloatFormat(valueIn)), 6, true, out value);
        }

        public float GetRSHeaterPreHeatSetPoint(int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("07PT{0}", nChannel.ToString("0")), 6, true, out value);
            return float.Parse(value);
        }

        public void SetOvershootDuration(int nController, int nChannel, float valueIn)
        {
            string value = "";

            logger.Debug("TEC; setting the overshootDuration to {0}", valueIn.ToString());
            WriteCmd(string.Format("{0}OD{1}={2}", (nController + 1).ToString("00"), nChannel.ToString("0"), FloatFormat(valueIn)),
                nController, true, out value);
        }

        public float GetOvershootDuration(int nController, int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("{0}OD{1}", (nController + 1).ToString("00"), nChannel.ToString("0")),
                nController, true, out value);
            return float.Parse(value);
        }

        public void SetOvershootOffset(int nController, int nChannel, float valueIn)
        {
            string value = "";
            logger.Debug("TEC; setting the overshootoffset to {0}", FloatFormat(valueIn));

            WriteCmd(string.Format("{0}OO{1}={2}", (nController + 1).ToString("00"), nChannel.ToString("0"), FloatFormat(valueIn)),
                nController, true, out value);
        }

        public float GetOvershootOffset(int nController, int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("{0}OO{1}", (nController + 1).ToString("00"), nChannel.ToString("0")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetRabbitGain(int nController, int nChannel, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}KR{1}={2}", (nController + 1).ToString("00"), nChannel.ToString("0"), FloatFormat(valueIn)), nController, true, out value);
        }

        public float GetRabbitGain(int nController, int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("{0}KR{1}", (nController + 1).ToString("00"), nChannel.ToString("0")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetRabbitGain2(int nController, int nChannel, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}KQ{1}={2}", (nController + 1).ToString("00"), nChannel.ToString("0"), FloatFormat(valueIn)), nController, true, out value);
        }

        public float GetRabbitGain2(int nController, int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("{0}KQ{1}", (nController + 1).ToString("00"), nChannel.ToString("0")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetRabbitGainDeriv(int nController, int nChannel, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}KS{1}={2}", (nController + 1).ToString("00"), nChannel.ToString("0"), FloatFormat(valueIn)), nController, true, out value);
        }

        public float GetRabbitGainDeriv(int nController, int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("{0}KS{1}", (nController + 1).ToString("00"), nChannel.ToString("0")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetRabbitGainOffset(int nController, int nChannel, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}KO{1}={2}", (nController + 1).ToString("00"), nChannel.ToString("0"), FloatFormat(valueIn)), nController, true, out value);
        }

        public float GetRabbitGainOffset(int nController, int nChannel)
        {
            string value = "";
            WriteCmd(string.Format("{0}KO{1}", (nController + 1).ToString("00"), nChannel.ToString("0")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetErrorTermBand(int nController, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}EB0={1}", (nController + 1).ToString("00"), FloatFormat(valueIn)), nController, true, out value);
        }

        public float GetErrorTermBand(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}EB0", (nController + 1).ToString("00")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetErrorTermCounts(int nController, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}EC0={1}", (nController + 1).ToString("00"), FloatFormat(valueIn)), nController, true, out value);
        }

        public float GetErrorTermCount(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}EC0", (nController + 1).ToString("00")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetPowerLimitCounts(int nController, int valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}PC0={1}", (nController + 1).ToString("00"), FloatFormat(valueIn)), nController, true, out value);
        }

        public int SetPowerLimitCounts(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}PC0", (nController + 1).ToString("00")), nController, true, out value);
            return int.Parse(value);
        }

        public void SetSteadyStatePowerLimit(int nController, float valueIn)
        {
            string value = "";
            WriteCmd(string.Format("{0}PE0={1}", (nController + 1).ToString("00"), FloatFormat(valueIn)), nController, true, out value);
        }

        public float GetSteadyStatePowerLimit(int nController)
        {
            string value = "";
            WriteCmd(string.Format("{0}PE0", (nController + 1).ToString("00")), nController, true, out value);
            return float.Parse(value);
        }

        public void SetFanDutyCycle(int dutyCycle)
        {
            string value = "";
            WriteCmd(string.Format("08FD0={0}", dutyCycle.ToString()), 7, true, out value);
        }
        
        public void EnableSystemFan(bool bOn)
        {
            string value = "";
            WriteCmd(string.Format("08SF0={0}", (bOn ? "1" : "0")), 7, false, out value);
        }

        public void EnableHeaterFan(bool bOn)
        {
            string value = "";
            WriteCmd(string.Format("08HF0={0}", (bOn ? "1" : "0")), 7, false, out value);
        }

        public void EnableTECFan(bool bOn)
        {
            string value = "";
            WriteCmd(string.Format("08TF0={0}", (bOn ? "1" : "0")), 7, false, out value);
        }
        /*
        public void EnableSystemFan(bool bOn)
        {
            string value = "";
            WriteCmd(string.Format("08SF0={0}", (bOn ? "1" : "0")), 7, true, out value);
        }

        public void EnableHeaterFan(bool bOn)
        {
            string value = "";
            WriteCmd(string.Format("08HF0={0}", (bOn ? "1" : "0")), 7, true, out value);
        }

        public void EnableTECFan(bool bOn)
        {
            string value = "";
            WriteCmd(string.Format("08TF0={0}", (bOn ? "1" : "0")), 7, true, out value);
        }*/




        //------------------------------------------------------------------------------------------
        // configuration

        /// <summary>
        /// Load parameters to controller
        /// </summary>
        /// <param name="axes">Axis to operate with (01 .. 04), could be list of</param>
        /// <param name="round">Type of parameters to load, workaround of "Resolution", which could be loaded only after homing
        /// "0" - load all parameters;
        /// "1" - load all parameters, except "Resolution"
        /// "2" - load "Resolution" parameter only
        /// </param>
        public void LoadParametersIntoTec(int nController, TecParameters tecParamsIn)
        {
           /* SetOvershootOffset(nController, 0, tecParamsIn.OvershootOffset);
            SetOvershootOffset(nController, 1, tecParamsIn.OvershootOffset);
            SetErrorTermBand(nController, tecParamsIn.ErrorBand);
            SetErrorTermCounts(nController, tecParamsIn.ErrorCountLimit);
            SetSteadyStatePowerLimit(nController, tecParamsIn.PowerLimit);*/
            SetPowerLimitCounts(nController, tecParamsIn.PowerLimitCount);
            SetLowClamp(nController, tecParamsIn.PowerLimitLow);
            SetHighClamp(nController, tecParamsIn.PowerLimitHigh);

            TECSetPID_P(nController, 0, tecParamsIn.HeatPid_P);
            TECSetPID_P(nController, 1, tecParamsIn.CoolPid_P);
            TECSetPID_I(nController, 0, tecParamsIn.HeatPid_I);
            TECSetPID_I(nController, 1, tecParamsIn.CoolPid_I);
            TECSetPID_D(nController, 0, tecParamsIn.HeatPid_D);
            TECSetPID_D(nController, 1, tecParamsIn.CoolPid_D);

            SetRabbitGain(nController, 0, tecParamsIn.HeatRabbit_A);
            SetRabbitGain(nController, 1, tecParamsIn.CoolRabbit_A);
            SetRabbitGain2(nController, 0, tecParamsIn.HeatRabbit_B);
            SetRabbitGain2(nController, 1, tecParamsIn.CoolRabbit_B);
            SetRabbitGainDeriv(nController, 0, tecParamsIn.HeatRabbit_C);
            SetRabbitGainDeriv(nController, 1, tecParamsIn.CoolRabbit_C);
            SetRabbitGainOffset(nController, 0, tecParamsIn.HeatRabbit_D);
            SetRabbitGainOffset(nController, 1, tecParamsIn.CoolRabbit_D);

            SetSampleTime(nController, tecParamsIn.SampleTime);
            SetThermACoeff(nController, tecParamsIn.sheq_A);
            SetThermBCoeff(nController, tecParamsIn.sheq_B);
            SetThermCCoeff(nController, tecParamsIn.sheq_C);

            SetPBand(nController, tecParamsIn.Propband);
            SetSetPointOffset(nController, tecParamsIn.SetpointOffset);

            // Set the profile.  This is typically a default value of the board,
            // but since we might have changed it during PCR Cycling, it's necessary to 
            // set it here.

            SetCurveProfile(nController, 1);
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
        // ~clsTec() {
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
