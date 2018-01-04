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
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Threading;

namespace DreamTeam
{
    /// <summary>
    /// Логика взаимодействия для Chat.xaml
    /// </summary>
    public class VisualHelper
    {
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
    public partial class Chat : Window
    {
        protected string ip = "";
        protected int port = 0;
        protected string login = "";
        protected Thread receiveThread;
        static TcpClient client;
        static NetworkStream stream;
        int selected = 0;
        string selectedUser = "";
        protected MainWindow mw;
        public Chat(string ip, int port, string login, MainWindow mw)
        {
            InitializeComponent();
            this.ip = ip;
            this.port = port;
            this.login = login;
            this.mw = mw;
            UserName.Text = login;
        }

        //отправка сообщений
        void SendMessage(string message)
        {
            try
            {
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        bool raz = true;
        //получение сообщений
        void ReceiveMessage()
        {
            if (raz)
            {
                raz = false;
                string message = ";;;getClient/" + login;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    stackPanelView.Children.Add(new TextBlock { Text = "Добро пожаловать", TextAlignment = (TextAlignment.Center) });
                });
            }
            while (true)
            {
                try
                {
                    byte[] data = new byte[256];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, data.Length));
                    }
                    while (stream.DataAvailable);
                    string message = builder.ToString();
                    //добавение клиентов в ComboBox1 для личных сообщений
                    #region
                    if (message.StartsWith(";;;list"))
                    {
                        //message.Remove(message.IndexOf("\0"));
                        string[] dat = { "" };
                        if (message.StartsWith(";;;"))
                            dat = message.Split('/');
                        var panels = new List<StackPanel>();
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate ()
                                {
                                    StackPanel SP = new StackPanel();
                                    SP.Orientation = Orientation.Horizontal;
                                    SP.Height = 35;
                                    SP.Width = 200;
                                    SP.Children.Add(new Image { Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/Images/user.png")) });
                                    SP.Children.Add(new TextBlock { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
                                    panels.Add(SP);
                                    SP.Children.Add(new TextBlock { Text = "Общий чат" });
                                });
                        if (dat.Length > 2)
                        {


                            for (int i = 1; i < dat.Length - 1; i++)
                            {
                                string tex = dat[i];
                                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate ()
                                {
                                    StackPanel SP = new StackPanel();
                                    SP.Orientation = Orientation.Horizontal;
                                    SP.Height = 35;
                                    SP.Width = 200;
                                    SP.Children.Add(new Image { Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/Images/user.png")) });
                                    SP.Children.Add(new TextBlock { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap });
                                    SP.Children.Add(new TextBlock { Text = tex });
                                    panels.Add(SP);

                                });
                            }
                        }
                        else MessageBox.Show("You are alone:( \nBut you can write to server ");
                        //panels.RemoveAt(0);
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate ()
                                {
                                    lbmain.ItemsSource = panels;
                                });
                        //this.Invoke((MethodInvoker)delegate ()
                        //{
                        //    if (kol > 0)
                        //    {
                        //        comboBox1.SelectedIndex = 0;
                        //        textBox3.Enabled = true;
                        //        buttonSendPM.Enabled = true;
                        //        comboBox1.Enabled = true;

                        //    }
                        //    else MessageBox.Show("В сети нет клиентов");
                        //});

                    }
                    if (message.StartsWith(";;;pm"))
                    {
                        string[] dat = { "" };
                        dat = message.Split('/');
                        if ((selectedUser == dat[2] && selectedUser != "" && selected >= 0))
                        {
                            dat = message.Split('/');
                            int position = message.IndexOf("\0");
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (ThreadStart)delegate ()
                            {
                                stackPanelView.Children.Add(new TextBlock { Text = dat[2] + ":" + dat[1] });
                                scrollR.ScrollToBottom();
                            });
                        }
                    }

                    if (message.StartsWith(";;;dialog"))
                    {
                        message = message.Remove(message.IndexOf("\0"));
                        double wid = scrollR.ActualWidth;
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (ThreadStart)delegate ()
                        {
                            stackPanelView.Children.Clear();
                        });
                        string[] dat = { "" };
                        if (message.StartsWith(";;;"))
                            dat = message.Split('|');
                        string[] user = dat[0].Split('/');
                        string[] dialog = dat[1].Split('/');
                        //0 2 - пользователь
                        //1 3 - сообщение
                        for (int i = 1; i < dialog.Length - 1; i++)
                        {
                            string[] three = dialog[i].Split('$');
                            string send = three[0];
                            string mess = three[1];
                            if (send == login)
                            {
                                string txt = "Вы: " + mess;
                                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate ()
                                {
                                    stackPanelView.Children.Add(new TextBlock { Text = txt, Background = new SolidColorBrush(Color.FromRgb(232, 246, 255)), Width = wid, TextWrapping = TextWrapping.Wrap });
                                    scrollR.ScrollToBottom();
                                });
                            }
                            else
                            {
                                string txt = send + ":" + mess;
                                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate ()
                                {
                                    stackPanelView.Children.Add(new TextBlock { Text = txt, Width = wid, TextWrapping = TextWrapping.Wrap });
                                    scrollR.ScrollToBottom();
                                });
                            }
                            
                        }

                    }
                    if (!message.StartsWith(";;;pm") && !message.StartsWith(";;;list") && !message.StartsWith(";;;dialog") && !message.StartsWith("\0"))
                    {
                        if (selected >= 0)
                        {
                            int position = message.IndexOf("\0");
                            if (selectedUser == "Общий чат" && selectedUser != "")
                            {
                                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (ThreadStart)delegate ()
                            {
                                stackPanelView.Children.Add(new TextBlock { Text = message.Remove(position) });
                                scrollR.ScrollToBottom();
                            });
                            }
                        }
                    }
                    #endregion


                    //}
                    //if (!message.StartsWith(";;;pm") && !message.StartsWith(";;;list") && !message.StartsWith(";;;fileTypes") && !message.StartsWith(";;;success") && !message.StartsWith(";;;MessageStat") && !message.StartsWith(";;;FileStat"))
                    //{
                    //    this.Invoke((MethodInvoker)delegate ()
                    //    {
                    //        textBoxChat.Text += "\r\n" + message;
                    //    });

                    //}

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Disconnect();
                }
            }
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
            //Environment.Exit(0);
        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxSend.Text != " " && textBoxSend.Text != "" && textBoxSend.Text != "  " && textBoxSend.Text != "   ")
            {
                string txt = "Вы:" + textBoxSend.Text;
                double wid = scrollR.ActualWidth;
                switch (selected)
                {
                    case 0:
                        {
                            SendMessage(";;;message/" + login + "/" + textBoxSend.Text);
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (ThreadStart)delegate ()
                            {
                                stackPanelView.Children.Add(new TextBlock { Text = txt, Background = new SolidColorBrush(Color.FromRgb(232, 246, 255)), Width = wid, TextWrapping = TextWrapping.Wrap });
                                scrollR.ScrollToBottom();
                            });
                            break;
                        }
                    default:
                        {
                            SendMessage(";;;pm/" + login + "/" + selectedUser + "/" + textBoxSend.Text + "/");
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (ThreadStart)delegate ()
                            {
                                stackPanelView.Children.Add(new TextBlock { Text = txt, Background = new SolidColorBrush(Color.FromRgb(232, 246, 255)), Width = wid, TextWrapping = TextWrapping.Wrap });
                                scrollR.ScrollToBottom();
                            });
                            break;
                        }
                }
                
                textBoxSend.Clear();
                
            }
            //  при нажатии на "отправить" switch на значение элемента в lbmain
            //  имя этого участника и пользователя отправляется на сервер, а сервер ищет все диалоги с этими участниками и возвращает {имя/сообщение/время}
            // это все в класс, там сортируем и отправляем клиенту в /name|message/
            // он это сразу выводит и не нужно снова сортировать и запоминать
            //
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            client = new TcpClient();
            try
            {
                client.Connect(ip, port);
                stream = client.GetStream();

                string message = UserName.Text;
                byte[] data = Encoding.Unicode.GetBytes(";;;connect/" + message + "/");
                stream.Write(data, 0, data.Length);


                receiveThread = new Thread(delegate () { ReceiveMessage(); });
                receiveThread.Start();


                //textBoxSend.Enabled = true;
                //buttonSend.Enabled = true;
                //сообщенияToolStripMenuItem.Enabled = false;
                //tabControl1.Enabled = true;
                //статистикаToolStripMenuItem.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Disconnect();
            }
        }

        private void buttonRefreshListOfClients_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = ";;;getClient/" + login;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void lbmain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int sel = lbmain.SelectedIndex;
            if (selected >= 0)
            {
                string selU = "Общий чат";
                List<TextBlock> tblist = new List<TextBlock>();
                foreach (TextBlock tb in VisualHelper.FindVisualChildren<TextBlock>(lbmain))
                {
                    if (tb.Text != "")
                        tblist.Add(tb);
                }
                if (tblist.Count > 0 && sel >= 0)
                {
                    selU = tblist[sel].Text;
                    selected = sel;
                    selectedUser = selU;
                }

                SendMessage(";;;dialog/" + login + "/" + selU + "/");
            }

        }

        private void textBoxSend_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)

        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            //List<TextBlock> tblist = new List<TextBlock>();
            //foreach (TextBlock tb in VisualHelper.FindVisualChildren<TextBlock>(lbmain))
            //{
            //    if (tb.Text != "")
            //        tblist.Add(tb);
            //}
            //string podstroka = TextBoxSearch.Text.ToLower();
            //for (int i = 0; i < tblist.Count; i++)
            //{
            //    string stroka = tblist[i].Text.ToLower();
            //    if (stroka.Contains(podstroka))
            //    {
            //        lbmain. = true;
            //        continue;
            //    }
            //    else .Rows[i].Selected = false;
            //}
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Helper h = new Helper(this);
            h.ShowDialog();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            //Disconnect();

            receiveThread.Abort();
            stream.Close();
            
            //MessageBox.Show("Вы отключены");
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            Disconnect();
            mw.Show();
            this.Close();
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            WindowSalut s = new WindowSalut();
            s.ShowDialog();
        }
    }
}
