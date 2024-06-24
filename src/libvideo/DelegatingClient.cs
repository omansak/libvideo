using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public class DelegatingClient : IDisposable
    {
        private bool disposed = false;
        private readonly HttpClient client;

        public DelegatingClient()
        {
            this.client = MakeClient();
        }

        #region IDisposable

        ~DelegatingClient()
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
        {
            var handler = new HttpClientHandler();

            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression =
                    DecompressionMethods.GZip |
                    DecompressionMethods.Deflate;
            }

            return handler;
        }

        protected virtual HttpClient MakeClient(HttpMessageHandler handler)
        {
            return new HttpClient(handler);
        }

        #endregion

        #region Synchronous wrappers

        public HttpResponseMessage Get(string uri) =>
            GetAsync(uri).GetAwaiter().GetResult();

        public byte[] GetByteArray(string uri) =>
            GetByteArrayAsync(uri).GetAwaiter().GetResult();

        public Stream GetStream(string uri) =>
            GetStreamAsync(uri).GetAwaiter().GetResult();

        public string GetString(string uri) =>
            GetStringAsync(uri).GetAwaiter().GetResult();

        #endregion

        #region HttpClient wrappers

        // TODO: Support other kinds of HTTP requests, 
        // such as PUT, POST, DELETE, etc.

        public Task<HttpResponseMessage> GetAsync(string uri) =>
            client.GetAsync(uri);

        public Task<byte[]> GetByteArrayAsync(string uri) =>
            client.GetByteArrayAsync(uri);

        public Task<Stream> GetStreamAsync(string uri) =>
            client.GetStreamAsync(uri);

        public Task<string> GetStringAsync(string uri) =>
            client.GetStringAsync(uri);

        #endregion
    }
}
