using System;
using VideoLibrary;

namespace YoutubeExtractor
{
    public class VideoInfo
    {
        private readonly YouTubeVideo video;

        internal VideoInfo(YouTubeVideo video)
        {
            this.video = video;
        }

        public AdaptiveType AdaptiveType
        {
            get
            {
                switch (video.AdaptiveKind)
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
                int result = video.AudioBitrate;
                return result == -1 ? 0 : result;
            }
        }

        public string AudioExtension
        {
            get
            {
                switch (video.AudioFormat)
                {
                    case AudioFormat.Aac: return ".aac";
                    case AudioFormat.Vorbis: return ".ogg";
                    case AudioFormat.Opus: return ".ogg";
                    case AudioFormat.Unknown: return String.Empty;
                    default:
                        throw new NotImplementedException($"Audio format {video.AudioFormat} is unrecognized! Please file an issue at libvideo on GitHub.");
                }
            }
        }

        public AudioType AudioType
        {
            get
            {
                switch (video.AudioFormat)
                {
                    case AudioFormat.Aac:
                        return AudioType.Aac;
                    case AudioFormat.Vorbis:
                        return AudioType.Vorbis;
                    case AudioFormat.Opus:
                        return AudioType.Opus;
                    default:
                        return AudioType.Unknown;
                }
            }
        }

        public bool CanExtractAudio => false;

        public string DownloadUrl => video.Uri;

        public int FormatCode
        {
            get
            {
                int result = video.FormatCode;
                return result == -1 ? 0 : result;
            }
        }

        public int Fps
        {
            get
            {
                int fps = video.Fps;
                return fps == -1 ? 0 : fps;
            }
        }

        public bool RequiresDecryption => false;

        public int Resolution
        {
            get
            {
                int result = video.Resolution;
                return result == -1 ? 0 : result;
            }
        }

        public string Title => video.Title;

        public string VideoExtension => video.FileExtension;

        public VideoType VideoType
        {
            get
            {
                switch (video.Format)
                {
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
            return string.Format($"Full Title\t{Title + VideoExtension}\nType\t{VideoType}\nResolution\t{Resolution}p\nFormat\t{FormatCode}\nFPS\t{Fps}\nAudioType\t{AudioType}\nBitrate\t{AudioBitrate}\n");
        }
    }
}
