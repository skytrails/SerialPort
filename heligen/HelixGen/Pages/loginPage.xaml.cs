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
    /// loginPage.xaml 的交互逻辑
    /// </summary>
    public partial class loginPage : Page
    {
        protected MainWindow _parentWnd;
        //protected string
        public loginPage(MainWindow parentWnd)
        {
            string[] array = { "小李", "Xiaowang", "MrSun", "Misszhao", "Missli" };
            _parentWnd = parentWnd;
            InitializeComponent();
            // cbProtocols.ItemsSource = array;
            for (int i = 0; i < 5; i++)
            {
               
                cbProtocols.Items.Add(array[i]);
            }
            //cbProtocols.ItemsSource = array;
            //array = { "Red", "Green", "White", "Blue", "Yellow" };

            //cbProtocols.ItemsSource = { "xiaoli","xiaowang"};
            cbProtocols.SelectedIndex = 2;


            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _parentWnd.Startup();
            //_parentWnd.IsLogined =true;
        }
    }
}
