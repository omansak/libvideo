using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoLibrary.Helpers;

namespace VideoLibrary
{
    public partial class YouTubeVideo : Video
    {
        private readonly string jsPlayer;

        private string uri;
        private bool encrypted;
        //Your Service
        // https://libvideo.azurewebsites.net/dfunctionregex.html is not reliable please use own server
        private static string DFuctionRegexService = "https://libvideo.azurewebsites.net/dfunctionregex.html";
        internal YouTubeVideo(string title,
            UnscrambledQuery query, string jsPlayer)
        {
            this.Title = title;
            this.uri = query.Uri;
            this.jsPlayer = jsPlayer;
            if (DFunctionRegex == null)
            {
                DFunctionRegex = new Regex(Task.Run(GetDecryptRegex).Result);
            }
            this.encrypted = query.IsEncrypted;
            this.FormatCode = int.Parse(new Query(uri)["itag"]);
        }
        private async Task<string> GetDecryptRegex()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var r = await httpClient.GetAsync(DFuctionRegexService);
                return await r.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public override string Title { get; }
        public override WebSites WebSite => WebSites.YouTube;

        public override string Uri =>
            GetUriAsync().GetAwaiter().GetResult();

        public string GetUri(Func<DelegatingClient> makeClient) =>
            GetUriAsync(makeClient).GetAwaiter().GetResult();

        public override Task<string> GetUriAsync() =>
            GetUriAsync(() => new DelegatingClient());

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
