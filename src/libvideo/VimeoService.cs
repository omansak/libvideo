using VideoLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace VideoLibrary
{
    public class VimeoService : ServiceBase
    {
        protected override Video VideoSelector(IEnumerable<Video> videos) =>
            videos.Last(); // The first video for Vimeo is for mobile, which is tiny. This is standard definition.

        public async override Task<IEnumerable<Video>> GetAllVideosAsync(
            string videoUri, Func<string, Task<string>> sourceFactory)
        {
            videoUri = videoUri.Replace("vimeo.com", "player.vimeo.com/video");

            string source = await
                sourceFactory(videoUri)
                .ConfigureAwait(false);

            return ParseVideos(source);
        }

        // TODO: Make static if possible.
        private IEnumerable<Video> ParseVideos(string source)
        {
            string title = Html.GetNodeValue("title", source);

            string between = Text.StringBetween(@"""mobile"":{", "}}", source);

            var fragments = between.Split(new[] { "}," },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var fragment in fragments)
                yield return new Video(title, Json.GetKeyValue("url", fragment), WebSites.Vimeo);
        }
    }
}
