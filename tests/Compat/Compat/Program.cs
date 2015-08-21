using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace Compat
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            Do nothing here.
            We just want to make sure this code compiles.
            */
        }

        static void CompileThis()
        {
            AdaptiveType a = AdaptiveType.Audio;
            a = AdaptiveType.None;
            a = AdaptiveType.Video;

            AudioExtractionException b = new AudioExtractionException(default(string));

            AudioType c = AudioType.Aac;
            c = AudioType.Mp3;
            c = AudioType.Unknown;
            c = AudioType.Vorbis;

            DownloadUrlResolver.DecryptDownloadUrl(default(VideoInfo));
            IEnumerable<VideoInfo> d = DownloadUrlResolver.GetDownloadUrls(default(string));
            d = DownloadUrlResolver.GetDownloadUrls(default(string), default(bool));
            string e = default(string);
            bool f = DownloadUrlResolver.TryNormalizeYoutubeUrl(e, out e);

            VideoInfo g = default(VideoInfo);
            AdaptiveType h = g.AdaptiveType;
            int i = g.AudioBitrate;
            string j = g.AudioExtension;
            AudioType k = g.AudioType;
            bool l = g.CanExtractAudio;
            string m = g.DownloadUrl;
            int n = g.FormatCode;
            bool o = g.Is3D;
            bool p = g.RequiresDecryption;
            int q = g.Resolution;
            string r = g.Title;
            string s = g.VideoExtension;
            VideoType t = g.VideoType;

            VideoNotAvailableException u = new VideoNotAvailableException();
            u = new VideoNotAvailableException(default(string));

            VideoType v = VideoType.Flash;
            v = VideoType.Mobile;
            v = VideoType.Mp4;
            v = VideoType.Unknown;
            v = VideoType.WebM;

            YoutubeParseException w = new YoutubeParseException(default(string), default(Exception));
        }
    }
}
