using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Accel.HeaterBoard
{
    //## Thermistor and Fan require ref to FanController in order to Issue()
    //## and each should get the literal string for the command
    //## OR, each could be passed an Activate or Read delegate encoding both of the above

    public enum FanId { System, Tec, Heater };
    public enum ThermId { Board, System, Tec, Heater };

    public class Thermistor : PropertyNotifier, IThermo
    {
        // TODO  error or throw for unavailable thermo
        public float Temperature { get { return _lastTemperature; } }
        public bool Available { get; internal set; }

        public void Read()
        {
            _controller.ReadTemperature(_command, (temp) => { _lastTemperature = temp; NotifyPropertyChange("Temperature"); });
        }

        internal Thermistor(FanController controller, string command)
        {
            _controller = controller;
            _command = command;
            Available = false;
        }

        private float _lastTemperature;
        private FanController _controller;
        private string _command;
    }

    public class Fan : PropertyNotifier, IFan
    {
        internal Fan(FanController controller, string command)
        {
            _active = false;
            _controller = controller;
            _command = command;
        }
        public void Activate(bool active)
        {
            _controller.ActivateFan(_command, active, (ival) => { _active = active; NotifyPropertyChange("Active"); });
        }
        public bool Active { get { return _active; } }
        public int GlobalDutyCycle
        {
            get { return _controller.FanDutyCycle; }
            set { _controller.FanDutyCycle = value; }
        }

        internal bool _active;
        private FanController _controller;
        private string _command;
    }


    public class FanController : BoardDevice
    {
        internal FanController(Board board, uint id)
            : base(board, id, MinimumFanPollInterval)
        {
            logger.Debug("E: FanController(): construct {0}:{1:d2}", board.Name, id);
            _fans = new List<Fan>();
            _fans.Add(new Fan(this, FActivate0));
            _fans.Add(new Fan(this, FActivate1));
            _fans.Add(new Fan(this, FActivate2));

            _thermos = new List<Thermistor>();
            _thermos.Add(new Thermistor(this, FGetTherm0));
            _thermos.Add(new Thermistor(this, FGetTherm1));
            _thermos.Add(new Thermistor(this, FGetTherm2));
            _thermos.Add(new Thermistor(this, FGetTherm3));

            _dutycycle = 100;
        }
        override public void Initialize(ConfigurationElement config)
        {
            base.Initialize(config);
            // query which thermistors are available for reading
            Issue(iCmd(FGetAvailableThermos, (mask) => SetAvailableThermistors(mask)), true);
        }
        private void SetAvailableThermistors(int mask)
        {
            int selector = 1;
            for (int itherm = 0; itherm < _thermos.Count; ++itherm)
            {
                _thermos[itherm].Available = (mask & selector) != 0;
                selector <<= 1;
            }
        }

        // Query the temperature from each available thermistor
        // If no thermistors are available, this will still get executed (and do nothing),
        // if the FanController is in the polling list.
        override internal void Poll()
        {
            base.Poll();
            foreach (var therm in _thermos)
                if (therm.Available)
                    therm.Read();
        }

        override internal ErrorCode TranslateError(int code)
        {
            const int FanErrorOffset = ErrorCode.InvalidModuleAddress - ErrorCode.Internal_Math1;
            // Fan and heater share some (lower-numbered) error values, but then
            // diverge; this adjusts the code number as needed so that OnStatus()
            // signals the appropriate error for a fan
            if (code >= (int)ErrorCode.Internal_Math1)
                code += FanErrorOffset;
            return base.TranslateError(code);
        }

        public int FanDutyCycle
        {
            get { return _dutycycle; }
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException("dutycycle");
                logger.Debug("E: {0} FanDutyCycle <= {1}", Name, value);
                Issue(iCmd(FPWM, DefaultChannel(), value.ToString(), (ival) => { _dutycycle = value; }), true);
            }
        }

        public Fan Fan(FanId id)
        {
            return _fans[(int)id];
        }

        public Thermistor Thermistor(ThermId id)
        {
            return _thermos[(int)id];
        }

        // Fan objects call this with their given command to activate
        internal void ActivateFan(string command, bool active, Action<int> handler)
        {
            logger.Debug("E: {0} SetPoint <= {1}", Name, active);
            Issue(iCmd(command, DefaultChannel(), ActivationValue(active), handler), true);
        }
        // Thermistor objects call this with their given command to read
        internal void ReadTemperature(string command, Action<float> handler)
        {
            Issue(fCmd(command, handler), true);
        }

        internal List<Fan> _fans;
        internal List<Thermistor> _thermos;
        internal int _dutycycle;

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}