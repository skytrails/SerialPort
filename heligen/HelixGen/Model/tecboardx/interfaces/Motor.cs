using System;

namespace Accel
{
    public class MoveCompletedArgs : EventArgs
    {
        public int Position { get; private set; }

        public MoveCompletedArgs(int pos) : base() { Position = pos; }
    }

    /// <summary>
    /// Interface defining the minimum expected features of any generic motor.
    /// </summary>
    public interface IMotor
    {
        int Position { get; }           // Always position in motor steps
        float Coordinate { get; }       // Usually converted from motor steps to real-world units (mm, degrees, etc)

        void Home();
        void Move(int position);
        void Move(float coord);
        void Step(int step);
        void Step(float distance);

        void Stop();

        void SetPosition(int position);  // this should not be a usual action, so don't expose it as a property accessor

        event StrongTypedEventHandler<IMotor, MoveCompletedArgs> MoveCompleted;
    }
}

/*
 * Parameters are not exposed in this interface (yet).
 * There are many parameters, but generally these are not set dynamically within the system.
 * Rather than expose methods for each parameter, the idea is that the Initialize() method of the
 * implementation get its parameter values from a store that is specific to the implementation,
 * and set them according to the internal rules of the implementation.
 * 
 * Possible parameters that might want public exposure:
 *   Velocity, Acceleration, (possibly Deceleration or Jerk but these are not universally available)
 * But generally, these will be implementation-specific
 * => some controllers might expose P,I,D, for instance, but not Accel's.
 * 
 * Parameters from the accel board that are unlikely to need public exposure:
 *   Direction polarity, Profile mode, Microstep mode, current levels
 * Instead, the configuration values for these will be made available to the board object,
 * which will set them at init time and then forget them.
 */