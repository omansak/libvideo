using System;
using System.Diagnostics;

namespace VideoLibrary.Debug
{
    class Program
    {
        static void Main()
        {
            string[] queries =
            {
                "https://www.youtube.com/watch?v=jfobiCq0YUc&ab_channel=EminemMusic",                   //1080
                "https://www.youtube.com/watch?v=LXb3EKWsInQ&ab_channel=Jacob%2BKatieSchwarz",          //2060
                "https://www.youtube.com/watch?v=U2XK_TJZ3PI",                                          //JSON Parse Error
            };

            TestVideoLib(queries);
            Console.WriteLine("Done.");
            Console.ReadKey();
        }

        public static void TestVideoLib(string[] queries)
        {
            //new Test().Run();

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
                        foreach (YouTubeVideo v in videoInfos)
                        {
                            if (v.Resolution > 0 && v.AudioBitrate < 0)
                            {
                                Console.WriteLine(v.Uri);
                                Console.WriteLine(string.Format($"Full Title\t{v.Title + v.FileExtension}\nType\t{v.AdaptiveKind}\nResolution\t{v.Resolution}p\nFormat\t{v.FormatCode}\nFPS\t{v.Fps}\nBitrate\t{v.AudioBitrate}\n"));
                                Console.WriteLine("Success : " + v.Head().CanRead);
                                Console.WriteLine();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                        Debugger.Break();
                    }
                }
            }
        }
    }
}