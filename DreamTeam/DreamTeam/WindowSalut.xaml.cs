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
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Baml2006;
using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace DreamTeam
{
    /// <summary>
    /// Логика взаимодействия для WindowSalut.xaml
    /// </summary>
    public partial class WindowSalut : Window
    {
        WriteableBitmap _bitmap;

       
        public WindowSalut()
        {
            InitializeComponent();
            Files = Directory.GetFiles(@"C:\Users\Dima\Documents\Visual Studio 2017\Projects\DreamTeam\Images");
            this.DataContext = this;
            lst.SelectedIndex = 0;
            timerStart();
        }
        public string[] Files
        { get; set; }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
        private DispatcherTimer timer = null;
        private int x;

        private void timerStart()
        {
            timer = new DispatcherTimer();  // если надо, то в скобках указываем приоритет, например DispatcherPriority.Render
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 2500);
            timer.Start();
        }

        private void timerTick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
