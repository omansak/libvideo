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
        private const string YouTubeDecryptSigUri = "https://www.youtube.com/watch?v=09R8_2nJtjg";
        private const string YouTubeWithDataManifest = "https://www.youtube.com/watch?v=EphGWZKtXvE";
        private const string YouTubeShortsUri = "https://www.youtube.com/shorts/xuCO7-DLCaA";

        // private const string VimeoUri = "https://vimeo.com/131417856";

        [Theory]
        [InlineData(YouTubeUri)]
        [InlineData(YouTubeDecryptSigUri)]
        [InlineData(YouTubeWithDataManifest)]
        [InlineData(YouTubeShortsUri)]
        public void YouTube_GetAllVideos(string uri)
        {
            var videos = YouTube.Default.GetAllVideos(uri);

            Assert.NotNull(videos);
            Assert.DoesNotContain(null, videos);

            foreach (var video in videos)
            {
                Assert.NotNull(video.Uri);
                Assert.Equal(video.WebSite, WebSites.YouTube);
            }
        }

        [Theory]
        [InlineData(YouTubeInvalidUriOne)]
        [InlineData(YouTubeInvalidUriTwo)]
        public void YouTube_ThrowOnInvalidUri(string invalid)
        {
            Assert.Throws<ArgumentException>(() 
                => YouTube.Default.GetVideo(invalid));
        }
    }
}
