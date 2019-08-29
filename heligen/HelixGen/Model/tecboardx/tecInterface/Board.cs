using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Accel.HeaterBoard
{
    public enum ErrorCode
    {
        UnrecognizedErrorCode = -1,
        // 0-7 common between heaters & fan-controller
        None = 0,
        InvalidCommandFormat,
        InvalidCommandDataLength,
        Internal_ParseState,
        InvalidWriteCommand,
        InvalidReadCommand,
        InvalidCommandData,
        InvalidChannel,
        // First heater-specific error
        Internal_Math1,
        Internal_Math2,
        Internal_Math3,
        InvalidControlMode,

        OverTemperature,
        ControllerUnresponsive,
        UnderTemperature,
        TecCurrentMonitorError,
        InvalidChecksum_Heater, // *
        TemperatureErrorOutOfRange,
        PowerOutOfRange,

        // First fan-controller-specific error
        InvalidModuleAddress,
        ThermistorOverLimit,
        Internal_I2CFailure,
        InvalidChecksum_Fan,    // * redundant w/ InvalidChecksum_Heater but not identically coded

        // Errors beyond firmware-reported error codes
        ControllerReset,
        InvalidProxiedCommand
    }

    public enum ResetCause
    {
        UnknownResetState = -3,
        UnrecognizedReset = -2,
        NoResetReported = -1,
        FirmwareReset = 0,      // 0000  d'oh
        Watchdog_Sleeping = 3,  // 0011
        Watchdog = 7,           // 0111
        Hardware_Sleeping = 11, // 1011
        PowerUp = 12,           // 1100
        Brownout = 14,          // 1110
        Hardware = 15,          // 1111
    }

    public class ErrEventArgs : EventArgs
    {
        public ErrEventArgs(ErrorCode code) { Error = code; }

        public ErrorCode Error { get; private set; }
    }

    internal struct Status
    // wraps the status bytes in a response
    {
        // Field definitions
        const byte GlobalError = 0x01;
        const byte CommandError = 0x02;
        const byte Reset = 0x04;
        const byte Transition_0 = 0x08;
        const byte Control_0 = 0x10;
        const byte Transition_1 = 0x20;
        const byte Control_1 = 0x40;
        const byte Busy = 0x80;         // Unused
        // Constructor
        public Status(byte b0, byte b1)
        {
            byte nib0, nib1;
            // Does not validate that the bytes are valid hex characters.
            nib0 = (b0 >= 'A') ? (byte)((b0 - (byte)'A') + 10) : (byte)(b0 - (byte)'0');
            nib1 = (b1 >= 'A') ? (byte)((b1 - (byte)'A') + 10) : (byte)(b1 - (byte)'0');
            _status = (byte)(nib0 << 4 | nib1 & 0x0F);
                
        }
        public static implicit operator byte(Status st)  // implicit digit to byte conversion operator
        {
            return st._status;
        }
        // Properties
        public bool HasGlobalError { get { return (_status & GlobalError) != 0; } }
        public bool HasCommandError { get { return (_status & CommandError) != 0; } }
        public bool HasBeenReset { get { return (_status & Reset) != 0; } }
        public bool Is0Transitioning { get { return (_status & Transition_0) != 0; } }
        public bool Is0Controlling { get { return (_status & Control_0) != 0; } }
        public bool Is1Transitioning { get { return (_status & Transition_1) != 0; } }
        public bool Is1Controlling { get { return (_status & Control_1) != 0; } }
        public bool IsBusy { get { return (_status & Busy) != 0; } }
        // Member
        private byte _status;
    }

    public partial class BoardDevice : PropertyNotifier, INamed
    {
        internal const int MinimumHeaterPollInterval = 100;
        internal const int MinimumFanPollInterval = 250;
        virtual public event StrongTypedEventHandler<INamed, ErrEventArgs> ErrorReceived;

        internal BoardDevice(Board brd, uint id, int poll_min)
        {
            _board = brd;
            _addr = id.ToString("d2");

            // Round up the minimum poll interval to be a multiple of the tick interval
            if ((poll_min / Board.Tick) * Board.Tick < poll_min)
                poll_min = (poll_min / Board.Tick + 1) * Board.Tick;
            _min_poll_interval = poll_min;
            _poll_interval = 0;
            _reset_cause = ResetCause.UnknownResetState;
        }
        virtual public string Name { get { return _board.Name + ":" + _addr; } }
        public string FirmwareRevision { get { return _firmware; } }
        public ResetCause ReportedResetCause { get { return _reset_cause; } }

        virtual public void Initialize(ConfigurationElement config)
        {
            logger.Trace("I: {0} BoardDevice.Initialize()", Name);
            ReadFirmwareVersion();
            if (config != null)
                LoadParams(config);       // reads values from config, sets params in controller
            logger.Trace("O: BoardDevice.Initialize()");
        }
        virtual protected void LoadParams(ConfigurationElement config)
        {
            // If LoadParams() is called on this device, then the device should be polled.
            // Set the polling interval (override from 0).
            PollInterval = ((DeviceCfg)config).PollInterval;
        }

        //## s/b public?  Value settable via config, probably not to be changed nor queried.
        internal int PollInterval
        {
            get { return _poll_interval; }
            set
            { 
                _poll_interval = (value > 0 ? Math.Max(value, _min_poll_interval) : 0);
                // if nonzero, set TicksRemaining to 1 to start the polling for this device ASAP
                TicksRemaining = _poll_interval == 0 ? 0 : 1;
            }
        }
        internal int TicksRemaining { get; set; }

        internal void ReadFirmwareVersion()
        {
            Issue(sCmd(GetFirmwareVer, "0", (ver) => { _firmware = ver; }));
        }

        // TranslateError() converts from the integer returned by the board
        // to the ErrorCode enum (trivial, because that's how the enum is set up)
        // Is a virtual method because distinct errors from the FanController overlap
        // the numeric range: see FanController.TranslateError().
        virtual internal ErrorCode TranslateError(int code)
        {
            return ErrorCode.IsDefined(typeof(ErrorCode), code) ?
                (ErrorCode)code :
                ErrorCode.UnrecognizedErrorCode;
        }

        // Each response from the board includes a status field, which is handled here for most messages.
        // If an error is indicated in the status field, issues a query for the error code.
        // If a reset is indicated, issues a query for the reset code.
        // Only one query issues; command error takes precedence over reset, reset takes
        // precedence over global error.
        // Returns False if the rest of the response should be ignored, True otherwise.
        virtual internal bool OnStatus(Status stat)
        {
            ICommand query = null;
            if (stat.HasCommandError || (stat.HasGlobalError && !stat.HasBeenReset))
                query = new iCommand(_addr + GetError + DefaultChannel(),
                        // Process the ER response's status: if HasCommandError, comm is not working:
                        //   so, return false to prevent the callback action from executing.
                        (st) => { return !st.HasCommandError; },
                        // Got an error response, raise any actual error in an event.
                        (err) =>
                        {
                            // TODO: global errors tend to be persistent; suppress raising them for every message
                            if (err != 0)
                                ErrorReceived.Raise(this, new ErrEventArgs(TranslateError(err)));
                        });
            else if (stat.HasBeenReset)
                // query the reset parameter to find out why, and to clear it
                query = new iCommand(_addr + ResetController + DefaultChannel(),
                        (st) => { return !st.HasCommandError; },
                        // Got a reset response, save the reported cause for later querying
                        (cause) =>
                        {
                            // Report reset as an error only if reset state is already known.
                            // A reset flag present in first communications with device does not result in error.
                            if (_reset_cause != ResetCause.UnknownResetState)
                                ErrorReceived.Raise(this, new ErrEventArgs(ErrorCode.ControllerReset));
                            _reset_cause = ResetCause.IsDefined(typeof(ResetCause), cause) ?
                                    (ResetCause)cause :
                                    ResetCause.UnrecognizedReset;
                        });
            // no error or reset, it's a good response; do a little housekeeping
            else if (_reset_cause == ResetCause.UnknownResetState)
                _reset_cause = ResetCause.NoResetReported; // State is no longer unknown
            if (query != null)
                _board.Issue(query, true);  // Issue query right away, in front of other messages in queue

            return !stat.HasCommandError;
        }

        virtual internal void Poll()
        {
            logger.Trace("E: {0} BoardDevice.Poll()", Name);
        }

        // Returns a string representation of "0" to use as channel specifier in outgoing command strings
        // where we'd like the command to work for both types of heaters.  (e.g HeaterBase.SetPoint)
        // (This is overridden by ResistiveHeater to return the heater's actual channel.)
        virtual protected string DefaultChannel() { return "0"; }

        // Returns the argument to use for the activate command of a heater (MD) or fan (SF/TF/HF)
        // For TECs and fans, use this trivial implementation
        // For resistive heaters, see the override in ResistiveHeater
        virtual protected string ActivationValue(bool state)
        {
            return state ? "1" : "0";
        }


        internal void Issue(Command cmd) { _board.Issue(cmd); }

        private Board _board;
        private string _addr;

        private string _firmware;
        private ResetCause _reset_cause;

        private int _poll_interval;
        private int _min_poll_interval;

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }

    internal class Transport : SerialTransport
    {
        const int Baud = 115200;
        public Transport(string port) : base(port, Baud) {}

        override protected byte[] Frame(byte[] text)
        {
            int response_len = text.Length + wprefix.Length + eom.Length;
            // HACK: Incoherent checksum support
            // set flag that will be used here and in the next call to ExtractResponse()
            _no_checksum = (text[1] == '8') ||          // FanController does not use checksum
                    (text[2] == 'C' && text[3] == 'S'); // Checksum command only used to re-enable checksums
            if (!_no_checksum)
                response_len += 2;
            byte[] response = new byte[response_len];
            int ibyte = 0;
            Buffer.BlockCopy(wprefix, 0, response, ibyte, wprefix.Length);
            ibyte += wprefix.Length;
            Buffer.BlockCopy(text, 0, response, ibyte, text.Length);
            ibyte += text.Length;
            if (!_no_checksum)
            {
                Buffer.BlockCopy(ChecksumBytes(response, ibyte), 0, response, ibyte, 2);
                ibyte += 2;
            }
            Buffer.BlockCopy(eom, 0, response, ibyte, eom.Length);
            return response;
        }

        // Two bytes that represent a response consisting of a status indication of "command error",
        // with no payload: returned if the response framing is bad.
        static byte[] CommandError = new byte[2] { (byte)'0', (byte)'2' };
        override protected byte[] ExtractResponse(byte[] text)
        {
            const int MinResponseLength = 2;    // two bytes for status, always (may be more payload)
            // Confirm SOM is correct.  (EOM already confirmed via ResponseComplete(), called by Receive().)
            if (!MatchSubfield(text, 0, rprefix))
                return CommandError;
            int resp_size = text.Length - rprefix.Length - eom.Length;
            // HACK: _no_checksum was set by the previous call to Frame()
            if (!_no_checksum)
            {
                resp_size -= 2;
                int icksum = rprefix.Length + resp_size;
                // Validate checksum
                byte[] calc_checksum = ChecksumBytes(text, icksum);
                if (!MatchSubfield(text, icksum, calc_checksum))
                    return CommandError;
            }
            if (resp_size < MinResponseLength)
                return CommandError;
            byte[] response = new byte[resp_size];
            Buffer.BlockCopy(text, rprefix.Length, response, 0, resp_size);
            return response;
        }
        // Distinctive frames in read vs write because who knows.
        // Two-byte EOM for the same reason
        static byte[] wprefix = { (byte)'>' };
        static byte[] rprefix = { (byte)'<' };
        static byte[] eom = { (byte)'\r', (byte)'\n' };

//#warning this is kind of gross, can it be done more cleanly?
        bool MatchSubfield(byte[] text, int offset, byte[] subfield)
        {
            if (offset < 0)
                return false;
            if (text.Length < offset + subfield.Length)
                return false;
            for (int ibyte = 0; ibyte < subfield.Length; ++ibyte)
                if (text[offset + ibyte] != subfield[ibyte])
                    return false;
            return true;
        }

        private bool _no_checksum;

        private static byte HexDigit(byte value)
        {
            value &= (byte)0x0F;
            return (byte)((value < 10) ? value + '0' : value - 10 + 'A');
        }
        internal static byte[] ChecksumBytes(byte[] text, int length)
        {
            Debug.Assert(length <= text.Length);
            uint checksum = 0;
            for (int ibyte=0; ibyte < length; ++ibyte)
                checksum += text[ibyte];
            checksum %= 255;
            return new byte[2] { HexDigit((byte)((checksum & 0xF0) >> 4)), HexDigit((byte)(checksum & 0x0F)) };
        }

        override protected bool ResponseComplete(byte[] buff, int icount)
        {
            return ((buff[icount - 1] == (byte)'\n' && buff[icount - 2] == (byte)'\r') ||
                (buff[icount - 1] == (byte)'\r' && buff[icount - 2] == (byte)'\n'));
        }
    }

    public partial class Board : INamed, IDisposable
    {
        internal const int nTecHeaters = 6;
        internal const uint Tec0 = 1;
        internal const int nResHeaters = 2;
        internal const uint Res0 = 7;
        internal const int nFans = 3;
        internal const int nThermos = 4;
        internal const uint Fans0 = 8;
        internal const string BoardProxyAddr = "01";    // Use Tec0
        internal const int Tick = 100;

        public event StrongTypedEventHandler<INamed, ErrEventArgs> ErrorReceived;

        public string Name { get; private set; }
        public string BoardRevision { get { return _board_rev; } }
        public string SerialNumber { get { return _serial_no; } }

        public Board(string name, TransportBase tport = null)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            logger.Trace("I: Board(): construct {0}{1}", name, (tport==null?"":" with supplied transport"));
            Name = name;
            _transport = tport;
            // Each TEC heater has its own address
            _tec_heaters = new List<TecHeater>();
            for (uint ii = 0; ii < nTecHeaters; ++ii)
            {
                _tec_heaters.Add(new TecHeater(this, Tec0 + ii));
                _tec_heaters[(int)ii].ErrorReceived += Board_ErrorReceived;
            }
            // The two resistive heaters share an address
            _res_heaters = new List<ResistiveHeater>(nResHeaters);
            for (uint ichannel = 0; ichannel < nResHeaters; ++ichannel)
            {
                _res_heaters.Add(new ResistiveHeater(this, Res0, ichannel));
                _res_heaters[(int)ichannel].ActivationValueProxy = this.ResistiveHeaterActivationValue;
                _res_heaters[(int)ichannel].ErrorReceived += Board_ErrorReceived;
            }
            _fans = new FanController(this, Fans0);
            _fans.ErrorReceived += Board_ErrorReceived;

            _pollees = new List<BoardDevice>();
            _pollees.AddRange(_tec_heaters);
            _pollees.AddRange(_res_heaters);
            _pollees.Add(_fans);
            _poll_timer = new System.Threading.Timer(OnPoll);

            logger.Trace("O: Board()");
        }

        void Board_ErrorReceived(INamed sender, ErrEventArgs args)
        {
            logger.Trace("{0} relayed error {1} from {2}", Name, args.Error, sender.Name);
            // errors from devices are dispatched to anything that's monitoring our error
            ErrorReceived.Raise(this, args);
        }

        public void Initialize(ConfigurationElement config)
        {
            if (config == null)
                throw new ArgumentNullException("config");
            var cfg = (BoardCfgSec)config;
            logger.Trace("I: Board.Initialize() {0} with config for {1} TECs, {2} RHs", Name, cfg.Tecs.Count, cfg.ResHtrs.Count);
            if (_transport == null)
            {
                _transport = new Transport(cfg.Port);
            }
            try
            {
                _transport.Start();
            }
            catch (System.IO.IOException e)
            {
                logger.Fatal(e, Name + " could not start transport: " + e.Message);
                throw;
            }

            // Query a processor (just one, altho there are eight) for board-specific data
            // (Each processor should have the same information.)
            Issue(new sCommand(BoardProxyAddr + BoardDevice.BoardRev + "0", OnProxyStatus, (rev) => { _board_rev = rev; }));
            Issue(new sCommand(BoardProxyAddr + BoardDevice.BoardSerNo + "0", OnProxyStatus, (rev) => { _serial_no = rev; }));
            DeactivateAll(false);
            // Continue initialization into board devices
            foreach (TecCfg teccfg in cfg.Tecs)
            {
                _tec_heaters[teccfg.Index].Initialize(teccfg);
            }
            foreach (ResHtrCfg rhcfg in cfg.ResHtrs)
            {
                _res_heaters[rhcfg.Index].Initialize(rhcfg);
            }
            _fans.Initialize(cfg.FanController);

            // Start polling timer with an immediate tick
            _poll_timer.Change(0, Timeout.Infinite);

            logger.Trace("O: Board.Initialize()");
        }

        // Use special status handler for the proxied messages: global errors and resets are specific
        // to the specific controller.  Rather than get involved in one controller's error handling,
        // just bypass it.
        private bool OnProxyStatus(Status st)
        {
            if (st.HasCommandError)
            {
                ErrorReceived.Raise(this, new ErrEventArgs(ErrorCode.InvalidProxiedCommand));
                return false;
            }
            else
                return true;
        }

        // Device accessors
        public TecHeater TecHeater(int index)
        {
            return _tec_heaters[index];
        }
        public ResistiveHeater ResHeater(int index)
        {
            return _res_heaters[index];
        }
        public Fan Fan(FanId id) { return _fans.Fan(id); }
        public Thermistor Thermistor(ThermId id) { return _fans.Thermistor(id); }

        /// <summary>
        /// Issue is used by BoardDevices (and the Board itself) to place a command in the queue.
        /// When the command has been transmitted and a response received, the command's
        /// ParseResponse() method will be called.
        /// </summary>
        /// <param name="cmd">ICommand object</param>
        /// <param name="priority">True if command has priority and should be sent before
        /// any already-queued messages.</param>
        /// <remarks>Use of priority is limited and should not be used for setting parameters
        /// or for most device operations.</remarks>
        internal void Issue(ICommand cmd, bool priority = false)
        {
#if true
            // HACK Enforce checksum because disabling it causes too much trouble
            byte[] text = cmd.Text();
            // Look for xxCSx=0  (xx indicates address and channel fields, exact values unimportant)
            if (text.Length == 7 && text[2] == 'C' && text[3] == 'S' && text[6] == '0')
                throw new InvalidOperationException("HeaterBoard.Board.Issue(): disabling checksum is not supported");
#endif
            if (priority)
                _transport.Send(cmd);
            else
                _transport.Queue(cmd);
        }

        // Callback from polling timer
        private void OnPoll(object state)
        {
            PollNextDeviceBurst();
            _poll_timer.Change(Tick, Timeout.Infinite);    // Kick off another tick more-or-less on schedule
        }
        // Poll a single device on the board
        // Polling typically is reading the temperature(s) from the device; TEC board
        // also queries current and power.
        // The idea here is to spread out the queries over time rather than sending every
        // message at one go (up to twenty-four queries).
        internal void PollNextDevice()
        {
            BoardDevice active = null;
            // identify the device to next be polled
            foreach (BoardDevice dev in _pollees)
            {
                // Ignore devices that aren't getting polled
                if (dev.TicksRemaining == 0)
                    continue;
                // Decrement the countdown for each device
                dev.TicksRemaining -= 1;
                // Has this one elapsed?
                if (dev.TicksRemaining == 0)
                {
                    // First in list to elapse?
                    if (active == null)
                    {
                        active = dev;
                        // reset to poll again after the interval expires
                        dev.TicksRemaining = dev.PollInterval / Tick;
                    }
                    else
                    {
                        // A device earlier in the list will be polled;
                        // need to defer this one until next available tik
                        dev.TicksRemaining += 1;
                    }
                }
            }
            // Have an elapsed one?
            if (active != null)
            {
                // Poll it
                active.Poll();
                // Move device to the end of the list
                //  so that older ones get a chance if this has a short interval
                _pollees.Remove(active);
                _pollees.Add(active);
            }
        }

        // Poll a single device on the board
        // Polling typically is reading the temperature(s) from the device; TEC board
        // also queries current and power.
        internal void PollNextDeviceBurst()
        {
            BoardDevice active = null;
            // identify the device to next be polled
            foreach (BoardDevice dev in _pollees)
            {
                dev.Poll();
            }
        }

        //## ick ick ick ick ick
        internal int ResistiveHeaterActivationValue(uint channel, bool state)
        {
            if (channel > 1)
                throw new ArgumentOutOfRangeException("index");
            var active = (_res_heaters[0].Active ? 0x01 : 0) + (_res_heaters[1].Active ? 0x02 : 0);
            if (channel == 0)
            {
                if (state)
                    active |= 0x01;
                else
                    active &= 0xFE;
            }
            else if (channel == 1)
            {
                if (state)
                    active |= 0x02;
                else
                    active &= 0xFD;
            }
            if (active != 0)
                active |= 0x04;
            return active;
        }

        /// <summary>
        /// Issues commands to deactivate all heaters and fans, to be called at
        /// initialization and shutdown.
        /// </summary>
        /// <remarks>For shutdown, it's necessary to wait for all the messages to be processed
        /// because immediately after this call, the serial transport is stopped and discarded.</remarks>
        /// <param name="block">Set to true if the </param>
        internal void DeactivateAll(bool block)
        {
            // just turn off each of the 
            foreach (var htr in _tec_heaters)
                htr.Activate(false);
            foreach (var fan in _fans._fans)
                fan.Activate(false);

            //## ick ick ick ick ick
            Action on_last_message = null;
            object comsync = null;
            bool last_response_rcvd = false;
            if (block)
            {
                comsync = new object();
                on_last_message = () => { lock (comsync) { last_response_rcvd = true; Monitor.Pulse(comsync); } };
            }
            // Issue the command that shuts off both resistive heaters at once;
            // don't bother processing the response as there's nothing more to be done about it.
            // When the response arrives, the on_last_message action is executed: could do nothing (if null)
            // or could clear the block in the subsequent clause.
            Issue(new vCommand("07MD0=0", (st) => true, on_last_message));
            if (block) lock(comsync)
            {
                while (!last_response_rcvd)
                    Monitor.Wait(comsync);
            }
        }

        // When Board is disposed, make sure the transport is disposed first,
        // which releases hardware handles and stops the comm thread;
        // and stop and dispose of the polling timer.
        public void Dispose()
        {
            _poll_timer.Change(Timeout.Infinite, Timeout.Infinite);
            _poll_timer.Dispose();

            DeactivateAll(true);    // Issue deactivation commands and wait until all processed

            _transport.Dispose();
        }

        private TransportBase _transport;

        internal List<TecHeater> _tec_heaters;
        internal List<ResistiveHeater> _res_heaters;
        internal FanController _fans;

        private List<BoardDevice> _pollees;
        private Timer _poll_timer;

        private string _board_rev;
        private string _serial_no;

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}