using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary.Helpers;

namespace VideoLibrary
{
    public static class Client
    {
        public static Client<T> For<T>(ServiceBase<T> baseService) 
            where T : Video => new Client<T>(baseService);
    }

    public class Client<T> : IService<T>, IAsyncService<T>, IDisposable 
        where T : Video
    {
        private bool disposed = false;
        private readonly ServiceBase<T> baseService;
        private readonly HttpClient client;

        private Task<string> SourceFactory(string address) =>
            client.GetStringAsync(address);

        internal Client(ServiceBase<T> baseService)
        {
            Require.NotNull(baseService, nameof(baseService));

            this.baseService = baseService;
            this.client = baseService.MakeClient();
        }

        #region IDisposable

        ~Client()
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

        public T GetVideo(string videoUri) =>
            baseService.GetVideo(videoUri, SourceFactory);

        public IEnumerable<T> GetAllVideos(string videoUri) =>
            baseService.GetAllVideos(videoUri, SourceFactory);

        public Task<T> GetVideoAsync(string videoUri) =>
            baseService.GetVideoAsync(videoUri, SourceFactory);

        public Task<IEnumerable<T>> GetAllVideosAsync(string videoUri) =>
            baseService.GetAllVideosAsync(videoUri, SourceFactory);
    }
}
