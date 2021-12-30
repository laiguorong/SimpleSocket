using System;
using System.Net.Sockets;
using SimpleSocket.Config;

namespace SimpleSocket
{
    public abstract class SocketListenerBase : ISocketListener
    {
        protected SocketListenerBase(ListenOptions listenOptions, ChannelOptions channelOptions)
        {
            this.ListenOptions = listenOptions;
            this.ChannelOptions = channelOptions;
        }

        public ListenOptions ListenOptions { get; private set; }
        public ChannelOptions ChannelOptions { get; private set; }

        public event EventHandler Started;
        public event EventHandler Stopped;
        public event NewClientAcceptHandler NewClientAccepted;
        public event ErrorHandler Error;

        public abstract void Start();
        public abstract void Stop();

        protected void OnStarted()
        {
            var handler = Started;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        protected void OnStopped()
        {
            var handler = Stopped;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        protected virtual void OnNewClientAccepted(Socket socket, object state)
        {
            var handler = NewClientAccepted;

            if (handler != null)
                handler(this, socket, state);
        }
        protected void OnNewClientAcceptedAsync(Socket socket, object state)
        {
            var handler = NewClientAccepted;

            if (handler != null)
                handler.BeginInvoke(this, socket, state, null, null);
        }
        protected void OnError(Exception e)
        {
            var handler = Error;

            if (handler != null)
                handler(this, e);
        }
        protected void OnError(string errorMessage)
        {
            OnError(new Exception(errorMessage));
        }
    }
}
