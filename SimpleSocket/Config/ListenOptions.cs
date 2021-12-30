using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSocket.Config
{
    public class ListenOptions
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public bool NoDelay { get; set; }
    }
}
