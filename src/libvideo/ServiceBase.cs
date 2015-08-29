using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    /// <summary>
    /// Provides the base class for most services.
    /// </summary>
    public abstract class ServiceBase : IService, IAsyncService
    {
        /// <summary>
        /// Chooses which <see cref="Video"/> to return from a pool of <see cref="Video"/> objects.
        /// </summary>
        /// <param name="videos">The pool of <see cref="Video"/> objects.</param>
        /// <returns>The suggested <see cref="Video"/> to return.</returns>
        protected virtual Video VideoSelector(IEnumerable<Video> videos) =>
            videos.First();

        #region Synchronous wrappers
        /// <summary>
        /// Retrieves the <see cref="Video"/> specified by <paramref name="videoUri"/>.
        /// </summary>
        /// <param name="videoUri">The URL to visit.</param>
        /// <returns>A <see cref="Video"/> representing the information from <paramref name="videoUri"/>.</returns>
        public Video GetVideo(string videoUri) =>
            GetVideoAsync(videoUri).GetAwaiter().GetResult();

        internal Video GetVideo(string videoUri, 
            Func<string, Task<string>> sourceFactory) =>
            GetVideoAsync(videoUri, sourceFactory).GetAwaiter().GetResult();

        /// <summary>
        /// Retrieves the <see cref="IEnumerable{Video}"/> specified by <paramref name="videoUri"/>.
        /// </summary>
        /// <param name="videoUri">The URL to visit.</param>
        /// <returns>A <see cref="IEnumerable{Video}"/> representing the information from <paramref name="videoUri"/>.</returns>
        public IEnumerable<Video> GetAllVideos(string videoUri) =>
            GetAllVideosAsync(videoUri).GetAwaiter().GetResult();

        internal IEnumerable<Video> GetAllVideos(string videoUri, 
            Func<string, Task<string>> sourceFactory) =>
            GetAllVideosAsync(videoUri, sourceFactory).GetAwaiter().GetResult();
        #endregion

        /// <summary>
        /// Retrieves the <see cref="Video"/> specified by <paramref name="videoUri"/> as an asynchronous operation.
        /// </summary>
        /// <param name="videoUri">The URL to visit.</param>
        /// <returns>A <see cref="Task"/> of the <see cref="Video"/> representing the information from <paramref name="videoUri"/>.</returns>
        public async Task<Video> GetVideoAsync(string videoUri)
        {
            using (var wrapped = new ClientService(this))
            {
                return await wrapped
                    .GetVideoAsync(videoUri)
                    .ConfigureAwait(false);
            }
        }

        internal async Task<Video> GetVideoAsync(
            string videoUri, Func<string, Task<string>> sourceFactory) =>
            VideoSelector(await GetAllVideosAsync(
                videoUri, sourceFactory).ConfigureAwait(false));

        /// <summary>
        /// Retrieves the <see cref="IEnumerable{Video}"/> specified by <paramref name="videoUri"/> as an asynchronous operation.
        /// </summary>
        /// <param name="videoUri">The URL to visit.</param>
        /// <returns>A <see cref="Task"/> of the <see cref="IEnumerable{Video}"/> representing the information from <paramref name="videoUri"/>.</returns>
        public async Task<IEnumerable<Video>> GetAllVideosAsync(string videoUri)
        {
            using (var wrapped = new ClientService(this))
            {
                return await wrapped
                    .GetAllVideosAsync(videoUri)
                    .ConfigureAwait(false);
            }
        }

        internal abstract Task<IEnumerable<Video>> GetAllVideosAsync(
            string videoUri, Func<string, Task<string>> sourceFactory);
    }
}
