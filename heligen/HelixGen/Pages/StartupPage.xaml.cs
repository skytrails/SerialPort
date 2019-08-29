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
using HelixGen.Model;
using NLog;

namespace HelixGen.Pages
{
    /// <summary>
    /// Arguments object sent as part of the startupProgress event.
    /// </summary>
    public class progressEventArgs : EventArgs
    {
        /// <summary>
        /// The percentage complete.
        /// </summary>
        public float percentageComplete { get; internal set;  }

        /// <summary>
        /// The message associated with the progress.
        /// </summary>
        public string message { get; internal set; }

        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public progressEventArgs(float percentageIn = 0.0f, string msgIn = "")
        {
            percentageComplete = percentageIn;
            message = msgIn;
        }
    }

    /// <summary>
    /// Interaction logic for Startup.xaml
    /// </summary>
    /// <remarks>
    /// The startup page is displayed while various initializations are
    /// taking place.
    /// </remarks>
    public partial class StartupPage : Page
    {
        /// <summary>
        /// Our reference to the model.
        /// </summary>
        protected HelixGenModel _model;


        public StartupPage(HelixGenModel modelIn)
        {
            _model = modelIn;

            this.Loaded += StartupPage_Loaded;

            InitializeComponent();

        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // Set the range for the progress bar.

            InitProgressBar.Minimum = 0;
            InitProgressBar.Maximum = 1.0;
            InitProgressBar.Value = 0;

            // Subscribe to the model's progress events.

            _model.initializationProgress += _model_initializationProgress;
        }

        private void StartupPage_Initialized(object sender, EventArgs e)
        {
            
        }

        private void StartupPage_Loaded(object sender, RoutedEventArgs e)
        {
 
        }

        private void _model_initializationProgress(object sender, EventArgs e)
        {
            HelixGen.Model.progressEventArgs progressArgs = (HelixGen.Model.progressEventArgs)e;

            // Update the various controls.

            try
            {
                // The code below is so the Navigation action is on the
                // thread of the GUI.

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    InitProgressBar.Value = (double)(progressArgs.percentageComplete);
                    ProgressMessage.Text = progressArgs.message;

                    Console.WriteLine(string.Format("_model_initializationProgress: Got the message; {0}.\r\n", ProgressMessage.Text
                        ));
                });
            }
            catch(Exception ex)
            {
                Console.WriteLine(string.Format("_model_initializationProgress: Caught an exception; {0}.\r\n",
                    ex.Message));
            }
        }
    }
}
