using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Accel
{
    public class ResponseArgs : EventArgs
    {
        public ResponseArgs(byte[] text) { _text = text; }
        public byte[] Text { get { return _text; } }
        private byte[] _text;
    }
    public delegate void ResponseHandler(object source, ResponseArgs e);

    abstract public class TransportBase : IDisposable
    {
        protected TransportBase()
        {
            Q = new Queue<ICommand>();
            sync = new object();
        }
        abstract protected string Id { get; }

        /// <summary>
        /// Begin the communications worker thread.
        /// </summary>
        /// <remarks>Derived classes MUST obtain and set up any OS resources (e.g. open and configure
        /// a serial port) and then call this base method.</remarks>
        virtual public void Start()
        {
            if (worker != null)
                throw new InvalidOperationException("TransportBase.Start(): thread already active");
            worker = new Thread(new ThreadStart(IssueAndListen));
            worker.Start();
        }
        /// <summary>
        /// Stop the worker thread.
        /// </summary>
        /// <remarks>Derived classes MUST release any OS resources (e.g. port handle) in an override
        /// of this method, after calling this base method.</remarks>
        virtual public void Stop()
        {
            if (worker == null)
                return;
            lock (sync)
            {
                terminate = true;
                Monitor.Pulse(sync);
            }
            worker.Join();
            worker = null;
        }
        /// <summary>
        /// Submit a command for transmission to the device.
        /// </summary>
        /// <param name="cmd">An ICommand object</param>
        //## Usually, this won't be overridden.
        //## This is virtual primarily to allow mocking, as the Moq library does not
        //## mock non-virtual methods.
        virtual public void Queue(ICommand cmd)
        {
            lock (sync)
            {
                Q.Enqueue(cmd);
                logger.Debug("Adding to queue, length is now; {0}", Q.Count.ToString());
                Monitor.Pulse(sync);
            }
        }
        /// <summary>
        /// Submit a command for transmission that will be sent before any items already queued up.
        /// </summary>
        /// <param name="cmd">A priority ICommand object</param>
        /// <remarks>Send() is not intended for general use.  Priority messages are generally not needed,
        /// but can be used 
        /// </remarks>
        //## Usually, this won't be overridden.
        //## This is virtual primarily to allow mocking, as the Moq library does not
        //## mock non-virtual methods.
        virtual public void Send(ICommand cmd)
        {
            lock (sync)
            {
                //## HACK May need to upgrade this from single instance to a high-priority queue
                //## or, replace Q with a prioritizable queue.
#if DEBUG
                //Debug.Assert(HiPriMessage == null);
#else
                if (HiPriMessage != null)
                    logger.Warn("TransportBase.Send() [{0}] call overwriting earlier, unprocessed priority message", Id);
#endif
                HiPriMessage = cmd;
                Monitor.Pulse(sync);
            }
        }

        virtual protected byte[] Frame(byte[] text) { return text; }
        virtual protected byte[] ExtractResponse(byte[] text) { return text; }
        abstract protected void Transmit(byte[] text);
        abstract protected byte[] Receive();

        private string XToString(byte[] bytes)
        {
            string response = string.Empty;

            foreach (byte b in bytes)
                response += (Char)b;

            return response;
        }

        // Background thread
        private void IssueAndListen()
        {
            try
            {
                ICommand cmd;
                bool quit;
                byte[] response;
                terminate = false;
                while (true)
                {
                    quit = false;
                    cmd = null;
                    lock (sync)
                    {
                        while (!terminate && HiPriMessage == null && (Q.Count == 0))
                        {
                            Monitor.Wait(sync);     // lock on 'sync' is released while Wait() is active
                        }
                        quit = terminate;
                        if (HiPriMessage != null)
                        {
                            cmd = HiPriMessage;
                            HiPriMessage = null;
                        }
                        else if (Q.Count > 0)
                        {
                            logger.Debug("Dequeuing an item, before length is {0}", Q.Count.ToString());
                            cmd = Q.Dequeue();
                            logger.Debug("post Dequeue an item, before length is {0}", Q.Count.ToString());
                        }
#if false
                        Monitor.Pulse(sync);    //## unblock possible caller who is Wait'ing on same sync -- who would that be?
#endif
                    }
                    if (quit)
                        break;
                    // Exception from I/O functions (or frame/extract) will terminate the thread

                    logger.Debug("IssueAndListen, sending command; {0}", XToString(cmd.Text()));
                    Transmit(Frame(cmd.Text()));
                    response = ExtractResponse(Receive());

                    // Failure in ParseResponse() is a programming error, but not the fault of TransportBase,
                    // so don't terminate.
                    try { cmd.ParseResponse(response); }
                    catch (Exception e)
                    {
                        //## what more can be done that makes sense?
                        logger.Error(e, "[{0}] exception in ParseResponse()", Id);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "[{0}] exception in IssueAndListen()\n :: {1}\n :: Thread Terminated", Id, e.Message.TrimEnd('\r', '\n'));
                //TODO would like to notify owner that this has happened (event or callback)
            }
            lock (sync)
                Q.Clear();
        }

        // When disposing the class, ensure the background thread is halted.
        public void Dispose()
        {
            Stop();
        }
        Queue<ICommand> Q;
        ICommand HiPriMessage;

        bool terminate;
        object sync;
        Thread worker;
        private static Logger logger = LogManager.GetCurrentClassLogger();
    }
}
