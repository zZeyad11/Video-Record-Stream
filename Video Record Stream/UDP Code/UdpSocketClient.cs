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
   
    public class UdpSocketClient : UdpSocketBase, IUdpSocketClient
    {
        private CancellationTokenSource _messageCanceller;

       
        public UdpSocketClient()
        {
            try
            {
                _backingUdpClient = new UdpClient
                {
                    EnableBroadcast = true
                };
                ProtectAgainstICMPUnreachable(_backingUdpClient);
            }
            catch (PlatformSocketException ex)
            {
                throw new PclSocketException(ex);
            }
        }

     
        public Task ConnectAsync(string address, int port)
        {
            _messageCanceller = new CancellationTokenSource();

            return Task.Run(() => {
                _backingUdpClient.Connect(address, port);
                base.RunMessageReceiver(_messageCanceller.Token);
            })
            .WrapNativeSocketExceptions();
        }

       
        public Task DisconnectAsync()
        {
            return Task.Run(() => {
                if (_messageCanceller != null)
                {
                    _messageCanceller.Cancel();
                    _messageCanceller.Dispose();
                    _messageCanceller = null;
                }
                if (_backingUdpClient != null)
                {
                    _backingUdpClient.Close();
                }
            });
        }

       
        public new Task SendAsync(byte[] data)
        {
            return base.SendAsync(data);
        }

        public new Task SendAsync(byte[] data, int length)
        {
            return base.SendAsync(data, length);
        }

      
        public new Task SendToAsync(byte[] data, string address, int port)
        {
            return base.SendToAsync(data, address, port);
        }

      
        public new Task SendToAsync(byte[] data, int length, string address, int port)
        {
            return base.SendToAsync(data, length, address, port);
        }
        
       
        public override void Dispose()
        {
            if (_messageCanceller != null && !_messageCanceller.IsCancellationRequested)
                _messageCanceller.Cancel();

            base.Dispose();
        }
    }
}
