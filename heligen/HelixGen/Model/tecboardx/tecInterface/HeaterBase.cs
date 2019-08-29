using Accel;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Accel.HeaterBoard
{
    /// <summary>
    /// class ProgressMonitor encapsulates the logic of ThermalProgress monitoring in the heaters.
    /// Each method updates the monitor appropriately, and returns true if a change in state occurs.
    /// Sets state to:
    ///  - Ramping, when first activated or when the setpoint is changed;
    ///  - Controlling when a certain number of responses have reported "controlling" in status;
    ///  - LostControl when a certain number of responses have reported "not controlling" once control was seen
    ///  - Idle when the heater is deactivated
    /// These states can be used in the heater's ThermalProgress notification args.
    /// </summary>
    internal class ProgressMonitor
    {
        public const int controlling_threshold = 5;    // ### number of responses, doesn't map to time
        public ProgressMonitor(IHeater heater)
        {
            _heater = heater;
            _has_been_in_control = false;
            _state = ThermalState.Idle;
        }
        public bool Activation(bool active)
        {
            // This method is only called when Active changes
            if (!active)
            {
                _state = ThermalState.Idle;
            }
            else
            {
                // newly active
                _activation_time = DateTime.Now;
                _setpoint_time = _activation_time;
                _has_been_in_control = false;
                _n_controlled = _n_uncontrolled = 0;
                _state = ThermalState.Ramping;
            }
            return true;
        }
        public bool SetpointChange()
        {
            if (!_heater.Active)
                return false;

            _setpoint_time = DateTime.Now;
            _has_been_in_control = false;
            _n_controlled = _n_uncontrolled = 0;
            _state = ThermalState.Ramping;
            return true;
        }
        public bool StatusUpdate(bool is_controlling)
        {
            if (!_heater.Active)
                return false;
            var oldstate = _state;
            if (is_controlling)
            {
                _n_uncontrolled = 0;
                if (_n_controlled < controlling_threshold)
                {
                    if (++_n_controlled == controlling_threshold)
                    {
                        _has_been_in_control = true;
                        _state = ThermalState.Controlling;
                    }
                }
            }
            else
            {
                _n_controlled = 0;
                if (_has_been_in_control)
                {
                    if (_n_uncontrolled < controlling_threshold)
                    {
                        if (++_n_uncontrolled == controlling_threshold)
                            _state = ThermalState.LostControl;
                    }
                }
            }
            return (oldstate != _state);
        }

        public ThermalState State { get { return _state; } }

        public TimeSpan TimeActive
        { get { return _heater.Active ? DateTime.Now - _activation_time : new TimeSpan(0); } }

        public TimeSpan TimeAtSetpoint
        { get { return _heater.Active ? DateTime.Now - _setpoint_time : new TimeSpan(0); } }

        IHeater _heater;

        bool _has_been_in_control;
        int _n_controlled;
        int _n_uncontrolled;
        DateTime _activation_time;
        DateTime _setpoint_time;
        ThermalState _state;
    }

    /// <summary>
    /// Base class for the two heater types on the Accel board
    /// Implements common functionality: activation, setpointing, temperature polling, and heater-specific
    /// handling of the status byte of message replies.
    /// </summary>
    public abstract partial class HeaterBase : BoardDevice, IHeater
    {
        internal HeaterBase(Board brd, uint id, string setpt_cmd)
            : base(brd, id, MinimumHeaterPollInterval)
        {
            _setpoint_cmd = setpt_cmd;   // provided by subclass constructors
            _active = false;
            _setpoint = 0;
            _last_status = new Status();
            monitor = new ProgressMonitor(this);
        }


        public void Activate(bool active)
        {
            logger.Trace("E: {0} Activating: {1}", Name, active);
            // command uses hardcoded "0" instead of DefaultChannel() -- activation always applies via
            // channel 0, even if the heater in question is a resistive on channel 1.
            // (see: ResistiveHeater.ActivationValue())
            Issue(iCmd(HActivate, "0", ActivationValue(active),
                    (ival) =>   // iVal is ignored in handler, assumes command successful
                    {
                        if (Active != active)
                        {
                            Active = active;
                            if (monitor.Activation(active))
                                ThermalProgress.Raise(this, new ThermalProgressArgs(_lastTemperature, monitor.TimeActive, monitor.State));
                        }
                    }));
        }
        public bool Active { get { return _active; }
                internal set { _active = value; NotifyPropertyChange("Active"); } }

        public float Temperature {
            get {
                return _lastTemperature;
            }
                internal set {
                _lastTemperature = value; NotifyPropertyChange("Temperature");
            }
        }

        public float SetPoint
        {
            get { return _setpoint; }
            set
            {
                logger.Trace("E: {0} SetPoint <= {1}", Name, value);
                Set(value, _setpoint_cmd,
                    (val) =>
                    {
                        _setpoint = val; NotifyPropertyChange("SetPoint");
                        if (monitor.SetpointChange())
                            ThermalProgress.Raise(this, new ThermalProgressArgs(_lastTemperature, monitor.TimeActive, monitor.State));
                    });
            }
        }

        /// <summary>
        /// The Sample Time in seconds.
        /// </summary>
        public float SampleTime
        {
            get { return _sampleTime; }
            set
            {
                logger.Trace("E: {0} SampleTime <= {1} seconds", Name, value);
                Set(value, HSampleTime,
                    (val) =>
                    {
                        _sampleTime = val; NotifyPropertyChange("SampleTime");
                    });
            }
        }

        public float OvershootOffset
        {
            get { return _overShootOffset; }
            set
            {
                logger.Trace("E: {0} OverShootOffset <= {1}", Name, value);
                Set(value, TOvershootOffset,
                    (val) =>
                    {
                        _overShootOffset = val; NotifyPropertyChange("OverShootOffset");
                    });
            }
        }

        public float OvershootDuration
        {
            get { return _OvershootDuration; }
            set
            {
                logger.Trace("E: {0} OvershootDuration <= {1}", Name, value);
                Set(value, HOvershootDuration,
                    (val) =>
                    {
                        _OvershootDuration = val; NotifyPropertyChange("OvershootDuration");
                    });
            }
        }

        public float SetpointOffset
        {
            get { return _SetpointOffset; }
            set
            {
                logger.Trace("E: {0} SetpointOffset <= {1}", Name, value);
                Set(value, TSetpointOffset,
                    (val) =>
                    {
                        _SetpointOffset = val; NotifyPropertyChange("SetpointOffset");
                    });
            }
        }

 

        public float ErrorBand
        {
            get { return _ErrorBand; }
            set
            {
                logger.Trace("E: {0} ErrorBand <= {1}", Name, value);
                Set(value, RkDeriv,
                    (val) =>
                    {
                        _ErrorBand = val; NotifyPropertyChange("ErrorBand");
                    });
            }
        }

        public int ErrorCountLimit
        {
            get { return _ErrorCountLimit; }
            set
            {
                logger.Trace("E: {0} ErrorCountLimit <= {1}", Name, value);
                Set(value, RkDeriv,
                    (val) =>
                    {
                        _ErrorCountLimit = (int)val; NotifyPropertyChange("ErrorCountLimit");
                    });
            }
        }

        public int Index
        {
            get { return _Index; }
            set
            {
                logger.Trace("E: {0} Index <= {1}", Name, value);
                _Index = value;
            }
        }

        public int PowerLimitCount
        {
            get { return _PowerLimitCount; }
            set
            {
                logger.Trace("E: {0} PowerLimitCount <= {1}", Name, value);
                Set(value, RkDeriv,
                    (val) =>
                    {
                        _PowerLimitCount = (int)val; NotifyPropertyChange("PowerLimitCount");
                    });
            }
        }

        public float sheq_A
        {
            get { return _sheq_A; }
            set
            {
                logger.Trace("E: {0} sheq_A <= {1}", Name, value);
                Set(value, HsheqA,
                    (val) =>
                    {
                        _sheq_A = val; NotifyPropertyChange("sheq_A");
                    });
            }
        }

        public float sheq_B
        {
            get { return _sheq_B; }
            set
            {
                logger.Trace("E: {0} sheq_B <= {1}", Name, value);
                Set(value, HsheqC,
                    (val) =>
                    {
                        _sheq_B = val; NotifyPropertyChange("sheq_B");
                    });
            }
        }

        public float sheq_C
        {
            get { return _sheq_C; }
            set
            {
                logger.Trace("E: {0} sheq_C <= {1}", Name, value);
                Set(value, HsheqC,
                    (val) =>
                    {
                        _sheq_C = val; NotifyPropertyChange("sheq_c");
                    });
            }
        }

       public float Propband
        {
            get { return _Propband; }
            set
            {
                logger.Trace("E: {0} Propband <= {1}", Name, value);
                Set(value, RkDeriv,
                    (val) =>
                    {
                        _Propband = val; NotifyPropertyChange("Propband");
                    });
            }
        }

        public float HeatRabbit_A
        {
            get { return _HeatRabbit_A; }
            set
            {
                logger.Trace("E: {0} HeatRabbit_A <= {1}", Name, value);
                Set(value, TkRabbitG2,
                    (val) =>
                    {
                        _Propband = val; NotifyPropertyChange("HeatRabbit_A");
                    });
            }
        }

        public float HeatRabbit_B
        {
            get { return _HeatRabbit_B; }
            set
            {
                logger.Trace("E: {0} HeatRabbit_B <= {1}", Name, value);
                Set(value, TkRabbitG,
                    (val) =>
                    {
                        _Propband = val; NotifyPropertyChange("HeatRabbit_B");
                    });
            }
        }

        public float HeatRabbit_C
        {
            get { return _HeatRabbit_C; }
            set
            {
                logger.Trace("E: {0} HeatRabbit_C <= {1}", Name, value);
                Set(value, TkRabbitO,
                    (val) =>
                    {
                        _Propband = val; NotifyPropertyChange("HeatRabbit_C");
                    });
            }
        }

        public float HeatRabbit_D
        {
            get { return _HeatRabbit_D; }
            set
            {
                logger.Trace("E: {0} HeatRabbit_D <= {1}", Name, value);
                Set(value, TkRabbitD,
                    (val) =>
                    {
                        _Propband = val; NotifyPropertyChange("HeatRabbit_D");
                    });
            }
        }

        public float CoolRabbit_A
        {
            get { return _CoolRabbit_A; }
            set
            {
                logger.Trace("E: {0} CoolRabbit_A <= {1}", Name, value);
                Set(value, TkRabbitG2,
                    (val) =>
                    {
                        _Propband = val; NotifyPropertyChange("CoolRabbit_A");
                    });
            }
        }

        public float CoolRabbit_B
        {
            get { return _CoolRabbit_B; }
            set
            {
                logger.Trace("E: {0} CoolRabbit_B <= {1}", Name, value);
                Set(value, TkRabbitG,
                    (val) =>
                    {
                        _Propband = val; NotifyPropertyChange("CoolRabbit_B");
                    });
            }
        }

        public float CoolRabbit_C
        {
            get { return _CoolRabbit_C; }
            set
            {
                logger.Trace("E: {0} CoolRabbit_C <= {1}", Name, value);
                Set(value, TkRabbitO,
                    (val) =>
                    {
                        _Propband = val; NotifyPropertyChange("CoolRabbit_C");
                    });
            }
        }

        public float CoolRabbit_D
        {
            get { return _CoolRabbit_D; }
            set
            {
                logger.Trace("E: {0} CoolRabbit_D <= {1}", Name, value);
                Set(value, TkRabbitD,
                    (val) =>
                    {
                        _Propband = val; NotifyPropertyChange("CoolRabbit_D");
                    });
            }
        }

        public TimeSpan TimeActive { get { return monitor.TimeActive; } }

        public void Reset()
        {
            // TODO fix this
#if ResetCommandSupported
            //## Why not supported?  because controller does not respond to this message,
            //## it just resets.  This causes the transport to hang; no timeout support yet.
            logger.Trace("E: {0} Reset()", Name);
            // "set" the parameter to a value (0) to reset
            Issue(vCmd(ResetController, "0=0", null));  // HACK fake-out "channel" string to include assignment value
#else
            throw new NotImplementedException("HeaterBase.Reset()");
#endif
        }

        public event StrongTypedEventHandler<IHeater, ThermalProgressArgs> ThermalProgress;


        override internal bool OnStatus(Status stat)
        {
            bool no_errors = base.OnStatus(stat);
            if (stat.HasBeenReset)
            {
                Active = false;
                monitor.Activation(false);
                // TODO is TimeSpan(0) the best for this notification?  Could calculate duration, but how much time between reset (deactivation) and now?
                ThermalProgress.Raise(this, new ThermalProgressArgs(_lastTemperature, new TimeSpan(0), ThermalState.Aborted));
                no_errors = false;
            }
            if (no_errors)
            {
                bool is_controlling = Controlling(stat);
#if DEBUG
                // Currently, transitioning state is not examined or used in public interface.
                // examine the controlling and transitioning status for consistency with assumptions behind this code
                bool is_transitioning = Transitioning(stat);
                // 
                if (is_controlling && is_transitioning)
                    logger.Debug("Heater {0} reports 'controlling' and 'transitioning' ({1:X})", Name, (byte)stat);
#endif
                if (monitor.StatusUpdate(is_controlling))
                    ThermalProgress.Raise(this, new ThermalProgressArgs(_lastTemperature, monitor.TimeActive, monitor.State));
            }
            _last_status = stat;
            return no_errors;
        }

        override internal void Poll()
        {
            base.Poll();
            Issue(fCmd(HGetTemperature, (temp) => { Temperature = temp; }));
        }

        // The two heater types need to interpret the status differently
        abstract internal bool Controlling(Status stat);
        abstract internal bool Transitioning(Status stat);

        // Polled values
        float _lastTemperature;     // TE
        // State
        bool _active;               // MD

        ProgressMonitor monitor;
        
        Status _last_status;

        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}