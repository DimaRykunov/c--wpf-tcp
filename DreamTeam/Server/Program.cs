using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;
using System.Data.OleDb;
using System.Data;
using System.Data.SqlClient;

namespace Server
{
    public class ServerObject
    {
        public int[] spis;
        static TcpListener tcpListener;
        List<ClientObject> clients = new List<ClientObject>();

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }


        protected internal void RemoveConnection(string id)
        {
            try
            {
                ClientObject client = clients.FirstOrDefault(c => c.ID == id);
                if (client != null)
                    clients.Remove(client);
            }
            catch(Exception ex)
            {

            }
        }

        //прослушивание всех входящих подключений
        protected internal void Listen()
        {

            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }
        //трансляция сообщения подключённым клиентам
        protected internal void BroadCastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].ID != id)//если клиент и отправляющий не совпадают
                {
                    clients[i].Stream.Write(data, 0, data.Length);
                }
            }
        }
        //отправка имен подключенных клиентов
        protected internal void GetClients(string id, string userSender)
        {
            string list = ";;;list";
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].ID != id && clients[i].userName != null)
                    list += "/" + clients[i].userName;
            }
            list += "/";
            byte[] data = Encoding.Unicode.GetBytes(list);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].ID == id)//отправляем тому, кто попросил
                {

                    clients[i].Stream.Write(data, 0, data.Length);
                }
            }
        }

        
        protected internal void authentication(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].ID == id)
                {
                    clients[i].Stream.Write(data, 0, data.Length);
                }
            }
        }
        //отправка личного сообщения
        protected internal void pm(string message, string userReciever)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].userName == userReciever)
                {
                    clients[i].Stream.Write(data, 0, data.Length);
                }
            }
        }

        //подключение пользователя с именем
        protected internal void connectUser(string log, string id)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].ID == id)
                {
                    clients[i].userName = log;
                }
            }
        }
      
        //отключение и выход
        protected internal void Disconnect()
        {
            tcpListener.Stop();

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close();
            }
            Environment.Exit(0);
        }
    }

    public class ClientObject
    {
        protected internal string ID { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        protected internal FileStream FStream { get; private set; }
        private string _userName;
        public string userName
        {
            get { return _userName; }
            set { _userName = value; }
        }
        TcpClient client;
        ServerObject server;

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            ID = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);

        }

        public string getLogin(string sql)
        {
            string rez = "";
            try
            {
                string connectionString = @"Data Source=localhost;Integrated Security=SSPI;Initial Catalog = DreamTeam";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.CommandText = sql;
                    command.Connection = connection;
                    SqlDataReader myDataReader = command.ExecuteReader();
                    if (myDataReader.HasRows)
                    {
                        while (myDataReader.Read()) // построчно считываем данные
                        {
                            rez = (myDataReader.GetValue(0).ToString());
                        }
                    }
                    connection.Close();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return rez;
        }

        public string getPassword(string sql)
        {
            string rez = "";

            string connectionString = @"Data Source=localhost;Integrated Security=SSPI;Initial Catalog = DreamTeam";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand();
                command.CommandText = sql;
                command.Connection = connection;
                SqlDataReader myDataReader = command.ExecuteReader();
                if (myDataReader.HasRows)
                {
                    while (myDataReader.Read()) // построчно считываем данные
                    {
                        rez = (myDataReader.GetValue(3).ToString());
                    }
                }
                connection.Close();
            }
            return rez;
        }

        private static void AdddToBase(string sql)
        {
            string connectionString = @"Data Source=localhost;Integrated Security=SSPI;Initial Catalog = DreamTeam";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }
        public string GetDialog(string sql)
        {
            string result = "/";
            try
            {
                
                string connectionString = @"Data Source=localhost;Integrated Security=SSPI;Initial Catalog = DreamTeam";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.CommandText = sql;
                    command.Connection = connection;
                    SqlDataReader myDataReader = command.ExecuteReader();
                    if (myDataReader.HasRows)
                    {
                        while (myDataReader.Read()) // построчно считываем данные
                        {
                            result += (string)(myDataReader.GetValue(1)) + "$" + (string)(myDataReader.GetValue(3)) + "/";
                            
                        }
                    }
                    connection.Close();
                }

            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return result;
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();//получаем имя пользователя
                string message = GetMessage();
                bool a = false;
                while (true)
                {
                    try
                    {
                        if (a == true)
                            message = GetMessage();
                        a = true;
                        string[] data = { "" };
                        if (message.StartsWith(";;;"))
                            data = message.Split('/');
                        switch (data[0])
                        {
                            case ";;;authentication":
                                {

                                    string userName = data[1];
                                    string password = data[2];
                                    string[] rez = { "false", "false" };
                                    if (getLogin("SELECT * FROM Users WHERE login='" + userName + "';") != "")
                                        rez[0] = "true";
                                    else rez[0] = "false";
                                    if (getPassword("SELECT * FROM Users WHERE password='" + password + "';") != "")
                                        rez[1] = "true";
                                    else rez[1] = "false";

                                    message = ";;;check/" + rez[0] + "/" + rez[1] + "/";
                                    server.authentication(message, this.ID);
                                    break;
                                }
                            case ";;;connect":
                                {
                                    userName = data[1];
                                    server.connectUser(userName, this.ID);
                                    message = userName + " вошёл в чат";
                                    server.BroadCastMessage(message, this.ID);//сообщение всем о входе в чат
                                    Console.WriteLine(message);
                                    break;
                                }
                            case ";;;message":
                                {
                                    userName = data[1];
                                    message = userName + ": " + data[2];
                                    server.BroadCastMessage(message, this.ID);//сообщение всем о входе в чат
                                    AdddToBase("INSERT INTO Statisticks(Sender, Receiver, Message, Date) VALUES('" + userName + "', '" + "public" + "', '" + data[2] + "', '" + DateTime.Today + "');");
                                    Console.WriteLine(message);
                                    break;
                                }
                            case ";;;getClient":
                                {
                                    userName = data[1];
                                    server.GetClients(this.ID, userName);//сообщение всем о входе в чат
                                    break;
                                }
                            case ";;;pm":
                                {
                                    userName = data[1];
                                    string receiverUser = data[2];
                                    AdddToBase("INSERT INTO Statisticks(Sender, Receiver, Message, Date) VALUES('" + userName +"', '" + receiverUser + "', '" + data[3] + "', '" + DateTime.Now + "');");
                                    message = ";;;pm/" + data[3] + "/" + data[1] + "/";
                                    server.pm(message, receiverUser);
                                    break;
                                }
                            case ";;;dialog":
                                {
                                    userName = data[1];
                                    string receiverUser="";
                                    if (data[2] == "Общий чат")
                                    {
                                        message = ";;;dialog/" + userName + "/|" + GetDialog("SELECT * FROM Statisticks WHERE Receiver='public';");
                                    }
                                    else
                                    {
                                        receiverUser = data[2];
                                        message = ";;;dialog/" + userName + "/|" + GetDialog("SELECT * FROM Statisticks WHERE (Sender='" + userName + "' and Receiver='" + receiverUser + "') or (Sender='" + receiverUser + "' and Receiver='" + userName + "');");
                                    }
                                    message += "\0";
                                    server.pm(message, userName);
                                    break;
                                }
                            case ";;;registration":
                                {
                                    string userName = data[1];
                                    string name = data[2];
                                    string surname = data[3];
                                    string password = data[4];
                                    string rez = "true";
                                    if (getLogin("SELECT * FROM Users WHERE login='" + userName + "';") != "")
                                    {
                                        rez = "false";
                                        AdddToBase("INSERT INTO Users(login, name, surname, password) VALUES('" + userName + "', '" + name + "', '" + surname + "', '" + password + "');");
                                    }
                                    message = ";;;registration/" + rez + "/";
                                    server.authentication(message, this.ID);
                                    break;
                                }
                            default:

                                break;
                        }

                    }
                    catch(Exception ex)
                        {
                        message = String.Format("{0}: покинул чат", userName);
                        Console.WriteLine(message);
                        server.BroadCastMessage(message, this.ID);
                        break;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.RemoveConnection(this.ID);
                Close();
            }
        }
        //Метод потока

        //чтение входящего сообщения и преобраование в строку
        private string GetMessage()
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);
            return builder.ToString();
        }

        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
    class Program
    {
        static ServerObject server;
        static Thread listenThread;
        static void Main(string[] args)
        {
            try
            {
                server = new ServerObject();
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.Start();
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }
    }
}
