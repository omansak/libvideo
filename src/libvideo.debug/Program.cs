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

                // "http://www.youtube.com/watch?v=6kLq3WMV1nU",
                // "http://www.youtube.com/watch?v=6kLq3WMV1nU",
                "https://www.youtube.com/watch?v=IB3lcPjvWLA",
                "https://www.youtube.com/watch?v=BgpXMA_M98o",
                "https://www.youtube.com/watch?v=nfWlot6h_JM"
            };

            using (var cli = Client.For(YouTube.Default))
            {
                for (int i = 0; i < queries.Length; i++)
                {
                    string query = queries[i];

                    var video = cli.GetVideo(query);
                    string uri = video.Uri;

                    string otherUri = DownloadUrlResolver
                        .GetDownloadUrls(query).First()
                        .DownloadUrl;
                }
            }
        }
    }
}
