using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeExtractor
{
    public class YoutubeParseException : Exception
    {
        public YoutubeParseException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
