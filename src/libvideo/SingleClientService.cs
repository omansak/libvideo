using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public class SingleClientService : IService, IAsyncService, IDisposable
    {
        private bool Disposed = false;
        private readonly ServiceBase BaseService;
        private readonly HttpClient Client;

        private Func<string, Task<string>> SourceFactory =>
            address => Client.GetStringAsync(address);

        public SingleClientService(ServiceBase baseService)
        {
            if (baseService == null)
                throw new ArgumentNullException(nameof(baseService));

            this.BaseService = baseService;

            var handler = new HttpClientHandler();

            // Be very careful because if any exceptions are 
            // thrown between here && the HttpClient ctor, 
            // we will leak resources.

            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression =
                    DecompressionMethods.GZip |
                    DecompressionMethods.Deflate;
            }

            this.Client = new HttpClient(handler);
        }

        #region IDisposable implementation
        ~SingleClientService()
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
            if (Disposed)
                return;
            Disposed = true;

            if (disposing)
            {
                if (Client != null)
                    Client.Dispose();
            }
        }
        #endregion

        public Video GetVideo(string videoUri) =>
            BaseService.GetVideo(videoUri, SourceFactory);

        public IEnumerable<Video> GetAllVideos(string videoUri) =>
            BaseService.GetAllVideos(videoUri, SourceFactory);

        public Task<Video> GetVideoAsync(string videoUri) =>
            BaseService.GetVideoAsync(videoUri, SourceFactory);

        public Task<IEnumerable<Video>> GetAllVideosAsync(string videoUri) =>
            BaseService.GetAllVideosAsync(videoUri, SourceFactory);
    }
}
