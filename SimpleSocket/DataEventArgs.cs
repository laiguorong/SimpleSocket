using System;
using System.Net;

namespace SimpleSocket
{
    public class DataEventArgs : EventArgs
    {
        public IPEndPoint EndPoint { get; set; }

        public byte[] Data { get; set; }
    }
}
