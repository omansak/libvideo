using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public abstract class ServiceBase : IService, IAsyncService
    {
        protected virtual Video VideoSelector(IEnumerable<Video> videos) =>
            videos.First();

        #region Synchronous wrappers
        public Video GetVideo(string videoUri) =>
            GetVideoAsync(videoUri).Result;

        internal Video GetVideo(string videoUri, 
            Func<string, Task<string>> sourceFactory) =>
            GetVideoAsync(videoUri, sourceFactory).Result;

        public IEnumerable<Video> GetAllVideos(string videoUri) =>
            GetAllVideosAsync(videoUri).Result;

        internal IEnumerable<Video> GetAllVideos(string videoUri, 
            Func<string, Task<string>> sourceFactory) =>
            GetAllVideosAsync(videoUri, sourceFactory).Result;
        #endregion

        public async Task<Video> GetVideoAsync(string videoUri)
        {
            using (var wrapped = new SingleClientService(this))
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

        public async Task<IEnumerable<Video>> GetAllVideosAsync(string videoUri)
        {
            using (var wrapped = new SingleClientService(this))
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
