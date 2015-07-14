using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrary
{
    public partial class Video
    {
        public Video(string title, 
            string uri, string webSite)
            : this(title, uri, webSite, -1)
        { }

        public Video(string title, string uri, 
            string webSite, int formatCode)
        {
            this.Uri = uri;
            this.Title = title;
            this.WebSite = webSite;
            this.FormatCode = formatCode;
        }

        public string Uri { get; }
        public string Title { get; }
        public string WebSite { get; }
        
        public int FormatCode { get; }

        public byte[] GetBytes() =>
            GetBytesAsync().Result;

        public async Task<byte[]> GetBytesAsync()
        {
            using (var client = new HttpClient())
            {
                return await client
                    .GetByteArrayAsync(Uri)
                    .ConfigureAwait(false);
            }
        }

        // TODO: Consider overriding ToString().
    }
}
