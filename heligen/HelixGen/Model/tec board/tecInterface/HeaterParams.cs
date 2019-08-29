using System;
using System.Collections.Generic;
using System.Configuration;

namespace Accel.HeaterBoard
{
    #region Custom System.Configuration-based classes
    // See: MSDN How To: Create Custom Configuration Sections Using ConfigurationSection
    // <https://msdn.microsoft.com/en-us/library/2tw134k3.aspx>
    // and then search the web for something with more clarity and focus, such as
    // <https://bardevblog.wordpress.com/2013/11/17/kickstart-c-custom-configuration/>

    /// <summary>
    /// Configuration parameter holding class - holds the board-specific parameter(s)
    /// and an array of structures for each heater.
    /// </summary>
    public class BoardCfgSec : ConfigurationSection
    {
        [ConfigurationProperty("port", IsRequired = true)]
        public string Port
        {
            get { return (string)this["port"]; }
            set { this["port"] = value; }
        }

        [ConfigurationProperty("tecs", IsRequired = true)]
        public TecSet Tecs {
            get { return (TecSet)base["tecs"]; }
            set { this["tecs"] = value; }   // SWE; Added.
        }

        [ConfigurationProperty("reshtrs", IsRequired = true)]
        public ResHtrSet ResHtrs {
            get { return (ResHtrSet)base["reshtrs"]; }
            set { this["reshtrs"] = value; }    // SWE; Added

        }

        [ConfigurationProperty("fancontroller", IsRequired = true)]
        public DeviceCfg FanController { get { return (DeviceCfg)base["fancontroller"]; } }
    }
    /// <summary>
    /// Configuration parameter holding class - holds the parameter(s) in common to all the board devices
    /// </summary>
    public class DeviceCfg : ConfigurationElement
    {
        [ConfigurationProperty("poll_interval", IsRequired = true)]
        public int PollInterval
        {
            get { return (int)this["poll_interval"]; }
            set { this["poll_interval"] = value; }
        }
    }
    /// <summary>
    /// Common configuration parameter holding class - holds the parameters common to both types of heaters
    /// </summary>
    /// <remarks>Maybe using a base class here is not useful; this particular implementation may
    /// be even less useful, or more confusing, because some members that are common here are
    /// implemented individually in the target heater classes.</remarks>
    public class HeaterCfg : DeviceCfg
    {
        [ConfigurationProperty("index", IsRequired = true)]
        public int Index
        {
            get { return (int)this["index"]; }
            set { this["index"] = value; }
        }
        [ConfigurationProperty("setpoint", IsRequired = false)]
        public float SetPoint
        {
            get { return (float)this["setpoint"]; }
            set { this["setpoint"] = value; }
        }
        [ConfigurationProperty("setpoint_offset", IsRequired = true)]
        public float SetpointOffset
        {
            get { return (float)this["setpoint_offset"]; }
            set { this["setpoint_offset"] = value; }
        }
        [ConfigurationProperty("deadband", IsRequired = true)]
        public float Deadband
        {
            get { return (float)this["deadband"]; }
            set { this["deadband"] = value; }
        }
        [ConfigurationProperty("propband", IsRequired = true)]
        public float Propband
        {
            get { return (float)this["propband"]; }
            set { this["propband"] = value; }
        }
        [ConfigurationProperty("sample_time", IsRequired = true)]
        public float SampleTime
        {
            get { return (float)this["sample_time"]; }
            set { this["sample_time"] = value; }
        }
        [ConfigurationProperty("overshoot_duration", IsRequired = true)]
        public float OvershootDuration
        {
            get { return (float)this["overshoot_duration"]; }
            set { this["overshoot_duration"] = value; }
        }
        [ConfigurationProperty("sheq_A", IsRequired = true)]
        public float sheq_A
        {
            get { return (float)this["sheq_A"]; }
            set { this["sheq_A"] = value; }
        }
        [ConfigurationProperty("sheq_B", IsRequired = true)]
        public float sheq_B
        {
            get { return (float)this["sheq_B"]; }
            set { this["sheq_B"] = value; }
        }
        [ConfigurationProperty("sheq_C", IsRequired = true)]
        public float sheq_C
        {
            get { return (float)this["sheq_C"]; }
            set { this["sheq_C"] = value; }
        }
    }
    /// <summary>
    /// Configuration parameter holding class - holds the parameters specified to the TEC
    /// </summary>
    public class TecCfg : HeaterCfg
    {
        [ConfigurationProperty("overshoot_offset", IsRequired = true)]
        public float OvershootOffset
        {
            get { return (float)this["overshoot_offset"]; }
            set { this["overshoot_offset"] = value; }
        }
        [ConfigurationProperty("errorband", IsRequired = true)]
        public float ErrorBand
        {
            get { return (float)this["errorband"]; }
            set { this["errorband"] = value; }
        }
        [ConfigurationProperty("error_count_limit", IsRequired = true)]
        public int ErrorCountLimit
        {
            get { return (int)this["error_count_limit"]; }
            set { this["error_count_limit"] = value; }
        }
        [ConfigurationProperty("power_limit", IsRequired = true)]
        public float PowerLimit
        {
            get { return (float)this["power_limit"]; }
            set { this["power_limit"] = value; }
        }
        [ConfigurationProperty("power_limit_count", IsRequired = true)]
        public int PowerLimitCount
        {
            get { return (int)this["power_limit_count"]; }
            set { this["power_limit_count"] = value; }
        }
        [ConfigurationProperty("power_clamp_lo", IsRequired = true)]
        public float PowerLimitLow
        {
            get { return (float)this["power_clamp_lo"]; }
            set { this["power_clamp_lo"] = value; }
        }
        [ConfigurationProperty("power_clamp_hi", IsRequired = true)]
        public float PowerLimitHigh
        {
            get { return (float)this["power_clamp_hi"]; }
            set { this["power_clamp_hi"] = value; }
        }
        [ConfigurationProperty("heat_pid_P", IsRequired = true)]
        public float HeatPid_P
        {
            get { return (float)this["heat_pid_P"]; }
            set { this["heat_pid_P"] = value; }
        }
        [ConfigurationProperty("heat_pid_I", IsRequired = true)]
        public float HeatPid_I
        {
            get { return (float)this["heat_pid_I"]; }
            set { this["heat_pid_I"] = value; }
        }
        [ConfigurationProperty("heat_pid_D", IsRequired = true)]
        public float HeatPid_D
        {
            get { return (float)this["heat_pid_D"]; }
            set { this["heat_pid_D"] = value; }
        }
        [ConfigurationProperty("cool_pid_P", IsRequired = true)]
        public float CoolPid_P
        {
            get { return (float)this["cool_pid_P"]; }
            set { this["cool_pid_P"] = value; }
        }
        [ConfigurationProperty("cool_pid_I", IsRequired = true)]
        public float CoolPid_I
        {
            get { return (float)this["cool_pid_I"]; }
            set { this["cool_pid_I"] = value; }
        }
        [ConfigurationProperty("cool_pid_D", IsRequired = true)]
        public float CoolPid_D
        {
            get { return (float)this["cool_pid_D"]; }
            set { this["cool_pid_D"] = value; }
        }
        [ConfigurationProperty("heat_rabbit_A", IsRequired = true)]
        public float HeatRabbit_A
        {
            get { return (float)this["heat_rabbit_A"]; }
            set { this["heat_rabbit_A"] = value; }
        }
        [ConfigurationProperty("heat_rabbit_B", IsRequired = true)]
        public float HeatRabbit_B
        {
            get { return (float)this["heat_rabbit_B"]; }
            set { this["heat_rabbit_B"] = value; }
        }
        [ConfigurationProperty("heat_rabbit_C", IsRequired = true)]
        public float HeatRabbit_C
        {
            get { return (float)this["heat_rabbit_C"]; }
            set { this["heat_rabbit_C"] = value; }
        }
        [ConfigurationProperty("heat_rabbit_D", IsRequired = true)]
        public float HeatRabbit_D
        {
            get { return (float)this["heat_rabbit_D"]; }
            set { this["heat_rabbit_D"] = value; }
        }
        [ConfigurationProperty("cool_rabbit_A", IsRequired = true)]
        public float CoolRabbit_A
        {
            get { return (float)this["cool_rabbit_A"]; }
            set { this["cool_rabbit_A"] = value; }
        }
        [ConfigurationProperty("cool_rabbit_B", IsRequired = true)]
        public float CoolRabbit_B
        {
            get { return (float)this["cool_rabbit_B"]; }
            set { this["cool_rabbit_B"] = value; }
        }
        [ConfigurationProperty("cool_rabbit_C", IsRequired = true)]
        public float CoolRabbit_C
        {
            get { return (float)this["cool_rabbit_C"]; }
            set { this["cool_rabbit_C"] = value; }
        }
        [ConfigurationProperty("cool_rabbit_D", IsRequired = true)]
        public float CoolRabbit_D
        {
            get { return (float)this["cool_rabbit_D"]; }
            set { this["cool_rabbit_D"] = value; }
        }
    }
    /// <summary>
    /// Configuration parameter holding class - holds the parameters specified to the resistive heater
    /// </summary>
    public class ResHtrCfg : HeaterCfg
    {
        [ConfigurationProperty("overshoot_target", IsRequired = true)]
        public float OvershootTarget
        {
            get { return (float)this["overshoot_target"]; }
            set { this["overshoot_target"] = value; }
        }
        [ConfigurationProperty("pid_p", IsRequired = true)]
        public float Pid_P
        {
            get { return (float)this["pid_p"]; }
            set { this["pid_p"] = value; }
        }
        [ConfigurationProperty("pid_i", IsRequired = true)]
        public float Pid_I
        {
            get { return (float)this["pid_i"]; }
            set { this["pid_i"] = value; }
        }
        [ConfigurationProperty("pid_d", IsRequired = true)]
        public float Pid_D
        {
            get { return (float)this["pid_d"]; }
            set { this["pid_d"] = value; }
        }
    }

    /// <summary>
    /// Collection class for the two different heater config structures
    /// </summary>
    /// <remarks>
    /// This is a lot of boilerplate.
    /// </remarks>
    [ConfigurationCollection(typeof(TecCfg), AddItemName = "tec", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class TecSet : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        { get { return ConfigurationElementCollectionType.BasicMap; } }
        protected override string ElementName
        { get { return "tec"; } }
        protected override ConfigurationElement CreateNewElement() { return new TecCfg(); }
        public TecCfg this[int index]
        {
            get { return (TecCfg)base.BaseGet(index); }
            set {
                if (base.Count > index)
                {
                    if (base.BaseGet(index) != null)
                        base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }
        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((TecCfg)element).Index;
        }
    }
    [ConfigurationCollection(typeof(ResHtrCfg), AddItemName = "reshtr", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class ResHtrSet : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        { get { return ConfigurationElementCollectionType.BasicMap; } }
        protected override string ElementName
        { get { return "reshtr"; } }
        protected override ConfigurationElement CreateNewElement() { return new ResHtrCfg(); }
        public ResHtrCfg this[int index]
        {
            get { return (ResHtrCfg)base.BaseGet(index); }
            set {
                if (base.BaseGet(index) != null)
                    base.BaseRemoveAt(index);
                base.BaseAdd(index, value);
            }
        }
        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((ResHtrCfg)element).Index;
        }
    }
    #endregion

    public abstract partial class HeaterBase : BoardDevice, IHeater
    {
        #region Parameter Set Methods
        /// <summary>
        /// Set a (float) parameter in the controller
        /// </summary>
        /// <param name="value">parameter value</param>
        /// <param name="cmd">command required to set the parameter</param>
        /// <param name="channel">channel to set in the command string</param>
        /// <param name="assign">delegate to do something with the value when command has completed</param>
        /// <remarks>This version is the core version that actually executes the command, and is called
        /// from the following Set() ConfigSet() and ConfigSetOrQuery() methods.</remarks>
        protected void Set(float value, string cmd, string channel, Action<float> assign = null)
        {
            // using a fCommand: gets a data payload in the response
            // checks to see that it matches the passed-in value significantly (but not precisely)
            //### Is this assumption valid??
            //### Alternative approach: issue a vCommand and assume the passed value 
            Issue(fCmd(cmd, channel, Command.FloatFormat(value),
                (val) =>
                {
                    if (Math.Abs(val - value) > float.Epsilon)  //## this is not a valid test...
                        logger.Warn("HeaterBase.Set(): dev {0} cmd {1} sent {2} received {3}", Name, cmd, value, val);
                    if (assign != null)
                        assign(val);
                }), true);
        }
        /// <summary>
        /// Set a (float) parameter in the controller, using the "default channel"
        /// </summary>
        /// <param name="value">parameter value</param>
        /// <param name="cmd">command required to set the parameter</param>
        /// <param name="assign">delegate to do something with the value when command has completed</param>
        protected void Set(float value, string cmd, Action<float> assign = null)
        {
            Set(value, cmd, DefaultChannel(), assign);
        }
        #endregion

        override protected void LoadParams(ConfigurationElement config)
        {
            base.LoadParams(config);
            var cfg = (HeaterCfg)config;

            // Setpoint is exposed as a property, so it has a backing value that gets updated when parameter set on firmware
            Set(cfg.SetPoint, _setpoint_cmd, (val) => { _setpoint = val; });

            Set(cfg.SampleTime, HSampleTime);
            Set(cfg.OvershootDuration, HOvershootDuration);
            Set(cfg.sheq_A, HsheqA);
            Set(cfg.sheq_B, HsheqB);
            Set(cfg.sheq_C, HsheqC);
        }
        // Certain commands differ between resistive and TEC with no difference to user model
        // So, HeaterBase manages the parameter, using the command
        internal string _setpoint_cmd;         // "SP" or "RS", set in subclass constructor
        // PARAMS
        // Parameter values 
        float _setpoint;              // SP | RS

        float _sampleTime;
        float _overShootOffset;
        float _OvershootDuration;
        float _SetpointOffset;

        float _Propband;
        float _ErrorBand;

        int _Index;
        int _ErrorCountLimit;
        int _PowerLimitCount;

        float _sheq_A;
        float _sheq_B;
        float _sheq_C;

        float _HeatRabbit_A;
        float _HeatRabbit_B;
        float _HeatRabbit_C;
        float _HeatRabbit_D;
        float _CoolRabbit_A;
        float _CoolRabbit_B;
        float _CoolRabbit_C;
        float _CoolRabbit_D;

        internal float _HeatPid_P;
        internal float _HeatPid_I;
        internal float _HeatPid_D;
        internal float _CoolPid_P;
        internal float _CoolPid_I;
        internal float _CoolPid_D;
    }

    public partial class TecHeater : HeaterBase
    {
        override protected void LoadParams(ConfigurationElement config)
        {
            base.LoadParams(config);
            var cfg = (TecCfg)config;

            Set(cfg.SetpointOffset, TSetpointOffset);
            Set(cfg.Deadband, TDeadband);
            Set(cfg.Propband, TPropband);

            Set(cfg.OvershootOffset, TOvershootOffset);
            Set(cfg.ErrorBand, TErrorBand);
            Set(cfg.ErrorCountLimit, TErrorCountLimit);
            Set(cfg.PowerLimit, TPowerLimit);
            Set(cfg.PowerLimitCount, TPowerLimitCount);
            Set(cfg.PowerLimitLow, TClampLo);
            Set(cfg.PowerLimitHigh, TClampHi);

            // These parameters are channel-specific in the TEC
            // P,I,D for heating and cooling
            Set(cfg.HeatPid_P, TkPropo, "0");
            Set(cfg.HeatPid_I, TkInteg, "0");
            Set(cfg.HeatPid_D, TkDeriv, "0");
            Set(cfg.CoolPid_P, TkPropo, "1");
            Set(cfg.CoolPid_I, TkInteg, "1");
            Set(cfg.CoolPid_D, TkDeriv, "1");
            // a, b, c, d of rabbit eq'n for heating and cooling
            Set(cfg.HeatRabbit_A, TkRabbitG2, "0");
            Set(cfg.HeatRabbit_B, TkRabbitG, "0");
            Set(cfg.HeatRabbit_C, TkRabbitO, "0");
            Set(cfg.HeatRabbit_D, TkRabbitD, "0");
            Set(cfg.CoolRabbit_A, TkRabbitG2, "1");
            Set(cfg.CoolRabbit_B, TkRabbitG, "1");
            Set(cfg.CoolRabbit_C, TkRabbitO, "1");
            Set(cfg.CoolRabbit_D, TkRabbitD, "1");
        }
    }

    public partial class ResistiveHeater : HeaterBase
    {
        override protected void LoadParams(ConfigurationElement config)
        {
            base.LoadParams(config);
            var cfg = (ResHtrCfg)config;

            Set(cfg.SetpointOffset, RSetpointOffset);
            Set(cfg.Deadband, RDeadband);
            Set(cfg.Propband, RPropband);

            Set(cfg.OvershootTarget, RPreheatTemp);
            Set(cfg.Pid_P, RkPropo);
            Set(cfg.Pid_I, RkInteg);
            Set(cfg.Pid_D, RkDeriv);
        }
    }
}
