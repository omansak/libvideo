using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    /// <summary>
    /// Represents what file format the video follows.
    /// </summary>
    public enum VideoFormat
    {
        /// <summary>
        /// The video is a Flash Video (.flv) file.
        /// </summary>
        Flash,
        /// <summary>
        /// The video is a .3gp file.
        /// </summary>
        Mobile,
        /// <summary>
        /// The video is an .mp4 file.
        /// </summary>
        Mp4,
        /// <summary>
        /// The video is a .webm file.
        /// </summary>
        WebM,
        /// <summary>
        /// The format of the video is unknown.
        /// </summary>
        Unknown
    }
}
