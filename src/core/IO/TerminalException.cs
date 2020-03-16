using System.Runtime.Serialization;

namespace System.IO
{
    [Serializable]
    public class TerminalException : IOException
    {
        public TerminalException()
        {
        }

        public TerminalException(string message)
            : base(message)
        {
        }

        public TerminalException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected TerminalException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
