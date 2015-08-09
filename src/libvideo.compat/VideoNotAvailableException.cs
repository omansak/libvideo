using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary.Compat
{
    public class VideoNotAvailableException : Exception
    {
        public VideoNotAvailableException()
        { }

        public VideoNotAvailableException(string message)
            : base(message)
        { }
    }
}
