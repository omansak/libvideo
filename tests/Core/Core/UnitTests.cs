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
            var video = YouTube.Default.GetVideo(YouTubeUri);

            Assert.NotNull(video.Uri);
            Assert.NotEqual(video.FormatCode, -1);
        }

        [Fact]
        public void YouTube_GetAllVideos()
        {
            var videos = YouTube.Default.GetAllVideos(YouTubeUri);

            Assert.NotNull(videos);
            Assert.DoesNotContain(null, videos);
        }

        [Fact]
        public void YouTube_ThrowOnInvalidUri()
        {
            Assert.Throws<ArgumentException>(() 
                => YouTube.Default.GetVideo(YouTubeInvalidUriOne));
            Assert.Throws<ArgumentException>(() 
                => YouTube.Default.GetVideo(YouTubeInvalidUriTwo));
        }
    }
}