using System;
using System.Threading.Tasks;
using VideoLibrary.Helpers;

namespace VideoLibrary
{
    public partial class YouTubeVideo : Video
    {
        private readonly string jsPlayerUrl;
        private string jsPlayer;
        private string uri;
        private readonly Query _uriQuery;
        private bool _encrypted;
        private bool _needNDescramble;
        internal YouTubeVideo(VideoInfo info, UnscrambledQuery query, string jsPlayerUrl)
        {
            this.Info = info;
            this.Title = info?.Title;
            this.uri = query.Uri;
            this._uriQuery = new Query(uri);
            this.jsPlayerUrl = jsPlayerUrl;
            this._encrypted = query.IsEncrypted;
            this._needNDescramble = _uriQuery.ContainsKey("n");
            this.FormatCode = int.Parse(_uriQuery["itag"]);
        }

        public override string Title { get; }

        public override VideoInfo Info { get; }

        public override WebSites WebSite => WebSites.YouTube;

        public override string Uri => GetUriAsync().GetAwaiter().GetResult();

        public string GetUri(Func<DelegatingClient> makeClient) => GetUriAsync(makeClient).GetAwaiter().GetResult();

        public override Task<string> GetUriAsync() => GetUriAsync(() => new DelegatingClient());

        public async Task<string> GetUriAsync(Func<DelegatingClient> makeClient)
        {
            if (_encrypted)
            {
                uri = await DecryptAsync(uri, makeClient).ConfigureAwait(false);
                _encrypted = false;
            }

            if (_needNDescramble)
            {
                uri = await DescrambleNAsync(uri, makeClient).ConfigureAwait(false);
                _needNDescramble = false;
            }

            return uri;
        }

        public int FormatCode { get; }

        public long? ContentLength
        {
            get
            {
                if (_contentLength.HasValue)
                    return _contentLength;
                _contentLength = this.GetContentLength(_uriQuery).Result;
                return _contentLength;
            }
        }

        public bool IsEncrypted => _encrypted;

        // Private's
        private long? _contentLength { get; set; }
        private async Task<long?> GetContentLength(Query query)
        {
            if (query.TryGetValue("clen", out string clen))
            {
                return long.Parse(clen);
            }
            using (var client = new VideoClient())
            {
                return await client.GetContentLengthAsync(uri);
            }
        }

    }
}
