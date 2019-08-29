using NLog;
using System;
using System.Collections.Generic;

namespace Accel.HeaterBoard
{
    public partial class TecHeater : HeaterBase
    {
        internal TecHeater(Board board, uint id)
            : base(board, id, TSetpoint)
        {
            //logger.Trace("E: TecHeater(): construct {0}:{1:d2}", board.Name, id);
            if (id < Board.Tec0 || id >= Board.Res0)
                throw new ArgumentOutOfRangeException("id");
        }

        public float PowerLevel { get { return _lastPower; } }
        public float Current { get { return _lastCurrent; } }

        override internal bool Controlling(Status stat)
        {
            return stat.Is0Controlling || stat.Is1Controlling;
        }
        override internal bool Transitioning(Status stat)
        {
            return stat.Is0Transitioning || stat.Is1Transitioning;
        }

        override internal void Poll()
        {
            base.Poll();
            Issue(fCmd(TGetPower,
                    (temp) => { _lastPower = temp; NotifyPropertyChange("PowerLevel"); }));
            Issue(fCmd(TGetCurrent,
                    (temp) => { _lastCurrent = temp; NotifyPropertyChange("Current"); }));
        }

        public float HeatPid_P
        {
            get { return _HeatPid_P; }
            set
            {
                if (_HeatPid_P != value)
                {
                    logger.Trace("E: {0} HeatPid_P <= {1}", Name, value);
                    Set(value, TkPropo, "0");
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
                    Set(value, TkInteg, "0");
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
                    Set(value, TkDeriv, "0");
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
                    Set(value, TkPropo, "1");
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
                    Set(value, TkInteg, "1");
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
                    Set(value, TkDeriv, "1");
                    _CoolPid_D = value;
                    NotifyPropertyChange("CoolPid_D");
                }
            }
        }

        // Polled values
        private float _lastPower;
        private float _lastCurrent;

        protected float _setPoint;

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}