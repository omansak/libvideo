using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace VideoLibrary.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] queries =
            {
                // test queries, borrowed from 
                // github.com/rg3/youtube-dl/blob/master/youtube_dl/extractor/youtube.py

                //"http://www.youtube.com/watch?v=6kLq3WMV1nU", // VEVO => KeyNotFound
                //"https://www.youtube.com/watch?v=B3eAMGXFw1o", // VEVO video, KeyNotFound
                "https://www.youtube.com/watch?v=IB3lcPjvWLA",
                "https://www.youtube.com/watch?v=BgpXMA_M98o",
                "https://www.youtube.com/watch?v=nfWlot6h_JM",
                "https://www.youtube.com/watch?v=EphGWZKtXvE", // Without adaptive map
                "http://youtube.com/watch?v=IB3lcPjvWLA",
                "https://www.youtube.com/watch?v=kp8u_Yrw76Q", //private
                "https://www.youtube.com/watch?v=09R8_2nJtjg", //encrypted
                "https://www.youtube.com/watch?v=ZAqC3Qh_oUs",
                "https://www.youtube.com/watch?v=pG_euGOe0ww"
            };

            TestLibCompat(queries);
            TestVideoLib(queries);
            Console.WriteLine("Done.");
            Console.ReadKey();
        }

        public static void TestVideoLib(string[] queries)
        {
            using (var cli = Client.For(YouTube.Default))
            {
                Console.WriteLine("Downloading...");
                for (int i = 0; i < queries.Length; i++)
                {
                    string uri = queries[i];
                    var videoInfos = cli.GetAllVideosAsync(uri).Result;

                    Console.WriteLine($"Link #{i + 1}");
                    foreach (var v in videoInfos) Console.WriteLine(v.Uri + "\n");
                }
            }
        }

        public static void TestLibCompat(string[] queries)
        {
            using (var cli = Client.For(YouTube.Default))
            {
                for (int i = 0; i < queries.Length; i++)
                {
                    string query = queries[i];

                    var video = cli.GetVideo(query);
                    string uri = video.Uri;

                    var Uris = DownloadUrlResolver
                        .GetDownloadUrls(query)
                        .Select(v => v.DownloadUrl);

                    Console.WriteLine($"Link #{i + 1}");
                    foreach (var v in Uris) Console.WriteLine(v + "\n");
                }
            }
        }
    }
}
