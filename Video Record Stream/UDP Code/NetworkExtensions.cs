using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using Video_Record_Stream;

namespace Video_Record_Stream
{
   
    public static class NetworkExtensions
    {
      
        public static CommsInterface ToCommsInterfaceSummary(this NetworkInterface nativeInterface)
        {
            return CommsInterface.FromNativeInterface(nativeInterface);
        }

       
        public static CommsInterfaceStatus ToCommsInterfaceStatus(this OperationalStatus nativeStatus)
        {
            switch (nativeStatus)
            {
                case OperationalStatus.Up:
                    return CommsInterfaceStatus.Connected;
                case OperationalStatus.Down:
                    return CommsInterfaceStatus.Disconnected;
                case OperationalStatus.Unknown:
                    return CommsInterfaceStatus.Unknown;


                default:
                    return CommsInterfaceStatus.Unknown;
            }
            
        }

      
        public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            var addressBytes = address.GetAddressBytes();
            var subnetBytes = subnetMask.GetAddressBytes();

            var broadcastBytes = addressBytes.Zip(subnetBytes, (a, s) => (byte) (a | (s ^ 255))).ToArray();

            return new IPAddress(broadcastBytes);
        }
    }
}