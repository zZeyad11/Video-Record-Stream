namespace Video_Record_Stream
{
   
    public class UdpSocketMessageReceivedEventArgs
    {
        private readonly string _remotePort;
        private readonly string _remoteAddress;
        private readonly byte[] _byteData;

        
        public UdpSocketMessageReceivedEventArgs(string remoteAddress, string remotePort, byte[] byteData)
        {
            _remoteAddress = remoteAddress;
            _remotePort = remotePort;
            _byteData = byteData;
        }

   
        public string RemoteAddress
        {
            get { return _remoteAddress; }
        }

        public string RemotePort
        {
            get { return _remotePort; }
        }

       
        public byte[] ByteData
        {
            get { return _byteData; }
        }
    }
}