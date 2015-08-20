using VideoLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public class YouTubeService : ServiceBase
    {
        public async override Task<IEnumerable<Video>> GetAllVideosAsync(
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

            videoUri = builder.Replace(" ", string.Empty)
                .Replace("youtu.be/", "youtube.com/watch?v=")
                .Replace("www.youtube", "youtube")
                .Replace("youtube.com/embed/", "youtube.com/watch?v=")
                .Replace("/v/", "/watch?v=")
                .Replace("/watch#", "/watch?")
                .ToString();

            string value;

            if (!Query.TryGetParamValue("v", videoUri, out value))
                return false;

            normalized = "http://youtube.com/watch?v=" + value;
            return true;
        }

        private IEnumerable<Video> ParseVideos(string source)
        {
            string title = Html.GetNodeValue("title", source);

            string map = Json.GetKeyValue("url_encoded_fmt_stream_map", source);
            map = map.Substring(map.IndexOf("url="));

            var links = map.Split(',')
                .Select(QuerySelector);

            foreach (var uri in links)
                yield return new Video(title, uri, WebSites.YouTube, GetFormatCode(uri));

            string adaptiveMap = Json.GetKeyValue("adaptive_fmts", source);

            links = adaptiveMap.Split(',')
                .Select(QuerySelector);

            foreach (var uri in links)
                yield return new Video(title, uri, WebSites.YouTube, GetFormatCode(uri));
        }

        // TODO: Consider making this static...
        private string QuerySelector(string query)
        {
            string uri = query.Substring(
                query.IndexOf("https%3A%2F%2F"));
            // bool encrypted = false; // TODO: Use this.
            string signature;

            if (Query.TryGetParamValue("s", query, out signature))
            {
                // encrypted = true;
                uri += Query.ParamsFor(signature, query);
            }
            else if (Query.TryGetParamValue("sig", query, out signature))
                uri += Query.ParamsFor(signature, query);

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
            Int32.Parse(Query.GetParamValue("itag", uri));
    }
}