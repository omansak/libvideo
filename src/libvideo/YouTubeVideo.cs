using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary.Helpers;

namespace VideoLibrary
{
    /// <summary>
    /// Enscapulates information about a video, such as title, format, and resolution.
    /// </summary>
    public partial class YouTubeVideo : Video
    {
        private string uri;
        private bool encrypted;

        internal YouTubeVideo(string title, 
            UnscrambledQuery query, string jsPlayer, 
            Func<string, Task<string>> sourceFactory)
        {
            this.Title = title;
            this.uri = query.Uri;
            this.jsPlayer = jsPlayer;
            this.encrypted = query.IsEncrypted;
            this.sourceFactory = sourceFactory;
            this.FormatCode = int.Parse(new Query(uri)["itag"]);
        }

        public override string Title { get; }
        public override WebSites WebSite => WebSites.YouTube;

        public override string Uri =>
            GetUriAsync().GetAwaiter().GetResult();

        public async override Task<string> GetUriAsync()
        {
            if (encrypted)
            {
                uri = await DecryptAsync(uri);
                encrypted = false;
            }

            return uri;
        }

        /// <summary>
        /// The format code of the video, which it used by YouTube to denote important information.
        /// </summary>
        public int FormatCode { get; }
        public bool IsEncrypted => encrypted;
    }
}
