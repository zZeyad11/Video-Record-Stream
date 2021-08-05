using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Video_Record_Stream;

namespace Video_Record_Stream
{
   
    public partial class CommsInterface : ICommsInterface
    {
       
        public string NativeInterfaceId { get; private set; }

       
        public string Name { get; private set; }

       
        public string IpAddress { get; private set; }

      
        public string GatewayAddress { get; private set; }

       
        public string BroadcastAddress { get; private set; }

       
        public CommsInterfaceStatus ConnectionStatus { get; private set; }

       
        public bool IsUsable
        {
            get { return !String.IsNullOrWhiteSpace(IpAddress); }
        }

        private readonly string[] _loopbackAddresses = { "127.0.0.1", "localhost" };

     
        public bool IsLoopback
        {
           
            get { return _loopbackAddresses.Contains(IpAddress); }
        }

        protected internal NetworkInterface NativeInterface;

       
        protected internal IPAddress NativeIpAddress;

       
        protected internal IPEndPoint EndPoint(int port)
        {
            return new IPEndPoint(NativeIpAddress, port);
        }

        internal static CommsInterface FromNativeInterface(NetworkInterface nativeInterface)
        {           
            var ip = 
                nativeInterface
                    .GetIPProperties()
                    .UnicastAddresses
                    .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);

            var gateway =
                nativeInterface
                    .GetIPProperties()
                    .GatewayAddresses
                    .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(a => a.Address.ToString())
                    .FirstOrDefault();

            var netmask = ip != null ? GetSubnetMask(ip) : null; 

            var broadcast = (ip != null && netmask != null) ? ip.Address.GetBroadcastAddress(netmask).ToString() : null;

            return new CommsInterface
            {
                NativeInterfaceId = nativeInterface.Id,
                NativeIpAddress = ip != null ? ip.Address : null,
                Name = nativeInterface.Name,
                IpAddress = ip != null ? ip.Address.ToString() : null,
                GatewayAddress = gateway,
                BroadcastAddress = broadcast,
                ConnectionStatus = nativeInterface.OperationalStatus.ToCommsInterfaceStatus(),
                NativeInterface = nativeInterface
            };
        }

       
        public static Task<List<CommsInterface>> GetAllInterfacesAsync()
        {
            var interfaces = Task.Run(() =>
                System.Net.NetworkInformation.NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Select(FromNativeInterface)
                    .ToList());

            return interfaces;
        }
    }
}
