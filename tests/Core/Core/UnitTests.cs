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
        private const string YouTubeUri = "https://www.youtube.com/watch?v=JjCaRS-CABk";
        // private const string VimeoUri = "https://vimeo.com/131417856";

        [Fact]
        public void YouTube_Download()
        {
            var service = new YouTubeService();

            byte[] bytes = service.Download(YouTubeUri);
        }

        [Fact]
        public void YouTube_DownloadMany()
        {
            var service = new YouTubeService();

            var arrays = service.DownloadMany(YouTubeUri);

            foreach (var array in arrays); // synchronous DownloadMany may use yield
        }
    }
}