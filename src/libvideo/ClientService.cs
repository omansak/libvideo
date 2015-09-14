using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public static class ClientService
    {
        public static ClientService<T> For<T>(ServiceBase<T> baseService) 
            where T : Video => new ClientService<T>(baseService);
    }

    /// <summary>
    /// A class that facilitates <see cref="HttpClient"/> reuse over multiple YouTube visits.
    /// </summary>
    public class ClientService<T> : IService<T>, IAsyncService<T>, IDisposable 
        where T : Video
    {
        private bool disposed = false;
        private readonly ServiceBase<T> baseService;
        private readonly HttpClient client;

        private Task<string> SourceFactory(string address) =>
            client.GetStringAsync(address);

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientService"/> class with the specified base service.
        /// </summary>
        /// <param name="baseService">The base service on which to send requests.</param>
        internal ClientService(ServiceBase<T> baseService)
        {
            if (baseService == null)
                throw new ArgumentNullException(nameof(baseService));

            this.baseService = baseService;
            this.client = baseService.ClientFactory();
        }

        #region IDisposable

        ~ClientService()
        {
            Dispose(false);
        }

        /// <summary>
        /// Frees any resources held by this instance of the <see cref="ClientService"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees resources held by this instance of the <see cref="ClientService"/> class.
        /// </summary>
        /// <param name="disposing">True if managed resources should be freed; otherwise, False.</param>
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
        /// Retrieves the <see cref="T"/> specified by <paramref name="videoUri"/>.
        /// </summary>
        /// <param name="videoUri">The URL to visit.</param>
        /// <returns>A <see cref="T"/> representing the information from <paramref name="videoUri"/>.</returns>
        public T GetVideo(string videoUri) =>
            baseService.GetVideo(videoUri, SourceFactory);

        /// <summary>
        /// Retrieves the <see cref="IEnumerable{T}"/> specified by <paramref name="videoUri"/>.
        /// </summary>
        /// <param name="videoUri">The URL to visit.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> representing the information from <paramref name="videoUri"/>.</returns>
        public IEnumerable<T> GetAllVideos(string videoUri) =>
            baseService.GetAllVideos(videoUri, SourceFactory);

        /// <summary>
        /// Retrieves the <see cref="T"/> specified by <paramref name="videoUri"/> as an asynchronous operation.
        /// </summary>
        /// <param name="videoUri">The URL to visit.</param>
        /// <returns>A <see cref="Task"/> of the <see cref="T"/> representing the information from <paramref name="videoUri"/>.</returns>
        public Task<T> GetVideoAsync(string videoUri) =>
            baseService.GetVideoAsync(videoUri, SourceFactory);

        /// <summary>
        /// Retrieves the <see cref="IEnumerable{T}"/> specified by <paramref name="videoUri"/> as an asynchronous operation.
        /// </summary>
        /// <param name="videoUri">The URL to visit.</param>
        /// <returns>A <see cref="Task"/> of the <see cref="IEnumerable{T}"/> representing the information from <paramref name="videoUri"/>.</returns>
        public Task<IEnumerable<T>> GetAllVideosAsync(string videoUri) =>
            baseService.GetAllVideosAsync(videoUri, SourceFactory);
    }
}
