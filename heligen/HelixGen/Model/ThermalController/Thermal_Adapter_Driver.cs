using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System_Defns;

namespace System_Instruments.Thermal_Controller
{
    public class CThermal_Adapter_Driver 
    {
        private SerialPort _m_spSerialPort = new SerialPort();
        private Object     _m_lockobj = new Object();
        private Object     _m_RHlockobj = new Object();
        private int        _m_iStatusByte;
        private bool       _m_errorBit = false;
        private bool       _m_errorCommBit = false;
        private bool       _m_resetBit = false;
        private bool       _m_busyBit = false;
        private bool       _m_tranBusy0 = false;
        private bool       _m_control0 = false;
        private bool       _m_tranBusy1 = false;
        private bool       _m_control1 = false;
        private bool       _m_checkSumError = false;
        //private string     _m_strModel;
        private float      _m_fCurrentSetpoint;
        private float      _m_fUpperControlThresholdinPercentofSetpoint; // Only needed for Oven Industries controller
        private float      _m_fLowerControlThresholdinPercentofSetpoint; // Only needed for Oven Industries controller

        public CThermal_Adapter_Driver()
        {
            _m_RampParamHistory = new Dictionary<string, CRampParameterHistory>(StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Initialize comm port 
        /// </summary>
        /// <param name="sPort">Serial port number</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string Initialize(string sPort, float fUpperControlThresholdinPercentofSetpoint = 1.0F, float fLowerControlThresholdinPercentofSetpoint = 1.0F)
        {
            try
            {
                _m_fUpperControlThresholdinPercentofSetpoint = fUpperControlThresholdinPercentofSetpoint;
                _m_fLowerControlThresholdinPercentofSetpoint = fLowerControlThresholdinPercentofSetpoint;

                if (_m_spSerialPort.IsOpen)
                {
                    _m_spSerialPort.Close();
                }
                _m_spSerialPort.PortName = sPort;

                _m_spSerialPort.BaudRate = 115200;
                _m_spSerialPort.ReadTimeout = 1000;
                _m_spSerialPort.WriteTimeout = 1000;
                _m_spSerialPort.StopBits = StopBits.One;
                _m_spSerialPort.DataBits = 8;
                _m_spSerialPort.Open();
                _m_spSerialPort.DiscardInBuffer();
                _m_spSerialPort.DiscardOutBuffer();
            }
            catch (Exception)
            {
                _m_spSerialPort.Close();
                throw new Exception("Error: TEC serial port init error.");
            }
            return "";
        }

        /// <summary>
        /// Deinitialize, close serial port
        /// </summary>
        /// <param name="sPort"></param>
        public void DeInitialize()
        {
            if (_m_spSerialPort != null)
            {
                if (_m_spSerialPort.IsOpen)
                {
                    _m_spSerialPort.Close();
                }
            }
        }


        /// <summary>
        /// Write data to serial port
        /// </summary>
        /// <param name="scmd">String to send (command)</param>
        /// <param name="do_checksum">Flag determines if checksum needs to be done</param>
        /// <param name="sresp">Response from comm port</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        private string WriteCmd(string scmd, bool do_checksum, out string sresp, out bool bErrorBit, out bool bErrorCommBit, out bool bResetBit)
        {
            //ZZ: implemented in "Delay()"
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

            sresp = "";

            bErrorBit = false;
            bErrorCommBit = false;
            bResetBit = false;

            lock (_m_lockobj)
            {
                if (do_checksum)
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

                try
                {
                    _m_spSerialPort.Write(scmd);
                }
                catch (Exception ex)
                {
                    return "Error: TEC board: port write error: " + ex.Message;
                }

                bool bTranBusyCh0, bTranBusyCh1, bControllingCh0, bControllingCh1;
                string sres = ReadCmd(out sresp, do_checksum, out bErrorBit, out bErrorCommBit, out bResetBit, out bTranBusyCh0, out bTranBusyCh1, out bControllingCh0, out bControllingCh1);

                return sres;
            }
        }

        /// <summary>
        /// Write data to serial port
        /// </summary>
        /// <param name="scmd">String to send (command)</param>
        /// <param name="do_checksum">Flag determines if checksum needs to be done</param>
        /// <param name="sresp">Response from comm port</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        private string WriteCmd(string scmd, bool do_checksum, out string sresp, out bool bErrorBit, out bool bErrorCommBit, out bool bResetBit, out bool bTranBusyCh0, out bool bTranBusyCh1, out bool bControllingCh0, out bool bControllingCh1)
        {
            //ZZ: implemented in "Delay()"
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

            sresp = "";

            bErrorBit = false;
            bErrorCommBit = false;
            bResetBit = false;
            bTranBusyCh0 = false;
            bTranBusyCh1 = false;
            bControllingCh0 = false;
            bControllingCh1 = false;

            lock (_m_lockobj)
            {
                if (do_checksum)
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

                try
                {
                    _m_spSerialPort.Write(scmd);
                }
                catch (Exception ex)
                {
                    return "Error: TEC board: port write error: " + ex.Message;
                }

                string sres = ReadCmd(out sresp, do_checksum, out bErrorBit, out bErrorCommBit, out bResetBit, out bTranBusyCh0, out bTranBusyCh1, out bControllingCh0, out bControllingCh1);

                return sres;
            }
        }

        /// <summary>
        /// Read data from serial port
        /// </summary>
        /// <param name="sres"></param>
        /// <param name="do_checksum"></param>
        /// <returns>Error message or empty string if there is no errors</returns>
        private string ReadCmd(out string sres, bool do_checksum, out bool bErrorBit, out bool bErrorCommBit, out bool bResetBit, out bool bTranBusyCh0, out bool bTranBusyCh1, out bool bControllingCh0, out bool bControllingCh1)
        {
            sres = "";
            string sresp = "";

            _m_errorBit = false;
            _m_errorCommBit = false;
            _m_resetBit = false;
            _m_busyBit = false;
            _m_tranBusy0 = false;
            _m_control0 = false;
            _m_tranBusy1 = false;
            _m_control1 = false;
            _m_checkSumError = false;

            bErrorBit = false;
            bErrorCommBit = false;
            bResetBit = false;
            bTranBusyCh0 = false;
            bTranBusyCh1 = false;
            bControllingCh0 = false;
            bControllingCh1 = false;

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
                    sresp = _m_spSerialPort.ReadLine();
                    strEntireStr += sresp;
                    break;
                }
                catch (Exception ex)
                {
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
                if ((start == -1) || (sresp.Length < 5))
                {
                    return "Error: TEC board: wrong response from port, read error";
                }

                //// resp = resp[start:]
                sresp = sresp.Substring(start);

                ////# Read and convert status byte
                ////statusChars = resp[1:3]
                ////statusByte = int(statusChars,16)
                string statusChars;
                statusChars = sresp.Substring(1, 2);
                int statusByte;
                _m_iStatusByte = statusByte = Convert.ToInt32(statusChars, 16);

                //# Check status byte for TEC error
                //if (statusByte & 1):
                //    errorBit = True
                bErrorBit = _m_errorBit = ((statusByte & 1) > 0);

                //# Check status byte for comm error
                //if (statusByte & 2):
                //    errorCommBit = True
                bErrorCommBit = _m_errorCommBit = ((statusByte & 2) > 0);

                //# Check status byte for reset bit
                //if (statusByte & 4):
                //    resetBit = True
                bResetBit = _m_resetBit = ((statusByte & 4) > 0);

                //# Get busy bit from status byte
                //if (statusByte & 128):
                //    busyBit = True
                _m_busyBit = ((statusByte & 128) > 0);

                //# Get other status bits from status byte
                //if (statusByte & 8):
                //    tranBusy0 = True
                //if (statusByte & 16):
                //    control0 = True
                //if (statusByte & 32):
                //    tranBusy1 = True
                //if (statusByte & 64):
                //    control1 = True
                bTranBusyCh0 = _m_tranBusy0 = ((statusByte & 8) > 0);
                bControllingCh0 = _m_control0 = ((statusByte & 16) > 0);
                bTranBusyCh1 = _m_tranBusy1 = ((statusByte & 32) > 0);
                bControllingCh1 = _m_control1 = ((statusByte & 64) > 0);

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

                if (do_checksum)
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
                        _m_checkSumError = true;
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

        private bool isCriticalErrorCode(int iErrorCode)
        {
            if ((iErrorCode != 12) && (iErrorCode != 15))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Start TEC control (power ON)
        /// </summary>
        /// <param name="tec_address">TEC channel to operate with ("00".."06")</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string StartTEC(string tec_address, out bool bCriticalError, out int iErrorCode)
        {
            string sres;
            string sresp;
            bool bErrorBit, bErrorCommBit, bResetBit;
            bCriticalError = false;
            iErrorCode = 0;

            //write_controlMode=1: turn TEC on
            sres = WriteCmd(">" + tec_address + "MD0=1", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit); 
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            return "";
        }

        /// <summary>
        /// IsTECStarted (Mode == 1)
        /// </summary>
        /// <param name="tec_address">TEC channel to operate with ("00".."06")</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string IsTECStarted(string tec_address, out bool bTECStarted, out bool bCriticalError, out int iErrorCode)
        {
            string sres;
            string sresp;
            bool bErrorBit, bErrorCommBit, bResetBit;

            bTECStarted = false;
            bCriticalError = false;
            iErrorCode = 0;

            //write_controlMode=1: turn TEC on
            sres = WriteCmd(">" + tec_address + "MD0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            bTECStarted = (sresp[0] == '1');
            return "";
        }

        /// <summary>
        /// Stop TEC control (power off)
        /// </summary>
        /// <param name="tec_address">TEC channel to operate with ("00".."06")</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string StopTEC(string tec_address, out bool bCriticalError, out int iErrorCode)
        {
            string sres;
            string sresp;
            bool bErrorBit, bErrorCommBit, bResetBit;

            bCriticalError = false;
            iErrorCode = 0;

            //write_controlMode=0: turn TEC off
            sres = WriteCmd(">" + tec_address + "MD0=0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            return "";
        }

        /// <summary>
        /// Start fan operation at 100%
        /// </summary>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string StartFan(string strFanId, string strdutyCycle, out bool bCriticalError, out int iErrorCode)
        {
            string sres;
            string sresp;
            bool bErrorBit, bErrorCommBit, bResetBit;

            bCriticalError = false;
            iErrorCode = 0;

            sres = WriteCmd(">08FD0=" + strdutyCycle, false, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit); 
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError("08", out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            if (strFanId.ToUpper() == "SYSTEM")
            {
                //write_systemFanEnable
                sres = WriteCmd(">08SF0=1", false, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError("08", out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
            }
            else if (strFanId.ToUpper() == "HEATER")
            {
                //write_heaterFanEnable
                sres = WriteCmd(">08HF0=1", false, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError("08", out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
            }
            else if (strFanId.ToUpper() == "TEC")
            {
                //write_TECFanEnable
                sres = WriteCmd(">08TF0=1", false, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError("08", out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
            }

            return "";
        }

        /// <summary>
        /// Stop fan operation
        /// </summary>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string StopFans(out bool bCriticalError, out int iErrorCode)
        {
            string sres;
            string sresp;
            bool bErrorBit, bErrorCommBit, bResetBit;

            bCriticalError = false;
            iErrorCode = 0;
           
            //write_fanDutyCycle: = 0%
            sres = WriteCmd(">08FD0=0", false, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit); 
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError("08", out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            //write_systemFanDisable
            sres = WriteCmd(">08SF0=0", false, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit); 
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError("08", out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            //write_heaterFanEnable
            sres = WriteCmd(">08HF0=0", false, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError("08", out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            //write_TECFanEnable
            sres = WriteCmd(">08TF0=0", false, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError("08", out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            return "";
        }

        /// <summary>
        /// Read and Clear TEC error
        /// </summary>
        /// <param name="tec_address">TEC channel to operate with ("00".."06")</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string ReadClearTECError(string tec_address, out string sresp)
        {
            string sres;
            bool bErrorBit, bErrorCommBit, bResetBit;

            //write_controlMode=1: turn TEC on
            sres = WriteCmd(">" + tec_address + "RE0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };

            return sresp;
        }
        
        /// <summary>
        /// Set temperature for specified TEC channel
        /// </summary>
        /// <param name="tec_address">TEC channel to operate with ("00".."06")</param>
        /// <param name="stemp">Temperature to set, C</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string SetTemperatureTEC(string tec_address, string stemp, out bool bCriticalError, out int iErrorCode)
        {
            string sres;
            string sresp;
            bool bErrorBit, bErrorCommBit, bResetBit;

            bCriticalError = false;
            iErrorCode = 0;

            // set temp
            sres = WriteCmd(">" + tec_address + "SP0=" + stemp, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            return sres;
        }

        /// <summary>
        /// Set temperature for specified TEC channel
        /// </summary>
        /// <param name="rh_channel">RH channel to operate with ("0" | "1")</param>
        /// <param name="stemp">Temperature to set, C</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string SetTemperatureRH(string rh_channel, string stemp, out bool bCriticalError, out int iErrorCode)
        {
            string strResult = "";
            bCriticalError = false;
            iErrorCode = 0;
            uint uiRHChannel = Convert.ToUInt32(rh_channel);
            string sres;
            string sresp;
            bool bErrorBit, bErrorCommBit, bResetBit;

            string mod_addr = "07";

            // set temp
            sres = WriteCmd(">" + mod_addr + "RS" + rh_channel + "=" + stemp, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            _m_fCurrentSetpoint = Convert.ToSingle(stemp);
            return strResult;
        }

        /// <summary>
        /// Get current temperature in C from selected TEC channel
        /// </summary>
        /// <param name="tec_address">TEC channel to operate with ("00".."06")</param>
        /// <param name="stemp">Current temperature, C</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string GetTemperatureTEC(string tec_address, out string stemp, out bool bCriticalError, out int iErrorCode, out bool btranBusyCh0, out bool bControlCh0)
        {
            stemp = "";
            string sres;
            float fresults;
            bool bErrorBit, bErrorCommBit, bResetBit;
            bool btranBusyCh1, bControlCh1; // not used

            bCriticalError = false;
            iErrorCode = 0;

            // get temp
            sres = WriteCmd(">" + tec_address + "TE0", true, out stemp, out bErrorBit, out bErrorCommBit, out bResetBit, out btranBusyCh0, out btranBusyCh1, out bControlCh0, out bControlCh1);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sresp;
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // check / format result
            if (float.TryParse(stemp, out fresults))
            {
                stemp = fresults.ToString("00.0");
            }
            else
            {
                stemp = "";
                return "Error: TEC board, TEC channel: " + tec_address + "; wrong response.";
            }
            return "";
        }

        /// <summary>
        /// Get current temperature in C from selected TEC channel
        /// </summary>
        /// <param name="tec_address">TEC channel to operate with ("00".."06")</param>
        /// <param name="stemp">Current temperature, C</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string GetTemperatureTEC(string tec_address, out string stemp, out bool bCriticalError, out int iErrorCode, out bool btranBusyCh0, out bool bControlCh0, out bool btranBusyCh1, out bool bControlCh1)
        {
            stemp = "";
            string sres;
            float fresults;
            bool bErrorBit, bErrorCommBit, bResetBit;

            bCriticalError = false;
            iErrorCode = 0;

            // get temp
            sres = WriteCmd(">" + tec_address + "TE0", true, out stemp, out bErrorBit, out bErrorCommBit, out bResetBit, out btranBusyCh0, out btranBusyCh1, out bControlCh0, out bControlCh1);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sresp;
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // check / format result
            if (float.TryParse(stemp, out fresults))
            {
                stemp = fresults.ToString("00.0");
            }
            else
            {
                stemp = "";
                return "Error: TEC board, TEC channel: " + tec_address + "; wrong response.";
            }
            return "";
        }

        /// <summary>
        /// Get current power in percent from selected TEC channel
        /// </summary>
        /// <param name="tec_address">TEC channel to operate with ("00".."06")</param>
        /// <param name="spwr">Current power usage, %</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string GetPowerTEC(string tec_address, out string spwr, out bool bCriticalError, out int iErrorCode)
        {
            spwr = "";
            string sres;
            float fresults;
            bool bErrorBit, bErrorCommBit, bResetBit;

            bCriticalError = false;
            iErrorCode = 0;

            // get power
            sres = WriteCmd(">" + tec_address + "PR0", true, out spwr, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sresp;
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // check / format result
            if (float.TryParse(spwr, out fresults))
            {
                spwr = fresults.ToString("0");
            }
            else
            {
                spwr = "";
                return "Error: TEC board, TEC channel: " + tec_address + "; wrong response.";
            }

            return "";
        }

        /// <summary>
        /// Get output current from selected TEC channel
        /// </summary>
        /// <param name="tec_address">TEC channel to operate with ("00".."06")</param>
        /// <param name="samp">Current power usage, %</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string GetOutputCurrentTEC(string tec_address, out string samp, out bool bCriticalError, out int iErrorCode)
        {
            samp = "";
            string sres;
            float fresults;
            bool bErrorBit, bErrorCommBit, bResetBit;

            bCriticalError = false;
            iErrorCode = 0;

            // get power
            sres = WriteCmd(">" + tec_address + "RA0", true, out samp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sresp;
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // check / format result
            if (float.TryParse(samp, out fresults))
            {
                samp = fresults.ToString();
            }
            else
            {
                samp = "";
                return "Error: TEC board, TEC channel: " + tec_address + "; wrong response.";
            }

            return "";
        }

        /// <summary>
        /// Get current TEC ramp control status for selected RH channel
        /// </summary>
        /// <param name="rh_channel">RH channel to operate with ("0".."1")</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public void GetTECRampState(string tec_address, out bool btranBusy, out bool bControl, out bool bCriticalError, out int iErrorCode)
        {
            string sresult;
            string sres;
            bool bErrorBit, bErrorCommBit, bResetBit;
            bool btranBusyCh1, bControlCh1;

            bCriticalError = false;
            iErrorCode = 0;

            sres = WriteCmd(">" + tec_address + "ST0", true, out sresult, out bErrorBit, out bErrorCommBit, out bResetBit, out btranBusy, out btranBusyCh1, out bControl, out bControlCh1);
            if (bErrorBit || bErrorCommBit)
            {
                string sresp;
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                }
            }
        }

        /// <summary>
        /// Get current temperature in C from selected RH channel
        /// </summary>
        /// <param name="rh_channel">RH channel to operate with ("0".."1")</param>
        /// <param name="stemp">Current temperature, C</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string GetTemperatureRH(string rh_channel, out string stemp, out bool btranBusy, out bool bControl, out bool bCriticalError, out int iErrorCode)
        {
            uint uiRHChannel = Convert.ToUInt32(rh_channel);
            string sres;
            float fresults;
            bool bErrorBit, bErrorCommBit, bResetBit;
            bool btranBusyCh0, btranBusyCh1, bControlCh0, bControlCh1;
            string mod_addr = "07";

            bCriticalError = false;
            iErrorCode = 0;
            stemp = "";
            btranBusy = false;
            bControl = false;

            // get temp
            sres = WriteCmd(">" + mod_addr + "TE" + rh_channel, true, out stemp, out bErrorBit, out bErrorCommBit, out bResetBit, out btranBusyCh0, out btranBusyCh1, out bControlCh0, out bControlCh1);
            if (Convert.ToInt32(rh_channel) == 0)
            {
                btranBusy = btranBusyCh0;
                bControl = bControlCh0;
            }
            else
            {
                btranBusy = btranBusyCh1;
                bControl = bControlCh1;
            }
            if (sres != "") { return sres; }
            if (bErrorBit || bErrorCommBit)
            {
                string sresp;
                string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // check / format result
            if (float.TryParse(stemp, out fresults))
            {
                stemp = fresults.ToString("00.0");
            }
            else
            {
                stemp = "";
                return "Error: TEC board, RH channel: " + rh_channel + "; wrong response.";
            }
            return "";
        }

        /// <summary>
        /// Get current RH ramp control status for selected RH channel
        /// </summary>
        /// <param name="rh_channel">RH channel to operate with ("0".."1")</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public void GetRHRampState(string rh_channel, out bool btranBusy, out bool bControl, out bool bCriticalError, out int iErrorCode)
        {
            bCriticalError = false;
            iErrorCode = 0;
            btranBusy = false;
            bControl = false;
            uint uiRHChannel = Convert.ToUInt32(rh_channel);
            string sresult;
            string sres;
            bool btranBusyCh0, btranBusyCh1, bControlCh0, bControlCh1;
            bool bErrorBit, bErrorCommBit, bResetBit;
            string mod_addr = "07";

            sres = WriteCmd(">" + mod_addr + "HS" + rh_channel, true, out sresult, out bErrorBit, out bErrorCommBit, out bResetBit, out btranBusyCh0, out btranBusyCh1, out bControlCh0, out bControlCh1);
            if (Convert.ToInt32(rh_channel) == 0)
            {
                btranBusy = btranBusyCh0;
                bControl = bControlCh0;
            }
            else
            {
                btranBusy = btranBusyCh1;
                bControl = bControlCh1;
            }
            if (bErrorBit || bErrorCommBit)
            {
                string sresp;
                string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                }
            }
        }

        /// <summary>
        /// Initialize TEC board, selected TEC channel with parameters
        /// </summary>
        /// <param name="tec_address">TEC channel to operate with ("01".."06")</param>
        /// <param name="P_value">"P" parameter</param>
        /// <param name="I_value">"I" parameter</param>
        /// <param name="D_value">"D" parameter</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string LoadInitialParametersTEC(string tec_address,
                                               string samplePeriod,
                                               string overshootOffset,
                                               string overshootDuration,
                                               string setpointOffset,
                                               string deadBand,
                                               string pBand,
                                               string rabbitGain_Ch0,
                                               string rabbitGain2_Ch0,
                                               string rabbitGainOffset_Ch0,
                                               string rabbitDerivGain_Ch0,
                                               string PGain_Ch0,
                                               string IGain_Ch0,
                                               string DGain_Ch0,
                                               string rabbitGain_Ch1,
                                               string rabbitGain2_Ch1,
                                               string rabbitGainOffset_Ch1,
                                               string rabbitDerivGain_Ch1,
                                               string PGain_Ch1,
                                               string IGain_Ch1,
                                               string DGain_Ch1,
                                               string highClamp,
                                               string lowClamp,
                                               string errorTermBand,
                                               string errorTermCount,
                                               string steadyState_powerLimit,
                                               string steadyState_powerLimitCount,
                                               string thermistor_Coefficent_A,
                                               string thermistor_Coefficent_B,
                                               string thermistor_Coefficent_C,
                                               out bool bCriticalError,
                                               out int iErrorCode
                                              )
        {
            string sres;
            string sresp;
            string sResult;
            bool bErrorBit, bErrorCommBit, bResetBit;

            bCriticalError = false;
            iErrorCode = 0;

            //-- PID parameters 

            // write_samplePeriod = 0.1
            sres = WriteCmd(">" + tec_address + "TP0=" + samplePeriod, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit); 
            if (sres != "") { return sres; }
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // Create Param History address record if first time set
            CRampParameterHistory tempParamHistoryObj;
            bool bFound = _m_RampParamHistory.TryGetValue(tec_address, out tempParamHistoryObj);
            if (!bFound)
            {
                _m_RampParamHistory.Add(tec_address, new CRampParameterHistory());
            }

            sResult = LoadRampDirectionalAllParametersTEC(tec_address,
                                                          overshootOffset,
                                                          overshootDuration,
                                                          setpointOffset,
                                                          deadBand,
                                                          pBand,
                                                          rabbitGain_Ch0,
                                                          rabbitGain2_Ch0,
                                                          rabbitGainOffset_Ch0,
                                                          rabbitDerivGain_Ch0,
                                                          PGain_Ch0,
                                                          IGain_Ch0,
                                                          DGain_Ch0,
                                                          rabbitGain_Ch1,
                                                          rabbitGain2_Ch1,
                                                          rabbitGainOffset_Ch1,
                                                          rabbitDerivGain_Ch1,
                                                          PGain_Ch1,
                                                          IGain_Ch1,
                                                          DGain_Ch1,
                                                          highClamp,
                                                          lowClamp,
                                                          out bCriticalError,
                                                          out iErrorCode
                                                         );
            if (sResult != "")
            {
                return "Critical error " + iErrorCode.ToString() + " occurred.\n";
            }

            // write_errorTerm_band = 100
            sres = WriteCmd(">" + tec_address + "EB0=" + errorTermBand, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit); 
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write_errorTerm_count = 100
            sres = WriteCmd(">" + tec_address + "EC0=" + errorTermCount, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write_steadyState_powerLimit = 100
            sres = WriteCmd(">" + tec_address + "PE0=" + steadyState_powerLimit, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit); 
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write_steadyState_powerLimitCount = 100
            sres = WriteCmd(">" + tec_address + "PC0=" + steadyState_powerLimitCount, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write_low_TEC_CurrentLimit = 100
            sres = WriteCmd(">" + tec_address + "CL0=0.05", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write Thermistor Coefficent A
            sres = WriteCmd(">" + tec_address + "AC0=" + thermistor_Coefficent_A, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            // write Thermistor Coefficent B
            sres = WriteCmd(">" + tec_address + "BC0=" + thermistor_Coefficent_B, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write Thermistor Coefficent C
            sres = WriteCmd(">" + tec_address + "CC0=" + thermistor_Coefficent_C, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            return "";
        }

        // Ramp Parameter history values
        private class CRampParameterHistory
        {
            public string[] m_strRabbitGain = new string[2] { "","" };
            public string[] m_strRabbitGain2 = new string[2] { "", "" };
            public string[] m_strRabbitGainOffset = new string[2] { "", "" };
            public string[] m_strRabbitDerivGain = new string[2] { "", "" };
            public string[] m_strPGain = new string[2] { "", "" };
            public string[] m_strDGain = new string[2] { "", "" };
            public string[] m_strIGain = new string[2] { "", "" };
            public string m_strDeadBand = "";
            public string m_strOvershootOffset = "";
            public string m_strOvershootDuration = "";
            public string m_strSetpointOffset = "";
            public string m_strPBand = "";
            public string m_strLowClamp = "";
            public string m_strHighClamp = "";
        }

        private Dictionary<string, CRampParameterHistory> _m_RampParamHistory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tec_address"></param>
        /// <param name="rabbitgain_ch0"></param>
        /// <param name="rabbitgain_ch1"></param>
        /// <param name="rabbitgain2_ch0"></param>
        /// <param name="rabbitgain2_ch1"></param>
        /// <param name="rabbitgainoffset_ch0"></param>
        /// <param name="rabbitgainoffset_ch1"></param>
        /// <param name="rabbitderivgain_ch0"></param>
        /// <param name="rabbitderivgain_ch1"></param>
        /// <param name="rabbitpgain_ch0"></param>
        /// <param name="rabbitpgain_ch1"></param>
        /// <param name="rabbitdgain_ch0"></param>
        /// <param name="rabbitdgain_ch1"></param>
        /// <param name="rabbitigain_ch0"></param>
        /// <param name="rabbitigain_ch1"></param>
        /// <param name="deadband"></param>
        /// <param name="overshootOffset"></param>
        /// <param name="overshootDuration"></param>
        /// <param name="setpointOffset"></param>
        /// <param name="pBand"></param>
        /// <param name="lowClamp"></param>
        /// <param name="highClamp"></param>
        /// <param name="bCriticalError"></param>
        /// <param name="iErrorCode"></param>
        /// <returns></returns>
        public string LoadRampDirectionalAllParametersTEC(string tec_address,
                                                          string overshootOffset,
                                                          string overshootDuration,
                                                          string setpointOffset,
                                                          string deadBand,
                                                          string pBand,
                                                          string rabbitGain_Ch0,
                                                          string rabbitGain2_Ch0,
                                                          string rabbitGainOffset_Ch0,
                                                          string rabbitDerivGain_Ch0,
                                                          string PGain_Ch0,
                                                          string IGain_Ch0,
                                                          string DGain_Ch0,
                                                          string rabbitGain_Ch1,
                                                          string rabbitGain2_Ch1,
                                                          string rabbitGainOffset_Ch1,
                                                          string rabbitDerivGain_Ch1,
                                                          string PGain_Ch1,
                                                          string IGain_Ch1,
                                                          string DGain_Ch1,
                                                          string highClamp,
                                                          string lowClamp,
                                                          out bool bCriticalError,
                                                          out int iErrorCode
                                                         )
        {
            string sres;
            string sresp;
            bool bErrorBit, bErrorCommBit, bResetBit;

            bCriticalError = false;
            iErrorCode = 0;

            // Create Param History address record if first time set
            CRampParameterHistory tempParamHistoryObj;
            bool bFound = _m_RampParamHistory.TryGetValue(tec_address, out tempParamHistoryObj);
            if (!bFound)
            {
                _m_RampParamHistory.Add(tec_address, new CRampParameterHistory());
            }

            if (_m_RampParamHistory[tec_address].m_strOvershootOffset.ToUpper() != overshootOffset.ToUpper())
            {
                // write_overshootOffset = 0
                sres = WriteCmd(">" + tec_address + "OO0=" + overshootOffset, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strOvershootOffset = overshootOffset;
            }

            if (_m_RampParamHistory[tec_address].m_strOvershootDuration.ToUpper() != overshootDuration.ToUpper())
            {
                // write_overshootDuration = 0
                sres = WriteCmd(">" + tec_address + "OD0=" + overshootDuration, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strOvershootDuration = overshootDuration;
            }

            if (_m_RampParamHistory[tec_address].m_strSetpointOffset.ToUpper() != setpointOffset.ToUpper())
            {
                // write_setpointOffset = 0
                sres = WriteCmd(">" + tec_address + "OF0=" + setpointOffset, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strSetpointOffset = setpointOffset;
            }

            if (_m_RampParamHistory[tec_address].m_strDeadBand.ToUpper() != deadBand.ToUpper())
            {
                // write_deadBand = 0
                sres = WriteCmd(">" + tec_address + "DB0=" + deadBand, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strDeadBand = deadBand;
            }

            if (_m_RampParamHistory[tec_address].m_strPBand.ToUpper() != pBand.ToUpper())
            {
                // write_pBand = 0
                sres = WriteCmd(">" + tec_address + "PB0=" + pBand, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strPBand = pBand;
            }

            if (_m_RampParamHistory[tec_address].m_strRabbitGain[0].ToUpper() != rabbitGain_Ch0.ToUpper())
            {
                // write_rabbitGain = 0
                sres = WriteCmd(">" + tec_address + "KR0=" + rabbitGain_Ch0, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strRabbitGain[0] = rabbitGain_Ch0;
            }

            if (_m_RampParamHistory[tec_address].m_strRabbitGain2[0].ToUpper() != rabbitGain2_Ch0.ToUpper())
            {
                // write_rabbitGain_2 = 0
                sres = WriteCmd(">" + tec_address + "KQ0=" + rabbitGain2_Ch0, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strRabbitGain2[0] = rabbitGain2_Ch0;
            }

            if (_m_RampParamHistory[tec_address].m_strRabbitGainOffset[0].ToUpper() != rabbitGainOffset_Ch0.ToUpper())
            {
                // write_rabbitGainOffset = 0
                sres = WriteCmd(">" + tec_address + "KO0=" + rabbitGainOffset_Ch0, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                    _m_RampParamHistory[tec_address].m_strRabbitGainOffset[0] = rabbitGainOffset_Ch0;
                }
            }

            if (_m_RampParamHistory[tec_address].m_strRabbitDerivGain[0].ToUpper() != rabbitDerivGain_Ch0.ToUpper())
            {
                // write_rabbitDerivGain = 0
                sres = WriteCmd(">" + tec_address + "KS0=" + rabbitDerivGain_Ch0, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strRabbitDerivGain[0] = rabbitDerivGain_Ch0;
            }

            if (_m_RampParamHistory[tec_address].m_strPGain[0].ToUpper() != PGain_Ch0.ToUpper())
            {
                // write_pGain = 0 // P-GAIN (ch0, heating) (75)
                sres = WriteCmd(">" + tec_address + "KP0=" + PGain_Ch0, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strPGain[0] = PGain_Ch0;
            }

            if (_m_RampParamHistory[tec_address].m_strIGain[0].ToUpper() != IGain_Ch0.ToUpper())
            {
                // write_iGain = 0 // I-GAIN (ch0, heating) (0.05)
                sres = WriteCmd(">" + tec_address + "KI0=" + IGain_Ch0, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strIGain[0] = IGain_Ch0;
            }

            if (_m_RampParamHistory[tec_address].m_strDGain[0].ToUpper() != DGain_Ch0.ToUpper())
            {
                // write_dGain = 0 // D-GAIN (ch0, heating) (0)
                sres = WriteCmd(">" + tec_address + "KD0=" + DGain_Ch0, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strDGain[0] = DGain_Ch0;
            }

            if (_m_RampParamHistory[tec_address].m_strRabbitGain[1].ToUpper() != rabbitGain_Ch1.ToUpper())
            {
                // write_rabbitGain = 0 (ch1)
                sres = WriteCmd(">" + tec_address + "KR1=" + rabbitGain_Ch1, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strRabbitGain[1] = rabbitGain_Ch1;
            }

            if (_m_RampParamHistory[tec_address].m_strRabbitGain2[1].ToUpper() != rabbitGain2_Ch1.ToUpper())
            {
                // write_rabbitGain_2 = 0 (ch1)
                sres = WriteCmd(">" + tec_address + "KQ1=" + rabbitGain2_Ch1, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strRabbitGain2[1] = rabbitGain2_Ch1;
            }

            if (_m_RampParamHistory[tec_address].m_strRabbitGainOffset[1].ToUpper() != rabbitGainOffset_Ch1.ToUpper())
            {
                // write_rabbitGainOffset = 0 (ch1)
                sres = WriteCmd(">" + tec_address + "KO1=" + rabbitGainOffset_Ch1, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strRabbitGainOffset[1] = rabbitGainOffset_Ch1;
            }

            if (_m_RampParamHistory[tec_address].m_strRabbitDerivGain[1].ToUpper() != rabbitDerivGain_Ch1.ToUpper())
            {
                // write_rabbitDerivGain = 0 (ch1)
                sres = WriteCmd(">" + tec_address + "KS1=" + rabbitDerivGain_Ch1, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strRabbitDerivGain[1] = rabbitDerivGain_Ch1;
            }

            if (_m_RampParamHistory[tec_address].m_strPGain[1].ToUpper() != PGain_Ch1.ToUpper())
            {
                // write_pGain = 0 // P-GAIN (ch1, cooling) (75)
                sres = WriteCmd(">" + tec_address + "KP1=" + PGain_Ch1, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strPGain[1] = PGain_Ch1;
            }

            if (_m_RampParamHistory[tec_address].m_strIGain[1].ToUpper() != IGain_Ch1.ToUpper())
            {
                // write_iGain = 0 // I-GAIN (ch1, cooling) (0.05)
                sres = WriteCmd(">" + tec_address + "KI1=" + IGain_Ch1, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strIGain[1] = IGain_Ch1;
            }

            if (_m_RampParamHistory[tec_address].m_strDGain[1].ToUpper() != DGain_Ch1.ToUpper())
            {
                // write_dGain = 0 // D-GAIN (ch1, cooling) (0)
                sres = WriteCmd(">" + tec_address + "KD1=" + DGain_Ch1, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strDGain[1] = DGain_Ch1;
            }

            //-- block runaway prevention feature

            if (_m_RampParamHistory[tec_address].m_strHighClamp.ToUpper() != highClamp.ToUpper())
            {
                // write_highClamp = 99
                sres = WriteCmd(">" + tec_address + "HC0=" + highClamp, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strHighClamp = highClamp;
            }

            if (_m_RampParamHistory[tec_address].m_strLowClamp.ToUpper() != lowClamp.ToUpper())
            {
                // write_lowClamp = -99
                sres = WriteCmd(">" + tec_address + "LC0=" + lowClamp, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
                _m_RampParamHistory[tec_address].m_strLowClamp = lowClamp;
            }

            return "";
        }

        /// <summary>
        /// Read TEC board selected chahnel parameters
        /// </summary>
        /// <param name="tec_address">TEC channel to operate with ("01".."06")</param>
        /// <param name="P_value">"P" parameter</param>
        /// <param name="I_value">"I" parameter</param>
        /// <param name="D_value">"D" parameter</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string ReadParametersTEC(string tec_address,
                                        out string samplePeriod,
                                        out string overshootOffset,
                                        out string overshootDuration,
                                        out string setpointOffset,
                                        out string deadBand,
                                        out string pBand,
                                        out string rabbitGain_Ch0,
                                        out string rabbitGain2_Ch0,
                                        out string rabbitGainOffset_Ch0,
                                        out string rabbitDerivGain_Ch0,
                                        out string PGain_Ch0,
                                        out string IGain_Ch0,
                                        out string DGain_Ch0,
                                        out string rabbitGain_Ch1,
                                        out string rabbitGain2_Ch1,
                                        out string rabbitGainOffset_Ch1,
                                        out string rabbitDerivGain_Ch1,
                                        out string PGain_Ch1,
                                        out string IGain_Ch1,
                                        out string DGain_Ch1,
                                        out string highClamp,
                                        out string lowClamp,
                                        out string errorTermBand,
                                        out string errorTermCount,
                                        out string steadyState_powerLimit,
                                        out string steadyState_powerLimitCount,
                                        out string thermistor_Coefficent_A,
                                        out string thermistor_Coefficent_B,
                                        out string thermistor_Coefficent_C,
                                        out bool bCriticalError,
                                        out int iErrorCode
                                       )
        {
            string sres;
            string sresp;
            bool bErrorBit, bErrorCommBit, bResetBit;

            bCriticalError = false;
            iErrorCode = 0;

            samplePeriod = "";
            overshootOffset = "";
            overshootDuration = "";
            setpointOffset = "";
            deadBand = "";
            pBand = "";
            rabbitGain_Ch0 = "";
            rabbitGain2_Ch0 = "";
            rabbitGainOffset_Ch0 = "";
            rabbitDerivGain_Ch0 = "";
            PGain_Ch0 = "";
            IGain_Ch0 = "";
            DGain_Ch0 = "";
            rabbitGain_Ch1 = "";
            rabbitGain2_Ch1 = "";
            rabbitGainOffset_Ch1 = "";
            rabbitDerivGain_Ch1 = "";
            PGain_Ch1 = "";
            IGain_Ch1 = "";
            DGain_Ch1 = "";
            highClamp = "";
            lowClamp = "";
            errorTermBand = "";
            errorTermCount = "";
            steadyState_powerLimit = "";
            steadyState_powerLimitCount = "";
            thermistor_Coefficent_A = "";
            thermistor_Coefficent_B = "";
            thermistor_Coefficent_C = "";

            //-- PID parameters 

            // write_samplePeriod
            sres = WriteCmd(">" + tec_address + "TP0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            samplePeriod = sresp;

            // write_overshootOffset
            sres = WriteCmd(">" + tec_address + "OO0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            overshootOffset = sresp;

            // write_overshootDuration
            sres = WriteCmd(">" + tec_address + "OD0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            overshootDuration = sresp;

            // write_setpointOffset
            sres = WriteCmd(">" + tec_address + "OF0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            setpointOffset = sresp;

            // write_deadBand
            sres = WriteCmd(">" + tec_address + "DB0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            deadBand = sresp;

            // write_pBand
            sres = WriteCmd(">" + tec_address + "PB0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            pBand = sresp;

            // write_rabbitGain
            sres = WriteCmd(">" + tec_address + "KR0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            rabbitGain_Ch0 = sresp;

            // write_rabbitGain_2
            sres = WriteCmd(">" + tec_address + "KQ0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            rabbitGain2_Ch0 = sresp;

            // write_rabbitGainOffset
            sres = WriteCmd(">" + tec_address + "KO0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            rabbitGainOffset_Ch0 = sresp;

            // write_rabbitDerivGain
            sres = WriteCmd(">" + tec_address + "KS0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sresp; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            rabbitDerivGain_Ch0 = sresp;

            // write_pGain // P-GAIN (ch0, heating) (75)
            sres = WriteCmd(">" + tec_address + "KP0" , true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            PGain_Ch0 = sresp;

            // write_iGain  // I-GAIN (ch0, heating) (0.05)
            sres = WriteCmd(">" + tec_address + "KI0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            IGain_Ch0 = sresp;

            // write_dGain  // D-GAIN (ch0, heating) (0)
            sres = WriteCmd(">" + tec_address + "KD0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            DGain_Ch0 = sresp;

            // write_rabbitGain  (ch1)
            sres = WriteCmd(">" + tec_address + "KR1", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            rabbitGain_Ch1 = sresp;

            // write_rabbitGain_2  (ch1)
            sres = WriteCmd(">" + tec_address + "KQ1", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            rabbitGain2_Ch1 = sresp;

            // write_rabbitGainOffset  (ch1)
            sres = WriteCmd(">" + tec_address + "KO1", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            rabbitGainOffset_Ch1 = sresp;

            // write_rabbitDerivGain  (ch1)
            sres = WriteCmd(">" + tec_address + "KS1", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            rabbitDerivGain_Ch1 = sresp;

            // write_pGain  // P-GAIN (ch1, cooling) (75)
            sres = WriteCmd(">" + tec_address + "KP1", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            PGain_Ch1 = sresp;

            // write_iGain  // I-GAIN (ch1, cooling) (0.05)
            sres = WriteCmd(">" + tec_address + "KI1", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            IGain_Ch1 = sresp;

            // write_dGain  // D-GAIN (ch1, cooling) (0)
            sres = WriteCmd(">" + tec_address + "KD1", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            DGain_Ch1 = sresp;

            //-- block runaway prevention feature

            // write_highClamp 
            sres = WriteCmd(">" + tec_address + "HC0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            highClamp = sresp;

            // write_lowClamp 
            sres = WriteCmd(">" + tec_address + "LC0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            lowClamp = sresp;

            // write_errorTerm_band 
            sres = WriteCmd(">" + tec_address + "EB0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            errorTermBand = sresp;

            // write_errorTerm_count 
            sres = WriteCmd(">" + tec_address + "EC0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            errorTermCount = sresp;

            // write_steadyState_powerLimit 
            sres = WriteCmd(">" + tec_address + "PE0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            steadyState_powerLimit = sresp;

            // write_steadyState_powerLimitCount 
            sres = WriteCmd(">" + tec_address + "PC0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            steadyState_powerLimitCount = sresp;

            // write Thermistor Coefficent A
            sres = WriteCmd(">" + tec_address + "AC0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            thermistor_Coefficent_A = sresp;

            // write Thermistor Coefficent B
            sres = WriteCmd(">" + tec_address + "BC0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            thermistor_Coefficent_B = sresp;

            // write Thermistor Coefficent C
            sres = WriteCmd(">" + tec_address + "CC0", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(tec_address, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            thermistor_Coefficent_C = sresp;

            return "";
        }

        /// <summary>
        /// Initialize TEC board, selected RH channel with parameters
        /// </summary>
        /// <param name="rh_channel">RH channel to operate with ("0".."1")</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string LoadInitialParametersRH(string rh_channel,
                                              string P_value,
                                              string I_value,
                                              string D_value,
                                              string PreheatTemperature,
                                              string OvershootDuration,
                                              string ProportionalBand,
                                              string Deadband,
                                              string SetpointOffset,
                                              string Setpoint,
                                              string thermistor_Coefficent_A,
                                              string thermistor_Coefficent_B,
                                              string thermistor_Coefficent_C,
                                              out bool bCriticalError,
                                              out int iErrorCode,
                                              int iSensorSelection = 1,    // Used only for Oven Industries controller
                                              int iSetTemperatureType = 0, // Used only for Oven Industries controller
                                              int iHeatCool = 0,           // Used only for Oven Industries controller
                                              int iControlMode = 0,        // Used only for Oven Industries controller
                                              int iMaxOutputLevel = 100,   // Used only for Oven Industries controller
                                              int iDirectDrive = 14400,    // Used only for Oven Industries controller
                                              int iCurrentLimit = 100      // Used only for Oven Industries controller
                                             )
        {
            string strResult = "";
            string sres;
            string sresp;
            bool bErrorBit, bErrorCommBit, bResetBit;
            uint uiRHChannel = Convert.ToUInt32(rh_channel);
            string mod_addr = "07";

            bCriticalError = false;
            iErrorCode = 0;

            if (uiRHChannel > 1)
            {
                return "LoadInitialParametersRH(): Out of range channel => " + rh_channel + ".\n"; ;
            }
            //-- General params

            // write_resHtrSetpoint 
            sres = WriteCmd(">" + mod_addr + "RS" + rh_channel + "=" + Setpoint, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write_resHtrPreheatTemp
            sres = WriteCmd(">" + mod_addr + "PT" + rh_channel + "=" + PreheatTemperature, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write_overshootDuration
            sres = WriteCmd(">" + mod_addr + "OD" + rh_channel + "=" + OvershootDuration, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write_resHtrPropBand
            sres = WriteCmd(">" + mod_addr + "RO" + rh_channel + "=" + ProportionalBand, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write DeadBand
            sres = WriteCmd(">" + mod_addr + "RB" + rh_channel + "=" + Deadband, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write_samplePeriod = 0.1
            sres = WriteCmd(">" + mod_addr + "TP" + rh_channel + "=" + "0.1", true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write SetpointOffset
            sres = WriteCmd(">" + mod_addr + "HO" + rh_channel + "=" + SetpointOffset, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            //-- PID parameters 

            // write_resHtrPGain = 
            sres = WriteCmd(">" + mod_addr + "RP" + rh_channel + "=" + P_value, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write_resHtrIGain = 
            sres = WriteCmd(">" + mod_addr + "RI" + rh_channel + "=" + I_value, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write_resHtrDGain = 
            sres = WriteCmd(">" + mod_addr + "RD" + rh_channel + "=" + D_value, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write Thermistor Coefficent A
            sres = WriteCmd(">" + mod_addr + "AC" + rh_channel + "=" + thermistor_Coefficent_A, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write Thermistor Coefficent B
            sres = WriteCmd(">" + mod_addr + "BC" + rh_channel + "=" + thermistor_Coefficent_B, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }

            // write Thermistor Coefficent C
            sres = WriteCmd(">" + mod_addr + "CC" + rh_channel + "=" + thermistor_Coefficent_C, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
            if (sres != "") { return sres; };
            if (bErrorBit || bErrorCommBit)
            {
                string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                iErrorCode = Convert.ToInt32(sresp);
                if (isCriticalErrorCode(iErrorCode))
                {
                    bCriticalError = true;
                    return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                }
            }
            return strResult;
        }

        /// <summary>
        /// Initialize TEC board, selected RH channel for temperature monitoring only parameters
        /// </summary>
        /// <param name="rh_channel">RH channel to monitor temperature with ("0".."1")</param>
        /// <returns>Error message or empty string if there is no errors</returns>
        public string LoadInitialParametersMonitorOnlyRH(string rh_channel,
                                                         string thermistor_Coefficent_A,
                                                         string thermistor_Coefficent_B,
                                                         string thermistor_Coefficent_C,
                                                         out bool bCriticalError,
                                                         out int iErrorCode
                                                        )
        {
            string strResult = "";
            bCriticalError = false;
            iErrorCode = 0;
            uint uiRHChannel = Convert.ToUInt32(rh_channel);

            if (uiRHChannel > 1)
            {
                return "[AppliedBiotech_350-0104] LoadInitialParametersMonitorOnlyRH(): Out of range channel => " + rh_channel + ".\n"; ;
            }
            else
            {   // Valid channel
                string sres;
                string sresp;
                bool bErrorBit, bErrorCommBit, bResetBit;

                bCriticalError = false;
                iErrorCode = 0;

                string mod_addr = "07";

                //-- General params

                // write Thermistor Coefficent A
                sres = WriteCmd(">" + mod_addr + "AC" + rh_channel + "=" + thermistor_Coefficent_A, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }

                // write Thermistor Coefficent B
                sres = WriteCmd(">" + mod_addr + "BC" + rh_channel + "=" + thermistor_Coefficent_B, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }

                // write Thermistor Coefficent C
                sres = WriteCmd(">" + mod_addr + "CC" + rh_channel + "=" + thermistor_Coefficent_C, true, out sresp, out bErrorBit, out bErrorCommBit, out bResetBit);
                if (sres != "") { return sres; };
                if (bErrorBit || bErrorCommBit)
                {
                    string sr = ReadClearTECError(mod_addr, out sresp); // Attempt to clear the error, while still reporting it.
                    iErrorCode = Convert.ToInt32(sresp);
                    if (isCriticalErrorCode(iErrorCode))
                    {
                        bCriticalError = true;
                        return "Critical error " + iErrorCode.ToString() + " occurred.\n";
                    }
                }
            }
            return strResult;
        }

        // ********************************************************************************************
        //
        // The following method definitions are specific to the Oven Industries 5R9-350 Thermal adapter
        // 
        // ********************************************************************************************

        // ASKS THE CONTROLLER A QUESTION AND WAITS FOR THE RESPONSE
        // INPUT:STRING (COMMAND) AND INT (VALUE, 0 IF NO REPLY)
        // OUTPUT: INT VALUE
        private int _OI_GetComVal(string s, short n)
        {
            lock (_m_spSerialPort)
            {
                string command;
                short data = 0;

                command = "*";
                command += s;
                command += n.ToString("X4").ToLower();
                command += _OI_CheckSum(command);
                command += "\x0d";

                for (int x = 0; x < command.Length; x++)
                {
                    try
                    {
                        _m_spSerialPort.Write(command.ToCharArray(), x, 1);
                    }
                    catch (Exception)
                    {
                        data = Int16.Parse("f009", System.Globalization.NumberStyles.HexNumber);
                        return data;
                    }
                }

                string indata;
                try
                {
                    indata = _m_spSerialPort.ReadLine();
                }
                catch (Exception)
                {
                    data = Int16.Parse("f000", System.Globalization.NumberStyles.HexNumber);
                    _m_spSerialPort.DiscardInBuffer();
                    _m_spSerialPort.DiscardOutBuffer();
                    return data;
                }
                indata += '^';
                if (indata != "")
                {
                    data = _OI_ProcessData(indata);
                    _m_spSerialPort.DiscardInBuffer();
                }
                return data;
            }
        }

        private string _OI_CheckSum(string s)
        {
            short value = 0;
            char[] c = new char[100];
            c = s.ToCharArray();

            for (int x = 1; x < c.Length; x++)
                value += (short)c[x];
            value = (short)(value % 256);

            return value.ToString("X").ToLower(); ;
        }

        private bool _OI_acceptchar(string s)
        {
            int x;
            bool y = true;
            for (x = 1; x < 5; x++)
            {
                switch (s[x])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                        break;
                    default:
                        y = false;
                        break;
                }
            }
            return y;
        }

        private bool _OI_AntiCheckSum(string s)
        {
            int calcSum = 0, retSum = 0;
            string calcVal, retVal;
            if (s.Length < 8) return false;
            calcVal = s.Substring(0, s.Length - 3);
            if (s[0] != '*') return false;
            if (_OI_acceptchar(calcVal) == false) return false;
            calcSum = int.Parse(_OI_CheckSum(calcVal), System.Globalization.NumberStyles.HexNumber);

            retVal = s.Substring(5);
            retVal.ToUpper();
            retVal = retVal.Substring(0, retVal.Length - 1);
            try
            {
                retSum = int.Parse(retVal, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception /*Exc*/)
            {
                return false;
            }

            if (calcSum != retSum)
                return false;
            return true;
        }

        private short _OI_ProcessData(string s)
        {
            string var;
            short data = 0;

            if (_OI_AntiCheckSum(s))
            {
                if (s.Length == 8)
                {
                    var = s.Substring(1, s.Length - 4);
                    data = Int16.Parse(var, System.Globalization.NumberStyles.HexNumber);
                }
                else
                {
                    data = Int16.Parse("f001", System.Globalization.NumberStyles.HexNumber); ;
                }
            }
            else
            {
                data = Int16.Parse("f002", System.Globalization.NumberStyles.HexNumber); ;
            }
            return data;
        }
    }
}
