// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using HelixGen.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace HelixGen.ViewModel
{
    public class RunScriptCmd : ICommand
    {
        protected vmMain _theVm;

        public RunScriptCmd(vmMain theVM)
        {
            _theVm = theVM;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add
            {
                //throw new NotImplementedException();
            }

            remove
            {
                //throw new NotImplementedException();
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        void ICommand.Execute(object parameter)
        {
            HelixGenModel model = ((HelixGen.App)App.Current).Model;

            string protocolFile = Path.Combine(HelixGen.CSystem_Defns.strDefaultProtocolPath,
                _theVm.ProtocolFile + ".xml");

            Task.Run(() =>
            {
                model.RunScript(protocolFile);
            });
        }
    }

    public class StopScriptCmd : ICommand
    {
        protected vmMain _theVm;

        public StopScriptCmd(vmMain theVM)
        {
            _theVm = theVM;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add
            {
                //throw new NotImplementedException();
            }

            remove
            {
                //throw new NotImplementedException();
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        void ICommand.Execute(object parameter)
        {
            HelixGenModel model = ((HelixGen.App)App.Current).Model;

            Task.Run(() =>
            {
                model.StopScript();
            });
        }
    }

    public class PumpOn : ICommand
    {
        public PumpOn()
        {

        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add
            {
                //throw new NotImplementedException();
            }

            remove
            {
                //throw new NotImplementedException();
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        void ICommand.Execute(object parameter)
        {
            HelixGenModel model = ((HelixGen.App)App.Current).Model;

            model.PumpOn();
        }
    }

    public class SetPCRTemperature : ICommand
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        public SetPCRTemperature()
        {

        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add
            {
                //throw new NotImplementedException();
            }

            remove
            {
                //throw new NotImplementedException();
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        void ICommand.Execute(object parameter)
        {
            logger.Debug("Running the SetPCRTemperature command.  Temperature is {0}", (string)parameter);            

            HelixGenModel model = ((HelixGen.App)App.Current).Model;

            model.DevicePCRCycler.Temperature = double.Parse((string)parameter);
            //model.DevicePCRCycler.Temperature2 = double.Parse((string)parameter);
        }
    }

    public class SetHeaterTemperature : ICommand
    {
        public SetHeaterTemperature()
        {

        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add
            {
                //throw new NotImplementedException();
            }

            remove
            {
               // throw new NotImplementedException();
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        void ICommand.Execute(object parameter)
        {
            HelixGenModel model = ((HelixGen.App)App.Current).Model;
            model.DeviceHeater.Temperature = double.Parse((string)parameter);
            //model.DevicePCRCycler.Temperature = double.Parse((string)parameter);
        }
    }

    public class vmMain : INotifyPropertyChanged
    {
        protected HelixGenModel _model;

        /// <summary>
        /// List of the protocol files.
        /// </summary>
        protected List<string> _protocolFiles;

        protected string _protocolExecutionLine;

        protected bool _bPumpOn;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The temperature of the heater.
        /// </summary>
        /// <remarks>
        /// This is updated in response to events from the model and 
        /// reflects the latest known temperature.
        /// </remarks>
        protected double _heaterTemp;

        /// <summary>
        /// The temperature of the PCR cycler.
        /// </summary>
        /// <remarks>
        /// This is updated in response to events from the model and 
        /// reflects the latest known temperature.
        /// </remarks>
        protected double _pcrCyclerTemp;
        protected double _pcrCyclerTemp2;

        protected bool _pcrCyclerTransitioning;
        protected bool _pcrCyclerTransitioning2;
        protected bool _pcrCyclerControlling;
        protected bool _pcrCyclerControlling2;

        protected double _pcrCyclerSetPoint;


        /// <summary>
        /// The name of the protocol file to execute.
        /// </summary>
        protected string _protocolFile;

        protected SetHeaterTemperature _setHeaterTemperatureCmd;
        protected SetPCRTemperature _setPCRTemperatureCmd;
        protected RunScriptCmd _runScriptCmd;
        protected StopScriptCmd _stopScriptCmd;
        protected PumpOn _pumpOn;

        /// <summary>
        /// Instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public vmMain()
        {
            _setHeaterTemperatureCmd = new SetHeaterTemperature();
            _setPCRTemperatureCmd = new SetPCRTemperature();
            _runScriptCmd = new RunScriptCmd(this);
            _stopScriptCmd = new StopScriptCmd(this);
            _pumpOn = new PumpOn();

            // Get the model.

            _model = ((HelixGen.App)App.Current).Model;

            // Start a process to update the list of the protocol files.

            Task.Run(() =>
                {
                    // Form the directory name.

                    string protocolsDir = Path.Combine(HelixGen.CSystem_Defns.strDefaultProtocolPath);
                    string[] filenames = Directory.GetFiles(protocolsDir, "*.xml");

                    // Raffle through the list and reform the names to only include the actual file names.

                    _protocolFiles = new List<string>();

                    foreach(string fn in filenames)
                    {
                        _protocolFiles.Add(Path.GetFileNameWithoutExtension(fn));
                    }
                }

            );

            // Subscribe to temperature updates

            _model.HeaterTempChanged += _model_HeaterTempChanged;
            _model.PCRCyclerTempChanged += _model_PCRCyclerTempChanged;
            _model.PCRCyclerTempChanged2 += _model_PCRCyclerTempChanged2;
            _model.evtPCRCyclerSetPointTemperature += _model_evtPCRCyclerSetPointTemperature;

            // Subscribe to position updates from the various piston pumps.

            _model.DeviceChassisPiston.PumpPositionChanged += DeviceChassisPiston_PumpPositionChanged;
            _model.DeviceHeaterPiston.PumpPositionChanged += DeviceHeaterPiston_PumpPositionChanged;
            _model.DeviceOpticsMotor.PumpPositionChanged += DeviceOpticsMotor_PumpPositionChanged;
            _model.DeviceR1Piston.PumpPositionChanged += DeviceR1Piston_PumpPositionChanged;
            _model.DeviceR2Piston.PumpPositionChanged += DeviceR2Piston_PumpPositionChanged;
            _model.DeviceSlider.PumpPositionChanged += DeviceSlider_PumpPositionChanged;

            // Subscribe to protocol execution updates.

            _model.ProtocolExecutionStepped += _model_ProtocolExecutionStepped;
        }

        private void DeviceSlider_PumpPositionChanged(int position)
        {
            SliderPos = position;
        }

        private void DeviceR2Piston_PumpPositionChanged(int position)
        {
            R2PistonPos = position;
        }

        private void DeviceR1Piston_PumpPositionChanged(int position)
        {
            R1PistonPos = position;
        }

        private void DeviceOpticsMotor_PumpPositionChanged(int position)
        {
            OpticsMotorPos = position;
        }

        private void DeviceHeaterPiston_PumpPositionChanged(int position)
        {
            HeaterPistonPos = position;
        }

        private void DeviceChassisPiston_PumpPositionChanged(int position)
        {
            ChassisPistonPos = position;
        }

        private void _model_ProtocolExecutionStepped(string desc)
        {
            ProtocolExecutionLine = desc;
        }

        private void _model_PCRCyclerTempChanged(double temperature, double power, bool controlling, bool transitioning)
        {
#if false
            logger.Debug("Got a _model_PCRCyclerTempChanged; temperature: {0} power: {1}", 
                temperature.ToString(),
                power.ToString());
#endif

            PCRCyclerTemperature = temperature;
            _pcrCyclerControlling = controlling;
            _pcrCyclerTransitioning = transitioning;
            PCRCyclerTempAtTemp = (controlling & !transitioning);
        }

        private void _model_PCRCyclerTempChanged2(double temperature, double power, bool controlling, bool transitioning)
        {
#if false
            logger.Debug("Got a _model_PCRCyclerTempChanged2; temperature: {0} power: {1}",
                temperature.ToString(),
                power.ToString());
#endif

            PCRCyclerTemperature2 = temperature;
            _pcrCyclerControlling2 = controlling;
            _pcrCyclerTransitioning2 = transitioning;
            PCRCyclerTempAtTemp2 = (controlling & !transitioning);
        }

        private void _model_evtPCRCyclerSetPointTemperature(double temperature)
        {
            logger.Debug("Got a _model_PCRCyclerSetpointTemperature; temperature: {0}",
                 temperature.ToString());

            PCRCyclerSetpoint = temperature;
        }

        private void _model_HeaterTempChanged(double temperature)
        {
#if false
            logger.Debug("Got a _model_HeaterTempChanged; {0}", temperature.ToString());
#endif
            HeaterTemperature = temperature;
        }

        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

#region properties
        /// <summary>
        /// The list of protocol file names.
        /// </summary>
        public List<string> ProtocolFiles
        {
            get { return _protocolFiles; }
            set { _protocolFiles = value; }
        }

        /// <summary>
        /// The list of protocol file names.
        /// </summary>
        public string ProtocolExecutionLine
        {
            get { return _protocolExecutionLine; }
            set { _protocolExecutionLine = value;
                NotifyPropertyChanged("ProtocolExecutionLine");
            }
        }

        /// <summary>
        /// The list of protocol file names.
        /// </summary>
        public bool PumpStatus
        {
            get {
                if (_model.DevicePump != null)
                {

                    return _model.DevicePump.dispense;
                }
                else
                    return false;
            }
            set {
                _model.DevicePump.dispense = value;
                NotifyPropertyChanged("PumpStatus");
            }
        }

        public bool bPumpOn
        {
            get
            {
                return _bPumpOn;
            }

            set
            {
                _bPumpOn = value;
                NotifyPropertyChanged("bPumpOn");
            }
        }

        /// <summary>
        /// The current position of the slider.
        /// </summary>
        public int SliderPos
        {
            get {
                if (_model.DeviceSlider != null)
                {
                    return _model.DeviceSlider.Position;
                }
                else
                {
                    return 0;
                }
            }
            set {
                _model.DeviceSlider.Position = value;
                NotifyPropertyChanged("SliderPos");
            }
        }

        /// <summary>
        /// The current position of the Optics Motor.
        /// </summary>
        public int OpticsMotorPos
        {
            get
            {
                if (_model.DeviceOpticsMotor != null)
                {
                    return _model.DeviceOpticsMotor.Position;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                _model.DeviceOpticsMotor.Position = value;
                NotifyPropertyChanged("OpticsMotorPos");
            }
        }

        /// <summary>
        /// The current position of the R1 Piston
        /// </summary>
        public int R1PistonPos
        {
            get
            {
                if (_model.DeviceR1Piston != null)
                {
                    return _model.DeviceR1Piston.Position;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                _model.DeviceR1Piston.Position = value;
                NotifyPropertyChanged("R1PistonPos");
            }
        }

        public int R2PistonPos
        {
            get
            {
                if (_model.DeviceR2Piston != null)
                {
                    return _model.DeviceR2Piston.Position;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                _model.DeviceR2Piston.Position = value;
                NotifyPropertyChanged("R2PistonPos");
            }
        }

        public int HeaterPistonPos
        {
            get
            {
                if (_model.DeviceR2Piston != null)
                {
                    return _model.DeviceHeaterPiston.Position;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                _model.DeviceHeaterPiston.Position = value;
                NotifyPropertyChanged("HeaterPistonPos");
            }
        }

        public int ChassisPistonPos
        {
            get
            {
                if (_model.DeviceR2Piston != null)
                {
                    return _model.DeviceChassisPiston.Position;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                _model.DeviceChassisPiston.Position = value;
                NotifyPropertyChanged("ChassisPistonPos");
            }
        }

        public bool PCRCyclerTempAtTemp
        {
            set
            {
                NotifyPropertyChanged("PCRCyclerTempAtTemp");
            }
            get { return _pcrCyclerControlling & !_pcrCyclerTransitioning; }
        }

        public bool PCRCyclerTempAtTemp2
        {
            set
            {
                NotifyPropertyChanged("PCRCyclerTempAtTemp2");
            }
            get { return _pcrCyclerControlling2 & !_pcrCyclerTransitioning2; }
        }


        public double PCRCyclerTemperature
        {
            set {
                _pcrCyclerTemp = value;
                NotifyPropertyChanged("PCRCyclerTemperature");
            }
            get { return _pcrCyclerTemp; }
        }

        public double PCRCyclerTemperature2
        {
            set
            {
                _pcrCyclerTemp2 = value;
                NotifyPropertyChanged("PCRCyclerTemperature2");
            }
            get { return _pcrCyclerTemp2; }
        }

        public double PCRCyclerSetpoint
        {
            set
            {
                _pcrCyclerSetPoint = value;
                NotifyPropertyChanged("PCRCyclerSetpoint");
            }
            get { return _pcrCyclerSetPoint; }
        }

        public double HeaterTemperature
        {
            set
            {
                _heaterTemp = value;
                NotifyPropertyChanged("HeaterTemperature");
            }
            get { return _heaterTemp; }
        }

        public string ProtocolFile
        {
            get { return _protocolFile; }
            set {
                _protocolFile = value;
                NotifyPropertyChanged("ProtocolFile");
            }
        }

#endregion


        public ICommand setHeaterTemperatureCmd
        {
            get { return _setHeaterTemperatureCmd; }
        }

        public ICommand setPCRTemperatureCmd
        {
            get { return _setPCRTemperatureCmd; }
        }

        public ICommand runScriptCmd
        {
            get { return _runScriptCmd; }
        }

        public ICommand stopScriptCmd
        {
            get { return _stopScriptCmd; }
        }

        public ICommand pumpOnCmd
        {
            get { return _pumpOn; }
        }

    }
}
