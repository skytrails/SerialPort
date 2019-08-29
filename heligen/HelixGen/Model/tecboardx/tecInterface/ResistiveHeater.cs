using NLog;
using System;
using System.Collections.Generic;

namespace Accel.HeaterBoard
{
    public partial class ResistiveHeater : HeaterBase
    {
        internal ResistiveHeater(Board board, uint id, uint channel)
            : base(board, id, RSetpoint)
        {
            logger.Trace("E: ResistiveHeater(): construct {0}:{1:d2}:{2:d1}", board.Name, id, channel);
            if (channel != 0 && channel != 1)
                throw new ArgumentOutOfRangeException("channel");
            _channel = channel;
        }
        override public string Name { get { return base.Name + ":" + _channel.ToString(); } }

        override internal bool Controlling(Status stat)
        {
            return _channel == 0 ? stat.Is0Controlling : stat.Is1Controlling;
        }
        override internal bool Transitioning(Status stat)
        {
            return _channel == 0 ? stat.Is0Transitioning : stat.Is1Transitioning;
        }

        // Returns the value to the activate (MD) command, which is unfortunately dependent on the
        // state of the other heater, unknown to this one.  So, the value is requested from the board.
        override protected string ActivationValue(bool state)
        {
            return ActivationValueProxy(_channel, state).ToString();
        }
        override protected string DefaultChannel() { return _channel.ToString(); }

        private uint _channel;
        internal Func<uint, bool, int> ActivationValueProxy;

        public float HeatPid_P
        {
            get { return _HeatPid_P; }
            set
            {
                if (_HeatPid_P != value)
                {
                    logger.Trace("E: {0} HeatPid_P <= {1}", Name, value);
                    Set(value, RkPropo, "0");
                    _HeatPid_P = value;
                    NotifyPropertyChange("HeatPid_P");
                }
            }
        }

        public float HeatPid_I
        {
            get { return _HeatPid_I; }
            set
            {
                if (_HeatPid_I != value)
                {
                    logger.Trace("E: {0} HeatPid_I <= {1}", Name, value);
                    Set(value, RkInteg, "0");
                    _HeatPid_I = value;
                    NotifyPropertyChange("HeatPid_I");
                }
            }
        }

        public float HeatPid_D
        {
            get { return _HeatPid_D; }
            set
            {
                if (_HeatPid_D != value)
                {
                    logger.Trace("E: {0} HeatPid_D <= {1}", Name, value);
                    Set(value, RkDeriv, "0");
                    _HeatPid_D = value;
                    NotifyPropertyChange("HeatPid_D");
                }
            }
        }

        public float CoolPid_P
        {
            get { return _CoolPid_P; }
            set
            {
                if (_CoolPid_P != value)
                {
                    logger.Trace("E: {0} CoolPid_P <= {1}", Name, value);
                    Set(value, RkPropo, "1");
                    _CoolPid_P = value;
                    NotifyPropertyChange("CoolPid_P");
                }
            }
        }

        public float CoolPid_I
        {
            get { return _CoolPid_I; }
            set
            {
                if (_CoolPid_I != value)
                {
                    logger.Trace("E: {0} CoolPid_I <= {1}", Name, value);
                    Set(value, RkInteg, "1");
                    _CoolPid_I = value;
                    NotifyPropertyChange("CoolPid_I");
                }
            }
        }

        public float CoolPid_D
        {
            get { return _CoolPid_D; }
            set
            {
                if (_CoolPid_D != value)
                {
                    logger.Trace("E: {0} CoolPid_D <= {1}", Name, value);
                    Set(value, RkDeriv, "1");
                    _CoolPid_D = value;
                    NotifyPropertyChange("CoolPid_D");
                }
            }
        }

        // Polled values

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}