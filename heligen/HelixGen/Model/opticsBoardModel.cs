// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using ABot2;

namespace HelixGen.Model
{
    public class opticsBoardModel : INotifyPropertyChanged, IDisposable
    {
        public clsAMBOptics _devOptics;

        /// <summary>
        /// The firmware version of the optics board.
        /// </summary>
        /// <remarks>
        /// This is retrieved upon initialization.
        /// </remarks>
        protected string _firmwareVersion;

        protected double[] _currents;
        protected double[] _pdReadings;
        protected double[] _temperatures;

        protected HelixGenModel _model;


        public event PropertyChangedEventHandler PropertyChanged;


        public opticsBoardModel(HelixGenModel modelIn = null)
        {
            _model = modelIn;

            _currents = new double[clsAMBOptics.MAX_CHANNELS];
            _pdReadings = new double[clsAMBOptics.MAX_CHANNELS];
            _temperatures = new double[clsAMBOptics.MAX_CHANNELS];

            _devOptics = new clsAMBOptics();
        }

        public bool Initialize()
        {
            bool bInitialized = true;

            _devOptics.Initialize(_model.Config.m_OpticsController_Configuration.m_strPort);

            // Read the firmware version.

            FirmwareVersion = _devOptics.ReadFirmwareVersion();

#if false
            double[] currents = _devOptics.GetLEDCurrent();

            double[] pdValues = _devOptics.GetPDReadings();

            double[] driverTemps = _devOptics.GetLEDDriverTemperatures();
            double[] temps = _devOptics.GetLEDTemperatures();

            _devOptics.SetLEDCurrent(1, 0.3);
            _devOptics.SetLEDCurrent(1, 0);
#endif

            // Turn off all the LEDS
            /*
            for(int nLED = 0; nLED < 6; nLED++)
            {
                _devOptics.SetLEDCurrent(nLED+1, 0f);
            }*/

            return bInitialized;
        }

        public void SetLedCurrent(int nLed, double current)
        {

            _devOptics.SetLEDCurrent(nLed, current);
        }
        public void StopSample()
        {

            _devOptics.StopSample();
        }

        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        #region accessors
        /// <summary>
        /// The firmware version of the Optics board.
        /// </summary>
        public string FirmwareVersion
        {
            get { return _firmwareVersion; }
            set
            {
                _firmwareVersion = value;
                NotifyPropertyChanged("FirmwareVersion");
            }
        }

        /// <summary>
        /// The current currents from the optics board.
        /// </summary>
        public double[] Currents
        {
            get { return _currents; }
            set
            {
                _currents = value;
                NotifyPropertyChanged("Currents");
            }
        }

        /// <summary>
        /// The current PhotoDetector readings from the optics board.
        /// </summary>
        public double[] PDReadings
        {
            get { return _pdReadings; }
            set
            {
                _pdReadings = value;
                NotifyPropertyChanged("PDReadings");
            }
        }

        /// <summary>
        /// The current Termperatures from the optics board.
        /// </summary>
        public double[] Temperatures
        {
            get { return _temperatures; }
            set
            {
                _temperatures = value;
                NotifyPropertyChanged("Temperatures");
            }
        }
        #endregion

        /// <summary>
        /// Requests the model to read the currents on the Optics board.
        /// </summary>
        public void ReadCurrent()
        {
            Currents = _devOptics.GetLEDCurrent();
        }

        /// <summary>
        /// Requests the model to read the currents on the Optics board.
        /// </summary>
        public void ReadCurrent(int nChannel)
        {
            Currents[nChannel] = _devOptics.GetLEDCurrent()[nChannel];
        }

        /// <summary>
        /// Requests the model to read the photodetector values on the Optics board.
        /// </summary>
        public double[] ReadPhotodetector()
        {
           // _devOptics.TriggerPDReadings(true);
            PDReadings = _devOptics.GetPDReadings();
            return PDReadings;
        }
        public void TriggerPhotodetector(bool mode)
        {
            _devOptics.TriggerPDReadings( mode);
           // PDReadings = _devOptics.GetPDReadings();
            //return PDReadings;
        }

        /// <summary>
        /// Requests the model to read the photodetector values on the Optics board.
        /// </summary>
        public void ReadPhotodetector(int nChannel, bool mode)
        {
            _devOptics.TriggerPDReadings(mode);
            PDReadings[nChannel] = _devOptics.GetPDReadings()[nChannel];
        }

        /// <summary>
        /// Requests the model to read the LED Temperatures on the Optics board.
        /// </summary>
        public double[] ReadTemperatures()
        {
            Temperatures = _devOptics.GetLEDTemperatures();
            return Temperatures;
        }

        /// <summary>
        /// Requests the model to read the LED Temperatures on the Optics board.
        /// </summary>
        public void ReadTemperature(int nChannel)
        {
            Temperatures[nChannel] = _devOptics.GetLEDTemperatures()[nChannel];
        }

        public bool UnpackFrame()
        {
            return _devOptics.UnpackFrame();
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

                _devOptics = null;

                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~opticsBoardModel()
        {
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
