using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    /// <summary>
    /// Enscapulates information about a video, such as title, format, and resolution.
    /// </summary>
    public partial class YouTubeVideo : Video
    {
        internal YouTubeVideo(string title, 
            string uri, int formatCode)
        {
            this.Uri = uri;
            this.Title = title;
            this.FormatCode = formatCode;
        }

        public override string Uri { get; }
        public override string Title { get; }
        public override WebSites WebSite => WebSites.YouTube;

        /// <summary>
        /// The format code of the video, which it used by YouTube to denote important information.
        /// </summary>
        public int FormatCode { get; }
    }
}
