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
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Net.Mail;

namespace DreamTeam
{
    /// <summary>
    /// Логика взаимодействия для Helper.xaml
    /// </summary>
    public partial class Helper : Window
    {
        public Chat ch;
        public Helper(Chat ch)
        {
            InitializeComponent();
            this.ch = ch;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("от кого");
                mail.To.Add(new MailAddress("кому"));
                mail.Subject = caption.Text;
                mail.Body = message.Text;
                //mail.IsBodyHtml = true;
                SmtpClient client = new SmtpClient();
                client.Host = "smtp.mail.ru";
                client.Port = 587;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential("Почта", "Пароль");
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(mail);
                //mail.Dispose();
                MessageBox.Show("Сообщение отправлено");
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
