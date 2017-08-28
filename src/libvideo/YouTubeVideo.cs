using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary.Helpers;

namespace VideoLibrary
{
    public partial class YouTubeVideo : Video
    {
        private string uri;
        private bool encrypted;

        internal YouTubeVideo(string title,
            UnscrambledQuery query, string jsPlayer)
        {
            this.Title = title;
            this.uri = query.Uri;
            this.jsPlayer = jsPlayer;
            this.encrypted = query.IsEncrypted;
            this.FormatCode = int.Parse(new Query(uri)["itag"]);
        }

        public override string Title { get; }
        public override WebSites WebSite => WebSites.YouTube;

        [Obsolete("Uri property is deprecated, please use the method GetUri() instead.")]
        public override string Uri => GetUri();

        public override Task<string> GetUriAsync()
        {
            return GetUriAsync(() => new DelegatingClient());
        }

        public async Task<string> GetUriAsync(Func<DelegatingClient> makeClient)
        {
            if (encrypted)
            {
                uri = await
                    DecryptAsync(uri, makeClient)
                    .ConfigureAwait(false);
                encrypted = false;
            }

            return uri;
        }

        public int FormatCode { get; }
        public bool IsEncrypted => encrypted;
    }
}
