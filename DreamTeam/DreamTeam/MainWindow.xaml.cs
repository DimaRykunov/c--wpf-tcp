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
using System.Net.Sockets;
using System.Net;

namespace DreamTeam
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected string ip="";
        protected int port = 0;
            static TcpClient client;
        static NetworkStream stream;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ip = TextBoxIP.Text;
            port = Convert.ToInt32(TextBoxPort.Text);
            client = new TcpClient();
            try
            {
                client.Connect(ip, port);
                stream = client.GetStream();
                string login = textBoxLogin.Text;
                string password = textBoxPassword.Text;
                string message = String.Concat(";;;authentication/" + login + "/" + password + "/");
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

                if (message.StartsWith(";;;check"))
                {
                    string[] check = { "" };
                    check = message.Split('/');
                    if (check[1] == "false")
                        MessageBox.Show("Такого пользователя не существует");
                    if (check[1] == "true" && check[2] == "false")
                        MessageBox.Show("Неверный пароль");
                    if (check[1] == "true" && check[2] == "true")
                    {
                        Chat form = new Chat(TextBoxIP.Text, Convert.ToInt32(TextBoxPort.Text), textBoxLogin.Text,this);
                        this.Hide();
                        form.ShowDialog();
                        
                    }
                    


                }
            }
            catch(Exception ex)
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
            Registration r = new Registration(TextBoxIP.Text, Convert.ToInt32(TextBoxPort.Text), this);
            this.Hide();
            r.ShowDialog();
        }
    }
}
