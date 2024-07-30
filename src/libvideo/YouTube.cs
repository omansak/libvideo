using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VideoLibrary.Exceptions;
using VideoLibrary.Helpers;


namespace VideoLibrary
{
    public class YouTube : ServiceBase<YouTubeVideo>
    {
        private const string Playback = "videoplayback";
        private static string _signatureKey;
        public static YouTube Default { get; } = new YouTube();
        public const string YoutubeUrl = "https://youtube.com/";

        internal override async Task<IEnumerable<YouTubeVideo>> GetAllVideosAsync(string videoUri, Func<string, Task<string>> sourceFactory)
        {
            if (!TryNormalize(videoUri, out videoUri))
                throw new ArgumentException("URL is not a valid YouTube URL!");

            string source = await sourceFactory(videoUri).ConfigureAwait(false);

            return await ParseVideos(source);
        }
        public static string GetSignatureKey()
        {
            return string.IsNullOrWhiteSpace(_signatureKey) ? "signature" : _signatureKey;
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
                .Replace("youtube.com/shorts/", "youtube.com/watch?v=")
                .ToString();

            var query = new Query(videoUri);

            string value;

            if (!query.TryGetValue("v", out value))
                return false;

            normalized = $"{YoutubeUrl}watch?v=" + value;
            return true;
        }

        private async Task<IEnumerable<YouTubeVideo>> ParseVideos(string source)
        {
            var videos = new List<YouTubeVideo>();

            string jsPlayer = ParseJsPlayer(source);
            if (jsPlayer == null)
            {
                return videos; // Return an empty list instead of using yield break
            }

            var playerResponseJson = JToken.Parse(Json.Extract(ParsePlayerJson(source)));

            // PlayerJson from android content
            var videoId = playerResponseJson.SelectToken("videoDetails.videoId")?.Value<string>();
            var data = videoId != null ? await GetPlayerResponseAndroidAsync(videoId) : null;

            if (data != null)
            {
                playerResponseJson = JToken.Parse(data);
            }

            if (string.Equals(playerResponseJson.SelectToken("playabilityStatus.status")?.Value<string>(), "error", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnavailableStreamException("Video has unavailable stream.");
            }

            var errorReason = playerResponseJson.SelectToken("playabilityStatus.reason")?.Value<string>();
            if (!string.IsNullOrWhiteSpace(errorReason))
            {
                throw new UnavailableStreamException($"Error caused by Youtube. ({errorReason})");
            }

            var isLiveStream = playerResponseJson.SelectToken("videoDetails.isLive")?.Value<bool>() == true;
            var videoInfo = new VideoInfo(
                playerResponseJson.SelectToken("videoDetails.title")?.Value<string>(),
                playerResponseJson.SelectToken("videoDetails.lengthSeconds")?.Value<int>(),
                playerResponseJson.SelectToken("videoDetails.author")?.Value<string>());

            if (isLiveStream)
            {
                throw new UnavailableStreamException("This is a live stream, so the stream is unavailable.");
            }

            string map = Json.GetKey("url_encoded_fmt_stream_map", source);
            if (!string.IsNullOrWhiteSpace(map))
            {
                var queries = map.Split(',').Select(Unscramble);
                videos.AddRange(queries.Select(query => new YouTubeVideo(videoInfo, query, jsPlayer)));
            }
            else
            {
                List<JToken> streamObjects = new List<JToken>();

                // Extract Muxed streams
                var streamFormat = playerResponseJson.SelectToken("streamingData.formats");
                if (streamFormat != null)
                {
                    streamObjects.AddRange(streamFormat.ToArray());
                }

                // Extract AdaptiveFormat streams
                var streamAdaptiveFormats = playerResponseJson.SelectToken("streamingData.adaptiveFormats");
                if (streamAdaptiveFormats != null)
                {
                    streamObjects.AddRange(streamAdaptiveFormats.ToArray());
                }

                foreach (var item in streamObjects)
                {
                    var urlValue = item.SelectToken("url")?.Value<string>();
                    if (!string.IsNullOrEmpty(urlValue))
                    {
                        var query = new UnscrambledQuery(urlValue, false);
                        videos.Add(new YouTubeVideo(videoInfo, query, jsPlayer));
                        continue;
                    }
                    var cipherValue = ((item.SelectToken("cipher") ?? item.SelectToken("signatureCipher")) ?? string.Empty).Value<string>();
                    if (!string.IsNullOrEmpty(cipherValue))
                    {
                        videos.Add(new YouTubeVideo(videoInfo, Unscramble(cipherValue), jsPlayer));
                    }
                }

                // adaptive_fmts
                string adaptiveMap = Json.GetKey("adaptive_fmts", source);
                if (!string.IsNullOrWhiteSpace(adaptiveMap))
                {
                    var adaptiveQueries = adaptiveMap.Split(',').Select(Unscramble);
                    videos.AddRange(adaptiveQueries.Select(query => new YouTubeVideo(videoInfo, query, jsPlayer)));
                }
                else
                {
                    // dashmpd
                    string dashmpdMap = Json.GetKey("dashmpd", source);
                    if (!string.IsNullOrWhiteSpace(dashmpdMap))
                    {
                        using (HttpClient hc = new HttpClient())
                        {
                            try
                            {
                                dashmpdMap = WebUtility.UrlDecode(dashmpdMap).Replace(@"\/", "/");
                                var manifest = await hc.GetStringAsync(dashmpdMap);
                                manifest = manifest.Replace(@"\/", "/");
                                var uris = Html.GetUrisFromManifest(manifest);

                                if (uris != null)
                                {
                                    foreach (var v in uris)
                                    {
                                        videos.Add(new YouTubeVideo(videoInfo, UnscrambleManifestUri(v), jsPlayer));
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                throw new UnavailableStreamException(e.Message);
                            }
                        }
                    }
                }
            }

            return videos;
        }


        private string ParsePlayerJson(string source)
        {
            string playerResponseMap = null, ytInitialPlayerPattern = @"\s*var\s*ytInitialPlayerResponse\s*=\s*(\{\""responseContext\"".*\});", ytWindowInitialPlayerResponse = @"\[\""ytInitialPlayerResponse\""\]\s*=\s*(\{.*\});", ytPlayerPattern = @"ytplayer\.config\s*=\s*(\{\"".*\""\}\});";
            Match match;
            if ((match = Regex.Match(source, ytPlayerPattern)).Success && Json.TryGetKey("player_response", match.Groups[1].Value, out string json))
            {
                playerResponseMap = Regex.Unescape(json);
            }
            if (string.IsNullOrWhiteSpace(playerResponseMap) && (match = Regex.Match(source, ytInitialPlayerPattern)).Success)
            {
                playerResponseMap = match.Groups[1].Value;
            }
            if (string.IsNullOrWhiteSpace(playerResponseMap) && (match = Regex.Match(source, ytWindowInitialPlayerResponse)).Success)
            {
                playerResponseMap = match.Groups[1].Value;
            }
            if (string.IsNullOrWhiteSpace(playerResponseMap))
            {
                throw new UnavailableStreamException("Player json has no found.");
            }
            return playerResponseMap.Replace(@"\u0026", "&").Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\\&", "\\\\&");
        }

        private string ParseJsPlayer(string source)
        {
            if (Json.TryGetKey("jsUrl", source, out var jsPlayer) || Json.TryGetKey("PLAYER_JS_URL", source, out jsPlayer))
            {
                jsPlayer = jsPlayer.Replace(@"\/", "/");
            }
            else
            {
                // Alternative solution
                Match match = Regex.Match(source, "<script\\s*src=\"([-a-zA-Z0-9()@:%_\\+.~#?&//=]*)\".*name=\"player_ias/base\".*>\\s*</script>");
                if (match.Success)
                {
                    jsPlayer = match.Groups[1].Value.Replace(@"\/", "/");
                }
                else
                {
                    return null;
                }
            }

            if (jsPlayer.StartsWith("/yts") || jsPlayer.StartsWith("/s"))
            {
                return $"https://www.youtube.com{jsPlayer}";
            }

            // Fall back on old implementation (not sure it's needed)
            if (!jsPlayer.StartsWith("http"))
            {
                jsPlayer = $"https:{jsPlayer}";
            }

            return jsPlayer;
        }

        private UnscrambledQuery Unscramble(string queryString)
        {
            queryString = queryString.Replace(@"\u0026", "&");
            var query = new Query(queryString);
            string uri = query["url"];

            query.TryGetValue("sp", out _signatureKey);

            bool encrypted = false;
            string signature;

            if (query.TryGetValue("s", out signature))
            {
                encrypted = true;
                uri += GetSignatureAndHost(GetSignatureKey(), signature, query);
            }
            else if (query.TryGetValue("sig", out signature))
                uri += GetSignatureAndHost(GetSignatureKey(), signature, query);

            uri = WebUtility.UrlDecode(
                WebUtility.UrlDecode(uri));

            var uriQuery = new Query(uri);

            if (!uriQuery.ContainsKey("ratebypass"))
                uri += "&ratebypass=yes";

            return new UnscrambledQuery(uri, encrypted);
        }

        private string GetSignatureAndHost(string key, string signature, Query query)
        {
            string result = $"&{key}={signature}";

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

        private async Task<string> GetPlayerResponseAndroidAsync(string id)
        {
            var androidClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.youtube.com/youtubei/v1/player");

            var content = new
            {
                videoId = id,
                context = new
                {
                    client = new
                    {
                        clientName = "ANDROID_TESTSUITE",
                        clientVersion = "1.9",
                        androidSdkVersion = 30,
                        hl = "en",
                        gl = "US",
                        utcOffsetMinutes = 0
                    }
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(content));
            request.Headers.Add("User-Agent", "com.google.android.youtube/17.36.4 (Linux; U; Android 12; GB) gzip");
            var response = await androidClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {

                var responseContent = await response.Content.ReadAsStringAsync();
                androidClient.Dispose();
                request.Dispose();
                request.Dispose();

                return responseContent;
            }

            return null;
        }
    }
}
