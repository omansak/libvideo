using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace VideoLibrary.Debug
{
    class CustomHandler
    {
        public HttpMessageHandler GetHandler()
        {
            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(new Cookie("CONSENT", "YES+cb", "/", "youtube.com"));
            return new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookieContainer
            };

        }
    }
    class CustomYouTube : YouTube
    {
        private long chunkSize = 10_485_760;
        private long _fileSize = 0L;
        private HttpClient _client = new HttpClient();
        protected override HttpClient MakeClient(HttpMessageHandler handler)
        {
            return base.MakeClient(handler);
        }
        protected override HttpMessageHandler MakeHandler()
        {
            return new CustomHandler().GetHandler();
        }
        public async Task CreateDownloadAsync(Uri uri, string filePath, IProgress<Tuple<long, long>> progress)
        {
            var totalBytesCopied = 0L;
            _fileSize = await GetContentLengthAsync(uri.AbsoluteUri) ?? 0;
            if (_fileSize == 0)
            {
                throw new Exception("File has no any content !");
            }
            using (Stream output = File.OpenWrite(filePath))
            {
                var segmentCount = (int)Math.Ceiling(1.0 * _fileSize / chunkSize);
                for (var i = 0; i < segmentCount; i++)
                {
                    var from = i * chunkSize;
                    var to = (i + 1) * chunkSize - 1;
                    var request = new HttpRequestMessage(HttpMethod.Get, uri);
                    request.Headers.Range = new RangeHeaderValue(from, to);
                    using (request)
                    {
                        // Download Stream
                        var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                        if (response.IsSuccessStatusCode)
                            response.EnsureSuccessStatusCode();
                        var stream = await response.Content.ReadAsStreamAsync();
                        //File Steam
                        var buffer = new byte[81920];
                        int bytesCopied;
                        do
                        {
                            bytesCopied = await stream.ReadAsync(buffer, 0, buffer.Length);
                            output.Write(buffer, 0, bytesCopied);
                            totalBytesCopied += bytesCopied;
                            progress.Report(new Tuple<long, long>(totalBytesCopied, _fileSize));
                        } while (bytesCopied > 0);
                    }
                }
            }
        }
        private async Task<long?> GetContentLengthAsync(string requestUri, bool ensureSuccess = true)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Head, requestUri))
            {
                var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                if (ensureSuccess)
                    response.EnsureSuccessStatusCode();
                return response.Content.Headers.ContentLength;
            }
        }
    }
    class Test
    {
        public void Run()
        {
            // Custom Youtube
            var youtube = new CustomYouTube();
            var videos = youtube.GetAllVideosAsync("https://www.youtube.com/watch?v=qK_NeRZOdq4").GetAwaiter().GetResult();
            var maxResolution = videos.First(i => i.Resolution == videos.Max(j => j.Resolution));
            youtube
                .CreateDownloadAsync(
                new Uri(maxResolution.Uri),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), maxResolution.FullName),
                new Progress<Tuple<long, long>>((Tuple<long, long> v) =>
                 {
                     var percent = (int)((v.Item1 * 100) / v.Item2);
                     Console.Write(string.Format("Downloading.. ( % {0} ) {1} / {2} MB\r", percent, (v.Item1 / (double)(1024 * 1024)).ToString("N"), (v.Item2 / (double)(1024 * 1024)).ToString("N")));
                 }))
                .GetAwaiter().GetResult();
        }
    }
}
