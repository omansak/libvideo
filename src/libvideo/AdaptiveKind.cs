using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    /// <summary>
    /// Represents whether a video is adaptive or not.
    /// </summary>
    public enum AdaptiveKind
    {
        /// <summary>
        /// The Video is not adaptive.
        /// </summary>
        None,
        /// <summary>
        /// The video has adaptive audio.
        /// </summary>
        Audio,
        /// <summary>
        /// The video is fully adaptive.
        /// </summary>
        Video
    }
}
