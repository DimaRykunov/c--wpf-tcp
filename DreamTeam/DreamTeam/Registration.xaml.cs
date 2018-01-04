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
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace DreamTeam
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        protected string ip = "";
        protected int port = 0;
        protected string login = "";
        static TcpClient client;
        static NetworkStream stream;
        MainWindow mw;
        public Registration(string ip, int port, MainWindow mw)
        {
            InitializeComponent();
            this.ip = ip;
            this.port = port;
            this.login = login;
            this.mw = mw;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxLogin.Text != "" && textBoxName.Text != "" && TextBoxurname.Text != "" && TextBoxPassword.Text != "")
            {
                client = new TcpClient();
                try
                {

                    client.Connect(ip, port);
                    stream = client.GetStream();
                    string message = String.Concat(";;;registration/" + textBoxLogin.Text + "/" + textBoxName.Text + "/" + TextBoxurname.Text + "/" + TextBoxPassword.Text + "/");
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                    ReceiveMessage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Disconnect();
                }
            }
                
        }
        //получение сообщения
        void ReceiveMessage()
        {
            try
            {
                byte[] data = new byte[64];
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, data.Length));
                }
                while (stream.DataAvailable);
                string message = builder.ToString();

                if (message.StartsWith(";;;registration"))
                {
                    string[] check = { "" };
                    check = message.Split('/');
                    if (check[1] == "false")
                        MessageBox.Show("Пользователь с таким логином уже существует", "",MessageBoxButton.OK,MessageBoxImage.Error);
                    else
                    {
                        MessageBox.Show("Пользователь " + textBoxLogin.Text + " зарегестирован", "",MessageBoxButton.OK, MessageBoxImage.Information);
                        mw.Show();
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Disconnect();
            }

        }

        //отключение
        static void Disconnect()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
            Environment.Exit(0);
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            mw.Show();
            this.Close();
        }
    }
}
