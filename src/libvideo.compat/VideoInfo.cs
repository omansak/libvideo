using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;

namespace YoutubeExtractor
{
    public class VideoInfo
    {
        private readonly Video Video;

        internal VideoInfo(Video video)
        {
            this.Video = video;
        }

        public AdaptiveType AdaptiveType
        {
            get
            {
                switch (Video.AdaptiveKind)
                {
                    case AdaptiveKind.Audio:
                        return AdaptiveType.Audio;
                    case AdaptiveKind.Video:
                        return AdaptiveType.Video;
                    default:
                        return AdaptiveType.None;
                }
            }
        }

        public int AudioBitrate
        {
            get
            {
                int result = Video.AudioBitrate;

                return result == -1 ? 0 : result;
            }
        }

        public string AudioExtension
        {
            get
            {
                switch (Video.AudioFormat)
                {
                    case AudioFormat.Aac: return ".aac";
                    case AudioFormat.Mp3: return ".mp3";
                    case AudioFormat.Vorbis: return ".ogg";
                    case AudioFormat.Unknown: return String.Empty;
                    default:
                        // TODO: Consider a var format = AudioFormat; statement at beginning of getter.
                        throw new NotImplementedException($"Audio format {Video.AudioFormat} is unrecognized! Please file an issue at libvideo on GitHub.");
                }
            }
        }

        public AudioType AudioType
        {
            get
            {
                switch (Video.AudioFormat)
                {
                    case AudioFormat.Aac:
                        return AudioType.Aac;
                    case AudioFormat.Mp3:
                        return AudioType.Mp3;
                    case AudioFormat.Vorbis:
                        return AudioType.Vorbis;
                    default:
                        return AudioType.Unknown;
                }
            }
        }

        public bool CanExtractAudio => false;

        public string DownloadUrl => Video.Uri;

        public int FormatCode
        {
            get
            {
                int result = Video.FormatCode;

                return result == -1 ? 0 : result;
            }
        }

        public bool Is3D => Video.Is3D;

        public bool RequiresDecryption => false;

        public int Resolution
        {
            get
            {
                int result = Video.Resolution;

                return result == -1 ? 0 : result;
            }
        }

        public string Title => Video.Title;

        public string VideoExtension => Video.FileExtension;

        public VideoType VideoType
        {
            get
            {
                switch (Video.Format)
                {
                    case VideoFormat.Flash:
                        return VideoType.Flash;
                    case VideoFormat.Mobile:
                        return VideoType.Mobile;
                    case VideoFormat.Mp4:
                        return VideoType.Mp4;
                    case VideoFormat.WebM:
                        return VideoType.WebM;
                    default:
                        return VideoType.Unknown;
                }
            }
        }

        public override string ToString()
        { 
            return string.Format("Full Title: {0}, Type: {1}, Resolution: {2}p", this.Title + this.VideoExtension, this.VideoType, this.Resolution); 
        }
    }
}
