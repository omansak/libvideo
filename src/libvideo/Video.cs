using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public abstract class Video
    {
        internal Video()
        {
        }

        public abstract string Uri { get; }
        public abstract string Title { get; }
        public abstract WebSites WebSite { get; }
        public virtual VideoFormat Format => VideoFormat.Unknown;
        // public virtual AudioFormat AudioFormat => AudioFormat.Unknown;

        /// <summary>
        /// Gets the byte array representing the video's file.
        /// </summary>
        /// <returns>The bytes of the video, which are generally saved into a file.</returns>
        public byte[] GetBytes() =>
            GetBytesAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Gets the byte array representing the video's file as an asynchronous operation.
        /// </summary>
        /// <returns>A Task returning the bytes of the video, which are generally saved into a file.</returns>
        public async Task<byte[]> GetBytesAsync()
        {
            using (var client = new VideoClient())
            {
                return await client
                    .GetBytesAsync(this)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Streams the contents of the video.
        /// </summary>
        /// <returns>A stream to the binary contents of this video.</returns>
        public Stream Stream() =>
            StreamAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Streams the contents of the video as an asynchronous operation.
        /// </summary>
        /// <returns>A Task to a stream representing the binary contents of this video.</returns>
        public async Task<Stream> StreamAsync()
        {
            using (var client = new VideoClient())
            {
                return await client
                    .StreamAsync(this)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the appropriate file extension for this video, based on <see cref="Format"/>.
        /// </summary>
        public virtual string FileExtension
        {
            get
            {
                switch (Format)
                {
                    case VideoFormat.Flash: return ".flv";
                    case VideoFormat.Mobile: return ".3gp";
                    case VideoFormat.Mp4: return ".mp4";
                    case VideoFormat.WebM: return ".webm";
                    case VideoFormat.Unknown: return string.Empty;
                    default:
                        throw new NotImplementedException($"Format {Format} is unrecognized! Please file an issue at libvideo on GitHub.");
                }
            }
        }

        /// <summary>
        /// Gets the full name of the video, typically used as the file name when saving the video to disk.
        /// </summary>
        public string FullName
        {
            get
            {
                var builder = 
                    new StringBuilder(Title)
                    .Append(FileExtension);

                foreach (char bad in Path.GetInvalidFileNameChars())
                    builder.Replace(bad, '_');

                return builder.ToString();
            }
        }
    }
}
