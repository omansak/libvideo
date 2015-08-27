using VideoLibrary;
using System;
using System.IO;

namespace Valks
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Valks!");
            Console.WriteLine("Easily save your favorite videos from YouTube.");

            using (var service = 
                   new SingleClientService(new YouTubeService()))
            {
                while (true)
                {
                    Console.WriteLine();
                    Console.Write("Enter your video's ID: ");

                    string id = Console.ReadLine();

                    Console.WriteLine("Awesome! Downloading...");

                    var video = service.GetVideo("https://youtube.com/watch?v=" + id);

                    Console.Write("Finished! Would you like to save the video to Downloads? [y/n] ");

                    char opt = Console.ReadKey().KeyChar;

                    Console.WriteLine();

                    string folder;

                    if (Char.ToUpper(opt) == 'Y')
                        folder = GetDefaultFolder();
                    else
                    {
                        Console.Write("Please tell us where you'd like to save it: ");
                        folder = Console.ReadLine();
                    }

                    string path = Path.Combine(folder, video.FullName);

                    Console.WriteLine("Saving...");

                    File.WriteAllBytes(path, video.GetBytes());

                    Console.WriteLine("Done.");
                }
            }
        }

        static string GetDefaultFolder()
        {
            var home = Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile);

            return Path.Combine(home, "Downloads");
        }
    }
}
