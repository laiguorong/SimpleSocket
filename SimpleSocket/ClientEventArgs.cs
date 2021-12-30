using System;
using System.Net;

namespace SimpleSocket
{
    public class ClientEventArgs : EventArgs
    {
        public IPEndPoint EndPoint { get; set; }
    }
}
