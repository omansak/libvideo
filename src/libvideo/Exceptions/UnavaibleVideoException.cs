using System;

namespace VideoLibrary.Exceptions
{
    public class UnavailableStreamException : Exception
    {
        public UnavailableStreamException()
            : base()
        { }

        public UnavailableStreamException(string message)
            : base(message)
        { }

        public UnavailableStreamException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
