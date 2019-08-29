using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using NLog;


namespace Accel
{
    public class SerialTransport : TransportBase
    {
        const int blockLimit = 64;
        public SerialTransport(string port_id, int baud)
        {
            _port = new SerialPort(port_id, baud);

            // SWE start

            _port.StopBits = StopBits.One;
            _port.ReadTimeout = 1000;
            _port.WriteTimeout = 1000;
            _port.DataBits = 8;
            _port.ReadTimeout = (5 * 60 * 1000);

            // SWE end

            // Defaults to 8N1
            // Base classes that require different setup than 8N1 should set properties
            // on the _port directly, in their own constructor.
        }
        override protected string Id { get { return _port.PortName; } }

        override public void Start()
        {
            _port.Open();
            logger.Info("Serial port {0} opened", Id);
            base.Start();
        }
        override public void Stop()
        {
            base.Stop();
            _port.Close();
        }

        //## Just throwing IOException is simple, and the exception is handled in IssueAndListen();
        //## but if there is any handling that can be done automatically without needing to terminate
        //## the listener, we can catch the exception and handle it (and maybe re-throw selectively).

        // Transmit is a SYNCHRONOUS (blocking) call
        // This is OK because it runs on a background thread (and is fast)
        override protected void Transmit(byte[] text)
        {
            _port.BaseStream.Write(text, 0, text.Length);
        }

        // Receive is a SYNCHRONOUS (blocking) call.
        // This is OK because it runs on a background thread.
        override protected byte[] Receive()
        {
            //## member receive buffer could be constant
            //## and extracted into a new buffer...?
            //## As it is, each response takes three byte[] allocs: this new, the new in Resize(), and the new in Extract()
            //## and then typically copied to a string besides
            byte[] buffer = new byte[blockLimit];
            int ibuff = 0;

            do
            {
                ibuff += _port.BaseStream.Read(buffer, ibuff, blockLimit - ibuff);
            } while (!ResponseComplete(buffer, ibuff));     // not sure if this is needed...
            Array.Resize(ref buffer, ibuff);
            return buffer;
        }

        virtual protected bool ResponseComplete(byte[] buff, int icount)
        {
            // Usually, board responses end with LF (\n)
            // Override this as needed
            return (buff[icount - 1] == '\n');
        }

        protected SerialPort _port;
        protected byte[] _rcv_buffer;
        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
