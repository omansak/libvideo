using VideoLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace SpeedTest
{
    class Program
    {
        const int Times = 10;
        const string Uri = "https://www.youtube.com/watch?v=NLddPakLF1I";

        static void Main(string[] args)
        {
            int iterations = 10; // prevents exceptions from resetting the entire test
            if (args.Length != 0)
                iterations = Int32.Parse(args[0]);

            Console.WriteLine("Starting...");
            Console.WriteLine($"Iterations: {iterations}.");
            Console.WriteLine();

            Console.WriteLine("Getting results for YoutubeExtractor...");

            var elapsed = TimeSpan.Zero;

            for (int i = 0; i < iterations; i++)
            {
                RunChecked(() =>
                {
                    var watch = Stopwatch.StartNew();

                    for (int j = 0; j < Times; j++)
                        DownloadUrlResolver.GetDownloadUrls(Uri);

                    watch.Stop();
                    elapsed += watch.Elapsed;
                });
            }

            Console.WriteLine($"YoutubeExtractor: took {elapsed}.");
            Console.WriteLine();

            Console.WriteLine("Getting results for Livid...");

            elapsed = TimeSpan.Zero;

            using (var service =
                new SingleClientService(new YouTubeService()))
            {
                for (int i = 0; i < iterations; i++)
                {
                    RunChecked(() =>
                    {
                        var watch = Stopwatch.StartNew();

                        for (int j = 0; j < Times; j++)
                            service.GetAllUris(Uri);

                        watch.Stop();
                        elapsed += watch.Elapsed;
                    });
                }
            }

            Console.WriteLine($"Livid: took {elapsed}.");
        }

        static void RunChecked(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Console.WriteLine($@"
A {e.GetType()} was thrown!
Message:
{e}

Restarting... (PLEASE PLEASE quit if this is an OOME or something bad!)");
                RunChecked(action);
            }
        }
    }
}
