// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using HelixGen.ViewModel;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Common;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.ViewportRestrictions;
using HelixGen.Model;

namespace HelixGen.Pages
{
    /// <summary>
    /// Collection of points used by the temperature graph.
    /// </summary>
    public class TemperaturePointCollection : RingArray<TemperaturePoint>
    {
        private const int TOTAL_POINTS = 3000;

        public TemperaturePointCollection()
            : base(TOTAL_POINTS)
        {
        }
    }

    /// <summary>
    /// Point used in the temperature graph.
    /// </summary>
    public class TemperaturePoint
    {
        public int Count { get; set; }

        public double Pressure { get; set; }

        public TemperaturePoint(double Pressure, int count)
        {
            this.Count = count;
            this.Pressure = Pressure;
        }
    }

    /// <summary>
    /// Collection of points used by the PCR readings
    /// </summary>
    public class PCRPointCollection : RingArray<PCRPoint>
    {
        private const int TOTAL_POINTS = 3000;

        public PCRPointCollection()
            : base(TOTAL_POINTS)
        {
        }
    }

    /// <summary>
    /// A point used in the PCR graph.
    /// </summary>
    public class PCRPoint
    {
        public double Cycle { get; set; }

        public double Reading { get; set; }

        public PCRPoint(double readingIn, double nCycle)
        {
            Cycle = nCycle;
            Reading = readingIn;
        }
    }

    public class SliderPositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value == int.Parse((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? parameter : Binding.DoNothing;
        }
    }

    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class MainPage : Page, INotifyPropertyChanged
    {


        protected MainWindow _parentWnd;

        /// <summary>
        /// Instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected vmMain _vmMain;

        protected double _heaterTemp;

        protected double _PCRCyclerTemp;
        protected double _PCRCyclerTemp2;
        protected double _PCRCyclerSetPoint;

        public event PropertyChangedEventHandler PropertyChanged;

        protected TemperaturePointCollection graphTemps, graphTemps2;
        protected int graphTempsCount;

        protected PCRPointCollection graphPCR, graphPCR2, graphPCR3, graphPCR4;
        protected int graphPCRCycle;

        
        //m_data_x = new List<double>();
        //protected List<double> m_data_r2;
        //m_data_x = new List<double>();
        //protected List<double> m_data_r3;
        //m_data_x = new List<double>();
        //protected List<double> m_data_r4;
        //m_data_x = new List<double>();

        public MainPage(MainWindow parentWnd)
        {
            _parentWnd = parentWnd;
            _vmMain = new vmMain();
            DataContext = _vmMain;
            InitializeComponent();

            HelixGenModel model = ((HelixGen.App)App.Current).Model;

            // Initialize the temperature graph.

            graphTemps = new TemperaturePointCollection();
            graphTempsCount = 0;

            var ds = new EnumerableDataSource<TemperaturePoint>(graphTemps);
            ds.SetXMapping(x => (double)x.Count);
            ds.SetYMapping(y => y.Pressure);

            plotter.AddLineGraph(ds, Colors.Green, 1, "T1");
            plotter.LegendVisible = false;

            graphTemps2 = new TemperaturePointCollection();
            graphTempsCount = 0;

            var ds2 = new EnumerableDataSource<TemperaturePoint>(graphTemps2);
            ds2.SetXMapping(x => (double)x.Count);
            ds2.SetYMapping(y => y.Pressure);

            plotter.AddLineGraph(ds2, Colors.Red, 1, "T2");
           // plotter.LegendVisible = false;

            // Initialize the PCR graph.

            graphPCR = new PCRPointCollection();
            graphPCRCycle = 0;

            var dsPCR = new EnumerableDataSource<PCRPoint>(graphPCR);
            dsPCR.SetXMapping(x => (double)x.Cycle);
            dsPCR.SetYMapping(y => y.Reading);

            pcrplotter.AddLineGraph(dsPCR, Colors.Green, 1, "R1");
            pcrplotter.LegendVisible = false;

            graphPCR2 = new PCRPointCollection();
           // graphPCRCycle = 0;

            var dsPCR2 = new EnumerableDataSource<PCRPoint>(graphPCR2);
            dsPCR2.SetXMapping(x => (double)x.Cycle);
            dsPCR2.SetYMapping(y => y.Reading);

            pcrplotter.AddLineGraph(dsPCR2, Colors.Red, 1, "R2");
            //pcrplotter.LegendVisible = false;

            graphPCR3 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR3 = new EnumerableDataSource<PCRPoint>(graphPCR3);
            dsPCR3.SetXMapping(x => (double)x.Cycle);
            dsPCR3.SetYMapping(y => y.Reading);

            pcrplotter.AddLineGraph(dsPCR3, Colors.Blue, 1, "R3");

            graphPCR4 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR4 = new EnumerableDataSource<PCRPoint>(graphPCR4);
            dsPCR4.SetXMapping(x => (double)x.Cycle);
            dsPCR4.SetYMapping(y => y.Reading);

            pcrplotter.AddLineGraph(dsPCR4, Colors.Brown, 1, "R4");

            _vmMain.PropertyChanged += _vmMain_PropertyChanged;
            model.PCRCyclerTempChanged += Model_PCRCyclerTempChanged;
            model.PCRCyclerTempChanged2 += Model_PCRCyclerTempChanged2;

            model.evtPCRReadingsStarted += Model_evtPCRReadingsStarted;
            model.evtPCRReadingsTaken += Model_evtPCRReadingsTaken;
            //model.evtPCRReadingsTaken2 += Model_evtPCRReadingsTaken2;
            model.evtBeginAnalysis += model_evtBeginAnalysis;

            //protected List<double> m_data_r2;
            //m_data_r2 = new List<double>();
            //protected List<double> m_data_r3;
            //m_data_r3 = new List<double>();
            //protected List<double> m_data_r4;
            //m_data_r4 = new List<double>();

        }

       private void  model_evtBeginAnalysis()
        {
            GoToOptics_Button_Click(null,null);
        }

    private void Model_evtPCRReadingsTaken(double[,] readings)
        {
            //logger.Debug("Readings taken got the values; {0}", string.Join(",",readings));
            
            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;
            Model_PCRReadingTaken((-readings[1, 0] - 0));
            Model_PCRReadingTaken2((-readings[1, 1] - 0));
            Model_PCRReadingTaken3((-readings[1,2] - 0));
            Model_PCRReadingTaken4((-readings[1,3] - 0));
            /*
           Model_PCRReadingTaken((readings[3,3]-0));
            Model_PCRReadingTaken2((readings[4, 0] - 0));
            Model_PCRReadingTaken3((readings[4, 1] - 0));
            Model_PCRReadingTaken4((readings[4, 2] - 0));
            */
            /*
            Model_PCRReadingTaken((readings[2, 0] - 0));
            Model_PCRReadingTaken2((readings[2, 1] - 0));
            Model_PCRReadingTaken3((readings[2, 2] - 0));
            Model_PCRReadingTaken4((readings[2, 3] - 0));
            */
            /*
            Model_PCRReadingTaken(10 * (readings[2, 0] - theModel.optic_zero[0]));
            Model_PCRReadingTaken2(10 * (readings[2, 1] - theModel.optic_zero[1]));
            Model_PCRReadingTaken3(10 * (readings[2, 2] - theModel.optic_zero[2]));
            Model_PCRReadingTaken4(10 * (readings[2, 3] - theModel.optic_zero[3]));
            */
            //Model_PCRReadingTaken2
        }
        private void Model_evtPCRReadingsTaken2(double[,] readings)
        {
            //logger.Debug("Readings taken got the values; {0}", string.Join(",",readings));
            Model_PCRReadingTaken2(readings[2, 3]);
        }

        private void Model_evtPCRReadingsStarted()
        {
            graphPCR.Clear();
            graphPCR2.Clear(); graphPCR3.Clear(); graphPCR4.Clear();
            graphPCRCycle = 0;
        }
        private void Model_evtTempingsStarted()
        {
            graphTemps.Clear();
            graphTemps2.Clear();
            graphTempsCount = 0;
        }

        private void Model_PCRCyclerTempChanged(double temperature, double power, bool controlling, bool transitioning)
        {
                        plotter.Dispatcher.Invoke(
                            (Action)(() =>
                            {
                                graphTemps.Add(new TemperaturePoint(temperature, ++graphTempsCount));
                            }
                            )
                            );
        }
        private void Model_PCRCyclerTempChanged2(double temperature, double power, bool controlling, bool transitioning)
        {
            plotter.Dispatcher.Invoke(
                (Action)(() =>
                {
                    graphTemps2.Add(new TemperaturePoint(temperature, graphTempsCount));
                }
                )
                );
        }

        private void Model_PCRReadingTaken(double reading)
        {
            plotter.Dispatcher.Invoke(
                (Action)(() => 
                {
                    graphPCR.Add(new PCRPoint(reading, ++graphPCRCycle));
                }
                )
                );
        }
        private void Model_PCRReadingTaken2(double reading)
        {
            plotter.Dispatcher.Invoke(
                (Action)(() =>
                {
                    graphPCR2.Add(new PCRPoint(reading, graphPCRCycle));
                }
                )
                );
        }
        private void Model_PCRReadingTaken3(double reading)
        {
            plotter.Dispatcher.Invoke( 
                (Action)(() =>
                {
                    graphPCR3.Add(new PCRPoint(reading, graphPCRCycle));
                }
                )
                );
        }
        private void Model_PCRReadingTaken4(double reading)
        {
            plotter.Dispatcher.Invoke(
                (Action)(() =>
                {
                    graphPCR4.Add(new PCRPoint(reading , graphPCRCycle));
                }
                )
                );
        }

        private void Model_PCRReadingTaken1(double reading, double temp)
        {
            plotter.Dispatcher.Invoke(
                (Action)(() =>
                {
                    graphPCR.Add(new PCRPoint(reading, temp));
                }
                )
                );
        }
        private void Model_PCRReadingTaken21(double reading,double temp)
        {
            plotter.Dispatcher.Invoke(
                (Action)(() =>
                {
                    graphPCR2.Add(new PCRPoint(reading, temp));
                }
                )
                );
        }
        private void Model_PCRReadingTaken31(double reading, double temp)
        {
            plotter.Dispatcher.Invoke(
                (Action)(() =>
                {
                    graphPCR3.Add(new PCRPoint(reading, temp));
                }
                )
                );
        }
        private void Model_PCRReadingTaken41(double reading, double temp)
        {
            plotter.Dispatcher.Invoke(
                (Action)(() =>
                {
                    graphPCR4.Add(new PCRPoint(reading, temp));
                }
                )
                );
        }

        private void _vmMain_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "HeaterTemperature":
                    {
                        HeaterTemp = _vmMain.HeaterTemperature;

                        // Update the appropriate control.


                    }

                    break;
                case "PCRCyclerTempAtTemp":
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            if (_vmMain.PCRCyclerTempAtTemp)
                            {
                                tbPCRTemperature.Background = new SolidColorBrush(Colors.LightGreen);
                            }
                            else
                            {
                                tbPCRTemperature.Background = new SolidColorBrush(Colors.White);
                            }
                        });

                    }
                    break;

                case "PCRCyclerTempAtTemp2":
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            if (_vmMain.PCRCyclerTempAtTemp2)
                            {
                                tbPCRTemperature2.Background = new SolidColorBrush(Colors.LightGreen);
                            }
                            else
                            {
                                tbPCRTemperature2.Background = new SolidColorBrush(Colors.White);
                            }
                        });
                    }
                    break;

                case "PCRCyclerSetpoint":
                    {
                        PCRCyclerSetPoint = _vmMain.PCRCyclerSetpoint;
                    }
                    break;

                case "PCRCyclerTemp":
                    {
                        PCRCyclerTemp = _vmMain.PCRCyclerTemperature;
                    }
                    break;

                case "PCRCyclerTemp2":
                    {
                        PCRCyclerTemp2 = _vmMain.PCRCyclerTemperature2;
                    }
                    break;

                case "ProtocolExecutionLine":
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            StatusWnd.Text += "\r\n" + _vmMain.ProtocolExecutionLine;
                            StatusWnd.ScrollToEnd();
                        }
                        );
                    }
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

        private void Optics_Button_Click(object sender, RoutedEventArgs e)
        {
            // This is a temporary insertion to allow navigation to the optics page.

            _parentWnd.NavToOptics();
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public double HeaterTemp
        {
            get { return _heaterTemp; }
            set
            {
                _heaterTemp = value;
                NotifyPropertyChanged("HeaterTemp");
            }
        }

        public double PCRCyclerTemp
        {
            get { return _PCRCyclerTemp; }
            set
            {
                _PCRCyclerTemp = value;
                NotifyPropertyChanged("PCRCyclerTemp");
            }
        }

        public double PCRCyclerTemp2
        {
            get { return _PCRCyclerTemp2; }
            set
            {
                _PCRCyclerTemp = value;
                NotifyPropertyChanged("PCRCyclerTemp2");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;
            Model_evtPCRReadingsStarted();
            theModel.Analysis();
            for (int i = 0; i < theModel._slope.Count/4 ; i++)
            {
                //Model_PCRCyclerTempChanged(theModel._slope.ElementAt(i),0,false,false);
                Model_PCRReadingTaken1(10000*(theModel._slope.ElementAt(i*4+0)), theModel.m_data_t.ElementAt(i));
                Model_PCRReadingTaken21(10000 * (theModel._slope.ElementAt(i * 4 +1)), theModel.m_data_t.ElementAt(i));
                Model_PCRReadingTaken31(10000 * (theModel._slope.ElementAt(i * 4 +2)), theModel.m_data_t.ElementAt(i));
                Model_PCRReadingTaken41(10000 * (theModel._slope.ElementAt(i * 4 +3)), theModel.m_data_t.ElementAt(i));

            }
            //theModel.m_data_r1.Add(temp_data);
            //for (uint ui = 5; ui <theModel.m_data_t.Count-5 ; ui++)
            //{

            //}
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        public double PCRCyclerSetPoint
        {
            get { return _PCRCyclerSetPoint; }
            set
            {
                _PCRCyclerSetPoint = value;
                NotifyPropertyChanged("PCRCyclerSetPoint");

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    tbSetCyclerTemperature.Text = value.ToString();
                });
            }
        }

        private void GoToOptics_Button_Click(object sender, RoutedEventArgs e)
        {
            //_parentWnd.showOpticsPage();
            _parentWnd.showAnaPage();
        }
        private void GoToOptics_Button_Click1(object sender, RoutedEventArgs e)
        {
            //_parentWnd.showOpticsPage();
             _parentWnd.showSetUpPage();
            //System.Diagnostics.Process.Start("shutdown.exe", "/s /t 0");
        }

        private void tbHeaterTemperature_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void tbSetHeaterTemperature_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
