using System;
using System.Collections.Generic;
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
        public byte[] Download(string videoUri) =>
            DownloadAsync(videoUri).Result;

        // TODO: Make this use yield to download videos lazily.
        public IEnumerable<byte[]> DownloadMany(string videoUri) =>
            DownloadManyAsync(videoUri).Result;

        public string GetUri(string videoUri) =>
            GetUriAsync(videoUri).Result;
        public string GetUri(string videoUri, Func<string, string> sourceFactory) =>
            GetUriAsync(videoUri, sourceFactory).Result;
        public string GetUri(string videoUri, Func<string, Task<string>> sourceFactory) =>
            GetUriAsync(videoUri, sourceFactory).Result;

        public IEnumerable<string> GetAllUris(string videoUri) =>
            GetAllUrisAsync(videoUri).Result;
        public IEnumerable<string> GetAllUris(string videoUri, Func<string, string> sourceFactory) =>
            GetAllUrisAsync(videoUri, sourceFactory).Result;
        public IEnumerable<string> GetAllUris(string videoUri, Func<string, Task<string>> sourceFactory) =>
            GetAllUrisAsync(videoUri, sourceFactory).Result;

        public Video GetVideo(string videoUri) =>
            GetVideoAsync(videoUri).Result;
        public Video GetVideo(string videoUri, Func<string, string> sourceFactory) =>
            GetVideoAsync(videoUri, sourceFactory).Result;
        public Video GetVideo(string videoUri, Func<string, Task<string>> sourceFactory) =>
            GetVideoAsync(videoUri, sourceFactory).Result;

        public IEnumerable<Video> GetAllVideos(string videoUri) =>
            GetAllVideosAsync(videoUri).Result;
        public IEnumerable<Video> GetAllVideos(string videoUri, Func<string, string> sourceFactory) =>
            GetAllVideosAsync(videoUri, sourceFactory).Result;
        public IEnumerable<Video> GetAllVideos(string videoUri, Func<string, Task<string>> sourceFactory) =>
            GetAllVideosAsync(videoUri, sourceFactory).Result;
        #endregion

        public async Task<byte[]> DownloadAsync(string videoUri)
        {
            using (var client = new HttpClient())
            {
                string uri = await GetUriAsync(videoUri,
                    address => client.GetStringAsync(address))
                    .ConfigureAwait(false); // Never forget to ConfigureAwait(false) or our synchronous wrappers will hang!

                return await client
                    .GetByteArrayAsync(uri)
                    .ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<byte[]>> DownloadManyAsync(string videoUri)
        {
            using (var client = new HttpClient())
            {
                var links = await GetAllUrisAsync(videoUri,
                    address => client.GetStringAsync(address))
                    .ConfigureAwait(false);

                var tasks = links.Select(
                    uri => client.GetByteArrayAsync(uri)); // No ConfigureAwait(false) needed here.

                return await Task.WhenAll(tasks)
                    .ConfigureAwait(false); // This is not lazy. You cannot use async and yield simultaneously.
            }
        }

        public async Task<string> GetUriAsync(string videoUri)
        {
            using (var client = new HttpClient())
            {
                return await GetUriAsync(videoUri,
                    uri => client.GetStringAsync(uri))
                    .ConfigureAwait(false);
            }
        }

        public Task<string> GetUriAsync(string videoUri, Func<string, string> sourceFactory) =>
            GetUriAsync(videoUri, uri => Task.FromResult(sourceFactory(uri)));

        public async Task<string> GetUriAsync(string videoUri, Func<string, Task<string>> sourceFactory) =>
            (await GetVideoAsync(videoUri, sourceFactory).ConfigureAwait(false)).Uri;

        public async Task<IEnumerable<string>> GetAllUrisAsync(string videoUri)
        {
            using (var client = new HttpClient())
            {
                return await GetAllUrisAsync(videoUri,
                    uri => client.GetStringAsync(uri))
                    .ConfigureAwait(false);
            }
        }

        public Task<IEnumerable<string>> GetAllUrisAsync(string videoUri, Func<string, string> sourceFactory) =>
            GetAllUrisAsync(videoUri, uri => Task.FromResult(sourceFactory(uri)));

        public async Task<IEnumerable<string>> GetAllUrisAsync(string videoUri, Func<string, Task<string>> sourceFactory) =>
            (await GetAllVideosAsync(videoUri, sourceFactory).ConfigureAwait(false)).Select(v => v.Uri);

        public async Task<Video> GetVideoAsync(string videoUri)
        {
            using (var client = new HttpClient())
            {
                return await GetVideoAsync(videoUri,
                    uri => client.GetStringAsync(uri))
                    .ConfigureAwait(false);
            }
        }

        public Task<Video> GetVideoAsync(string videoUri, Func<string, string> sourceFactory) =>
            GetVideoAsync(videoUri, uri => Task.FromResult(sourceFactory(uri)));

        public async Task<Video> GetVideoAsync(string videoUri, Func<string, Task<string>> sourceFactory) =>
            VideoSelector(await GetAllVideosAsync(videoUri, sourceFactory).ConfigureAwait(false));

        public async Task<IEnumerable<Video>> GetAllVideosAsync(string videoUri)
        {
            using (var client = new HttpClient())
            {
                return await GetAllVideosAsync(videoUri,
                    uri => client.GetStringAsync(uri))
                    .ConfigureAwait(false);
            }
        }

        public Task<IEnumerable<Video>> GetAllVideosAsync(string videoUri, Func<string, string> sourceFactory) =>
            GetAllVideosAsync(videoUri, uri => Task.FromResult(sourceFactory(uri)));

        public abstract Task<IEnumerable<Video>> GetAllVideosAsync(string videoUri, Func<string, Task<string>> sourceFactory);
    }
}
