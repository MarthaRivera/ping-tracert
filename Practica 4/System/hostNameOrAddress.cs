using System.Runtime.Serialization;

namespace System
{
    [Serializable]
    internal class hostNameOrAddress : Exception
    {
        public hostNameOrAddress()
        {
        }

        public hostNameOrAddress(string message) : base(message)
        {
        }

        public hostNameOrAddress(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected hostNameOrAddress(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}