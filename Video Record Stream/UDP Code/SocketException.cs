using System;

namespace Video_Record_Stream
{
    public sealed class SocketException : Exception
    {
        public SocketException(Exception innerException) : base(innerException.Message, innerException)
        {

        }
    }
}