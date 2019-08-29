using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accel
{
    abstract public class CommandBase : ICommand
    {
        abstract public byte[] Text();
        abstract public void ParseResponse(byte[] resptext);

        // Conversion methods for these bytes enforce ASCII: no chars allowed beyond 0x7F
        static public byte[] AsBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }
        static public string AsString(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }
    }
#if AsyncMessageSupport
    //## issues with this:
    //##  timeout
    //##  cancellation
    //##  command response not parseable as int

    // Support class for 
    internal class AsyncCommand : CommandBase
    {
        public AsyncCommand(ICommand cmd)
        {
            _cmd = cmd;
            _evt = new AutoResetEvent(true);    // create event locked
            response_value = 0;
        }

        public byte[] Text() { return _cmd.Text(); }
        public void ParseResponse(byte[] resp)
        {
            try
            {
                _cmd.ParseResponse(resp);
                string val_rep = AsString(resp).Substring(1);
                response_value = Int32.Parse(val_rep);
            }
            catch (Exception)
            {
            }
            _evt.Reset();
        }

        //## Instead of spawning own Task, could use AsyncAutoResetEvent instead of AutoResetEvent,
        //## and return its WaitAsync() directly;
        //## but that class is in Microsoft.VisualStudio.Threading -- not available to me with VS2013Express!
        public Task WaitAsync() { return new Task(() => _evt.WaitOne()); }
        public int Result { get { return response_value; } }

        ICommand _cmd;
        AutoResetEvent _evt;
        int response_value;
    }
#endif
}
