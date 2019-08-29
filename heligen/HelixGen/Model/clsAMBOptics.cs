using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Xml.Serialization;
using HelixGen;
using NLog;
using HelixGen.Model;

namespace ABot2
{
    public class clsAMBOptics : IDisposable
    {
        /// <summary>
        /// The number of channels on this board.
        /// </summary>
        public const int MAX_CHANNELS = 6;

        private SerialPort objSerialPort = new SerialPort();
        private Object lockobj = new Object();

        private string tmpsSPData = "";

        private int statusByte;
        private bool statusBusy;
        private bool statusError;
        private int errorcode;

        protected string thePort;

        public static Logger logger = LogManager.GetCurrentClassLogger();

        char[] buffer = new char[2000];
        char[] DataBuf = new char[3000];

        public int sCurrPacketLen = 0;
        public bool sPacketIdGot = false;
        //bool sPacketEnd0dGot = false;
        public bool sPacketEnd0aGot = false;
        public bool gCommErr = false;



        /// <summary>
        /// Initialize and open serial port
        /// </summary>
        /// <param name="sPort">Port symbolic name to operate with</param>
        public void Initialize(string sPort)
        {
            thePort = sPort;

            try
            {
                if (objSerialPort.IsOpen)
                {
                    objSerialPort.Close();
                }
            }
            catch (Exception ex)
            {

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

            objSerialPort.DiscardInBuffer();
            objSerialPort.DiscardOutBuffer();
        }
        public double[] UnpackWithCheckSum(string buffer, int count)
        {
            
            string dataBack =buffer;
            dataBack = dataBack.Remove(0, 2);
            

            double[] valuesOut = new double[24];

            //logger.Debug("Optics PR readings back are; {0}", dataBack);
            string[] strValues = dataBack.Split(',');
            int lengthdata = 0;
            if (strValues.Length > 24)
                lengthdata = 24;
            else
                lengthdata = strValues.Length;
            //if (strValues.Length==24)
            {
                //for (int ndx = 0; ndx < strValues.Length; ndx++)
                for (int ndx = 0; ndx < lengthdata; ndx++)
                {
                    try { valuesOut[ndx] = double.Parse(strValues[ndx]); }
                    catch (Exception ex)
                    {
                        logger.Debug("stingProcess, got an exception: {0}", ex.Message);

                    }
                }
            }

            return valuesOut;
        }

        //
         public bool UnpackFrame()
        {
            char currChar;
            
            int data =0;
            int dataNumber = 0;

            bool sResult = false;

            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;
            //data = DataBuf;

            

            try
            {
                dataNumber = objSerialPort.Read(DataBuf, 0, 20);
            }
            catch (Exception ex)
            {
                logger.Debug("opticReadCmd, got an exception: {0}", ex.Message);
                
            }

            //dataNumber = Uart0ReadString(BUFFER_LEN, data);

            //	buffer = (UCHAR*)sCurrPacket.data;
            // buffer = (UCHAR*)&sCurrPacket;

            // repeat untill no data in receive buffer
            while (0 < dataNumber)
            {
                currChar = DataBuf[data];

                // packet ID has been received
                if (sPacketIdGot)
                {
                    // current byte is a valid packet data,
                    // a packet data must greater than or equal to 0x80
                    //if (0x80 <= currChar )
                    //{
                    // be careful: data stored begin from the second byte
                    if (!sPacketEnd0aGot)
                    {
                        if (currChar == 0x0a)
                        {
                            sPacketEnd0aGot = true;
                        }
                        else
                        {
                            buffer[sCurrPacketLen] = currChar;
                            sCurrPacketLen++;
                            if (sCurrPacketLen >= 1900)
                            {
                                gCommErr = true;
                                sPacketIdGot = false;
                                sPacketEnd0aGot = false;
                                sCurrPacketLen = 0;
                                //send err msg
                            }
                        }
                    }
                    else if (currChar == 0x0d)
                    {
                        gCommErr = false;
                        string stri = new string( buffer);

                       // public double[,] allReadings = new double[6, 4];
                       // public double[] readings = new double[24];
                        theModel.readings = UnpackWithCheckSum(stri, sCurrPacketLen);
                        sResult = true;
                        /*
                        switch ( result )
                        {
                        case 0: // Ok
                            ProcessPack(&sCurrPacket);
                            break;
                        case 1: // packet length error
    //						gSysStatus.mCommStatus |= COMM_CMD_LEN_ERR;
                            break;
                        case 2: // packet check sum error
    //						gSysStatus.mCommStatus |= COMM_CHECKSUM_ERR;
                            break;

                        default:
                            break;
                      }
                       */

                        sPacketIdGot = false;
                        sPacketEnd0aGot = false;
                        //sPacketEnd0aGot =0;
                    }
                    else
                    {
                        gCommErr = true;
                        sPacketIdGot = false;
                        sPacketEnd0aGot = false;
                        sCurrPacketLen = 0;
                    }
                    //}
                    // current byte is not a valid packet data, maybe is a packet ID,
                    // unget it for further analysis

                }
                // packet ID has not been received
                else if (currChar == '<')//">"
                {
                    sPacketIdGot = true;
                    sCurrPacketLen = 0;
                }
                // point to the next byte
                data++;

                // the rest byte number decreasing
                dataNumber--;
            } // while

            return sResult;
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
                        //string dataIn;

                        //logger.Debug("ABot2::clsAMB::ExecCMD sending command; \"{0}\"",
                        //    indata);

                        int nRetries = 3;
                        while(!(objSerialPort.IsOpen) & (nRetries > 0))
                        {
                            nRetries--;
                            logger.Debug("Found the optics port closed, reopening.");
                            Thread.Sleep(10);
                            Initialize(thePort);
                        }

                        //logger.Debug("Sending command \"{0}\"", indata);

                        objSerialPort.WriteLine(indata + "\r\n");

                        if (do_skip_read)
                        {
                            return;
                        }

                        nRetries = 3;
                        while (!(objSerialPort.IsOpen) & (nRetries > 0))
                        {
                            nRetries--;
                            logger.Debug("Found the optics port closed, reopening.");
                            Thread.Sleep(10);
                            Initialize(thePort);
                        }
                        outdata = objSerialPort.ReadLine();

                         if (!do_response_parsing)
                        {
                            return;
                        }

                        // We seem to be leaving a '\r' at the front of the buffer, 
                        // this is a hack to remove it.

                        if (outdata[0] == '\r')
                            outdata = outdata.Remove(0, 1);

                        //logger.Debug("Response was \"{0}\"", outdata);

                        if (outdata[0] != '<')
                        {
                            msg = "Error: (AMB) response parsing error. Data: " + outdata + "; command: " + indata + "; iteration: " + irepeat_count.ToString();
                            logger.Error("ABot2::clsOptics::ExecCMD throwing Exception {0}", msg);
                            //throw new Exception(msg);
                        }
                        string statusChars;
                        statusChars = outdata.Substring(1, 2);
                        //UInt16 statusByte;
                        //_m_iStatusByte[nController] = statusByte = Convert.ToUInt16(statusChars, 16);
                        statusByte = (int)Convert.ToUInt16(statusChars, 16);
                        statusBusy = ((statusByte & 0x80) > 0);
                        statusError = ((statusByte & 0x07) > 0);
                        errorcode = (statusByte & 0x07);
                        /*
                        statusByte = (int)outdata[2];
                        statusBusy = (!((statusByte & 32) > 0));
                        statusError = ((statusByte & 31) > 0);
                        errorcode = (statusByte & 31);*/

                        if (outdata.Length > 3)
                        {
                            outdata = outdata.Remove(0, 3).Replace("\r", "").Replace("\n", "");
                            return;
                        }
                        return;
                        //msg = "Error: (AMB) empty data returned; command: " + indata + "; iteration: " + irepeat_count.ToString();
                        //logger.Error("ABot2::clsOptics::ExecCMD throwing Exception {0}",
                        //    msg);
                        //throw new Exception(msg);
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
                            logger.Error("ABot2::clsOptics::ExecCMD throwing Exception {0}",
                                msg);
                            throw new Exception(msg, ex);
                        }
                    }
                } while (true);
            } // end of lock
        }

        /// <summary>
        /// Read the current error code.
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">A value representing the stepper motor missed steps</param>
        /// <returns>The number of missed steps.</returns>
        public int ReadErrorCodes()
        {
            string value = "";

            string cmd = ">RE01";
            ExecCmd(cmd, out value);

            // SWE; TODO; Interprete the error code here.

            return Int32.Parse(value);
        }

        /// Read the current error code.
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">A value representing the stepper motor missed steps</param>
        /// <returns>The number of missed steps.</returns>
        public int ClearErrorCodes()
        {
            string value = "";
            ExecCmd(">RE01=", out value);

            return Int32.Parse(value);
        }

        /// <summary>
        /// Move Stepper Motor to absolute position
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Position to move to in microsteps (0-134217727)</param>
        public string ReadFirmwareVersion()
        {
            string versionOut;
            ExecCmd(">VR01", out versionOut);
            return versionOut;
        }

        /// <summary>
        /// Sets the LED current for the specified channel.
        /// </summary>
        /// <param name="channel">Channel to operate with (1 ... 6)</param>
        /// <param name="current">Distance to move in microsteps (0-134217727)</param>
        public void SetLEDCurrent(int channel, double current)
        {
            // Range check the current value.

            if (current < 0 || current > 1.0)
            {
                // Range error.
            }
            else
            {
                string cmd = ">LC" + channel.ToString("00") + "=" + current.ToString();
                ExecCmd(cmd, out tmpsSPData);
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Move Stepper Motor relative in positive direction
        /// </summary>
        /// <param name="channel">Channel to operate with (1 ... 6)</param>
        public double[] GetLEDCurrent()
        {
            string strCurrent;
            ExecCmd(">LC00", out strCurrent);

            double[] valuesOut = new double[MAX_CHANNELS];

            string[] strValues = strCurrent.Split(',');

            // N.B. The first value is the voltage reference.

            int ndxV = 0;
            for (int ndx = 1; ndx <= MAX_CHANNELS; ndx++)
            {
                valuesOut[ndxV++] = double.Parse(strValues[ndx]);
            }

            return valuesOut;
        }

        /// <summary>
        /// </summary>

        public double[] GetPDReadings()
        {
            string dataBack;
            ExecCmd(">AL01", out dataBack);

            // If we get an error, clear it and try again.

            if (statusError)
            {
                ClearErrorCodes();

                ExecCmd(">AL01", out dataBack);
            }

            double[] valuesOut = new double[MAX_CHANNELS];

            //logger.Debug("Optics PR readings back are; {0}", dataBack);
            string[] strValues = dataBack.Split(',');

            for (int ndx = 0; ndx < MAX_CHANNELS; ndx++)
            {
                valuesOut[ndx] = double.Parse(strValues[ndx]);
            }

            return valuesOut;
        }

        /// <summary>
        /// Triggers the PDC readings.
        /// </summary>
        /// <param name="avgWindow">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Direction to set (0,1; default: 0)</param>
        public void TriggerPDReadings(bool mode)
        {
            string dataBack;
            if(mode)
                ExecCmd(">DR01=1", out dataBack);
            else
                ExecCmd(">DR01=0", out dataBack);
        }
        public void StopSample()
        {
            string dataBack;
            
                ExecCmd(">ST00", out dataBack);
            
        }

        

        /// <summary>
        /// Set Stepper Motor position without moving
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Position to set (0-134217727)</param>
        public double[] GetLEDDriverTemperatures()
        {
            string dataBack;
            ExecCmd(">GT00", out dataBack);

            double[] valuesOut = new double[MAX_CHANNELS];
            string[] strValues = dataBack.Split(',');

            for (int ndx = 0; ndx < MAX_CHANNELS; ndx++)
            {
                valuesOut[ndx] = double.Parse(strValues[ndx]) / 100;
            }

            return valuesOut;
        }

        /// <summary>
        /// Set Stepper Motor position without moving
        /// </summary>
        /// <param name="axis">Axis to operate with (01 .. 04)</param>
        /// <param name="value">Position to set (0-134217727)</param>
        public double[] GetLEDTemperatures()
        {
            string dataBack;
            ExecCmd(">AT00", out dataBack);

            double[] valuesOut = new double[MAX_CHANNELS];
            string[] strValues = dataBack.Split(',');

            for (int ndx = 0; ndx < MAX_CHANNELS; ndx++)
            {
                valuesOut[ndx] = double.Parse(strValues[ndx]) / 100;
            }

            return valuesOut;
        }

        //------------------------------------------------------------------------------------------
        // configuration

        /// <summary>
        /// Load parameters to motor controller
        /// </summary>
        /// <param name="axes">Axis to operate with (01 .. 04), could be list of</param>
        /// <param name="round">Type of parameters to load, workaround of "Resolution", which could be loaded only after homing
        /// "0" - load all parameters;
        /// "1" - load all parameters, except "Resolution"
        /// "2" - load "Resolution" parameter only
        /// </param>
        public void LoadParametersIntoController()
        {
 
          
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
        // ~clsAMBOptics() {
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
