using VideoLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public class YouTube : ServiceBase<YouTubeVideo>
    {
        private const string Playback = "videoplayback";

        public static YouTube Default { get; } = new YouTube();

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

            var query = new Query(videoUri);

            string value;

            if (!query.TryGetValue("v", out value))
                return false;

            normalized = "https://youtube.com/watch?v=" + value;
            return true;
        }

        private IEnumerable<YouTubeVideo> ParseVideos(string source)
        {
            string title = Html.GetNode("title", source);

            string jsPlayer = "http:" + Json.GetKey("js", source).Replace(@"\/", "/");

            string map = Json.GetKey("url_encoded_fmt_stream_map", source);
            var queries = map.Split(',').Select(Unscramble);

            foreach (var query in queries)
                yield return new YouTubeVideo(title, query, jsPlayer);

            string adaptiveMap = Json.GetKey("adaptive_fmts", source);

            // If there is no adaptive_fmts key, then in the file
            // will be dashmpd key containing link to a XML
            // file containing links and other data
            if (adaptiveMap == String.Empty)
            {
                using (HttpClient hc = new HttpClient())
                {
                    string temp = Json.GetKey("dashmpd", source);
                    temp = WebUtility.UrlDecode(temp).Replace(@"\/", "/");

                    var manifest = hc.GetStringAsync(temp)
                        .GetAwaiter().GetResult()
                        .Replace(@"\/", "/");

                    var uris = Html.GetUrisFromManifest(manifest);

                    foreach (var v in uris)
                    {
                        yield return new YouTubeVideo(title,
                            UnscrambleManifestUri(v),
                            jsPlayer);
                    }
                }
            }
            else queries = adaptiveMap.Split(',').Select(Unscramble);

            foreach (var query in queries)
                yield return new YouTubeVideo(title, query, jsPlayer);
        }

        // TODO: Consider making this static...
        private UnscrambledQuery Unscramble(string queryString)
        {
            queryString = queryString.Replace(@"\u0026", "&");
            var query = new Query(queryString);
            string uri = query["url"];

            bool encrypted = false;
            string signature;

            if (query.TryGetValue("s", out signature))
            {
                encrypted = true;
                uri += GetSignatureAndHost(signature, query);
            }
            else if (query.TryGetValue("sig", out signature))
                uri += GetSignatureAndHost(signature, query);

            uri = WebUtility.UrlDecode(
                WebUtility.UrlDecode(uri));

            var uriQuery = new Query(uri);

            if (!uriQuery.ContainsKey("ratebypass"))
                uri += "&ratebypass=yes";

            return new UnscrambledQuery(uri, encrypted);
        }

        private string GetSignatureAndHost(string signature, Query query)
        {
            string result = "&signature=" + signature;

            string host;

            if (query.TryGetValue("fallback_host", out host))
                result += "&fallback_host=" + host;

            return result;
        }

        private UnscrambledQuery UnscrambleManifestUri(string manifestUri)
        {
            int start = manifestUri.IndexOf(Playback) + Playback.Length;
            string baseUri = manifestUri.Substring(0, start);
            string parametersString = manifestUri.Substring(start, manifestUri.Length - start);
            var parameters = parametersString.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var builder = new StringBuilder(baseUri);
            builder.Append("?");
            for (var i = 0; i < parameters.Length; i += 2)
            {
                builder.Append(parameters[i]);
                builder.Append('=');
                builder.Append(parameters[i + 1].Replace("%2F", "/"));
                if (i < parameters.Length - 2)
                {
                    builder.Append('&');
                }
            }

            return new UnscrambledQuery(builder.ToString(), false);
        }
    }
}
