using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoLibraryNetCore;

namespace libvideo_netcore.debug
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WriteVideos();
        }

        static void WriteVideos()
        {
            string localPath = @"C:\Users\atorris\Documents\Nouveau dossier (2)\videos\";

            string[] queries =
               {
                "https://www.youtube.com/watch?v=eQfJA24m7IIA",
                "https://www.youtube.com/watch?v=2vH5Yoaia20"
            };

            var youTube = YouTube.Default; // starting point for YouTube actions
            foreach (string s in queries)
            {
                Console.WriteLine("START " + s);
                var video = youTube.GetVideo(s); // gets a Video object with info about the video
                File.WriteAllBytes(localPath + video.FullName, video.GetBytes());
                Console.WriteLine("DONE " + s);
            }

            Console.ReadKey();
        }
    }
}