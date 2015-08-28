using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    /// <summary>
    /// A class that facilitates <see cref="HttpClient"/> reuse over multiple YouTube visits.
    /// </summary>
    public class SingleClientService : IService, IAsyncService, IDisposable
    {
        private bool Disposed = false;
        private readonly ServiceBase BaseService;
        private readonly HttpClient Client;

        private Func<string, Task<string>> SourceFactory =>
            address => Client.GetStringAsync(address);

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBase"/> class with the specified base service.
        /// </summary>
        /// <param name="baseService">The base service on which to send requests.</param>
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

        /// <summary>
        /// Frees any resources held by this instance of the <see cref="ServiceBase"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees resources held by this instance of the <see cref="ServiceBase"/> class.
        /// </summary>
        /// <param name="disposing">True if managed resources should be freed; otherwise, False.</param>
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

        /// <summary>
        /// Retrieves the <see cref="Video"/> specified by <paramref name="videoUri"/>.
        /// </summary>
        /// <param name="videoUri">The URL to visit.</param>
        /// <returns>A <see cref="Video"/> representing the information from <paramref name="videoUri"/>.</returns>
        public Video GetVideo(string videoUri) =>
            BaseService.GetVideo(videoUri, SourceFactory);

        /// <summary>
        /// Retrieves the <see cref="IEnumerable{Video}"/> specified by <paramref name="videoUri"/>.
        /// </summary>
        /// <param name="videoUri">The URL to visit.</param>
        /// <returns>A <see cref="IEnumerable{Video}"/> representing the information from <paramref name="videoUri"/>.</returns>
        public IEnumerable<Video> GetAllVideos(string videoUri) =>
            BaseService.GetAllVideos(videoUri, SourceFactory);

        /// <summary>
        /// Retrieves the <see cref="Video"/> specified by <paramref name="videoUri"/> as an asynchronous operation.
        /// </summary>
        /// <param name="videoUri">The URL to visit.</param>
        /// <returns>A <see cref="Task"/> of the <see cref="Video"/> representing the information from <paramref name="videoUri"/>.</returns>
        public Task<Video> GetVideoAsync(string videoUri) =>
            BaseService.GetVideoAsync(videoUri, SourceFactory);

        /// <summary>
        /// Retrieves the <see cref="IEnumerable{Video}"/> specified by <paramref name="videoUri"/> as an asynchronous operation.
        /// </summary>
        /// <param name="videoUri">The URL to visit.</param>
        /// <returns>A <see cref="Task"/> of the <see cref="IEnumerable{Video}"/> representing the information from <paramref name="videoUri"/>.</returns>
        public Task<IEnumerable<Video>> GetAllVideosAsync(string videoUri) =>
            BaseService.GetAllVideosAsync(videoUri, SourceFactory);
    }
}
