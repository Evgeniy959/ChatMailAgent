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
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;

namespace ChatMailAgent
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpClient client;
        NetworkStream networkStream;
        List<string> Names;
        public MainWindow()
        {
            InitializeComponent();
            Names = new List<string>() { "Игорь", "Ольга", "Евгений", "Ирина", "Василий" };
           
            client = new TcpClient("127.0.0.1", 8888);
            networkStream = client.GetStream();

            Thread thread = new Thread(ServerListner);
            thread.Start();
            Random random = new Random();
            SendMessage("<NAME>"+Names[random.Next(Names.Count)]);
        }
        string message;
        void ServerListner()
        {
            while (true)
            {
                NetworkStream networkStream = client.GetStream();
                byte[] buffer = new byte[255];
                networkStream.Read(buffer, 0, 255);
                message = Encoding.Default.GetString(buffer);
                message = message.Remove(message.IndexOf("\0"));
                if (message.IndexOf("<LIST>") == 0)
                {

                    message = message.Remove(0, 6);
                    List<string> names = JsonConvert.DeserializeObject<List<string>>(message);
                    Dispatcher.Invoke(new Action(() => Clients.Items.Clear()));
                    foreach (string name in names)
                    {
                        Dispatcher.Invoke(new Action(() => Clients.Items.Add(name)));
                    }
                }
                else
                {
                    message += "\n";
                    Dispatcher.Invoke(new Action(() => History.AppendText(message)));
                }
            }
        }

        void SendMessage(string message)
        {
            byte[] buffer = Encoding.Default.GetBytes(message);
            networkStream.Write(buffer, 0, message.Length);
            networkStream.Flush();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string message = Message.Text;
            SendMessage(message);
            Message.Text = "";
        }
    }
}
