using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Accel
{
    /// <summary>
    /// MotorSync defines extension methods for IMotor objects.
    /// IMotor object movement calls must return immediately, with the end of the move signaled via
    /// the MoveCompleted event.  There are many situations, however, where the caller simply wants to
    /// wait anyway.  Rather than force the caller to set up the synchronization locally at every time,
    /// these extension methods handle it all in a way that works for any IMotor implementation, as
    /// easily as calling the corresponding non-waiting method.
    /// </summary>
    public static class MotorSync
    {
        public static void HomeAndWait(this IMotor mtr)
        {
            // sync used to wait for move in first axis before proceeding to second axis
            object sync = new object();
            // flag to avoid deadlock in case notify() is called before before the sync-block after each Move()
            bool move_completed = false;
            StrongTypedEventHandler<IMotor, MoveCompletedArgs> notify =
                    (motor, args) => { lock (sync) { move_completed = true;  Monitor.Pulse(sync); } };
            mtr.MoveCompleted += notify;
            try
            {
                mtr.Home();
                lock (sync) { if (!move_completed) Monitor.Wait(sync); }
            }
            finally
            {
                mtr.MoveCompleted -= notify;
            }
        }
        public static void MoveAndWait(this IMotor mtr, float position)
        {
            // sync used to wait for move in first axis before proceeding to second axis
            object sync = new object();
            // flag to avoid deadlock in case notify() is called before before the sync-block after each Move()
            bool move_completed = false;
            StrongTypedEventHandler<IMotor, MoveCompletedArgs> notify =
                    (motor, args) => { lock (sync) { move_completed = true;  Monitor.Pulse(sync); } };
            mtr.MoveCompleted += notify;
            try
            {
                mtr.Move(position);
                lock (sync) { if (!move_completed) Monitor.Wait(sync); }
            }
            finally
            {
                mtr.MoveCompleted -= notify;
            }
        }
        public static void StepAndWait(this IMotor mtr, float step)
        {
            // sync used to wait for move in first axis before proceeding to second axis
            object sync = new object();
            // flag to avoid deadlock in case notify() is called before before the sync-block after each Move()
            bool move_completed = false;
            StrongTypedEventHandler<IMotor, MoveCompletedArgs> notify =
                    (motor, args) => { lock (sync) { move_completed = true;  Monitor.Pulse(sync); } };
            mtr.MoveCompleted += notify;
            try
            {
                mtr.Step(step);
                lock (sync) { if (!move_completed) Monitor.Wait(sync); }
            }
            finally
            {
                mtr.MoveCompleted -= notify;
            }
        }
    }
}
