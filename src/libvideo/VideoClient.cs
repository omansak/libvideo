using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public class VideoClient : IDisposable
    {
        private bool disposed = false;
        private readonly HttpClient client = new HttpClient();

        public VideoClient()
        {
        }

        #region IDisposable

        ~VideoClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        public byte[] GetBytes(Video video) => GetBytesAsync(video).Result;

        public async Task<byte[]> GetBytesAsync(Video video)
        {
            return await client
                .GetByteArrayAsync(video.Uri)
                .ConfigureAwait(false);
        }

        public Stream Stream(Video video) => StreamAsync(video).Result;

        public async Task<Stream> StreamAsync(Video video)
        {
            return await client
                .GetStreamAsync(video.Uri)
                .ConfigureAwait(false);
        }
    }
}
