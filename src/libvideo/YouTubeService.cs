using VideoLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace VideoLibrary
{
    /// <summary>
    /// Provides the entry point for the YouTube-specific API.
    /// </summary>
    public class YouTubeService : ServiceBase<YouTubeVideo>
    {
        /// <summary>
        /// Gets the default instance of the <see cref="YouTubeService"/> class.
        /// </summary>
        public static YouTubeService Default { get; } = new YouTubeService();

        internal async override Task<IEnumerable<YouTubeVideo>> GetAllVideosAsync(
            string videoUri, Func<string, Task<string>> sourceFactory)
        {
            if (!TryNormalize(videoUri, out videoUri))
                throw new ArgumentException("URL is not a valid YouTube URL!");

            string source = await
                sourceFactory(videoUri)
                .ConfigureAwait(false);

            return ParseVideos(source);
        }

        private bool TryNormalize(string videoUri, out string normalized)
        {
            // If you fix something in here, please be sure to fix in 
            // DownloadUrlResolver.TryNormalizeYoutubeUrl as well.

            normalized = null;

            var builder = new StringBuilder(videoUri);

            videoUri = builder.Replace("youtu.be/", "youtube.com/watch?v=")
                .Replace("youtube.com/embed/", "youtube.com/watch?v=")
                .Replace("/v/", "/watch?v=")
                .Replace("/watch#", "/watch?")
                .ToString();

            string value;

            if (!Query.TryGetParam("v", videoUri, out value))
                return false;

            normalized = "https://youtube.com/watch?v=" + value;
            return true;
        }

        private IEnumerable<YouTubeVideo> ParseVideos(string source)
        {
            string title = Html.GetNodeValue("title", source);

            string map = Json.GetKeyValue("url_encoded_fmt_stream_map", source);
            map = map.Substring(map.IndexOf("url="));

            var links = map.Split(',')
                .Select(QuerySelector);

            foreach (var uri in links)
                yield return new YouTubeVideo(title, uri, GetFormatCode(uri));

            string adaptiveMap = Json.GetKeyValue("adaptive_fmts", source);

            links = adaptiveMap.Split(',')
                .Select(QuerySelector);

            foreach (var uri in links)
                yield return new YouTubeVideo(title, uri, GetFormatCode(uri));
        }

        // TODO: Consider making this static...
        private string QuerySelector(string query)
        {
            string uri = query.Substring(
                query.IndexOf("https%3A%2F%2F"));
            // bool encrypted = false; // TODO: Use this.
            string signature;

            if (Query.TryGetParam("s", query, out signature))
            {
                // encrypted = true;
                uri += Query.GetSignatureAndHost(signature, query);
            }
            else if (Query.TryGetParam("sig", query, out signature))
                uri += Query.GetSignatureAndHost(signature, query);

            uri = WebUtility.UrlDecode(
                WebUtility.UrlDecode(uri));

            int index = uri.IndexOf(@"\u0026");
            if (index != -1)
                uri = uri.Substring(0, index); // Got stuck on this for a week.

            if (!Query.ContainsParam("ratebypass", uri))
                uri += "&ratebypass=yes";

            return uri;
        }

        private static int GetFormatCode(string uri) =>
            int.Parse(Query.GetParam("itag", uri));

        #region HttpService

        // Called internally by ClientService to 
        // initialize the HttpClient. Not 
        // intended for direct consumption.

        internal override HttpClient ClientFactory()
        {
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

            return new HttpClient(handler);
        }

        #endregion
    }
}