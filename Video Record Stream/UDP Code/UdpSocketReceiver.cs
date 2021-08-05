using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Video_Record_Stream;

using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = Video_Record_Stream.SocketException;
// ReSharper disable once CheckNamespace

namespace Video_Record_Stream
{
   
    public class UdpSocketReceiver : UdpSocketBase, IUdpSocketReceiver
    {

        public int Port;
        private CancellationTokenSource _messageCanceller;

  
        public Task StartListeningAsync(int port = 0, ICommsInterface listenOn = null)
        {
            Port = port;
            if (listenOn != null && !listenOn.IsUsable)
                throw new InvalidOperationException("Cannot listen on an unusable interface. Check the IsUsable property before attemping to bind.");

            return Task
                .Run(() =>
                {
                    var ip = listenOn != null ? ((CommsInterface)listenOn).NativeIpAddress : IPAddress.Any;
                    var ep = new IPEndPoint(ip, port);

                    _messageCanceller = new CancellationTokenSource();

                    _backingUdpClient = new UdpClient(ep)
                    {
                        EnableBroadcast = true
                    };
                    ProtectAgainstICMPUnreachable(_backingUdpClient);

                    RunMessageReceiver(_messageCanceller.Token);
                })
                .WrapNativeSocketExceptions();
        }

      
        public Task StopListeningAsync()
        {
            return Task.Run(() =>
            {
                _messageCanceller.Cancel();
                _backingUdpClient.Close();
            });
        }

      
        public new Task SendToAsync(byte[] data, string address, int port)
        {
            return SendToAsync(data, data.Length, address, port);
        }

    
        public new async Task SendToAsync(byte[] data, int length, string address, int port)
        {
            if (_backingUdpClient == null)
            {
         
                try
                {
                    _backingUdpClient = new UdpClient { EnableBroadcast = true };
                    ProtectAgainstICMPUnreachable(_backingUdpClient);
                }
                catch (PlatformSocketException ex)
                {
                    throw new PclSocketException(ex);
                }

                using (_backingUdpClient)
                {
                    await base.SendToAsync(data, length, address, port);
                }

                // clear _backingUdpClient because it has been disposed and is unusable. 
                _backingUdpClient = null;
            }
            else
            {
                await base.SendToAsync(data, length, address, port);
            }
        }

     
        public override void Dispose()
        {
            if (_messageCanceller != null && !_messageCanceller.IsCancellationRequested)
                _messageCanceller.Cancel();

            base.Dispose();
        }
    }
}