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
    /// RunResultPage.xaml 的交互逻辑
    /// </summary>
    public partial class RunResultPage : Page
    {
        protected MainWindow _parentWnd;
        public RunResultPage(MainWindow parentWnd)
        {
            _parentWnd = parentWnd;
            InitializeComponent();
            string[] array = { " ","小李", "Xiaowang", "MrSun", "Misszhao", "Missli" };
            
            // cbProtocols.ItemsSource = array;
            for (int i = 0; i < 6; i++)
            {

                cbProtocols.Items.Add(array[i]);
            }
            //cbProtocols.ItemsSource = array;
            //array = { "Red", "Green", "White", "Blue", "Yellow" };

            //cbProtocols.ItemsSource = { "xiaoli","xiaowang"};
            cbProtocols.SelectedIndex = 2;

            string[] array1 = { " ","男", "女" };

            // cbProtocols.ItemsSource = array;
            for (int i = 0; i < 3; i++)
            {

                chan1Sex.Items.Add(array1[i]);
            }
            //cbProtocols.ItemsSource = array;
            //array = { "Red", "Green", "White", "Blue", "Yellow" };

            //cbProtocols.ItemsSource = { "xiaoli","xiaowang"};
            chan1Sex.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _parentWnd.showAnaPage();
        }
        /*
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
            chan1Button.Background = Brushes.LawnGreen;
        }

        private void onMouseLeft(object sender, MouseEventArgs e)
        {
            chan1Button.Background = Brushes.LightBlue;

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _parentWnd.showAnaPage();
            _parentWnd.GetAnalyPage.runAnalysis();
            //runAnalysis();
        }
        */
    }
}
