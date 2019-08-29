using System;
using System.ComponentModel;
namespace Accel
{
    // Lifted from
    // <http://stackoverflow.com/questions/1046016/event-signature-in-net-using-a-strong-typed-sender>
    // Sets the type of TSender so clients don't need to cast from 'object'.
    // Works even if a StrongTypedEventHandler event is defined in a generic class.
    [SerializableAttribute]
    public delegate void StrongTypedEventHandler<TSender, TEventArgs>(
        TSender sender,
        TEventArgs e
    )
    where TEventArgs : EventArgs;


    // Lifted from
    // <http://codeblog.jonskeet.uk/category/cs6/>
    // and modified to use StrongTypedEventHandler.
    //
    // Note that with C# v.6 this can be eliminated; instead of XEvent.Raise(this, args),
    // use   XEvent?.Invoke(this, args)
    public static class EventUtils
    {
        // Use    event.Raise(args);    rather than    if (event!=null) event(args);
        //
        // As a parameter, 'handler' does not change; this way if the only subscriber
        // unsubscribes between the "== null" test and the invocation, the invocation
        // will still succeed because 'handler' will still be non-null.
        public static void Raise<TSender, TEventArgs>(this StrongTypedEventHandler<TSender, TEventArgs> handler, TSender sender, TEventArgs args)
            where TEventArgs : EventArgs
        {
            if (handler != null)
            {
                handler(sender, args);
            }
        }

        // Same implementation for the IPropertyChanged event handler & args type.
        // Cannot do this with a template based on EventHandler<T> because PropertyChangedEventArgs is not template-based.
        public static void Raise(this PropertyChangedEventHandler handler, object sender, PropertyChangedEventArgs args)
        {
            if (handler != null)
            {
                handler(sender, args);
            }
        }
    }

    public class PropertyNotifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChange(string name)
        {
            PropertyChangedEventHandler dlgt = PropertyChanged;
            if (dlgt != null)
                dlgt(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// Interface to use as a commonality of any nameable type.
    /// Primarily intended so that both Board and BoardDevice classes can be used in a single context.
    /// </summary>
    public interface INamed
    {
        string Name { get; }
    }
}