using System;
using System.Collections.Generic;
using System.Diagnostics;
using NLog;

namespace Accel.HeaterBoard
{
    abstract class Command : Accel.CommandBase
    {
        public delegate bool StatusHandler(Status stat);
        override public byte[] Text() { return text; }



        internal Command(string cmd, StatusHandler hndlr)
        {
            text = AsBytes(cmd);
            HandleStatus = hndlr;
        }

        private byte[] text;
        protected StatusHandler HandleStatus;

        /// <summary>
        /// Returns a string with a formatted float value (single-precision) at maximum precision,
        /// but using a string of 0s after the decimal, instead of an explicit exponent.
        /// There may be up to six zeros between the period and the first nonzero digit,
        /// depending on the value.
        /// For very small values, limit the precision so as not to overrun the 18-character
        /// limit imposed by the TEC controller's message structure.
        /// </summary>
        internal static string FloatFormat(float value)
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

            return string.Format("{0:" + floatSpec + "}", value);
        }
    }

    // Four flavors of command based on expected format of response
    class vCommand : Command
    {
        public vCommand(string str, StatusHandler shndlr, Action dlgt)
            : base(str, shndlr) { OnComplete = dlgt; }
        override public void ParseResponse(byte[] resptext)
        {
            if (HandleStatus(new Status(resptext[0], resptext[1])) && OnComplete != null)
                OnComplete();
        }

        private Action OnComplete;
    }
    class fCommand : Command
    {


        public fCommand(string str, StatusHandler shndlr, Action<float> dlgt)
            : base(str, shndlr)
        {
            
            OnComplete = dlgt;
        }
        override public void ParseResponse(byte[] resptext)
        {
            if (HandleStatus(new Status(resptext[0], resptext[1])) && OnComplete != null)
                OnComplete(Single.Parse(Accel.CommandBase.AsString(resptext).Substring(2)));
        }

        private Action<float> OnComplete;
    }
    class iCommand : Command
    {
        public iCommand(string str, StatusHandler shndlr, Action<int> dlgt)
            : base(str, shndlr) { OnComplete = dlgt; }
        override public void ParseResponse(byte[] resptext)
        {
            if (HandleStatus(new Status(resptext[0], resptext[1])) && OnComplete != null)
                OnComplete(Int32.Parse(Accel.CommandBase.AsString(resptext).Substring(2)));
        }

        private Action<int> OnComplete;
    }
    class sCommand : Command
    {
        public sCommand(string str, StatusHandler shndlr, Action<string> dlgt)
            : base(str, shndlr) { OnComplete = dlgt; }
        override public void ParseResponse(byte[] resptext)
        {
            if (HandleStatus(new Status(resptext[0], resptext[1])) && OnComplete != null)
                OnComplete(Accel.CommandBase.AsString(resptext).Substring(2));
        }
        private Action<string> OnComplete;
    }

    public partial class BoardDevice
    {
        // Methods to generate the above types of actions for a heater-board device
        // prepends the command string with the address
        // Sets the status-handler to the default one; if this needs to be overridden,
        //  generate the xCommand object explicitly

        internal Command vCmd(string cmd, string chan, Action hndlr)
        {
            return new vCommand(_addr + cmd + chan, this.OnStatus, hndlr);
        }

        internal Command fCmd(string cmd, string chan, string value, Action<float> hndlr)
        {
            return new fCommand(_addr + cmd + chan + "=" + value, this.OnStatus, hndlr);
        }
        internal Command fCmd(string cmd, string chan, Action<float> hndlr)
        {
            return new fCommand(_addr + cmd + chan, this.OnStatus, hndlr);
        }
        internal Command fCmd(string cmd, Action<float> hndlr)
        {
            return fCmd(cmd, DefaultChannel(), hndlr);
        }

        internal Command iCmd(string cmd, string chan, string value, Action<int> hndlr)
        {
            return new iCommand(_addr + cmd + chan + "=" + value, this.OnStatus, hndlr);
        }
        internal Command iCmd(string cmd, string chan, Action<int> hndlr)
        {
            return new iCommand(_addr + cmd + chan, this.OnStatus, hndlr);
        }
        internal Command iCmd(string cmd, Action<int> hndlr)
        {
            return iCmd(cmd, DefaultChannel(), hndlr);
        }
        internal Command sCmd(string cmd, string chan, Action<string> hndlr)
        {
            return new sCommand(_addr + cmd + chan, this.OnStatus, hndlr);
        }

        #region CommandStrings
        // First char of label indicates which controller(s) it works on:
        //  H -> Heater (TEC or Resistive)
        //  T -> TEC only
        //  R -> Resistive only
        //  F -> Fan controller
        internal const string HActivate = "MD";
        internal const string HGetTemperature = "TE";
        internal const string HGetMinMax = "MX";
        internal const string HSampleTime = "TP";
        // "sheq" => Steinhart-Hart equation
        internal const string HsheqA = "AC";
        internal const string HsheqB = "BC";
        internal const string HsheqC = "CC";
        internal const string HOvershootDuration = "OD";
        internal const string TOvershootOffset = "OO";
        internal const string RPreheatTemp = "PT";
        internal const string TSetpoint = "SP";
        internal const string RSetpoint = "RS";
        internal const string TSetpointOffset = "OF";
        internal const string RSetpointOffset = "HO";
        internal const string TDeadband = "DB";
        internal const string RDeadband = "RB";
        internal const string TPropband = "PB";
        internal const string RPropband = "RO";

        internal const string TkPropo = "KP";
        internal const string TkInteg = "KI";
        internal const string TkDeriv = "KD";
        internal const string RkPropo = "RP";
        internal const string RkInteg = "RI";
        internal const string RkDeriv = "RD";

        internal const string TGetStatus = "ST";
        internal const string TGetPower = "PR";
        internal const string TGetCurrent = "RA";
        internal const string TkRabbitG2 = "KQ";
        internal const string TkRabbitG = "KR";
        internal const string TkRabbitO = "KO";
        internal const string TkRabbitD = "KS";
        internal const string TClampLo = "LC";
        internal const string TClampHi = "HC";

        internal const string TErrorBand = "EB";
        internal const string TErrorCountLimit = "EC";
        internal const string TPowerLimit = "PE";
        internal const string TPowerLimitCount = "PC";

        // 0,1,2,3 ordering follows enums FanId and ThermId
        internal const string FActivate0 = "SF";
        internal const string FActivate1 = "TF";
        internal const string FActivate2 = "HF";
        internal const string FGetTherm0 = "BT";
        internal const string FGetTherm1 = "AT";
        internal const string FGetTherm2 = "TT";
        internal const string FGetTherm3 = "HT";
        internal const string FPWM = "FD";
        internal const string FGetAvailableThermos = "SS";

        internal const string GetError = "RE";
        internal const string GetFirmwareVer = "VR";
        internal const string BoardRev = "97";
        internal const string BoardSerNo = "98";
        internal const string ResetController = "RC";
        #endregion
    }
}