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
    public partial class Video
    {
        internal Video(string title, 
            string uri, int formatCode)
        {
            this.Uri = uri;
            this.Title = title;
            this.FormatCode = formatCode;
        }

        /// <summary>
        /// The URL where the video can be downloaded.
        /// </summary>
        public string Uri { get; }
        /// <summary>
        /// The video's title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The format code of the video, which it used by YouTube to denote important information.
        /// </summary>
        public int FormatCode { get; }

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
        /// Gets the full name of the video, typically used as the file name when saving the video to disk.
        /// </summary>
        public string FullName
        {
            get
            {
                var builder = new StringBuilder(Title + FileExtension);

                foreach (char bad in Path.GetInvalidFileNameChars())
                    builder.Replace(bad, '_');

                return builder.ToString();
            }
        }

        /// <summary>
        /// Gets the full name of the video, typically used as the file name when saving the video to disk.
        /// </summary>
        /// <returns>The full name of the video. Refer to <see cref="FullName"/>.</returns>
        public override string ToString() => FullName;
    }
}
