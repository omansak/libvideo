using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    /// <summary>
    /// Represents the type of audio present within the video.
    /// </summary>
    public enum AudioFormat
    {
        /// <summary>
        /// The video has MP3 audio.
        /// </summary>
        Mp3,
        /// <summary>
        /// The video has AAC audio.
        /// </summary>
        Aac,
        /// <summary>
        /// The video has OGG (Vorbis) audio.
        /// </summary>
        Vorbis,
        /// <summary>
        /// The type of audio is unknown.
        /// </summary>
        Unknown
    }
}
