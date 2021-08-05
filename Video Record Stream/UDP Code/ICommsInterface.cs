using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Video_Record_Stream
{
    
    public interface ICommsInterface
    {
       
        string NativeInterfaceId { get; }

        
        string Name { get; }

       
        string IpAddress { get; }

        string GatewayAddress { get; }

       
        string BroadcastAddress { get; }

        CommsInterfaceStatus ConnectionStatus { get; }

       
        bool IsUsable { get; }

      
        bool IsLoopback { get; }
    }
}
