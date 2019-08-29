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

namespace HelixGen.Pages
{
    /// <summary>
    /// IDInput.xaml 的交互逻辑
    /// </summary>
    public partial class IDInput : Page
    {
        protected MainWindow _parentWnd;
        protected char channel;

        public IDInput(MainWindow parentWnd)
        {
            _parentWnd = parentWnd;
            InitializeComponent();
        }

        public char Channel
        {
            get { return this.channel; }
            set { this.channel = value; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _parentWnd.showSetUpPage();
            if(channel==1)
            { 
                _parentWnd.GetSetupPage.chan1patientID.Text = chan1patientID.Text;
                _parentWnd.GetSetupPage.chan1materID.Text = chan1materID.Text;
            }
            else if (channel == 2)
            {
                _parentWnd.GetSetupPage.chan2patientID.Text = chan1patientID.Text;
                _parentWnd.GetSetupPage.chan2materID.Text = chan1materID.Text;
            }
            else if (channel == 3)
            {
                _parentWnd.GetSetupPage.chan3patientID.Text = chan1patientID.Text;
                _parentWnd.GetSetupPage.chan3materID.Text = chan1materID.Text;
            }
            else if (channel == 4)
            {
                _parentWnd.GetSetupPage.chan4patientID.Text = chan1patientID.Text;
                _parentWnd.GetSetupPage.chan4materID.Text = chan1materID.Text;
            }
            else if (channel == 5)
            {
                _parentWnd.GetSetupPage.chan5patientID.Text = chan1patientID.Text;
                _parentWnd.GetSetupPage.chan5materID.Text = chan1materID.Text;
            }
            
            else if (channel == 6)
            {
                _parentWnd.GetSetupPage.chan6patientID.Text = chan1patientID.Text;
                _parentWnd.GetSetupPage.chan6materID.Text = chan1materID.Text;
            }
            


        }
    }
}
