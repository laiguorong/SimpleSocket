using System;
using System.Net.Sockets;
using SimpleSocket.Config;

namespace SimpleSocket
{
    public interface ISocketListener
    {
        ListenOptions ListenOptions { get; }
        ChannelOptions ChannelOptions { get; }

        event EventHandler Started;
        event EventHandler Stopped;
        event NewClientAcceptHandler NewClientAccepted;
        event ErrorHandler Error;

        void Start();
        void Stop();
    }

    public delegate void ErrorHandler(ISocketListener listener, Exception e);
    public delegate void NewClientAcceptHandler(ISocketListener listener, Socket client, object state);
}
