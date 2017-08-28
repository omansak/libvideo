using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public abstract class Video
    {
        internal Video()
        {
        }

        [Obsolete("Uri property is deprecated, please use the method GetUri() instead.")]
        public abstract string Uri { get; }

        public string GetUri()
        {
            return GetUriAsync().GetAwaiter().GetResult();
        }
        public abstract Task<string> GetUriAsync();
        public abstract string Title { get; }
        public abstract WebSites WebSite { get; }
        public virtual VideoFormat Format => VideoFormat.Unknown;
        // public virtual AudioFormat AudioFormat => AudioFormat.Unknown;


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

        public Stream Stream() =>
            StreamAsync().GetAwaiter().GetResult();

        public async Task<Stream> StreamAsync()
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
