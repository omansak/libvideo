using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VideoLibraryNetCore;

namespace libvideo_netcore.debug
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string[] queries =
               {
                "https://www.youtube.com/watch?v=eQfJA24m7IIA",
                "https://www.youtube.com/watch?v=2vH5Yoaia20"
            };

            TestYoutubeExtractor(queries);
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

                    try
                    {
                        var videoInfos = cli.GetAllVideosAsync(uri).GetAwaiter().GetResult();

                        Console.WriteLine($"Link #{i + 1}");
                        foreach (var v in videoInfos)
                        {
                            Console.WriteLine(v.Uri);
                            Console.WriteLine();
                        }
                    }
                    catch
                    {
                        Debugger.Break();
                    }
                }
            }
        }

        public static void TestYoutubeExtractor(string[] queries)
        {
            using (var cli = Client.For(YouTube.Default))
            {
                for (int i = 0; i < queries.Length; i++)
                {
                    string query = queries[i];

                    var video = cli.GetVideo(query);
                    string uri = video.Uri;

                    try
                    {
                        var uris = DownloadUrlResolver
                            .GetDownloadUrls(query)
                            .Select(v => v.DownloadUrl);

                        Console.WriteLine($"Link #{i + 1}");
                        foreach (var v in uris)
                        {
                            Console.WriteLine(v);
                            Console.WriteLine();
                        }
                    }
                    catch { }
                }
            }
        }
    }
}