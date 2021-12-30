using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Caching;
using SimpleSocket.Common;
using SimpleSocket.Config;

namespace SimpleSocket
{
    public class SimpleUdp : IDisposable
    {
        private Socket listenSocket;
        private SocketAsyncEventArgs m_ReceiveSAE;
        private LRUCache<string, Socket> _RemoteSockets = new LRUCache<string, Socket>(100, 1, false);
#if NET35
        private Semaphore _SendLock = new Semaphore(1, 1);
#else
        private SemaphoreSlim _SendLock = new SemaphoreSlim(1, 1);
#endif

        public ListenOptions Options { get; }
        public ChannelOptions ChannelOptions { get; }
        public List<string> Endpoints
        {
            get
            {
                return _RemoteSockets.GetKeys();
            }
        }

        public SimpleUdp(ListenOptions options, ChannelOptions channelOptions)
        {
            Options = options;
            ChannelOptions = channelOptions;
        }

        public event EventHandler Started;
        public event EventHandler Stopped;
        public event EventHandler<ClientEventArgs> NewClientAccepted;
        public event EventHandler<DataEventArgs> DataReceived;
        public event EventHandler<ErrorEventArgs> Error;

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Stop();

                    if (_SendLock != null)
                    {
#if NET35
                        _SendLock.Close();
#else
                        _SendLock.Dispose();
#endif
                        _SendLock = null;
                    }
                }

                disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void OnStarted()
        {
            var handler = Started;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        private void OnStopped()
        {
            var handler = Stopped;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        private void OnNewClientAccepted(IPEndPoint endPoint)
        {
            var handler = NewClientAccepted;

            if (handler != null)
                handler.BeginInvoke(this, new ClientEventArgs() { EndPoint = endPoint }, null, null);
        }
        private void OnDataReceived(DataEventArgs args)
        {
            var handler = DataReceived;

            if (handler != null)
                handler.BeginInvoke(this, args, null, null);
        }
        private void OnError(Exception e)
        {
            var handler = Error;

            if (handler != null)
                handler(this, new ErrorEventArgs() { Exception = e });
        }
        private void OnError(string errorMessage)
        {
            OnError(new Exception(errorMessage));
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
                    var s = e.AcceptSocket;
                    var endpoint = e.RemoteEndPoint as IPEndPoint;
                    if (!_RemoteSockets.Contains(endpoint.ToString()))
                    {
                        _RemoteSockets.AddReplace(endpoint.ToString(), s);
                        OnNewClientAccepted(endpoint);
                    }

                    OnDataReceived(new DataEventArgs { EndPoint = endpoint, Data = e.Buffer.CloneRange(e.Offset, e.BytesTransferred) });
                }
                catch (Exception exc)
                {
                    OnError(exc);
                }

                try
                {
                    listenSocket.ReceiveFromAsync(e);
                }
                catch (Exception exc)
                {
                    OnError(exc);
                }
            }
        }
        public void Start()
        {
            if (listenSocket != null) return;

            try
            {
                var options = Options;

                var listenEndpoint = options.GetListenEndPoint();
                listenSocket = new Socket(listenEndpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

                if (options.NoDelay)
                    listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);

                listenSocket.Bind(listenEndpoint);

                try
                {
                    uint IOC_IN = 0x80000000;
                    uint IOC_VENDOR = 0x18000000;
                    uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

                    byte[] optionInValue = { Convert.ToByte(false) };
                    byte[] optionOutValue = new byte[4];
                    listenSocket.IOControl((int)SIO_UDP_CONNRESET, optionInValue, optionOutValue);
                }
                catch (PlatformNotSupportedException)
                {
                    //Mono doesn't support it
                    //Failed to set socket option SIO_UDP_CONNRESET because the platform doesn't support it.
                }

                var eventArgs = new SocketAsyncEventArgs();
                m_ReceiveSAE = eventArgs;

                eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(eventArgs_Completed);
                eventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                int receiveBufferSize = ChannelOptions.ReceiveBufferSize <= 0 ? 2048 : ChannelOptions.ReceiveBufferSize;
                var buffer = new byte[receiveBufferSize];
                eventArgs.SetBuffer(buffer, 0, buffer.Length);

                listenSocket.ReceiveFromAsync(eventArgs);

                OnStarted();
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }
        public void Stop()
        {
            if (listenSocket == null)
                return;

            lock (this)
            {
                if (listenSocket == null)
                    return;

                try
                {
                    if (m_ReceiveSAE != null)
                    {
                        m_ReceiveSAE.Completed -= new EventHandler<SocketAsyncEventArgs>(eventArgs_Completed);
                        m_ReceiveSAE.Dispose();
                        m_ReceiveSAE = null;
                    }
                }
                catch { }

                try
                {
                    listenSocket.Shutdown(SocketShutdown.Both);
                }
                catch { }

                try
                {
                    listenSocket.Close();
                }
                catch { }
                finally
                {
                    listenSocket = null;
                }
            }

            OnStopped();
        }
        public void Send(byte[] buffer, IPEndPoint remoteEndPoint)
        {
            if (listenSocket == null) return;

#if NET35
            _SendLock.WaitOne();
#else
            _SendLock.Wait();
#endif
            try
            {
                listenSocket?.SendTo(buffer, remoteEndPoint);
            }
            finally
            {
                _SendLock.Release();
            }
        }
    }
}
