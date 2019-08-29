using System;

namespace Accel
{
    public interface IThermo
    {
        float Temperature { get; }
    }

#if true
    public enum ThermalState { Idle, Ramping, Controlling, LostControl, Aborted };
    public class ThermalProgressArgs : EventArgs
    {
        public ThermalProgressArgs(float temp, TimeSpan elapsed, ThermalState state)
        {
            Temperature = temp;
            SecondsSinceStarted = elapsed;
            State = state;
        }

        public float Temperature { get; internal set; }
        public TimeSpan SecondsSinceStarted { get; internal set; }
        public ThermalState State { get; internal set; }
    }
#endif

    public interface IHeater : IThermo
    {
        float SetPoint { get; set; }

        void Activate( bool active );
        bool Active { get; }
        
        TimeSpan TimeActive { get; }
#if true
        event StrongTypedEventHandler<IHeater, ThermalProgressArgs> ThermalProgress;
#endif
    }

    public interface IFan
    {
        void Activate( bool active );
        bool Active { get; }
    }
}