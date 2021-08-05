using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Video_Record_Stream
{
    public partial class CommsInterface
    {
       
        protected static IPAddress GetSubnetMask(UnicastIPAddressInformation ip)
        {
            return ip.IPv4Mask;
        }
    }
}
