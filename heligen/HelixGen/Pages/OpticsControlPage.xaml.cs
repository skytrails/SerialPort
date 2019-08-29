// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelixGen.ViewModel;
using System.ComponentModel;
using HelixGen.controls;
using NLog;

namespace HelixGen.Pages
{
    /// <summary>
    /// Interaction logic for OpticsControl.xaml
    /// </summary>
    public partial class OpticsControlPage : Page, INotifyPropertyChanged
    {
        protected vmOptics _vmOptics;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public OpticsControlPage()
        {
            InitializeComponent();

            _vmOptics = new vmOptics();

            this.DataContext = _vmOptics;

            _vmOptics.PropertyChanged += _vmOptics_PropertyChanged;




        }

        public void Initialize()
        {

        }

        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void _vmOptics_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Currents":
                    UpdateCurrents();
                    break;
                case "PDReadings":
                    UpdatePDReadings();
                    break;
                case "Temperatures":
                    UpdateTemperatures();
                    break;
            }
        }

        protected void UpdateCurrents()
        {
            // Get the temperatures from the model.

            double[] currents = _vmOptics.Currents;

            // Push the temperatures to the controls.
            //
            // SWE: I was trying to do this via binding, but it's not working.

            string[] controlNames = {
                "ledCtrl0",
                "ledCtrl1",
                "ledCtrl2",
                "ledCtrl3",
                "ledCtrl4",
                "ledCtrl5"
            };

            for (int ndx = 0; ndx < 6; ndx++)
            {
                ledControl ctl = (ledControl)FindName(controlNames[ndx]);

                if (ctl != null)
                {
                    ctl.Currents = currents;
                }
            }
        }

        protected void UpdatePDReadings()
        {
            // Get the temperatures from the model.

            double[] pds = _vmOptics.PDReadings;

            // Push the temperatures to the controls.
            //
            // SWE: I was trying to do this via binding, but it's not working.

            string[] controlNames = {
                "ledCtrl0",
                "ledCtrl1",
                "ledCtrl2",
                "ledCtrl3",
                "ledCtrl4",
                "ledCtrl5"
            };
            /*
            for (int ndx = 0; ndx < 6; ndx++)
            {
                ledControl ctl = (ledControl)FindName(controlNames[ndx]);

                if (ctl != null)
                {
                    ctl.PDReadings = pds;
                }
            }*/
        }

        protected void UpdateTemperatures()
        {
            // Get the temperatures from the model.

            double[] temps = _vmOptics.Temperatures;

            // Push the temperatures to the controls.
            //
            // SWE: I was trying to do this via binding, but it's not working.

            string[] controlNames = {
                "ledCtrl0",
                "ledCtrl1",
                "ledCtrl2",
                "ledCtrl3",
                "ledCtrl4",
                "ledCtrl5"
            };

            for (int ndx = 0; ndx < 6; ndx++)
            {
                ledControl ctl = (ledControl)FindName(controlNames[ndx]);

                if (ctl != null)
                {
                    ctl.Temperatures = temps;
                }
            }
        }

        #region properties



        #endregion



        private void ledControl_ledCurrentChanged(object sender, EventArgs e)
        {
            ledCurrentChangedEventArgs args = (ledCurrentChangedEventArgs)e;
            _vmOptics.SetLedCurrent(args.ledId, args.current);
        }

        private void ledControl_readCurrent(object sender, EventArgs e)
        {
            readCurrentEventArgs args = (readCurrentEventArgs)e;
            _vmOptics.ReadCurrent(/*args.ledId*/);
        }

        private void ledControl_readPhotodetector(object sender, EventArgs e)
        {
            readDetectorEventArgs args = (readDetectorEventArgs)e;
            _vmOptics.ReadPhotodetector(/*args.ledId*/);
        }

        private void ledControl_readTemperature(object sender, EventArgs e)
        {
            readTemperatureEventArgs args = (readTemperatureEventArgs)e;
            _vmOptics.ReadTemperature(/*args.ledId*/);
        }
    }
}
