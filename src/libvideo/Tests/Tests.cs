#if DEBUG
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary.Tests
{
    [TestFixture]
    public class Tests
    {
        private const string YouTubeUri = "https://www.youtube.com/watch?v=JjCaRS-CABk";
        private const string VimeoUri = "https://vimeo.com/131417856";

        [TestCase]
        public void YouTube_Download()
        {
            var service = new YouTubeService();

            byte[] bytes = service.Download(YouTubeUri);
        }

        [TestCase]
        public void Vimeo_Download()
        {
            var service = new VimeoService();

            byte[] bytes = service.Download(VimeoUri);
        }

        [TestCase]
        public void YouTube_DownloadMany()
        {
            // DISABLE THIS TEST IF YOU DON'T NEED IT.

            var service = new YouTubeService();

            var arrays = service.DownloadMany(YouTubeUri);

            foreach (var array in arrays); // synchronous DownloadMany may use yield
        }

        // [TestCase]
        public void Vimeo_DownloadMany()
        {
            // DISABLE THIS TEST IF YOU DON'T NEED IT.

            var service = new VimeoService();

            var byteArrays = service.DownloadMany(VimeoUri);

            foreach (var array in byteArrays);
        }

        // [TestCase]
        public void SingleClientService_SpeedTest()
        {
            // DISABLE THIS TEST IF YOU DON'T NEED IT.

            byte[] bytes;
            var rawService = new VimeoService();

            using (var managedService = new SingleClientService(rawService))
            {
                var watch = Stopwatch.StartNew();

                for (int i = 0; i < 5; i++)
                    bytes = rawService.Download(VimeoUri);

                var rawTime = watch.Elapsed;
                watch.Restart();

                for (int i = 0; i < 5; i++)
                    bytes = managedService.Download(VimeoUri);

                var managedTime = watch.Elapsed;
                watch.Stop();

                var difference = managedTime - rawTime;
                Assert.That(difference > TimeSpan.Zero, $"Single client speed test failed! Difference is: {difference}.");
            }
        }
    }
}
#endif