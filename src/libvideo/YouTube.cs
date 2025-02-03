using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using VideoLibrary.Exceptions;
using VideoLibrary.Helpers;
using System.IO;


namespace VideoLibrary
{
    public class YouTube : ServiceBase<YouTubeVideo>
    {
        private const string Playback = "videoplayback";
        private static string _signatureKey;
        private static string _visitorData;
        public static YouTube Default { get; } = new YouTube();
        public const string YoutubeUrl = "https://youtube.com/";

        internal override async Task<IEnumerable<YouTubeVideo>> GetAllVideosAsync(string videoUri, Func<string, Task<string>> sourceFactory)
        {
            if (!TryNormalize(videoUri, out videoUri))
                throw new ArgumentException("URL is not a valid YouTube URL!");

            // TODO Remove
            string source = await sourceFactory(videoUri).ConfigureAwait(false);

            // TODO Remove
            string jsPlayer = ParseJsPlayer(source);

            if (jsPlayer == null)
            {
                throw new UnavailableStreamException($"JS Player is not found");
            }

            var playerResponseJson = JsonDocument.Parse(Json.Extract(ParsePlayerJson(source))).RootElement;

            // PlayerJson from IOS content
            var data = await GetPlayerResponseIOSAsync(playerResponseJson.GetProperty("videoDetails").GetNullableProperty("videoId")?.GetString())
                .ConfigureAwait(false);

            if (data != null)
            {
                playerResponseJson = JsonDocument.Parse(data).RootElement;
            }

            return ParseVideos(source, jsPlayer, playerResponseJson);
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

        private IEnumerable<YouTubeVideo> ParseVideos(string source, string jsPlayer, JsonElement playerResponseJson)
        {
            IEnumerable<UnscrambledQuery> queries;

            if (string.Equals(playerResponseJson.GetProperty("playabilityStatus").GetNullableProperty("status")?.GetString(), "error", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnavailableStreamException($"Video has unavailable stream.");
            }

            var errorReason = playerResponseJson.GetProperty("playabilityStatus").GetNullableProperty("reason")?.GetString();
            if (string.IsNullOrWhiteSpace(errorReason))
            {
                var isLiveStream = playerResponseJson.GetProperty("videoDetails").GetNullableProperty("isLive")?.GetBoolean() == true;
                var title = playerResponseJson.GetProperty("videoDetails").GetNullableProperty("title")?.GetString();
                var lengthSeconds = playerResponseJson.GetProperty("videoDetails").GetNullableProperty("lengthSeconds")?.GetString() ?? "0";
                var author = playerResponseJson.GetProperty("videoDetails").GetNullableProperty("author")?.GetString();

                var videoInfo = new VideoInfo(title, int.Parse(lengthSeconds), author);

                if (isLiveStream)
                {
                    throw new UnavailableStreamException($"This is live stream so unavailable stream.");
                }

                string map = Json.GetKey("url_encoded_fmt_stream_map", source);
                if (!string.IsNullOrWhiteSpace(map))
                {
                    queries = map.Split(',').Select(Unscramble);
                    foreach (var query in queries)
                        yield return new YouTubeVideo(videoInfo, query, jsPlayer);
                }
                else // player_response
                {
                    List<JsonElement> streamObjects = new List<JsonElement>();

                    // Extract Muxed streams
                    var streamFormat = playerResponseJson.GetNullableProperty("streamingData")?.GetNullableProperty("formats");
                    if (streamFormat != null)
                    {
                        streamObjects.AddRange(streamFormat?.EnumerateArray());
                    }

                    // Extract AdaptiveFormat streams
                    var streamAdaptiveFormats = playerResponseJson.GetNullableProperty("streamingData")?.GetNullableProperty("adaptiveFormats");
                    if (streamAdaptiveFormats != null)
                    {
                        streamObjects.AddRange(streamAdaptiveFormats?.EnumerateArray());
                    }

                    foreach (var item in streamObjects)
                    {
                        var urlValue = item.GetNullableProperty("url")?.GetString();
                        if (!string.IsNullOrEmpty(urlValue))
                        {
                            var query = new UnscrambledQuery(urlValue, false);
                            yield return new YouTubeVideo(videoInfo, query, jsPlayer);
                            continue;
                        }

                        var asd = item.ToString();
                        var cipherValue = (item.GetNullableProperty("cipher") ?? item.GetNullableProperty("signatureCipher"))?.GetString();
                        if (!string.IsNullOrEmpty(cipherValue))
                        {
                            yield return new YouTubeVideo(videoInfo, Unscramble(cipherValue), jsPlayer);
                        }
                    }
                }

                // adaptive_fmts
                string adaptiveMap = Json.GetKey("adaptive_fmts", source);
                if (!string.IsNullOrWhiteSpace(adaptiveMap))
                {
                    queries = adaptiveMap.Split(',').Select(Unscramble);
                    foreach (var query in queries)
                        yield return new YouTubeVideo(videoInfo, query, jsPlayer);
                }
                else
                {
                    // dashmpd
                    string dashmpdMap = Json.GetKey("dashmpd", source);
                    if (!string.IsNullOrWhiteSpace(adaptiveMap))
                    {
                        using (HttpClient hc = new HttpClient())
                        {
                            IEnumerable<string> uris = null;
                            try
                            {

                                dashmpdMap = WebUtility.UrlDecode(dashmpdMap).Replace(@"\/", "/");

                                var manifest = hc
                                    .GetStringAsync(dashmpdMap)
                                    .GetAwaiter()
                                    .GetResult()
                                    .Replace(@"\/", "/");

                                uris = Html.GetUrisFromManifest(manifest);
                            }
                            catch (Exception e)
                            {
                                throw new UnavailableStreamException(e.Message);
                            }

                            if (uris != null)
                            {
                                foreach (var v in uris)
                                {
                                    yield return new YouTubeVideo(videoInfo, UnscrambleManifestUri(v), jsPlayer);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                throw new UnavailableStreamException($"Error caused by Youtube.({errorReason}))");
            }
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

        private async Task<string> GetPlayerResponseIOSAsync(string id)
        {
            var androidClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.youtube.com/youtubei/v1/player");

            if (_visitorData is null)
            {
                _visitorData = await VisitorDataTokenGenerator.GetVisitorDataFromYouTube(androidClient);
            }

            var content = new
            {
                videoId = id,
                contentCheckOk = true,
                context = new
                {
                    client = new
                    {
                        clientName = "IOS",
                        clientVersion = "19.45.4",
                        deviceMake = "Apple",
                        deviceModel = "iPhone16,2",
                        platform = "MOBILE",
                        osName = "IOS",
                        osVersion = "18.1.0.22B83",
                        hl = "en",
                        gl = "US",
                        utcOffsetMinutes = 0,
                        visitorData = _visitorData,
                    }
                }
            };

            request.Content = new StringContent(JsonSerializer.Serialize(content));
            request.Headers.Add("User-Agent", "com.google.ios.youtube/19.45.4 (iPhone16,2; U; CPU iOS 18_1_0 like Mac OS X; US)");
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
