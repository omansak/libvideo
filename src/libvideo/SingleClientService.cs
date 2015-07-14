using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public class SingleClientService : IService, IAsyncService, IDisposable
    {
        private bool Disposed = false;
        private readonly ServiceBase BaseService;
        private readonly HttpClient Client = new HttpClient();

        private Func<string, Task<string>> SourceFactory =>
            address => Client.GetStringAsync(address);

        public SingleClientService(ServiceBase baseService)
        {
            if (baseService == null)
                throw new ArgumentNullException(nameof(baseService));

            this.BaseService = baseService;
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

        public byte[] Download(string videoUri) =>
            DownloadAsync(videoUri).Result;

        public async Task<byte[]> DownloadAsync(string videoUri)
        {
            var uri = await BaseService
                .GetUriAsync(videoUri, SourceFactory)
                .ConfigureAwait(false);

            return await Client
                .GetByteArrayAsync(uri)
                .ConfigureAwait(false);
        }

        public IEnumerable<byte[]> DownloadMany(string videoUri) =>
            DownloadManyAsync(videoUri).Result;

        public async Task<IEnumerable<byte[]>> DownloadManyAsync(string videoUri)
        {
            var links = await BaseService
                .GetAllUrisAsync(videoUri, SourceFactory)
                .ConfigureAwait(false);

            var tasks = links.Select(
                uri => Client.GetByteArrayAsync(uri)); // No ConfigureAwait(false) needed here.

            return await Task.WhenAll(tasks)
                .ConfigureAwait(false);
        }

        public IEnumerable<string> GetAllUris(string videoUri) =>
            BaseService.GetAllUris(videoUri, SourceFactory);

        public Task<IEnumerable<string>> GetAllUrisAsync(string videoUri) =>
            BaseService.GetAllUrisAsync(videoUri, SourceFactory);

        public string GetUri(string videoUri) =>
            BaseService.GetUri(videoUri, SourceFactory);

        public Task<string> GetUriAsync(string videoUri) =>
            BaseService.GetUriAsync(videoUri, SourceFactory);

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
