using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;
using Xunit;

namespace Core
{
    public class UnitTests
    {
        private const string YouTubeInvalidUriOne = "https://wikipedia.com";
        private const string YouTubeInvalidUriTwo = "123ABC!@#";
        private const string YouTubeUri = "https://www.youtube.com/watch?v=JjCaRS-CABk";
        // private const string VimeoUri = "https://vimeo.com/131417856";

        [Fact]
        public void YouTube_GetVideo()
        {
            var video = YouTubeService.Default.GetVideo(YouTubeUri);

            Assert.NotNull(video.Uri);
            Assert.NotEqual(video.FormatCode, -1);
            Assert.Equal(video.WebSite, WebSites.YouTube);
        }

        [Fact]
        public void YouTube_GetAllVideos()
        {
            var videos = YouTubeService.Default.GetAllVideos(YouTubeUri);

            Assert.NotNull(videos);
            Assert.DoesNotContain(null, videos);
        }

        [Fact]
        public void YouTube_ThrowOnInvalidUri()
        {
            // Assert.Throws<ArgumentException>(
            //     () => YouTubeService.Default.GetVideo(YouTubeInvalidUriOne));
            // Assert.Throws<ArgumentException>(
            //     () => YouTubeService.Default.GetVideo(YouTubeInvalidUriTwo));
        }
    }
}