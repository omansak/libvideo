using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public class VideoInfo
    {
        public VideoInfo(string title, int? second, string author)
        {
            this.Title = title;
            this.LengthSeconds = second;
            this.Author = author;
        }
        public string Title { get; }
        public int? LengthSeconds { get; }
        public string Author { get; }
    }
    public abstract class Video
    {
        internal Video()
        {
        }
        public abstract string Uri { get; }
        public abstract string Title { get; }
        public abstract VideoInfo Info { get; }
        public abstract WebSites WebSite { get; }
        public virtual VideoFormat Format => VideoFormat.Unknown;
        // public virtual AudioFormat AudioFormat => AudioFormat.Unknown;

        public virtual Task<string> GetUriAsync() =>
            Task.FromResult(Uri);

        public byte[] GetBytes() =>
            GetBytesAsync().GetAwaiter().GetResult();

        public async Task<byte[]> GetBytesAsync()
        {
            using (var client = new VideoClient())
            {
                return await client
                    .GetBytesAsync(this)
                    .ConfigureAwait(false);
            }
        }

        public Stream Stream() => StreamAsync().GetAwaiter().GetResult();

        public async Task<Stream> StreamAsync()
        {
            using (var client = new VideoClient())
            {
                return await client
                    .StreamAsync(this)
                    .ConfigureAwait(false);
            }
        }
        public Stream Head() => HeadAsync().GetAwaiter().GetResult();
        public async Task<Stream> HeadAsync()
        {
            using (var client = new VideoClient())
            {
                return await client
                    .StreamAsync(this)
                    .ConfigureAwait(false);
            }
        }
        public virtual string FileExtension
        {
            get
            {
                switch (Format)
                {
                    case VideoFormat.Flash: return ".flv";
                    case VideoFormat.Mobile: return ".3gp";
                    case VideoFormat.Mp4: return ".mp4";
                    case VideoFormat.WebM: return ".webm";
                    case VideoFormat.Unknown: return string.Empty;
                    default:
                        throw new NotImplementedException($"Format {Format} is unrecognized! Please file an issue at libvideo on GitHub.");
                }
            }
        }

        public string FullName
        {
            get
            {
                var builder =
                    new StringBuilder(Title)
                    .Append(FileExtension);

                foreach (char bad in Path.GetInvalidFileNameChars())
                    builder.Replace(bad, '_');

                return builder.ToString();
            }
        }
    }
}
