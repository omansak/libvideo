using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary.Helpers;

namespace VideoLibrary
{
    public abstract class ServiceBase<T> : IService<T>, IAsyncService<T>
        where T : Video
    {
        internal virtual T VideoSelector(IEnumerable<T> videos) =>
            videos.First();

        #region Synchronous wrappers
        public T GetVideo(string videoUri) =>
            GetVideoAsync(videoUri).GetAwaiter().GetResult();

        internal T GetVideo(string videoUri,
            Func<string, Task<string>> sourceFactory) =>
            GetVideoAsync(videoUri, sourceFactory).GetAwaiter().GetResult();

        public IEnumerable<T> GetAllVideos(string videoUri) =>
            GetAllVideosAsync(videoUri).GetAwaiter().GetResult();

        internal IEnumerable<T> GetAllVideos(string videoUri, 
            Func<string, Task<string>> sourceFactory) =>
            GetAllVideosAsync(videoUri, sourceFactory).GetAwaiter().GetResult();
        #endregion

        public async Task<T> GetVideoAsync(string videoUri)
        {
            using (var wrapped = Client.For(this))
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

        public async Task<IEnumerable<T>> GetAllVideosAsync(string videoUri)
        {
            using (var wrapped = Client.For(this))
            {
                return await wrapped
                    .GetAllVideosAsync(videoUri)
                    .ConfigureAwait(false);
            }
        }

        internal abstract Task<IEnumerable<T>> GetAllVideosAsync(
            string videoUri, Func<string, Task<string>> sourceFactory);

        internal HttpClient MakeClient() =>
            MakeClient(MakeHandler());

        protected virtual HttpMessageHandler MakeHandler()
        {
            var handler = new HttpClientHandler();

            // Be very careful because if any exceptions are 
            // thrown between here && the HttpClient ctor, 
            // we may leak resources.

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
            return new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMilliseconds(int.MaxValue) // Longest TimeSpan HttpClient will accept
            };
        }
    }
}
