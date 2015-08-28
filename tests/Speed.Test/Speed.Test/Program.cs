using VideoLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace Speed.Test
{
    class Program
    {
        const string Uri = "https://www.youtube.com/watch?v=NLddPakLF1I";

        static void Main(string[] args)
        {
            int times = args.Length == 0 ? 3 : int.Parse(args[0]);
            int iterations = args.Length <= 1 ? 2 : int.Parse(args[1]);

            Console.WriteLine("Starting...");
            Console.WriteLine($"Times: {times}.");
            Console.WriteLine($"Iterations: {iterations}.");
            Console.WriteLine("Getting results for YoutubeExtractor...");

            var elapsed = TimeSpan.Zero;

            for (int i = 0; i < times; i++)
            {
                RunChecked(() =>
                {
                    var watch = Stopwatch.StartNew();

                    for (int j = 0; j < iterations; j++)
                        GC.KeepAlive(DownloadUrlResolver.GetDownloadUrls(Uri));

                    watch.Stop();
                    elapsed += watch.Elapsed;
                });
            }

            Console.WriteLine($"YoutubeExtractor: took {elapsed}.");
            Console.WriteLine("Getting results for libvideo...");

            elapsed = TimeSpan.Zero;

            using (var service = new ClientService(new YouTubeService()))
            {
                for (int i = 0; i < times; i++)
                {
                    RunChecked(() =>
                    {
                        var watch = Stopwatch.StartNew();

                        for (int j = 0; j < iterations; j++)
                            GC.KeepAlive(service.GetAllVideos(Uri));

                        watch.Stop();
                        elapsed += watch.Elapsed;
                    });
                }
            }

            Console.WriteLine($"libvideo: took {elapsed}.");
        }

        static void RunChecked(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                string[] lines =
                {
                    $"A {e.GetType()} was thrown!",
                    "Message:",
                    String.Empty,
                    e.ToString(),
                    String.Empty,
                    "Restarting..."
                };
                
                Console.WriteLine(string.Join(Environment.NewLine, lines));
                
                RunChecked(action);
            }
        }
    }
}
