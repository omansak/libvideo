using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public partial class Video
    {
        /// <summary>
        /// Initializes a new instance of the VideoLibrary.Video class.
        /// </summary>
        /// <param name="title">The video title.</param>
        /// <param name="uri">The video URI.</param>
        /// <param name="webSite">The name of the video's website, i.e. "YouTube".</param>
        public Video(string title, 
            string uri, string webSite)
            : this(title, uri, webSite, -1)
        { }

        /// <summary>
        /// Initializes a new instance of the VideoLibrary.Video class.
        /// </summary>
        /// <param name="title">The video title.</param>
        /// <param name="uri">The URI where the video can be downloaded.</param>
        /// <param name="webSite">The name of the video's website, i.e. "YouTube".</param>
        /// <param name="formatCode">YouTube-only. The format code of the video, used internally to determine properties such as bitrate and resolution. If the code is unknown or not applicable, -1.</param>
        public Video(string title, string uri, 
            string webSite, int formatCode)
        {
            this.Uri = uri;
            this.Title = title;
            this.WebSite = webSite;
            this.FormatCode = formatCode;
        }

        /// <summary>
        /// The URI where the video can be downloaded.
        /// </summary>
        public string Uri { get; }
        /// <summary>
        /// The video's title.
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// The name of the website where the video came from. Videos coming from this library should have a WebSite matching one of the constants in static class WebSites.
        /// </summary>
        public string WebSite { get; }

        /// <summary>
        /// YouTube-only. The format code of the video, used internally to determine properties such as bitrate and resolution. If the code is unknown or not applicable, -1.
        /// </summary>
        public int FormatCode { get; }

        /// <summary>
        /// Gets the byte array representing the video's file.
        /// </summary>
        /// <returns>The bytes of the video, which are generally saved into a file.</returns>
        public byte[] GetBytes() =>
            GetBytesAsync().Result;

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
            StreamAsync().Result;

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
                string result = Title + FileExtension;

                foreach (char bad in Path.GetInvalidFileNameChars())
                    result = result.Replace(bad, '_');

                return result;
            }
        }

        /// <summary>
        /// Gets the full name of the video, typically used as the file name when saving the video to disk.
        /// </summary>
        /// <returns>The full name of the video. Refer to <see cref="FullName"/>.</returns>
        public override string ToString() => FullName;
    }
}
