using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SimpleSocket;
using SimpleSocket.Config;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("input 'quit' exit console");
            string ipAddress;
            int port;
            Console.WriteLine("input listen ip and port:");
            while (true)
            {
                string input = Console.ReadLine();//从键盘读取数据
                if (input == "quit")//结束标记
                {
                    return;
                }
                string[] list = input.Split(':');
                if (list.Length != 2)
                {
                    Console.WriteLine("input error");
                    Console.WriteLine("input ip and port:");
                    continue;
                }
                ipAddress = list[0];
                port = Convert.ToInt32(list[1]);
                break;
            }

            using (SimpleUdp udp = new SimpleUdp(new ListenOptions() { Ip = ipAddress, Port = port }, new ChannelOptions()))
            {
                udp.DataReceived += Udp_DataReceived;
                udp.NewClientAccepted += Udp_NewClientAccepted;
                udp.Started += Udp_Started;
                udp.Stopped += Udp_Stopped;
                udp.Start();

                IPEndPoint ip;
                Console.WriteLine("input client ip and port:");
                while (true)
                {
                    string input = Console.ReadLine();//从键盘读取数据
                    if (input == "quit")//结束标记
                    {
                        return;
                    }
                    string[] list = input.Split(':');
                    if (list.Length != 2)
                    {
                        Console.WriteLine("input error");
                        Console.WriteLine("input client ip and port:");
                        continue;
                    }
                    ip = new IPEndPoint(IPAddress.Parse(list[0]), Convert.ToInt32(list[1]));
                    break;
                }

                while (true)
                {
                    Console.WriteLine("input send data:");
                    string input = Console.ReadLine();//从键盘读取数据
                    if (input == "quit")//结束标记
                    {
                        break;
                    }
                    byte[] data = Encoding.UTF8.GetBytes(input);
                    udp.Send(data, ip);
                }
            }
        }

        private static void Udp_Stopped(object sender, EventArgs e)
        {
            Console.WriteLine("Udp Server Stopped");
        }

        private static void Udp_Started(object sender, EventArgs e)
        {
            Console.WriteLine("Udp Server Started");
        }

        private static void Udp_NewClientAccepted(object sender, ClientEventArgs e)
        {
            Console.WriteLine("NewClientAccepted = {0}", e.EndPoint);
        }

        private static void Udp_DataReceived(object sender, DataEventArgs e)
        {
            Console.WriteLine("DataReceived [{0}] = {1}", e.EndPoint, Encoding.UTF8.GetString(e.Data));
        }
    }
}
