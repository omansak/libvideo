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
    /// Provides the base class for most services.
    /// </summary>
    public abstract class ServiceBase<T> : IService<T>, IAsyncService<T>
        where T : Video
    {
        /// <summary>
        /// Chooses which <see cref="T"/> to return from a pool of <see cref="T"/> objects.
        /// </summary>
        /// <param name="videos">The pool of <see cref="T"/> objects.</param>
        /// <returns>The suggested <see cref="T"/> to return.</returns>
        protected virtual T VideoSelector(IEnumerable<T> videos) =>
            videos.First();

        #region Synchronous wrappers
        /// <summary>
        /// Retrieves the <see cref="T"/> specified by <paramref name="videoUri"/>.
        /// </summary>
        /// <param name="videoUri">The URL to visit.</param>
        /// <returns>A <see cref="T"/> representing the information from <paramref name="videoUri"/>.</returns>
        public T GetVideo(string videoUri) =>
            GetVideoAsync(videoUri).GetAwaiter().GetResult();

        internal T GetVideo(string videoUri, 
            Func<string, Task<string>> sourceFactory) =>
            GetVideoAsync(videoUri, sourceFactory).GetAwaiter().GetResult();

        /// <summary>
        /// Retrieves the <see cref="IEnumerable{T}"/> specified by <paramref name="videoUri"/>.
        /// </summary>
        /// <param name="videoUri">The URL to visit.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> representing the information from <paramref name="videoUri"/>.</returns>
        public IEnumerable<T> GetAllVideos(string videoUri) =>
            GetAllVideosAsync(videoUri).GetAwaiter().GetResult();

        internal IEnumerable<T> GetAllVideos(string videoUri, 
            Func<string, Task<string>> sourceFactory) =>
            GetAllVideosAsync(videoUri, sourceFactory).GetAwaiter().GetResult();
        #endregion

        /// <summary>
        /// Retrieves the <see cref="T"/> specified by <paramref name="videoUri"/> as an asynchronous operation.
        /// </summary>
        /// <param name="videoUri">The URL to visit.</param>
        /// <returns>A <see cref="Task"/> of the <see cref="T"/> representing the information from <paramref name="videoUri"/>.</returns>
        public async Task<T> GetVideoAsync(string videoUri)
        {
            using (var wrapped = ClientService.For(this))
            {
                return await wrapped
                    .GetVideoAsync(videoUri)
                    .ConfigureAwait(false);
            }
        }

        internal async Task<T> GetVideoAsync(
            string videoUri, Func<string, Task<string>> sourceFactory) =>
            VideoSelector(await GetAllVideosAsync(
                videoUri, sourceFactory).ConfigureAwait(false));

        /// <summary>
        /// Retrieves the <see cref="IEnumerable{T}"/> specified by <paramref name="videoUri"/> as an asynchronous operation.
        /// </summary>
        /// <param name="videoUri">The URL to visit.</param>
        /// <returns>A <see cref="Task"/> of the <see cref="IEnumerable{T}"/> representing the information from <paramref name="videoUri"/>.</returns>
        public async Task<IEnumerable<T>> GetAllVideosAsync(string videoUri)
        {
            using (var wrapped = ClientService.For(this))
            {
                return await wrapped
                    .GetAllVideosAsync(videoUri)
                    .ConfigureAwait(false);
            }
        }

        internal abstract Task<IEnumerable<T>> GetAllVideosAsync(
            string videoUri, Func<string, Task<string>> sourceFactory);

        internal virtual HttpClient ClientFactory() => new HttpClient();
    }
}
