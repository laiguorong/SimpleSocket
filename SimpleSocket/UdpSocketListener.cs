using System;
using System.Net;
using System.Net.Sockets;
using SimpleSocket.Common;
using SimpleSocket.Config;

namespace SimpleSocket
{
    public class UdpSocketListener : SocketListenerBase
    {
        private Socket socket;
        private SocketAsyncEventArgs socketAsyncEventArgs;

        public UdpSocketListener(ListenOptions listenOptions, ChannelOptions channelOptions)
            : base(listenOptions, channelOptions)
        {

        }
        public override void Start()
        {
            try
            {
                var listenEndpoint = this.ListenOptions.GetListenEndPoint();
                socket = new Socket(listenEndpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

                if (this.ListenOptions.NoDelay)
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);

                socket.Bind(listenEndpoint);

                try
                {
                    uint IOC_IN = 0x80000000;
                    uint IOC_VENDOR = 0x18000000;
                    uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

                    byte[] optionInValue = { Convert.ToByte(false) };
                    byte[] optionOutValue = new byte[4];
                    socket.IOControl((int)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
                }
                catch (PlatformNotSupportedException)
                {
                    //Mono doesn't support it
                    //Failed to set socket option SIO_UDP_CONNRESET because the platform doesn't support it.
                }

                var eventArgs = new SocketAsyncEventArgs();
                socketAsyncEventArgs = eventArgs;

                eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(eventArgs_Completed);
                eventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                int receiveBufferSize = this.ChannelOptions.ReceiveBufferSize <= 0 ? 2048 : this.ChannelOptions.ReceiveBufferSize;
                var buffer = new byte[receiveBufferSize];
                eventArgs.SetBuffer(buffer, 0, buffer.Length);

                socket.ReceiveFromAsync(eventArgs);

                OnStarted();
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        void eventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                var errorCode = (int)e.SocketError;

                //The listen socket was closed
                if (errorCode == 995 || errorCode == 10004 || errorCode == 10038)
                    return;

                OnError(new SocketException(errorCode));
            }

            if (e.LastOperation == SocketAsyncOperation.ReceiveFrom)
            {
                try
                {
                    OnNewClientAcceptedAsync(socket, new object[] { e.Buffer.CloneRange(e.Offset, e.BytesTransferred), e.RemoteEndPoint.Serialize() });
                }
                catch (Exception exc)
                {
                    OnError(exc);
                }

                try
                {
                    socket.ReceiveFromAsync(e);
                }
                catch (Exception exc)
                {
                    OnError(exc);
                }
            }
        }

        public override void Stop()
        {
            if (socket == null)
                return;

            lock (this)
            {
                if (socket == null)
                    return;

                socketAsyncEventArgs.Completed -= new EventHandler<SocketAsyncEventArgs>(eventArgs_Completed);
                socketAsyncEventArgs.Dispose();
                socketAsyncEventArgs = null;

                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch { }

                try
                {
                    socket.Close();
                }
                catch { }
                finally
                {
                    socket = null;
                }
            }

            OnStopped();
        }
    }
}
