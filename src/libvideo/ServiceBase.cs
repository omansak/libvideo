using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


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
            // Cookie
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri(YouTube.YoutubeUrl), new Cookie("CONSENT", "YES+cb", "/", ".youtube.com"));
            // Handler
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookieContainer
            };
            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            return handler;
        }

        protected virtual HttpClient MakeClient(HttpMessageHandler handler)
        {
            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36 Edg/89.0.774.76"
            );
            return new HttpClient(handler);
        }
    }
}
