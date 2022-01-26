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
        private static YouTube Service = new YouTube();

        public static void DecryptDownloadUrl(VideoInfo info)
        {
            // Nothing to do here, URL is decrypted automatically 
            // upon calling YouTubeVideo.Uri.
        }

        public static IEnumerable<VideoInfo> GetDownloadUrls(string videoUrl, bool decryptSignature = true)
        {
            Require.NotNull(videoUrl, nameof(videoUrl));

            // GetAllVideos normalizes the URL as of libvideo v0.4.1, 
            // don't call TryNormalizeYoutubeUrl here.

            return Service.GetAllVideos(videoUrl).Select(v => new VideoInfo(v));
        }

        public async static Task<IEnumerable<VideoInfo>> GetDownloadUrlsAsync(
            string videoUrl, bool decryptSignature = true)
        {
            var videos = await Service
                .GetAllVideosAsync(videoUrl)
                .ConfigureAwait(false);

            return videos.Select(v => new VideoInfo(v));
        }

        public static bool TryNormalizeYoutubeUrl(string url, out string normalizedUrl)
        {
            // If you fix something in here, please be sure to fix in 
            // YouTubeService.TryNormalize as well.

            normalizedUrl = null;

            var builder = new StringBuilder(url);

            url = builder.Replace("youtu.be/", "youtube.com/watch?v=")
                .Replace("youtube.com/embed/", "youtube.com/watch?v=")
                .Replace("/v/", "/watch?v=")
                .Replace("/watch#", "/watch?")
                .Replace("youtube.com/shorts/", "youtube.com/watch?v=")
                .ToString();

            string value;

            var query = new Query(url);

            if (!query.TryGetValue("v", out value))
                return false;

            normalizedUrl = "https://youtube.com/watch?v=" + value;
            return true;
        }
    }
}
