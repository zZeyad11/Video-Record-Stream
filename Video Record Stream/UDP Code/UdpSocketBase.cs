using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Video_Record_Stream;

using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = Video_Record_Stream.SocketException;
// ReSharper disable once CheckNamespace

namespace Video_Record_Stream
{

    public abstract class UdpSocketBase
    {
     
        protected UdpClient _backingUdpClient;

       
        public event EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived;

        internal async void RunMessageReceiver(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var didReceive = false;
                var msg = new UdpReceiveResult();

                try
                {
                   
                    msg = await _backingUdpClient
                        .ReceiveAsync()
                        .WrapNativeSocketExceptions();
                    
                    didReceive = true;
                }
                catch
                {
                  
                    if (!cancellationToken.IsCancellationRequested)
                        throw;
                }

                if (!didReceive)
                    return; 
                var remoteAddress = msg.RemoteEndPoint.Address.ToString();
                var remotePort = msg.RemoteEndPoint.Port.ToString();
                var data = msg.Buffer;

                var wrapperArgs = new UdpSocketMessageReceivedEventArgs(remoteAddress, remotePort, data);

                // fire
                if (MessageReceived != null)
                    MessageReceived(this, wrapperArgs);
            }
        }

     
        protected Task SendAsync(byte[] data)
        {
            return _backingUdpClient
                .SendAsync(data, data.Length)
                .WrapNativeSocketExceptions();
        }

       
        protected Task SendAsync(byte[] data, int length)
        {
            return _backingUdpClient
                .SendAsync(data, length)
                .WrapNativeSocketExceptions();
        }

       
        protected Task SendToAsync(byte[] data, string address, int port)
        {
            return _backingUdpClient
                .SendAsync(data, data.Length, address, port)
                .WrapNativeSocketExceptions();
        }

       
        protected Task SendToAsync(byte[] data, int length, string address, int port)
        {
            return _backingUdpClient
                .SendAsync(data, length, address, port)
                .WrapNativeSocketExceptions();
        }
        
        protected void ProtectAgainstICMPUnreachable(UdpClient udpClient)
        {
#if WINDOWS_DESKTOP

          

            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            udpClient.Client.IOControl((int)SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
#endif
        }

      
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

      
        ~UdpSocketBase()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_backingUdpClient != null)
                {
                    ((IDisposable)_backingUdpClient).Dispose();
                    _backingUdpClient = null;
                }
            }
        }
        
    }
}