// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using ABot2;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelixGen.Model
{
    public class motorBoardModel: IDisposable
    {
        /// <summary>
        /// A reference back to the parent model.
        /// </summary>
        protected HelixGenModel _model;

        /// <summary>
        /// Reference to the actual motorboard.
        /// </summary>
        protected clsAMB _motorBoard;

        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public motorBoardModel(HelixGenModel modelIn)
        {
            _model = modelIn;
            _motorBoard = new clsAMB();
        }

        /// <summary>
        /// Initializes the model.
        /// </summary>
        /// <remarks>
        /// The initialization includes initializing the board.
        /// </remarks>
        /// <returns></returns>
        public async Task<bool> Initialize(string port)
        {
            bool bResult = false;

            try
            {
                _motorBoard.Initialize(port);
                bResult = true;
            }
            catch(Exception ex)
            {
                logger.Debug("motorBoardModel::Initialize() Caught an exception: {0}", ex.Message);

                bResult = false;
            }

            return bResult;
        }

        /// <summary>
        /// A reference to the lower level motor board.
        /// </summary>
        public clsAMB board
        {
            get { return _motorBoard; }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _motorBoard = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~motorBoardModel() {
           // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
           Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
