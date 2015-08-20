using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;
using VideoLibrary.Helpers;

namespace YoutubeExtractor
{
    public static class DownloadUrlResolver
    {
        private static YouTubeService Service = new YouTubeService();

        public static void DecryptDownloadUrl(VideoInfo info)
        {
        }

        public static IEnumerable<VideoInfo> GetDownloadUrls(string videoUrl, bool decryptSignature = true)
        {
            if (videoUrl == null)
                throw new ArgumentNullException(nameof(videoUrl));

            if (!TryNormalizeYoutubeUrl(videoUrl, out videoUrl))
                throw new ArgumentException("URL is not a valid youtube URL!");

            return Service.GetAllVideos(videoUrl).Select(v => new VideoInfo(v));
        }

        public async static Task<IEnumerable<VideoInfo>> GetDownloadUrlsAsync(
            string videoUrl, bool decryptSignature = true) =>
            (await Service.GetAllVideosAsync(videoUrl)).Select(v => new VideoInfo(v));

        public static bool TryNormalizeYoutubeUrl(string url, out string normalizedUrl)
        {
            // If you fix something in here, please be sure to fix in 
            // YouTubeService.TryNormalize as well.

            normalizedUrl = null;

            var builder = new StringBuilder(url);

            url = builder.Replace(" ", string.Empty)
                .Replace("youtu.be/", "youtube.com/watch?v=")
                .Replace("www.youtube", "youtube")
                .Replace("youtube.com/embed/", "youtube.com/watch?v=")
                .Replace("/v/", "/watch?v=")
                .Replace("/watch#", "/watch?")
                .ToString();

            string value;

            if (!Query.TryGetParamValue("v", url, out value))
                return false;

            normalizedUrl = "http://youtube.com/watch?v=" + value;
            return true;
        }
    }
}
