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
                // test queries, borrowed from 
                // github.com/rg3/youtube-dl/blob/master/youtube_dl/extractor/youtube.py

                "http://www.youtube.com/watch?v=6kLq3WMV1nU", // VEVO => KeyNotFound
                "https://www.youtube.com/watch?v=B3eAMGXFw1o", // VEVO video, KeyNotFound
                "https://www.youtube.com/watch?v=IB3lcPjvWLA",
                "https://www.youtube.com/watch?v=BgpXMA_M98o",
                "https://www.youtube.com/watch?v=nfWlot6h_JM",
                "https://www.youtube.com/watch?v=EphGWZKtXvE", // Without adaptive map
                "http://youtube.com/watch?v=IB3lcPjvWLA",
                "https://www.youtube.com/watch?v=kp8u_Yrw76Q", //private
                "https://www.youtube.com/watch?v=09R8_2nJtjg", //encrypted
                "https://www.youtube.com/watch?v=ZAqC3Qh_oUs",
                "https://www.youtube.com/watch?v=pG_euGOe0ww",
                "https://www.youtube.com/watch?v=-zCkhuFqpFc",
                "https://www.youtube.com/watch?v=2vjPBrBU-TM",
                "https://www.youtube.com/watch?v=Alr26K0F4EQ", //player_response
                "https://www.youtube.com/watch?v=ADxntEqPysA", //encrypted player_response
                "https://www.youtube.com/watch?v=mCeF-IF7JMg",
                "https://www.youtube.com/watch?v=F2d2hEM1N6k"
            };

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
                        Console.WriteLine($"Link #{i + 1}");
                        foreach (var v in videoInfos)
                        {
                            Console.WriteLine(v.Uri);
                            Console.WriteLine("Success : " + v.Head().CanRead);
                            Console.WriteLine();
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