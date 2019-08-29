// (c) Copyright 2017 HelixGen, All Rights Reserved.
// (c) Copyright 2017 Accel BioTech, All Rights Reserved.

using HelixGen.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HelixGen
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _Singleton_Instance_Lock;

        /// <summary>
        /// This is the global instance of the model.
        /// </summary>
        protected HelixGenModel _theModel;

        public App()
        {
            _theModel = new HelixGenModel();
        }


        private void Initialize()
        {
            //bool bInit = await _theModel.Initialize();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNewFlag;

            base.OnStartup(e);

            _Singleton_Instance_Lock = new Mutex(false, "com.helixgen_Instrument_GUI_AppLock", out createdNewFlag);
            if (createdNewFlag == false)
            {
                MessageBox.Show("Another instance is currently running. Aborting.", "HelixGen PCR Controller");
                Shutdown(1);
            }

            Initialize();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _Singleton_Instance_Lock.Close();   // Release the mutex.
        }

        /// <summary>
        /// Public read only accessor for the model.
        /// </summary>
        public HelixGenModel Model
        {
            get { return _theModel; }

        }
    }
}
