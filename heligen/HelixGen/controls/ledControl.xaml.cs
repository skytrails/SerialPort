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
using System.ComponentModel;

namespace HelixGen.controls
{
    /// <summary>
    /// Arguments object sent as part of the ledCurrentChanged event.
    /// </summary>
    public class ledCurrentChangedEventArgs: EventArgs
    {
        protected double _current;
        protected int _ledId;

        public ledCurrentChangedEventArgs(double newValue = 0.0, int ledId = 0)
        {
            _ledId = ledId;
            _current = newValue;
        }

        public double current
        {
            get { return _current; }
        }

        public int ledId
        {
            get { return _ledId;  }
        }
    }

    /// <summary>
    /// Arguments object sent as part of the read temperature event.
    /// </summary>
    public class readTemperatureEventArgs : EventArgs
    {
        protected int _ledId;

        public readTemperatureEventArgs(int ledId = 0)
        {
            _ledId = ledId;
        }

        public int ledId
        {
            get { return _ledId; }
        }
    }

    /// <summary>
    /// Arguments object sent as part of the read voltage event.
    /// </summary>
    public class readDetectorEventArgs : EventArgs
    {
        protected int _ledId;

        public readDetectorEventArgs(int ledId = 0)
        {
            _ledId = ledId;
        }

        public int ledId
        {
            get { return _ledId; }
        }
    }

    /// <summary>
    /// Arguments object sent as part of the read voltage event.
    /// </summary>
    public class readCurrentEventArgs : EventArgs
    {
        protected int _ledId;

        public readCurrentEventArgs(int ledId = 0)
        {
            _ledId = ledId;
        }

        public int ledId
        {
            get { return _ledId; }
        }
    }

    public class TBArrayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strOut;

            if (values[0] == null)
                strOut = "";
            else
            {
                double[] theCurrents = values[0] as double[];
                int ndx = (int)values[1];

                strOut = (theCurrents[ndx - 1]).ToString();
            }

            return strOut;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GBTitleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Format("{0} {1}", values[0], values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// ledControl - Controls a single LED unit.
    /// </summary>
    /// <remarks>
    /// 
    /// 
    /// </remarks>
    public partial class ledControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// The led number that this control represents.
        /// </summary>
        protected int _nLed;

        /// <summary>
        /// The title of the item.
        /// </summary>
        /// <remarks>
        /// This is displayed in the outlining group box as the title of the element.
        /// </remarks>
        protected string _title;

        /// <summary>
        /// The current current on the channel.
        /// </summary>
        protected double[] _currents;

        /// <summary>
        /// The current current on the channel.
        /// </summary>
        protected double _current;

        /// <summary>
        /// The current PDReading on the channel.
        /// </summary>
        protected double[] _PDreading;

        /// <summary>
        /// The current temperature on the channel.
        /// </summary>
        protected double[] _temperature;

        /// <summary>
        /// Triggered when the user updates the ledCurrent.
        /// </summary>
        public event EventHandler ledCurrentChanged;

        /// <summary>
        /// Triggered when the user updates the ledCurrent.
        /// </summary>
        public event EventHandler readCurrent;

        /// <summary>
        /// Triggered when the user requests an update to the photo detector reading.
        /// </summary>
        public event EventHandler readPhotodetector;

        /// <summary>
        /// Triggered when the user requests an update to the photo detector reading.
        /// </summary>
        public event EventHandler readTemperature;

        public event PropertyChangedEventHandler PropertyChanged;


        public ledControl()
        {
            InitializeComponent();
            DataContext = this;

            this.Loaded += LedControl_Loaded;

            // Register for property changed messages from our parent.

            
        }

        private void LedControl_Loaded(object sender, RoutedEventArgs e)
        {
#if false
            Window parent = FindParentWindow(this);

            if (parent != null)
            {
                ((INotifyPropertyChanged)parent).PropertyChanged += ParentWnd_PropertyChanged;
            }
#endif

        }

#if false
        /// <summary>
        /// Recursive function works it way down the visual tree until it 
        /// finds a window that supports PropertyChanged messages.
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public Window FindParentWindow(DependencyObject child)
        {
            Window wndOut = null;

            DependencyObject parent = VisualTreeHelper.GetParent(this);

            if (parent != null)
            {
                Window parentWnd = parent as Window;

                if (parentWnd.PropertyChanged == null)
                {
                    FindParentWindow(parent);
                }
                else
                    return parent as Window;

                parentWnd.PropertyChanged += ParentWnd_PropertyChanged;
            }

            return wndOut;
        }
#endif



        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);


        }

        private void ParentWnd_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(e.PropertyName);
        }

        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// The led represented by this control.
        /// </summary>
        public int Led
        {
            set {
                _nLed = value;
                NotifyPropertyChanged("Led");
            }
            get { return _nLed;  }
        }

        /// <summary>
        /// The title of this control.
        /// </summary>
        public string Title
        {
            set
            {
                _title = value;
            }
            get { return _title; }
        }


#region accessors
        public double[] Currents
        {
            get { return _currents; }
            set
            {
                _currents = value;
                NotifyPropertyChanged("Currents");
            }
        }

        public double Current
        {
            get { return _current; }
            set
            {
                _current = value;
                NotifyPropertyChanged("Current");
            }
        }

        public double[] PDReadings
        {
            get { return _PDreading; }
            set
            {
                _PDreading = value;
                NotifyPropertyChanged("PDReadings");
            }
        }

        public double[] Temperatures
        {
            get { return _temperature; }
            set
            {
                _temperature = value;
                NotifyPropertyChanged("Temperatures");
            }
        }

#endregion

#region event handlers
        private void Slider_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (ledCurrentChanged != null)
                ledCurrentChanged(this, new ledCurrentChangedEventArgs(((Slider)sender).Value, _nLed));
        }

        private void Update_Current_Click(object sender, RoutedEventArgs e)
        {
            if (readCurrent != null)
                readCurrent(this, new readCurrentEventArgs(_nLed));
        }

        private void Update_Detector_Click(object sender, RoutedEventArgs e)
        {
            if (readPhotodetector != null)
                readPhotodetector(this, new readDetectorEventArgs(_nLed));
        }

        private void Update_Temperature_Click(object sender, RoutedEventArgs e)
        {
            if (readTemperature != null)
                readTemperature(this, new readTemperatureEventArgs(_nLed));
        }
#endregion
    }
}
