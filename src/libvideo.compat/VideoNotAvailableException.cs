using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeExtractor
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
