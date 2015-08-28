using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    /// <summary>
    /// Facilitates reuse of <see cref="HttpClient"/> instances when downloading videos.
    /// </summary>
    public class VideoClient : IDisposable
    {
        private bool disposed = false;
        private readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoClient"/> class.
        /// </summary>
        public VideoClient()
        {
        }

        #region IDisposable

        ~VideoClient()
        {
            Dispose(false);
        }

        /// <summary>
        /// Frees any resources held by this <see cref="VideoClient"/> instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees resources held by this <see cref="VideoClient"/> instance.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            disposed = true;

            if (disposing)
            {
                if (client != null)
                    client.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// Retrieves the binary contents of the specified <see cref="Video"/>.
        /// </summary>
        /// <param name="video">The <see cref="Video"/> to retrieve bytes from.</param>
        /// <returns>The binary contents of the video.</returns>
        public byte[] GetBytes(Video video) => GetBytesAsync(video).Result;

        /// <summary>
        /// Retrieves the binary contents of the specified <see cref="Video"/> as an asynchronous operation.
        /// </summary>
        /// <param name="video">The <see cref="Video"/> to retrieve bytes from.</param>
        /// <returns>A <see cref="Task"/> of the binary contents of the video.</returns>
        public async Task<byte[]> GetBytesAsync(Video video)
        {
            return await client
                .GetByteArrayAsync(video.Uri)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Streams the binary contents of the specified <see cref="Video"/>.
        /// </summary>
        /// <param name="video">The <see cref="Video"/> to stream.</param>
        /// <returns>A <see cref="System.IO.Stream"/> of the binary contents of the video.</returns>
        public Stream Stream(Video video) => StreamAsync(video).Result;

        /// <summary>
        /// Streams the binary contents of the specified <see cref="Video"/> as an asynchronous operation.
        /// </summary>
        /// <param name="video">The <see cref="Video"/> to stream.</param>
        /// <returns>A <see cref="Task"/> of a <see cref="System.IO.Stream"/> of the binary contents of the video.</returns>
        public async Task<Stream> StreamAsync(Video video)
        {
            return await client
                .GetStreamAsync(video.Uri)
                .ConfigureAwait(false);
        }
    }
}
