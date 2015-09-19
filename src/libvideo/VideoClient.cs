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

        public byte[] GetBytes(Video video) => GetBytesAsync(video).GetAwaiter().GetResult();

        public async Task<byte[]> GetBytesAsync(Video video)
        {
            string uri = await video.GetUriAsync();
            return await client
                .GetByteArrayAsync(uri)
                .ConfigureAwait(false);
        }

        public Stream Stream(Video video) => StreamAsync(video).GetAwaiter().GetResult();

        public async Task<Stream> StreamAsync(Video video)
        {
            string uri = await video.GetUriAsync();
            return await client
                .GetStreamAsync(uri)
                .ConfigureAwait(false);
        }
    }
}
