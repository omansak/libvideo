using System;

namespace VideoLibraryNetCore.Exceptions
{
    internal class BadQueryException : Exception
    {
        public BadQueryException()
            : base()
        { }

        public BadQueryException(string message)
            : base(message)
        { }

        public BadQueryException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}