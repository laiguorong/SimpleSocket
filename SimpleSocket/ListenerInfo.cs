using System;
using System.Net;
using System.Security.Authentication;

namespace SimpleSocket
{
    [Serializable]
    public class ListenerInfo
    {
        public IPEndPoint EndPoint { get; set; }
        public int BackLog { get; set; }
        public SslProtocols Security { get; set; }
    }
}
