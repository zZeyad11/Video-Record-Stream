using System;
using System.Threading.Tasks;

namespace Video_Record_Stream
{
   
    public interface IUdpSocketClient : IDisposable
    {
        
        Task ConnectAsync(string address, int port);

       
        Task DisconnectAsync();

       
        Task SendAsync(byte[] data);

        Task SendAsync(byte[] data, int length);

     
        Task SendToAsync(byte[] data, string address, int port);

    
        Task SendToAsync(byte[] data, int length, string address, int port);
        
        event EventHandler<UdpSocketMessageReceivedEventArgs> MessageReceived;
    }
}