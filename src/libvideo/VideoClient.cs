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
        private readonly HttpClient client;

        public VideoClient()
        {
            this.client = MakeClient();
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

        #region MakeClient/MakeHandler

        private HttpClient MakeClient() =>
            MakeClient(MakeHandler());

        protected virtual HttpMessageHandler MakeHandler() 
            => new HttpClientHandler();

        protected virtual HttpClient MakeClient(HttpMessageHandler handler)
        {
            return new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMilliseconds(int.MaxValue) // Longest TimeSpan HttpClient will accept
            };
        }

        #endregion

        public byte[] GetBytes(Video video) => 
            GetBytesAsync(video).GetAwaiter().GetResult();

        public async Task<byte[]> GetBytesAsync(Video video)
        {
            string uri = await video.GetUriAsync();
            return await client
                .GetByteArrayAsync(uri)
                .ConfigureAwait(false);
        }

        public Stream Stream(Video video) => 
            StreamAsync(video).GetAwaiter().GetResult();

        public async Task<Stream> StreamAsync(Video video)
        {
            string uri = await video.GetUriAsync();
            return await client
                .GetStreamAsync(uri)
                .ConfigureAwait(false);
        }
    }
}
