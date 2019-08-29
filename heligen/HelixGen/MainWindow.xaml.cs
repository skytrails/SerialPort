// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using HelixGen.Model;
using HelixGen.Pages;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace HelixGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The result of the initialization operation.
        /// </summary>
        protected Task<bool> inited;

        /// <summary>
        /// Create an instance of the logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        AnalysePage AnalysPage;
        MainPage _mainPage ;

        StartupPage startupPage;
        protected setUp setUpPage;
        IDInput idInputPage;
        loginPage Loginpage;
        RunResultPage runResultPage;

        public MainWindow()
        {
            inited = null;

            // The lines below fix an issue where the window was covering the taskbar
            // in release mode.

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;

            theModel.initializationProgress += TheModel_initializationProgress;

            // Show the version.

            Version thisVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            tbVersion.Text = string.Format("CopyRight To Helixgen,Version: {0}.{1}", 
                thisVersion.Major.ToString(),
                thisVersion.Minor.ToString());

            // showMainPage();
            //AnalysPage = new AnalysePage(this);

            startupPage = new StartupPage(theModel);
            
            setUpPage = new setUp(this);
            idInputPage = new IDInput(this);

            Loginpage = new loginPage(this);

            AnalysPage = new AnalysePage(this);
            runResultPage = new RunResultPage(this);
            //_mainPage = new MainPage(this);

            NavFrame.Navigate(Loginpage);

            /*
            StartupPage startupPage = new StartupPage(theModel);

            NavFrame.Navigate(startupPage);
            Thread.Sleep(0);

            NavFrame.LoadCompleted += NavFrame_Startup_Navigated;
            */

#if false
            OpticsControlPage opticsPage = new OpticsControlPage();
            NavFrame.Navigate(opticsPage);

            opticsPage.Initialize();
#endif
#if false
            Thread.Sleep(10000);

            Task.Factory.StartNew(() => {
                Application.Current.Dispatcher.Invoke(
                    delegate { showMainPage(); }
                    );
                
            }
                );
#endif
        }

        public void Startup()
        {
            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;
            theModel.initializationProgress += TheModel_initializationProgress;
            NavFrame.Navigate(startupPage);
            Thread.Sleep(0);

            NavFrame.LoadCompleted += NavFrame_Startup_Navigated;
        }

        public void NavToOptics()
        {
            OpticsControlPage opticsPage = new OpticsControlPage();
            NavFrame.Navigate(opticsPage);
        }

        private void TheModel_initializationProgress(object sender, EventArgs e)
        {
            // We're waiting for initialization to complete.

            HelixGen.Model.progressEventArgs progressArgs = (HelixGen.Model.progressEventArgs)e;

            // Update the various controls.

            try
            {
                if (progressArgs.completed)
                {
                    NavFrame.LoadCompleted -= NavFrame_Startup_Navigated;
                    NavFrame.LoadCompleted += NavFrame_LoadCompleted;

                    // The code below is so the Navigation action is on the
                    // thread of the GUI.

                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {

                         _mainPage = new MainPage(this);
                        // NavFrame.Navigate(_mainPage);
                        showSetUpPage();


                    }
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("TheModel_initializationProgress: Caught an exception; {0}.\r\n",
                    ex.Message));
            }
        }

        private void NavFrame_LoadCompleted(object sender, NavigationEventArgs e)
        {
        }

        private async void NavFrame_Startup_Navigated(object sender, NavigationEventArgs e)
        {
            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;

            Task.Run(() =>
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                theModel.Initialize();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            });
            //inited = theModel.Initialize();

            //bool binited = await inited;
        }
        /*
        public void showMainPage()
        {
            // Sleep for a short while.


            NavFrame.Navigate(new MainPage(this));
        }
        */

        public void showSetUpPage()
        {
            NavFrame.Navigate(setUpPage);
        }
       

        public void showIDInputPage(char channel)
        {

            NavFrame.Navigate(idInputPage);
            idInputPage.Channel = channel;
            idInputPage.chan1patientID.Text = "";
            idInputPage.chan1materID.Text = "";
        }

        public void showOpticsPage()
        {
            NavFrame.Navigate(new OpticsControlPage());
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;

            theModel.ShutDown();

            Close();

            theModel.Dispose();
            theModel = null;
        }
        public void showAnaPage()
        {
            
            NavFrame.Dispatcher.Invoke((Action)(()=> {
                AnalysPage = new AnalysePage(this);
                NavFrame.Navigate(AnalysPage);
            }));
            
        }
        public void showRunResultPage()
        {
            NavFrame.Navigate(_mainPage);
        }

        public setUp GetSetupPage
        {

            get { return setUpPage; }
        }
        public AnalysePage GetAnaPage
        {

            get { return AnalysPage; }
        }

        public RunResultPage GetResPage
        {

            get { return runResultPage; }
        }

        public void showRunResultPage1()
        {
            NavFrame.Navigate(runResultPage);
        }
    }
}
