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

using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Common;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using HelixGen.Model;
using System.Windows.Forms;

namespace HelixGen.Pages
{
    /// <summary>
    /// AnalysePage.xaml 的交互逻辑
    /// </summary>
    /// 



    public partial class AnalysePage : Page
    {
        protected TemperaturePointCollection graphTemps, graphTemps2;
        protected int graphTempsCount;

        protected PCRPointCollection graphPCR, graphPCR2, graphPCR3, graphPCR4, graphPCR5, graphPCR6, graphPCR7, graphPCR8;
        protected int graphPCRCycle;

        protected MainWindow _parentWnd;

        HelixGenModel theModel;
        public AnalysePage(MainWindow parentWnd)
        {
            _parentWnd = parentWnd;
            InitializeComponent();

            // Initialize the temperature graph.

            graphTemps = new TemperaturePointCollection();
            graphTempsCount = 0;

            var ds = new EnumerableDataSource<TemperaturePoint>(graphTemps);
            ds.SetXMapping(x => (double)x.Count);
            ds.SetYMapping(y => y.Pressure);

            //plotter.AddLineGraph(ds, Colors.Green, 1, "T1");
            //plotter.LegendVisible = false;
            /*
            graphTemps2 = new TemperaturePointCollection();
            graphTempsCount = 0;

            var ds2 = new EnumerableDataSource<TemperaturePoint>(graphTemps2);
            ds2.SetXMapping(x => (double)x.Count);
            ds2.SetYMapping(y => y.Pressure);

            plotter.AddLineGraph(ds2, Colors.Red, 1, "T2");
            // plotter.LegendVisible = false;*/

            // Initialize the PCR graph.

            graphPCR = new PCRPointCollection();
            graphPCRCycle = 0;

            var dsPCR = new EnumerableDataSource<PCRPoint>(graphPCR);
            dsPCR.SetXMapping(x => (double)x.Cycle);
            dsPCR.SetYMapping(y => y.Reading);

            pcrplotter.AddLineGraph(dsPCR, Colors.Green, 1, " ");
            pcrplotter.LegendVisible = false;

            graphPCR2 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR2 = new EnumerableDataSource<PCRPoint>(graphPCR2);
            dsPCR2.SetXMapping(x => (double)x.Cycle);
            dsPCR2.SetYMapping(y => y.Reading);

            pcrplotter.AddLineGraph(dsPCR2, Colors.Red, 1, " ");
            //pcrplotter.LegendVisible = false;

            graphPCR3 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR3 = new EnumerableDataSource<PCRPoint>(graphPCR3);
            dsPCR3.SetXMapping(x => (double)x.Cycle);
            dsPCR3.SetYMapping(y => y.Reading);

            pcrplotter.AddLineGraph(dsPCR3, Colors.Blue, 1, " ");

            graphPCR4 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR4 = new EnumerableDataSource<PCRPoint>(graphPCR4);
            dsPCR4.SetXMapping(x => (double)x.Cycle);
            dsPCR4.SetYMapping(y => y.Reading);

            pcrplotter.AddLineGraph(dsPCR4, Colors.Brown, 1, " ");


            graphPCR5 = new PCRPointCollection();
            graphPCRCycle = 0;

            var dsPCR5 = new EnumerableDataSource<PCRPoint>(graphPCR5);
            dsPCR5.SetXMapping(x => (double)x.Cycle);
            dsPCR5.SetYMapping(y => y.Reading);

            pcrplotter3.AddLineGraph(dsPCR5, Colors.Green, 1, " ");
            pcrplotter3.LegendVisible = false;

            graphPCR6 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR6 = new EnumerableDataSource<PCRPoint>(graphPCR6);
            dsPCR6.SetXMapping(x => (double)x.Cycle);
            dsPCR6.SetYMapping(y => y.Reading);

            pcrplotter3.AddLineGraph(dsPCR6, Colors.Red, 1, " ");
            //pcrplotter.LegendVisible = false;

            graphPCR7 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR7 = new EnumerableDataSource<PCRPoint>(graphPCR7);
            dsPCR7.SetXMapping(x => (double)x.Cycle);
            dsPCR7.SetYMapping(y => y.Reading);

            pcrplotter3.AddLineGraph(dsPCR7, Colors.Blue, 1, " ");

            graphPCR8 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR8 = new EnumerableDataSource<PCRPoint>(graphPCR8);
            dsPCR8.SetXMapping(x => (double)x.Cycle);
            dsPCR8.SetYMapping(y => y.Reading);

            pcrplotter3.AddLineGraph(dsPCR8, Colors.Brown, 1, " ");

            graphPCR = new PCRPointCollection();
            graphPCRCycle = 0;

            var dsPCR9 = new EnumerableDataSource<PCRPoint>(graphPCR);
            dsPCR.SetXMapping(x => (double)x.Cycle);
            dsPCR.SetYMapping(y => y.Reading);

            pcrplotter1.AddLineGraph(dsPCR, Colors.Green, 1, " ");
            pcrplotter1.LegendVisible = false;

            graphPCR2 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR10 = new EnumerableDataSource<PCRPoint>(graphPCR2);
            dsPCR2.SetXMapping(x => (double)x.Cycle);
            dsPCR2.SetYMapping(y => y.Reading);

            pcrplotter1.AddLineGraph(dsPCR2, Colors.Red, 1, " ");
            //pcrplotter.LegendVisible = false;

            graphPCR3 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR11 = new EnumerableDataSource<PCRPoint>(graphPCR3);
            dsPCR3.SetXMapping(x => (double)x.Cycle);
            dsPCR3.SetYMapping(y => y.Reading);

            pcrplotter1.AddLineGraph(dsPCR3, Colors.Blue, 1, " ");

            graphPCR4 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR12 = new EnumerableDataSource<PCRPoint>(graphPCR4);
            dsPCR4.SetXMapping(x => (double)x.Cycle);
            dsPCR4.SetYMapping(y => y.Reading);

            pcrplotter1.AddLineGraph(dsPCR4, Colors.Brown, 1, " ");


            graphPCR = new PCRPointCollection();
            graphPCRCycle = 0;

            var dsPCR13 = new EnumerableDataSource<PCRPoint>(graphPCR);
            dsPCR.SetXMapping(x => (double)x.Cycle);
            dsPCR.SetYMapping(y => y.Reading);

            pcrplotter0.AddLineGraph(dsPCR, Colors.Green, 1, " ");
            pcrplotter0.LegendVisible = false;

            graphPCR2 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR14 = new EnumerableDataSource<PCRPoint>(graphPCR2);
            dsPCR2.SetXMapping(x => (double)x.Cycle);
            dsPCR2.SetYMapping(y => y.Reading);

            pcrplotter0.AddLineGraph(dsPCR2, Colors.Red, 1, " ");
            //pcrplotter.LegendVisible = false;

            graphPCR3 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR15 = new EnumerableDataSource<PCRPoint>(graphPCR3);
            dsPCR3.SetXMapping(x => (double)x.Cycle);
            dsPCR3.SetYMapping(y => y.Reading);

            pcrplotter0.AddLineGraph(dsPCR3, Colors.Blue, 1, " ");

            graphPCR4 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR16 = new EnumerableDataSource<PCRPoint>(graphPCR4);
            dsPCR4.SetXMapping(x => (double)x.Cycle);
            dsPCR4.SetYMapping(y => y.Reading);

            pcrplotter0.AddLineGraph(dsPCR4, Colors.Brown, 1, " ");


            graphPCR = new PCRPointCollection();
            graphPCRCycle = 0;

            var dsPCR17 = new EnumerableDataSource<PCRPoint>(graphPCR);
            dsPCR.SetXMapping(x => (double)x.Cycle);
            dsPCR.SetYMapping(y => y.Reading);

            pcrplotter4.AddLineGraph(dsPCR, Colors.Green, 1, " ");
            pcrplotter4.LegendVisible = false;

            graphPCR2 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR18 = new EnumerableDataSource<PCRPoint>(graphPCR2);
            dsPCR2.SetXMapping(x => (double)x.Cycle);
            dsPCR2.SetYMapping(y => y.Reading);

            pcrplotter4.AddLineGraph(dsPCR2, Colors.Red, 1, " ");
            //pcrplotter.LegendVisible = false;

            graphPCR3 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR19 = new EnumerableDataSource<PCRPoint>(graphPCR3);
            dsPCR3.SetXMapping(x => (double)x.Cycle);
            dsPCR3.SetYMapping(y => y.Reading);

            pcrplotter4.AddLineGraph(dsPCR3, Colors.Blue, 1, " ");

            graphPCR4 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR20 = new EnumerableDataSource<PCRPoint>(graphPCR4);
            dsPCR4.SetXMapping(x => (double)x.Cycle);
            dsPCR4.SetYMapping(y => y.Reading);

            pcrplotter4.AddLineGraph(dsPCR4, Colors.Brown, 1, " ");

            graphPCR = new PCRPointCollection();
            graphPCRCycle = 0;

            var dsPCR21 = new EnumerableDataSource<PCRPoint>(graphPCR);
            dsPCR.SetXMapping(x => (double)x.Cycle);
            dsPCR.SetYMapping(y => y.Reading);

            pcrplotter5.AddLineGraph(dsPCR, Colors.Green, 1, " ");
            pcrplotter5.LegendVisible = false;

            graphPCR2 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR22 = new EnumerableDataSource<PCRPoint>(graphPCR2);
            dsPCR2.SetXMapping(x => (double)x.Cycle);
            dsPCR2.SetYMapping(y => y.Reading);

            pcrplotter5.AddLineGraph(dsPCR2, Colors.Red, 1, " ");
            //pcrplotter.LegendVisible = false;

            graphPCR3 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR23 = new EnumerableDataSource<PCRPoint>(graphPCR3);
            dsPCR3.SetXMapping(x => (double)x.Cycle);
            dsPCR3.SetYMapping(y => y.Reading);

            pcrplotter5.AddLineGraph(dsPCR3, Colors.Blue, 1, " ");

            graphPCR4 = new PCRPointCollection();
            // graphPCRCycle = 0;

            var dsPCR24 = new EnumerableDataSource<PCRPoint>(graphPCR4);
            dsPCR4.SetXMapping(x => (double)x.Cycle);
            dsPCR4.SetYMapping(y => y.Reading);

            pcrplotter5.AddLineGraph(dsPCR4, Colors.Brown, 1, " ");

            
            //plotter.HorizontalAxisNavigation.Remove();
            //plotter.VerticalAxisNavigation.Remove();
            //plotter.Children.Remove(plotter.MouseNavigation);
            //pcrplotter.Children.Remove(pcrplotter.MouseNavigation); 

            theModel = ((HelixGen.App)(App.Current)).Model;
            
            Button_Click1(null, null);
        }

        private void Model_PCRCyclerTempChanged(double temperature)
        {
            graphTemps.Add(new TemperaturePoint(temperature, ++graphTempsCount));

            pcrplotter.Dispatcher.Invoke(
                (Action)(() =>
                {
                    graphTemps.Add(new TemperaturePoint(temperature, ++graphTempsCount));
                }
                )
                );
        }

        private void Model_PCRReadingTaken1(double reading, double temp)
        {
            pcrplotter.Dispatcher.Invoke(
                (Action)(() =>
                {
                    graphPCR.Add(new PCRPoint(reading, temp));
                }
                )
                );
        }
        private void Model_PCRReadingTaken21(double reading, double temp)
        {
            pcrplotter.Dispatcher.Invoke(
                (Action)(() =>
                {
                    graphPCR2.Add(new PCRPoint(reading, temp));
                }
                )
                );
        }
        private void Model_PCRReadingTaken31(double reading, double temp)
        {
            pcrplotter.Dispatcher.Invoke(
                (Action)(() =>
                {
                    graphPCR3.Add(new PCRPoint(reading, temp));
                }
                )
                );
        }

        private void Model_PCRReadingTaken41(double reading, double temp)
        {
            pcrplotter.Dispatcher.Invoke(
                (Action)(() =>
                {
                    graphPCR4.Add(new PCRPoint(reading, temp));
                }
                )
                );
        }

        private void Model_PCRReadingTaken12(double reading, double temp)
        {
            pcrplotter.Dispatcher.Invoke(
                (Action)(() =>
                {
                    graphPCR5.Add(new PCRPoint(reading, temp));
                }
                )
                );
        }
        private void Model_PCRReadingTaken22(double reading, double temp)
        {
            pcrplotter.Dispatcher.Invoke(
                (Action)(() =>
                {
                    graphPCR6.Add(new PCRPoint(reading, temp));
                }
                )
                );
        }
        private void Model_PCRReadingTaken32(double reading, double temp)
        {
            pcrplotter.Dispatcher.Invoke(
                (Action)(() =>
                {
                    graphPCR7.Add(new PCRPoint(reading, temp));
                }
                )
                );
        }

        private void Model_PCRReadingTaken42(double reading, double temp)
        {
            pcrplotter.Dispatcher.Invoke(
                (Action)(() =>
                {
                    graphPCR8.Add(new PCRPoint(reading, temp));
                }
                )
                );
        }

        private void Button_Click_SelectData(object sender, RoutedEventArgs e)
        {
            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;
            /*
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";//注意这里写路径时要用c:\\而不是c:\
            openFileDialog.Filter = "文本文件|*.*|C#文件|*.cs|所有文件|*.*";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string  fName = openFileDialog.FileName;
                //File fileOpen = new File(fName);
                //isFileHaveName = true;
                //richTextBox1.Text = fileOpen.ReadFile();
                //richTextBox1.AppendText("");
            }*/

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            dialog.SelectedPath = "C:\\HelixGen\\logs\\measurement";//注意这里写路径时要用c:\\而不是c:\
            //dialog.InitialDirectory = "c:\\";//注意这里写路径时要用c:\\而不是c:\

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                theModel.anaFilePath = dialog.SelectedPath;
                //string savePath = dialog.SelectedPath;
                //textBox2.Text = savePath;
            }
        }

        private void OnCheckedEnlarge(object sender, RoutedEventArgs e)
        {
            //int i = 0;
           // pcrplotter.HorizontalAxisNavigation.
            Model_evtPCRReadingsStarted();
            theModel.catrigeSelect = 0;
            runpartAnalysis(0);
            theModel.catrigeSelect = 1;
            runpartAnalysis(0);
        }

        private void Model_evtPCRReadingsStarted()
        {


            graphPCR.Clear();
            graphPCR2.Clear(); graphPCR3.Clear(); graphPCR4.Clear();
            graphPCR5.Clear();
            graphPCR6.Clear(); graphPCR7.Clear(); graphPCR8.Clear();
            graphPCRCycle = 0;


            pcrplotter.Children.RemoveAll(typeof(LineGraph));
            pcrplotter3.Children.RemoveAll(typeof(LineGraph));
            graphPCR = new PCRPointCollection();
            graphPCR2 = new PCRPointCollection();
            graphPCR3 = new PCRPointCollection();
            graphPCR4 = new PCRPointCollection();
            graphPCR5 = new PCRPointCollection();
            graphPCR6 = new PCRPointCollection();
            graphPCR7 = new PCRPointCollection();
            graphPCR8 = new PCRPointCollection();

            var dsPCR = new EnumerableDataSource<PCRPoint>(graphPCR);
            dsPCR.SetXMapping(x => (double)x.Cycle);
            dsPCR.SetYMapping(y => y.Reading);

            pcrplotter.AddLineGraph(dsPCR, Colors.Green, 1, " ");

            var dsPCR2 = new EnumerableDataSource<PCRPoint>(graphPCR2);
            dsPCR2.SetXMapping(x => (double)x.Cycle);
            dsPCR2.SetYMapping(y => y.Reading);

            pcrplotter.AddLineGraph(dsPCR2, Colors.Red, 1, " ");

            var dsPCR3 = new EnumerableDataSource<PCRPoint>(graphPCR3);
            dsPCR3.SetXMapping(x => (double)x.Cycle);
            dsPCR3.SetYMapping(y => y.Reading);

            pcrplotter.AddLineGraph(dsPCR3, Colors.Blue, 1, " ");

            var dsPCR4 = new EnumerableDataSource<PCRPoint>(graphPCR4);
            dsPCR4.SetXMapping(x => (double)x.Cycle);
            dsPCR4.SetYMapping(y => y.Reading);

            pcrplotter.AddLineGraph(dsPCR4, Colors.Brown, 1, " ");

            var dsPCR5 = new EnumerableDataSource<PCRPoint>(graphPCR5);
            dsPCR5.SetXMapping(x => (double)x.Cycle);
            dsPCR5.SetYMapping(y => y.Reading);

            pcrplotter3.AddLineGraph(dsPCR5, Colors.Green, 1, " ");

            var dsPCR6 = new EnumerableDataSource<PCRPoint>(graphPCR6);
            dsPCR6.SetXMapping(x => (double)x.Cycle);
            dsPCR6.SetYMapping(y => y.Reading);

            pcrplotter3.AddLineGraph(dsPCR6, Colors.Red, 1, " ");

            var dsPCR7 = new EnumerableDataSource<PCRPoint>(graphPCR7);
            dsPCR7.SetXMapping(x => (double)x.Cycle);
            dsPCR7.SetYMapping(y => y.Reading);

            pcrplotter3.AddLineGraph(dsPCR7, Colors.Blue, 1, " ");

            var dsPCR8 = new EnumerableDataSource<PCRPoint>(graphPCR8);
            dsPCR8.SetXMapping(x => (double)x.Cycle);
            dsPCR8.SetYMapping(y => y.Reading);

            pcrplotter3.AddLineGraph(dsPCR8, Colors.Brown, 1, " ");
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            //pcrplotter.HorizontalAxis. = "CYCLE";
            Model_evtPCRReadingsStarted();
            theModel.catrigeSelect = 0;
            runpartAnalysis(1);
            
            theModel.catrigeSelect = 1;
            runpartAnalysis(1);
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            Model_evtPCRReadingsStarted();
           // DisplayCurve(2);

            theModel.catrigeSelect = 0;
            runpartAnalysis(2);
            theModel.catrigeSelect = 1;
            runpartAnalysis(2);

        }


        private void Model_evtTempingsStarted()
        {
            graphTemps.Clear();
            //graphTemps2.Clear();
            graphTempsCount = 0;

            //plotter.Children.RemoveAll(typeof(LineGraph));

            graphTemps = new TemperaturePointCollection();
            graphTempsCount = 0;

            var ds = new EnumerableDataSource<TemperaturePoint>(graphTemps);
            ds.SetXMapping(x => (double)x.Count);
            ds.SetYMapping(y => y.Pressure);

           // plotter.AddLineGraph(ds, Colors.Green, 1, "T1");
           // plotter.LegendVisible = false;
        }

        private void catrigeButton_Checked_1(object sender, RoutedEventArgs e)
        {
            theModel.catrigeSelect = 0;
            runpartAnalysis(2);
        }

        private void catrigeButton_Checked_2(object sender, RoutedEventArgs e)
        {
            theModel.catrigeSelect = 1;
            runpartAnalysis(2);
        }

        private void catrigeButton_Checked_3(object sender, RoutedEventArgs e)
        {
            theModel.catrigeSelect = 2;
            runpartAnalysis(2);
        }

        private void catrigeButton_Checked_4(object sender, RoutedEventArgs e)
        {
            theModel.catrigeSelect = 3;
            runpartAnalysis(2);
        }

        private void catrigeButton_Checked_5(object sender, RoutedEventArgs e)
        {
            theModel.catrigeSelect = 4;
            runpartAnalysis(2);
        }

        private void catrigeButton_Checked_6(object sender, RoutedEventArgs e)
        {
            theModel.catrigeSelect = 5;
            runpartAnalysis(2);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _parentWnd.showRunResultPage();
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {

            // runAnalysis();
            Model_evtPCRReadingsStarted();
            theModel.catrigeSelect = 0;
            runpartAnalysis(2);
            theModel.catrigeSelect = 1;
            runpartAnalysis(2);



        }
        private void Button_ClickOpen(object sender, RoutedEventArgs e)
        {


            theModel.DeviceHeaterPiston.RNPosition = 45020;


        }
       
        private void Button_ClickOpen1(object sender, RoutedEventArgs e)
        {

            _parentWnd.GetResPage.chan1patientID.Text = chan1patientID.Text;
            _parentWnd.GetResPage.chan1checkItemID.Text = chan1haocaiID.Text;
            _parentWnd.GetResPage.checkResult.Text = " ";
            //theModel.DeviceHeaterPiston.RNPosition = 45020;
            _parentWnd.showRunResultPage1();


        }
        private void Button_ClickOpen2(object sender, RoutedEventArgs e)
        {
           // _parentWnd.GetResPage.chan1patientID.Text = chan2patientID.Text;
           // _parentWnd.GetResPage.chan1checkItemID.Text = chan2haocaiID.Text;
            _parentWnd.GetResPage.chan1patientID.Text = "000000";
            _parentWnd.GetResPage.chan1checkItemID.Text = "CYP2C19";
            if (theModel.checkres[1]==(int)1 )
                _parentWnd.GetResPage.checkResult.Text = "CYP2C19*2:  纯合野生型";
            else if (theModel.checkres[1] == (int)2)
                _parentWnd.GetResPage.checkResult.Text = "CYP2C19*2:  杂合突变型";
            else if (theModel.checkres[1] == (int)3)
                _parentWnd.GetResPage.checkResult.Text = "CYP2C19*2:  纯合突变型";
            else 
                _parentWnd.GetResPage.checkResult.Text = "CYP2C19*2: ";

            if (theModel.checkres1[1] == (int)1)
                _parentWnd.GetResPage.checkResult.Text += "\nCYP2C19*3:  纯合野生型";
            else if (theModel.checkres1[1] == (int)2)
                _parentWnd.GetResPage.checkResult.Text += "\nCYP2C19*3:  杂合突变型";
            else if (theModel.checkres1[1] == (int)3)
                _parentWnd.GetResPage.checkResult.Text += "\nCYP2C19*3:  纯合突变型";
            else
                _parentWnd.GetResPage.checkResult.Text += "\nCYP2C19*3: ";

            _parentWnd.showRunResultPage1();
            //theModel.DeviceHeaterPiston.RNPosition = 45020;


        }
        private void Button_ClickOpen3(object sender, RoutedEventArgs e)
        {
            _parentWnd.GetResPage.chan1patientID.Text = chan3patientID.Text;
            _parentWnd.GetResPage.chan1checkItemID.Text = chan3haocaiID.Text;
            _parentWnd.GetResPage.checkResult.Text = " ";
            _parentWnd.showRunResultPage1();
            //theModel.DeviceHeaterPiston.RNPosition = 45020;


        }
        private void Button_ClickOpen4(object sender, RoutedEventArgs e)
        {
            // _parentWnd.GetResPage.chan1patientID.Text = chan4patientID.Text;
            // _parentWnd.GetResPage.chan1checkItemID.Text = chan4haocaiID.Text;
            _parentWnd.GetResPage.chan1patientID.Text = "123456";
            _parentWnd.GetResPage.chan1checkItemID.Text = "CYP2C19";
            if (theModel.checkres[3] == (int)1)
                _parentWnd.GetResPage.checkResult.Text = "CYP2C19*2:  纯合野生型";
            else if (theModel.checkres[3] == (int)2)
                _parentWnd.GetResPage.checkResult.Text = "CYP2C19*2:  杂合突变型";
            else if (theModel.checkres[3] == (int)3)
                _parentWnd.GetResPage.checkResult.Text = "CYP2C19*2:  纯合突变型";
            else
                _parentWnd.GetResPage.checkResult.Text = "CYP2C19*2: ";

            if (theModel.checkres1[3] == (int)1)
                _parentWnd.GetResPage.checkResult.Text += "\nCYP2C19*3:  纯合野生型";
            else if (theModel.checkres1[3] == (int)2)
                _parentWnd.GetResPage.checkResult.Text += "\nCYP2C19*3:  杂合突变型";
            else if (theModel.checkres1[3] == (int)3)
                _parentWnd.GetResPage.checkResult.Text += "\nCYP2C19*3:  纯合突变型";
            else
                _parentWnd.GetResPage.checkResult.Text += "\nCYP2C19*3: ";
            _parentWnd.showRunResultPage1();
            //theModel.DeviceHeaterPiston.RNPosition = 45020;


        }
        private void Button_ClickOpen5(object sender, RoutedEventArgs e)
        {
            _parentWnd.GetResPage.chan1patientID.Text = chan5patientID.Text;
            _parentWnd.GetResPage.chan1checkItemID.Text = chan5haocaiID.Text;
            _parentWnd.GetResPage.checkResult.Text = " ";
            _parentWnd.showRunResultPage1();
            // theModel.DeviceHeaterPiston.RNPosition = 45020;


        }
        private void Button_ClickOpen6(object sender, RoutedEventArgs e)
        {
            _parentWnd.GetResPage.chan1patientID.Text = chan6patientID.Text;
            _parentWnd.GetResPage.chan1checkItemID.Text = chan6haocaiID.Text;
            _parentWnd.GetResPage.checkResult.Text = " ";
            _parentWnd.showRunResultPage1();
            //theModel.DeviceHeaterPiston.RNPosition = 45020;


        }
        public void runAnalysis()
        {
            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;
            theModel.Analysis();
            //RadioButton_Checked_1(rongjie, new RoutedEventArgs());
            DisplayCurve(2);
            // rongjie. = true;
            Model_evtTempingsStarted();
            for (int i = 0; i < theModel.m_pcr_t.Count; i++)
            {
                Model_PCRCyclerTempChanged(theModel.m_pcr_t.ElementAt(i));

            }
        }
        public void runpartAnalysis(int sel)
        {
            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;
            theModel.Analysis();
            //RadioButton_Checked_1(rongjie, new RoutedEventArgs());

            DisplayCurve(sel);
            /*
            Model_evtTempingsStarted();
            for (int i = 0; i < theModel.m_pcr_t.Count; i++)
            {
                Model_PCRCyclerTempChanged(theModel.m_pcr_t.ElementAt(i));

            }*/
        }

        private void DisplayCurve(int selmode)
        {
            HelixGenModel theModel = ((HelixGen.App)(App.Current)).Model;

           

            switch (selmode)
            {
                case 0://扩增
                    
                    if (theModel.catrigeSelect == 0)
                    { 
                        for (int i = 0; i < theModel.m_data_pcr1.Count / 4; i++)
                        {
                            //Model_PCRCyclerTempChanged(theModel._slope.ElementAt(i),0,false,false);
                            Model_PCRReadingTaken1((theModel.m_data_pcr1.ElementAt(i * 4 + 0))*1, i + 2);
                            Model_PCRReadingTaken21((theModel.m_data_pcr1.ElementAt(i * 4 + 1))*1, i + 2);
                            Model_PCRReadingTaken31((theModel.m_data_pcr1.ElementAt(i * 4 + 2)) * 1, i + 2);
                            Model_PCRReadingTaken41((theModel.m_data_pcr1.ElementAt(i * 4 + 3)) * 1, i + 2);

                        }
                        /*
                        for (int i = theModel.m_data_pcr1.Count / 4; i < theModel.m_data_pcr1.Count / 1; i++)
                        {
                            //Model_PCRCyclerTempChanged(theModel._slope.ElementAt(i),0,false,false);
                            Model_PCRReadingTaken1((theModel.m_data_pcr1.ElementAt((theModel.m_data_pcr1.Count/4-1)*4 + 0)) * 1+0*i, i + 2);
                            Model_PCRReadingTaken21((theModel.m_data_pcr1.ElementAt((theModel.m_data_pcr1.Count / 4 - 1) * 4 + 1)) * 1 + 0 * i, i + 2);
                            Model_PCRReadingTaken31((theModel.m_data_pcr1.ElementAt((theModel.m_data_pcr1.Count / 4 - 1) * 4 + 2)) * 1 + 0 * i, i + 2);
                            Model_PCRReadingTaken41((theModel.m_data_pcr1.ElementAt((theModel.m_data_pcr1.Count / 4 - 1) * 4 + 3)) * 1 + 0 * i, i + 2);

                        }*/
                    }
                    else
                    { 
                        for (int i = 0; i < theModel.m_data_pcr1.Count / 4; i++)
                        {
                            //Model_PCRCyclerTempChanged(theModel._slope.ElementAt(i),0,false,false);
                            Model_PCRReadingTaken12((theModel.m_data_pcr1.ElementAt(i * 4 + 0)) * 1, i + 2);
                            Model_PCRReadingTaken22((theModel.m_data_pcr1.ElementAt(i * 4 + 1)) * 1, i + 2);
                            Model_PCRReadingTaken32((theModel.m_data_pcr1.ElementAt(i * 4 + 2)) * 1, i + 2);
                            Model_PCRReadingTaken42((theModel.m_data_pcr1.ElementAt(i * 4 + 3)) * 1, i + 2);

                        }
                        /*
                        for (int i = theModel.m_data_pcr1.Count / 4; i < theModel.m_data_pcr1.Count / 1; i++)
                        {
                            //Model_PCRCyclerTempChanged(theModel._slope.ElementAt(i),0,false,false);
                            Model_PCRReadingTaken12((theModel.m_data_pcr1.ElementAt((theModel.m_data_pcr1.Count / 4 - 1) * 4 + 0)) * 1 + 500 * (i- theModel.m_data_pcr1.Count / 4), i + 2);
                            Model_PCRReadingTaken22((theModel.m_data_pcr1.ElementAt((theModel.m_data_pcr1.Count / 4 - 1) * 4 + 1)) * 1 + 500 * (i - theModel.m_data_pcr1.Count / 4), i + 2);
                            Model_PCRReadingTaken32((theModel.m_data_pcr1.ElementAt((theModel.m_data_pcr1.Count / 4 - 1) * 4 + 2)) * 1 + 500 * (i - theModel.m_data_pcr1.Count / 4), i + 2);
                            Model_PCRReadingTaken42((theModel.m_data_pcr1.ElementAt((theModel.m_data_pcr1.Count / 4 - 1) * 4 + 3)) * 1 + 500 * (i - theModel.m_data_pcr1.Count / 4), i + 2);

                        }*/
                    }

                    break;

                case 1://溶解
                    if (theModel.catrigeSelect == 0)
                        for (int i = 0; i < theModel.m_data_r2.Count / 4; i++)
                    {
                        //Model_PCRReadingTaken1( (theModel.m_data_r2.ElementAt(i * 4 + 0)), 50 + 0.3125 * i);
                        Model_PCRReadingTaken1((theModel.m_data_r2.ElementAt(i * 4 + 0)), 45 + 0.195 * i);
                        Model_PCRReadingTaken21((theModel.m_data_r2.ElementAt(i * 4 + 1)), 45 + 0.195 * i);
                        Model_PCRReadingTaken31((theModel.m_data_r2.ElementAt(i * 4 + 2)), 45 + 0.195 * i);
                        Model_PCRReadingTaken41((theModel.m_data_r2.ElementAt(i * 4 + 3)), 45 + 0.195 * i);

                    }
                    else
                        for (int i = 0; i < theModel.m_data_r2.Count / 4; i++)
                        {
                            //Model_PCRReadingTaken1( (theModel.m_data_r2.ElementAt(i * 4 + 0)), 50 + 0.3125 * i);
                            Model_PCRReadingTaken12((theModel.m_data_r2.ElementAt(i * 4 + 0)), 45 + 0.195 * i);
                            Model_PCRReadingTaken22((theModel.m_data_r2.ElementAt(i * 4 + 1)), 45 + 0.195 * i);
                            Model_PCRReadingTaken32((theModel.m_data_r2.ElementAt(i * 4 + 2)), 45 + 0.195 * i);
                            Model_PCRReadingTaken42((theModel.m_data_r2.ElementAt(i * 4 + 3)), 45 + 0.195 * i);

                        }

                    /*
                    for (int i = 0; i < theModel.m_data_r1.Count / 4; i++)
                    {
                        //Model_PCRCyclerTempChanged(theModel._slope.ElementAt(i),0,false,false);
                        Model_PCRReadingTaken1((theModel.m_data_r1.ElementAt(i * 4 + 0)), 50 + 0.3125 * i);
                        Model_PCRReadingTaken21((theModel.m_data_r1.ElementAt(i * 4 + 1)), 50 + 0.3125 * i);
                        Model_PCRReadingTaken31((theModel.m_data_r1.ElementAt(i * 4 + 2)), 50 + 0.3125 * i);
                        Model_PCRReadingTaken41((theModel.m_data_r1.ElementAt(i * 4 + 3)), 50 + 0.3125 * i);

                    }*/
                    break;

                case 2://峰值
                    if(theModel.catrigeSelect == 0)
                    for (int i = 0; i < theModel._slope1.Count / 4; i++)
                    {
                            /*
                            Model_PCRReadingTaken1(10000 * (theModel._slope1.ElementAt(i * 4 + 0)), theModel.m_data_t.ElementAt(i));
                            Model_PCRReadingTaken21(10000 * (theModel._slope1.ElementAt(i * 4 + 1)), theModel.m_data_t.ElementAt(i));
                            Model_PCRReadingTaken31(10000 * (theModel._slope1.ElementAt(i * 4 + 2)), theModel.m_data_t.ElementAt(i));
                            Model_PCRReadingTaken41(10000 * (theModel._slope1.ElementAt(i * 4 + 3)), theModel.m_data_t.ElementAt(i));
                            */
                            /*
                            Model_PCRReadingTaken1((theModel._slope1.ElementAt(i * 4 + 0)), 41.5 + 0.17964 * i);
                            Model_PCRReadingTaken21((theModel._slope1.ElementAt(i * 4 + 1)), 41.5 + 0.17964 * i);
                            Model_PCRReadingTaken31((theModel._slope1.ElementAt(i * 4 + 2)), 41.5 + 0.17964 * i);
                            Model_PCRReadingTaken41((theModel._slope1.ElementAt(i * 4 + 3)), 41.5 + 0.17964 * i);
                            */
                            Model_PCRReadingTaken1((theModel._slope1.ElementAt(i * 4 + 0)), 50.4 + 0.15 * i);
                            Model_PCRReadingTaken21((theModel._slope1.ElementAt(i * 4 + 1)), 50.4 + 0.15 * i);
                            Model_PCRReadingTaken31((theModel._slope1.ElementAt(i * 4 + 2)), 50.4 + 0.15 * i);
                            Model_PCRReadingTaken41((theModel._slope1.ElementAt(i * 4 + 3)), 50.4 + 0.15 * i);

                        }
                    else
                        for (int i = 0; i < theModel._slope1.Count / 4; i++)
                        {
                            /*
                            Model_PCRReadingTaken1(10000 * (theModel._slope1.ElementAt(i * 4 + 0)), theModel.m_data_t.ElementAt(i));
                            Model_PCRReadingTaken21(10000 * (theModel._slope1.ElementAt(i * 4 + 1)), theModel.m_data_t.ElementAt(i));
                            Model_PCRReadingTaken31(10000 * (theModel._slope1.ElementAt(i * 4 + 2)), theModel.m_data_t.ElementAt(i));
                            Model_PCRReadingTaken41(10000 * (theModel._slope1.ElementAt(i * 4 + 3)), theModel.m_data_t.ElementAt(i));
                            */
                            /*
                            Model_PCRReadingTaken12((theModel._slope1.ElementAt(i * 4 + 0)), 41.5 + 0.17964 * i);
                            Model_PCRReadingTaken22((theModel._slope1.ElementAt(i * 4 + 1)), 41.5 + 0.17964 * i);
                            Model_PCRReadingTaken32((theModel._slope1.ElementAt(i * 4 + 2)), 41.5 + 0.17964 * i);
                            Model_PCRReadingTaken42((theModel._slope1.ElementAt(i * 4 + 3)), 41.5 + 0.17964 * i);*/
                            Model_PCRReadingTaken12((theModel._slope1.ElementAt(i * 4 + 0)), 48.5 + 0.15 * i);
                            Model_PCRReadingTaken22((theModel._slope1.ElementAt(i * 4 + 1)), 48.5 + 0.15 * i);
                            Model_PCRReadingTaken32((theModel._slope1.ElementAt(i * 4 + 2)), 48.5 + 0.15 * i);
                            Model_PCRReadingTaken42((theModel._slope1.ElementAt(i * 4 + 3)), 48.5 + 0.15 * i);

                        }


                    break;


                default:
                    break;
            }

        }



    }
}
