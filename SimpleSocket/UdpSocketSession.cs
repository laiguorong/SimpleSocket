using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimpleSocket
{
    public class UdpSocketSession
    {
        private Socket serverSocket;
        private SemaphoreSlim _SendLock = new SemaphoreSlim(1, 1);

        public IPEndPoint RemoteEndPoint { get; protected set; }

        public UdpSocketSession(Socket serverSocket, IPEndPoint remoteEndPoint)
        {
            this.serverSocket = serverSocket;
            this.RemoteEndPoint = remoteEndPoint;
        }
        public void Send(byte[] buffer)
        {
            _SendLock.Wait();
            try
            {
                this.serverSocket.SendTo(buffer, this.RemoteEndPoint);
            }
            finally
            {
                _SendLock.Release();
            }
        }
        public void Send(byte[] buffer, EndPoint remoteEP)
        {
            _SendLock.Wait();
            try
            {
                this.serverSocket.SendTo(buffer, remoteEP);
            }
            finally
            {
                _SendLock.Release();
            }
        }
    }
}
