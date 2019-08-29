// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using HelixGen.Model;

namespace HelixGen.ViewModel
{
    public class vmOptics : INotifyPropertyChanged
    {
        protected string _firmwareVersion;

        public event PropertyChangedEventHandler PropertyChanged;

        HelixGenModel model;
        opticsBoardModel _opticsModel;

        public vmOptics()
        {
            // Get the model.

            model = ((HelixGen.App)App.Current).Model;
            _opticsModel = model.OpticsModel;

            // Subscribe to property notifications from the model.

            _opticsModel.PropertyChanged += _opticsModel_PropertyChanged;
        }

        private void _opticsModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "FirmwareVersion":
                    FirmwareVersion = ((opticsBoardModel)sender).FirmwareVersion;
                    break;
                case "Currents":
                case "PDReadings":
                case "Temperatures":
                    NotifyPropertyChanged(e.PropertyName);
                    break;
            }
        }

 


        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void SetLedCurrent(int nLed, double current)
        {
            Console.WriteLine(string.Format("vmOptics::SetLedCurrent, nLed: {0} current {1}\r\n", nLed.ToString(), current.ToString()));
            _opticsModel.SetLedCurrent(nLed, current);
        }

        #region bindablefields
        /// <summary>
        /// The current version of the optics board firmware.
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
        /// The current led temperatures.
        /// </summary>
        public double[] Currents
        {
            get
            {
                // Front for the model here.

                return _opticsModel.Currents;
            }
        }

        /// <summary>
        /// The current PDReadings.
        /// </summary>
        public double[] PDReadings
        {
            get
            {
                // Front for the model here.

                return _opticsModel.PDReadings;
            }
        }

        /// <summary>
        /// The current led temperatures.
        /// </summary>
        public double[] Temperatures
        {
            get
            {
                // Front for the model here.

                return _opticsModel.Temperatures;
            }
        }


        #endregion

        #region actions
        public void ReadCurrent()
        {
            _opticsModel.ReadCurrent();
        }

        public void ReadCurrent(int nChannel)
        {
            _opticsModel.ReadCurrent(nChannel - 1);
        }

        public void ReadPhotodetector()
        {
            _opticsModel.ReadPhotodetector();
        }

        /*
        public void ReadPhotodetector(int nChannel)
        {
            _opticsModel.ReadPhotodetector(nChannel - 1);
        }*/

        public void ReadTemperature()
        {
            _opticsModel.ReadTemperatures();
        }

        public void ReadTemperature(int nChannel)
        {
            _opticsModel.ReadTemperature(nChannel - 1);
        }

        #endregion

    }
}
