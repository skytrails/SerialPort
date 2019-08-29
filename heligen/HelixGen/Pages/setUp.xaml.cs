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
using System.IO;
using HelixGen.Model;

namespace HelixGen.Pages
{
    /// <summary>
    /// setUp.xaml 的交互逻辑
    /// </summary>
    public partial class setUp : Page
    {
        protected MainWindow _parentWnd;
        protected List<string> _protocolFiles;
        protected string _protocolFile;
        public setUp(MainWindow parentWnd)
        {
            _parentWnd = parentWnd;
            InitializeComponent();
            /*
            string protocolsDir = System.IO.Path.Combine(HelixGen.CSystem_Defns.strDefaultProtocolPath);
            string[] filenames = Directory.GetFiles(protocolsDir, "*.xml");

            // Raffle through the list and reform the names to only include the actual file names.

            _protocolFiles = new List<string>();
            foreach (string fn in filenames)
            {
                _protocolFiles.Add(System.IO.Path.GetFileNameWithoutExtension(fn));
            }
            cbProtocols.ItemsSource = _protocolFiles;
            cbProtocols.SelectedIndex = 1;
            */
            //_protocolFile = cbProtocols.SelectedItem.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _parentWnd.showIDInputPage((char)1);

        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            _parentWnd.showIDInputPage((char)2);

        }
        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            _parentWnd.showIDInputPage((char)3);

        }
        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            _parentWnd.showIDInputPage((char)4);

        }
        private void Button_Click4(object sender, RoutedEventArgs e)
        {
            _parentWnd.showIDInputPage((char)5);

        }
        private void Button_Click5(object sender, RoutedEventArgs e)
        {
            _parentWnd.showIDInputPage((char)6);

        }
        /*
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _parentWnd.showRunPage();

            HelixGenModel model = ((HelixGen.App)App.Current).Model;

            string protocolFile = System.IO.Path.Combine(HelixGen.CSystem_Defns.strDefaultProtocolPath,
                _protocolFile + ".xml");

            Task.Run(() =>
            {
                model.RunScript(protocolFile);
            });
        }



        private void onProtocolSel(object sender, SelectionChangedEventArgs e)
        {
            _protocolFile = cbProtocols.SelectedItem.ToString();
        }
        */

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _parentWnd.showRunResultPage();
            _parentWnd.GetAnaPage.chan1patientID.Text = chan1materID.Text;
            _parentWnd.GetAnaPage.chan1haocaiID.Text = chan1patientID.Text;
            _parentWnd.GetAnaPage.chan2patientID.Text = chan2materID.Text;
            _parentWnd.GetAnaPage.chan2haocaiID.Text = chan2patientID.Text;
            _parentWnd.GetAnaPage.chan3patientID.Text = chan3materID.Text;
            _parentWnd.GetAnaPage.chan3haocaiID.Text = chan3patientID.Text;
            _parentWnd.GetAnaPage.chan4patientID.Text = chan4materID.Text;
            _parentWnd.GetAnaPage.chan4haocaiID.Text = chan4patientID.Text;
            _parentWnd.GetAnaPage.chan5patientID.Text = chan5materID.Text;
            _parentWnd.GetAnaPage.chan5haocaiID.Text = chan5patientID.Text;
            _parentWnd.GetAnaPage.chan6patientID.Text = chan6materID.Text;
            _parentWnd.GetAnaPage.chan6haocaiID.Text = chan6patientID.Text;
        }
    }
}
